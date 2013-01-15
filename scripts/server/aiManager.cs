//-----------------------------------------------------------------------------
// AI Manager system - coordinates groups of AI units
//-----------------------------------------------------------------------------
$AIManager::PriorityTime = 200;
$AIManager::IdleTime = 1000;
$AIManager::PriorityRadius = 75;
$AIManager::SleepRadius = 250;
$AIManager::ThinkTime = 500;

if (!isObject(AIManager))
    new ScriptObject(AIManager);

/// <summary>
/// This function starts an AIManager object.  The AIManager is to coordinate 
/// AI unit processing to help conserve processing time.
///
/// Units within <priorityRadius> will have their "think" method called 
/// every <priorityTime> milliseconds.  Units outside of <priorityRadius> but
/// inside of <sleepRadius> will have their "think" method called every
/// <idleTime> milliseconds.  Units outside of <sleepRadius> will not have their
/// "think" method called at all.
///
/// Units can be assigned a priority number which will be used to help determine if
/// that unit should ever sleep or if it ever needs priority timing.
///
/// Units with priority 2 or higher will never be put to "sleep."
/// Units with priority 1 will be sorted normally.
/// Units with priority 0 will never be processed faster than <idleTime>.
///
/// So, perhaps RTS combat units should be priority 2 so that they can always respond
/// to approaching enemies even when far from the center of attention.  Then ambient 
/// non-combatant or "decorative" units should have a priority of 1 or 0 depending on
/// whether you want them to respond quickly to nearby events or not.  Really, this 
/// would probably be more handy for RPG-light games where spawned units (vendors, guards)
/// would need to be present but might not need to actually do anything while the players
/// aren't in their immediate vicinity.
/// </summary>
/// <param name="priorityTime">The number of milliseconds between "think" calls for priority units.</param>
/// <param name="idleTime">The number of milliseconds between "think" calls for idle units.</param>
/// <param name="priorityRadius">The number of world units around any player camera within which units will be given priority timing.</param>
/// <param name="sleepRadius">The number of world units around any player camera outside of which units will be suspended from thinking.</param>
/// <param name="thinkTime">The number of milliseconds between manager think calls (for prioritizing AI units).</param>
function AIManager::start(%this, %priorityTime, %idleTime, %priorityRadius, %sleepRadius, %thinkTime)
{
    MissionCleanup.add(%this);
    %this.priorityRadius = (%priorityRadius !$= "" ? %priorityRadius : $AIManager::PriorityRadius);
    %this.sleepRadius = (%sleepRadius !$= "" ? %sleepRadius : $AIManager::SleepRadius);
    %this.priorityTime = (%priorityTime !$= "" ? %priorityTime : $AIManager::PriorityTime);
    %this.idleTime = (%idleTime !$= "" ? %idleTime : $AIManager::IdleTime);
    %this.thinkTime = (%thinkTime !$= "" ? %thinkTime : $AIManager::ThinkTime);

    if (!isObject(%this.priorityGroup))
        %this.priorityGroup = new SimSet();
    else
        %this.priorityGroup.clear();
    if (!isObject(%this.idleGroup))
        %this.idleGroup = new SimSet();
    else
        %this.idleGroup.clear();
    if (!isObject(%this.sleepGroup))
        %this.sleepGroup = new SimSet();
    else
        %this.sleepGroup.clear();

    %this.think();
    %this.priorityThink();
    %this.idleThink();

    %this.started = true;
}

/// <summary>
/// This function requests that the AIManager spawn a unit and add it to its group.
/// </summary>
/// <param name="name">The desired unit name - this is the SimName of the object and must be unique or "".</param>
/// <param name="spawnLocation">The position or object (spawnpoint, path object) to spawn the unit at.</param>
/// <param name="datablock">The datablock that the unit should use.</param>
/// <param name="priority">The priority of this unit. 0 to 2 from low to high priority.  Defaults to 1.</param>
/// <param name="onPath">If spawnLocation is a path, this should be true to get the unit to spawn on and follow the path.</param>
/// <return>Returns the new unit.</return>
function AIManager::addUnit(%this, %name, %spawnLocation, %datablock, %priority, %onPath)
{
    %newUnit = %this.spawn(%name, %spawnLocation, %datablock, %priority, %onPath);
    %this.loadOutUnit(%newUnit, false);
    if (%newUnit.priority > 0)
        %this.priorityGroup.add(%newUnit);
    else
        %this.idleGroup.add(%newUnit);
    return %newUnit;
}

/// <summary>
/// This function handles sorting the AIManager's managed units by distance and
/// priority.
/// </summary>
function AIManager::think(%this)
{
    // The purpose here is to reduce overhead from AI for units that are
    // farther "from the action."  So I'm using sorting units from one 
    // list to another based on the unit's distance from the nearest 
    // player's camera, since any unit near any player's 
    // camera needs to be "thinking" at the correct priority.
	%index = %this.priorityGroup.getCount() - 1;
	while (%index >= 0)
	{
		%unit = %this.priorityGroup.getObject(%index);
		if (!isObject(%unit) || %unit.getState() $= "dead")
		{
			if (%this.priorityGroup.isMember(%unit))
			{
				%this.priorityGroup.remove(%unit);
			}
			%index--;
			continue;
		}
		%unitPosition = %unit.getPosition();
		%clientCamLoc = %this.findNearestClientPosition(%unitPosition);
		if (%clientCamLoc $= "")
		{
		    %index--;
			continue;
		}
		%range = VectorDist( %clientCamLoc, %unitPosition );
		if (%this.priorityRadius < %range)
		{
			%this.priorityGroup.remove(%unit);
			%this.idleGroup.add(%unit);
		}
		%index--;
	}
	%index = %this.idleGroup.getCount() - 1;
	while (%index >= 0)
	{
		%unit = %this.idleGroup.getObject(%index);
		if (!isObject(%unit) || %unit.getState() $= "dead")
		{
			if (%this.idleGroup.isMember(%unit))
			{
				%this.idleGroup.remove(%unit);
			}
			%index--;
			continue;
		}
		%unitPosition = %unit.getPosition();
		%clientCamLoc = %this.findNearestClientPosition(%unitPosition);
		if (%clientCamLoc $= "")
		{
            %index--;
			continue;
		}
		%range = VectorDist( %clientCamLoc, %unitPosition );
		if (%this.sleepRadius < %range)
		{
			%this.idleGroup.remove(%unit);
			%this.sleepGroup.add(%unit);
		}
		if (%this.priorityRadius > %range && %unit.priority > 0)
		{
			%this.idleGroup.remove(%unit);
			%this.priorityGroup.add(%unit);
		}
		%index--;
	}
	%index = %this.sleepGroup.getCount() - 1;
	while (%index >= 0)
	{
		%unit = %this.sleepGroup.getObject(%index);
		if (!isObject(%unit) || %unit.getState() $= "dead")
		{
			if (%this.sleepGroup.isMember(%unit))
			{
				%this.sleepGroup.remove(%unit);
			}
			%index--;
			continue;
		}
		%unitPosition = %unit.getPosition();
		%clientCamLoc = %this.findNearestClientPosition(%unitPosition);
		if (%clientCamLoc $= "")
		{
		    %index--;
			continue;
		}
		%range = VectorDist( %clientCamLoc, %unitPosition );
		if (%this.sleepRadius > %range)
		{
			%this.sleepGroup.remove(%unit);
			%this.idleGroup.add(%unit);
		}
		%index--;
	}
    %this.schedule(%this.thinkTime, "think");
}

/// <summary>
/// This function finds the nearest client to the position in question.
/// </summary>
/// <param name="position">The position to test clients against.</param>
/// <return>Returns the position of the client nearest to <position>.</return>
function AIManager::findNearestClientPosition(%this, %position)
{
    %clientCount = ClientGroup.getCount();
    %dist = 125000; // arbitrarily large starting distance
    %pos = "";
    for (%i = 0; %i < %clientCount; %i++)
    {
        %client = ClientGroup.getObject(%i);
        if (isObject(%client.camera))
        {
            %clientPos = %client.camera.getPosition();
            %tempDist = VectorDist(%position, %clientPos);
            if (%dist > %tempDist)
            {
                %dist = %tempDist;
                %pos = %clientPos;
            }
        }
    }
    return %pos;
}

/// <summary>
/// This function finds the nearest client to the position in question.
/// </summary>
/// <param name="position">The position to test clients against.</param>
/// <return>Returns the position of the client nearest to <position>.</return>
function AIManager::findNearestUnit(%this, %unit, %radius)
{
    if(!isObject(%unit))
        return;
    %position = %unit.getPosition();

    %dist = 125000; // arbitrarily large starting distance
    %priorityUnitCount = %this.priorityGroup.getCount();
    for (%i = 0; %i < %priorityUnitCount; %i++)
    {
        %obj = %this.priorityGroup.getObject(%i);
        if (isObject(%obj) && %obj != %unit && %obj.team == %unit.team)
        {
            %targetPos = %obj.getPosition();
            %tempDist = VectorDist(%position, %targetPos);
            if (%dist > %tempDist && %tempDist < %radius)
            {
                %dist = %tempDist;
                %targetUnit = %obj;
            }
        }
    }
    %idleUnitCount = %this.idleGroup.getCount();
    for (%i = 0; %i < %idleUnitCount; %i++)
    {
        %obj = %this.idleGroup.getObject(%i);
        if (isObject(%obj) && %obj != %unit && %obj.team == %unit.team)
        {
            %targetPos = %obj.getPosition();
            %tempDist = VectorDist(%position, %targetPos);
            if (%dist > %tempDist && %tempDist < %radius)
            {
                %dist = %tempDist;
                %targetUnit = %obj;
            }
        }
    }
    return (isObject(%targetUnit) ? %targetUnit : 0);
}

/// <summary>
/// This function finds the nearest client to the position in question.
/// </summary>
/// <param name="position">The position to test clients against.</param>
/// <return>Returns the position of the client nearest to <position>.</return>
function AIManager::findNearestTarget(%this, %unit, %radius)
{
    %position = %unit.getPosition();

    %dist = 125000; // arbitrarily large starting distance
    %priorityUnitCount = %this.priorityGroup.getCount();
    for (%i = 0; %i < %priorityUnitCount; %i++)
    {
        %obj = %this.priorityGroup.getObject(%i);
        if (isObject(%obj) && %obj.team != %unit.team)
        {
            %targetPos = %obj.getPosition();
            %tempDist = VectorDist(%position, %targetPos);
            if (%dist > %tempDist && %tempDist < %radius)
            {
                %dist = %tempDist;
                %targetUnit = %obj;
            }
        }
    }
    %idleUnitCount = %this.idleGroup.getCount();
    for (%i = 0; %i < %idleUnitCount; %i++)
    {
        %obj = %this.idleGroup.getObject(%i);
        if (isObject(%obj) && %obj.team != %unit.team)
        {
            %targetPos = %obj.getPosition();
            %tempDist = VectorDist(%position, %targetPos);
            if (%dist > %tempDist && %tempDist < %radius)
            {
                %dist = %tempDist;
                %targetUnit = %obj;
            }
        }
    }
    return (isObject(%targetUnit) ? %targetUnit : 0);
}

/// <summary>
/// This function processes unit.think() for all units in the high priority
/// group every <priorityTime> milliseconds.
/// </summary>
function AIManager::priorityThink(%this)
{
    %count = %this.priorityGroup.getCount();
    %index = 0;
    while (%index < %count)
    {
        %unit = %this.priorityGroup.getObject(%index);
        %step = %this.priorityTime / %count;
        %time = getRandom(32, (32 + (%index * %step)) );
        %unit.schedule(%time, think);
        %index++;
    }
    %this.schedule(%this.priorityTime, "priorityThink");
}

/// <summary>
/// This function processes unit.think() for all units in the low priority 
/// group every <idleTime> milliseconds.
/// </summary>
function AIManager::idleThink(%this)
{
    %count = %this.idleGroup.getCount();
    %index = 0;
    while (%index < %count)
    {
        %unit = %this.idleGroup.getObject(%index);
        %step = %this.idleTime / %count;
        %time = getRandom(32, (32 + (%index * %step)) );
        %unit.schedule(%time, think);
        %index++;
    }
    %this.schedule(%this.idleTime, "idleThink");
}

/// <summary>
/// This function asks AIPlayer to spawn a unit.
/// </summary>
/// <param name="name">The desired unit name - this is the SimName of the object and must be unique or "".</param>
/// <param name="spawnLocation">The position or object (spawnpoint, path object) to spawn the unit at.</param>
/// <param name="datablock">The datablock that the unit should use.</param>
/// <param name="priority">The priority of this unit. 0 to 2 from low to high priority.  Defaults to 1.</param>
/// <param name="onPath">If spawnLocation is a path, this should be true to get the unit to spawn on and follow the path.</param>
/// <return>Returns the new unit, or 0 on failure.</return>
function AIManager::spawn(%this, %name, %spawnLocation, %datablock, %priority, %onPath)
{
    if (%onPath)
    {
        %path = (%spawnLocation !$= "" ? %spawnLocation : "MissionGroup/Paths/Path1");
        %player = AIPlayer::spawnOnPath(%name, %path, %datablock, %priority);

        if (isObject(%player))
        {
            %player.followPath(%path, -1);

            return %player;
        }
        else
            return 0;
    }
    else
    {
        %location = %spawnLocation;
        if (%location $= "")
        {
            %count = PlayerDropPoints.getCount();
            %index = getRandom(0, %count - 1);
            %location = PlayerDropPoints.getObject(%index);
        }
        %player = AIPlayer::spawn(%name, %location, %datablock, %priority);

        if (isObject(%player))
            return %player;
        else
            return 0;
    }
}

/// <summary>
/// This function loads out the unit with the equipment it should have based on 
/// </summary>
/// <param name="unit">The unit to equip.</param>
/// <param name="infiniteAmmo">If true, the weapon will not consume ammo.</param>
function AIManager::loadOutUnit(%this, %unit, %infiniteAmmo)
{
    %unit.clearWeaponCycle();
    
    %datablock = %unit.getDatablock();
    %weapon = %datablock.mainWeapon.image;
    %weapon.infiniteAmmo = %infiniteAmmo;
    %clip = %weapon.clip;
    %ammo = %weapon.ammo;

    %unit.setInventory(%weapon, 1);
    if (%clip !$= "")
        %unit.setInventory(%clip, %unit.maxInventory(%clip));
    %unit.setInventory(%ammo, %unit.maxInventory(%ammo));    // Start the gun loaded
    %unit.addToWeaponCycle(%weapon);

    if (%unit.getDatablock().mainWeapon.image !$= "")
        %unit.mountImage(%unit.getDatablock().mainWeapon.image, 0);
    else
        %unit.mountImage(Lurker, 0);
}