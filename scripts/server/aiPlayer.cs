//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// AIPlayer callbacks
// The AIPlayer class implements the following callbacks:
//
//    PlayerData::onStop(%this,%obj)
//    PlayerData::onMove(%this,%obj)
//    PlayerData::onReachDestination(%this,%obj)
//    PlayerData::onMoveStuck(%this,%obj)
//    PlayerData::onTargetEnterLOS(%this,%obj)
//    PlayerData::onTargetExitLOS(%this,%obj)
//    PlayerData::onAdd(%this,%obj)
//
// Since the AIPlayer doesn't implement it's own datablock, these callbacks
// all take place in the PlayerData namespace.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Demo Pathed AIPlayer.
//-----------------------------------------------------------------------------

function DemoPlayer::onReachDestination(%this,%obj)
{
   //echo( %obj @ " onReachDestination" );

   // Moves to the next node on the path.
   // Override for all player.  Normally we'd override this for only
   // a specific player datablock or class of players.
   if (%obj.path !$= "")
   {
      if (%obj.currentNode == %obj.targetNode)
         %this.onEndOfPath(%obj,%obj.path);
      else
         %obj.moveToNextNode();
   }
}

function DemoPlayer::onMoveStuck(%this,%obj)
{
   //echo( %obj @ " onMoveStuck" );
}

function DemoPlayer::onTargetExitLOS(%this,%obj)
{
}

function DemoPlayer::onTargetEnterLOS(%this,%obj)
{
}

function DemoPlayer::onEndOfPath(%this,%obj,%path)
{
   %obj.nextTask();
}

function DemoPlayer::onEndSequence(%this,%obj,%slot)
{
   echo("Sequence Done!");
   %obj.stopThread(%slot);
   %obj.nextTask();
}

function DemoPlayerData::fire(%this, %obj)
{
    if (%obj.target.getState() $= "dead")
        return;

    %targetData = %this.target.getDataBlock();
    %z = getWord(%targetData.boundingBox, 2) / 2;
    %offset = "0 0" SPC %z;

    // Tell our AI object to fire its weapon
    %obj.setImageTrigger(0, 1);
    %obj.schedule(64, setImageTrigger, 0, 0);
    if (%obj.target.getState() !$= "dead")
        %obj.trigger = %this.schedule(%obj.shootingDelay, fire, %obj);
    %obj.pushTask("checkTargetStatus");
    %obj.nextTask();
}

function AssaultUnitData::fire(%this, %obj)
{
    if (%obj.target.getState() $= "dead")
    {
        %obj.aimAt(0);
        %obj.setImageTrigger(0, 0);
        cancel(%obj.trigger);
        return;
    }

    %targetData = %this.target.getDataBlock();
    %z = getWord(%targetData.boundingBox, 2) / 2;
    %offset = "0 0" SPC %z;
    %fireState = %obj.getImageTrigger(0);

    if (!%fireState)
        %obj.setImageTrigger(0, 1);

    %obj.pushTask("checkTargetStatus");
    %obj.nextTask();
}

function GrenadierUnitData::fire(%this, %obj)
{
    if (%obj.target.getState() $= "dead")
    {
        %obj.nextTask();
        return;
    }

    // ok, here we need to calculate offset by figuring the angle that we need to aim
    // upwards to reach our target and move closer if we can't reach.
    %targetData = %obj.target.getDataBlock();
    %distToTarget = %obj.getTargetDistance(%obj.target);

    if (%distToTarget > $AIPlayer::GrenadierRange)
    {
        %obj.schedule(32, pushTask, "closeOnTarget");
        %obj.schedule(64, pushTask, "fire" TAB true);
        return;
    }

    %angleBoost = %distToTarget;
    %z = (getWord(%targetData.boundingBox, 2) / 2) + %angleBoost;
    %offset = "0 0" SPC %z;
    %obj.setAimObject(%obj.target, %offset);

    // Tell our AI object to fire its weapon
    %obj.setImageTrigger(0, 1);
    %obj.schedule(64, setImageTrigger, 0, 0);
    if (%obj.target.getState() !$= "dead")
        %obj.trigger = %obj.schedule(%obj.shootingDelay, fire, 1);
    %obj.pushTask("checkTargetStatus");
    %obj.nextTask();
}
//-----------------------------------------------------------------------------
// AIPlayer static functions
//-----------------------------------------------------------------------------
$AIPlayer::GrenadierRange = 30.0;
$AIPlayer::DefaultPriority = 1;

function AIPlayer::spawn(%name, %spawnPoint, %datablock, %priority)
{
    // Create the demo player object
    %player = new AiPlayer()
    {
        dataBlock = (%datablock !$= "" ? %datablock : DemoPlayer);
    };
    %player.priority = (%priority !$= "" ? %priority : $AIPlayer::DefaultPriority);

    %player.shootingDelay = %datablock.shootingDelay;
    MissionCleanup.add(%player);
    %player.setShapeName(%name);
    if (isObject(%spawnPoint) && getWordCount(%spawnPoint) < 2)
        %player.setTransform(%spawnPoint.getPosition());
    else
        %player.setTransform(%spawnPoint);
    return %player;
}

function AIPlayer::spawnOnPath(%name, %path, %datablock, %priority)
{
   // Spawn a player and place him on the first node of the path
   if (!isObject(%path))
      return 0;
   %node = %path.getObject(0);
   %player = AIPlayer::spawn(%name, %node.getTransform(), %datablock, %priority);
   return %player;
}

//-----------------------------------------------------------------------------
// AIPlayer methods
//-----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// Some handy getDistance/nearestTarget functions for the AI to use
// ----------------------------------------------------------------------------

function AIPlayer::getTargetDistance(%this, %target)
{
   %tgtPos = %target.getPosition();
   %eyePoint = %this.getWorldBoxCenter();
   %distance = VectorDist(%tgtPos, %eyePoint);
   return %distance;
}

// Return angle between two vectors
function AIPlayer::getAngle(%vec1, %vec2)
{
  %vec1n = VectorNormalize(%vec1);
  %vec2n = VectorNormalize(%vec2);

  %vdot = VectorDot(%vec1n, %vec2n);
  %angle = mACos(%vdot);

  // convert to degrees and return
  %degangle = mRadToDeg(%angle);
  return %degangle;
}

// return angle between eye vector and %pos
function AIPlayer::getAngleTo(%this, %pos)
{ return AIPlayer::getAngle(%this.getVectorTo(%pos), %this.getEyeVector()); }

// Return position vector to a position
function AIPlayer::getVectorTo(%this, %pos)
{ return VectorSub(%pos, %this.getPosition()); }

function AIPlayer::getTargetDirection(%this, %target)
{
    if (!%target)
        return 0;
    if (%target.getState() $= "dead")
        return 0;
    %tgtPos = %target.player.getPosition();
    %angle = %this.getAngleTo(%tgtPos);
    return %angle;
}

function AIPlayer::seeTarget(%this, %objPos, %targetPos, %angle)
{
    if ( %angle < 60 )
    {
        %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
            $TypeMasks::InteriorObjectType | $TypeMasks::ShapeBaseObjectType | 
            $TypeMasks::StaticObjectType | $TypeMasks::PlayerObjectType;

        // Search!
        %scanTarg = ContainerRayCast( %objPos, %targetPos, %searchMasks);
        if (%scanTarg)
        {
            %obj = getWord(%scanTarg, 0);
            %type = %obj.getClassName();
            if (%type $= "Player" || %type $= "AiPlayer")
            {
                %this.target = %obj;
                return true;
            }
            else
            {
                %this.target = "";
                return false;
            }
        }
    }
    return false;
}

function AIPlayer::closeOnTarget(%this)
{
    if (isObject(%this.target))
    {
        if (%this.getTargetDistance(%this.target) > $AIPlayer::GrenadierRange)
        {
            %this.setMoveDestination(%this.target.getPosition());
            %this.schedule(300, pushTask, "closeOnTarget");
        }
        else
            %this.setMoveDestination(%this.getPosition());
    }
    %this.nextTask();
}

function AIPlayer::checkTargetStatus(%this)
{
    if (isObject(%this.target))
    {
        if (%this.target.getState() $= "dead")
            %this.pushTask("clearTarget");
        else
            %this.schedule(%this.shootingDelay, pushTask, checkTargetStatus);
    }
    %this.nextTask();
}

function AIPlayer::clearTarget(%this)
{
    %this.aimAt(0);
    %this.target = "";
    %this.schedule(32, "setImageTrigger", 0, 0);
    %this.nextTask();
}

function AIPlayer::followPath(%this,%path,%node)
{
   // Start the player following a path
   %this.stopThread(0);
   if (!isObject(%path))
   {
      %this.path = "";
      return;
   }

   if (%node > %path.getCount() - 1)
      %this.targetNode = %path.getCount() - 1;
   else
      %this.targetNode = %node;

   if (%this.path $= %path)
      %this.moveToNode(%this.currentNode);
   else
   {
      %this.path = %path;
      %this.moveToNode(0);
   }
}

function AIPlayer::moveToNextNode(%this)
{
   if (%this.targetNode < 0 || %this.currentNode < %this.targetNode)
   {
      if (%this.currentNode < %this.path.getCount() - 1)
         %this.moveToNode(%this.currentNode + 1);
      else
         %this.moveToNode(0);
   }
   else
      if (%this.currentNode == 0)
         %this.moveToNode(%this.path.getCount() - 1);
      else
         %this.moveToNode(%this.currentNode - 1);
}

function AIPlayer::moveToNode(%this,%index)
{
   // Move to the given path node index
   %this.currentNode = %index;
   %node = %this.path.getObject(%index);
   %this.setMoveDestination(%node.getTransform(), %index == %this.targetNode);
}

//-----------------------------------------------------------------------------
//
//-----------------------------------------------------------------------------

function AIPlayer::pushTask(%this,%method)
{
    if (!isObject(%this.taskList))
        %this.taskList = new SimSet();
    %task = new ScriptObject();
    %task.method = %method;
    %this.taskList.add(%task);
    %this.executeTask();
}

function AIPlayer::clearTasks(%this)
{
    if (isObject(%this.taskList))
        %this.taskList.clear();
    else
        %this.taskList = new SimSet();
}

function AIPlayer::nextTask(%this)
{
    %this.executeTask();
}

function AIPlayer::executeTask(%this)
{
    %taskCount = %this.taskList.getCount();
    if (%taskCount > 0)
    {
        %task = %this.taskList.getObject(0);
        %count = getFieldCount(%task.method);
        %taskMethod = %task.method;
        %this.taskList.remove(%task);
        %task.delete();
        if(%count == 1)
            eval(%this.getId() @"."@ %taskMethod @"();");
        else
        {
            %method = getField(%taskMethod, 0);
            for (%i = 1; %i < %count; %i++)
            {
                if (%i == 1)
                    %data = %data @ getField(%taskMethod, %i);
                else
                    %data = %data @ ", " @ getField(%taskMethod, %i);
            }
            %data = trim(%data);
            eval(%this.getId() @ "." @ %method @ "(" @ %data @ ");");
        }
    }
}

//-----------------------------------------------------------------------------

function AIPlayer::singleShot(%this)
{
    // The shooting delay is used to pulse the trigger
    %this.setImageTrigger(0, true);
    %this.schedule(64, setImageTrigger, 0, false);

    if (%this.target !$= "" && isObject(%this.target) && %this.target.getState() !$= "dead")
        %this.trigger = %this.schedule(%this.shootingDelay, singleShot);
}

//-----------------------------------------------------------------------------

function AIPlayer::wait(%this, %time)
{
   %this.schedule(%time * 1000, "nextTask");
}

function AIPlayer::done(%this,%time)
{
   %this.schedule(0, "delete");
}

function AIPlayer::fire(%this, %bool)
{
    if (!isObject(%this.target))
        %bool = false;

    %datablock = %this.getDatablock();
    %type = %datablock.getName();
    if (%bool)
    {
        switch$(%type)
        {
            case "DemoPlayerData":
                cancel(%this.trigger);
                %datablock.fire(%this);

            case "AssaultUnitData":
                cancel(%this.trigger);
                %datablock.fire(%this);

            case "GrenadierUnitData":
                cancel(%this.trigger);
                %datablock.fire(%this);
        }
    }
    else
    {
        %fireState = %this.getImageTrigger(0);
        if (%fireState)
            %this.setImageTrigger(0, 0);
        cancel(%this.trigger);
    }
    %this.nextTask();
}

function AIPlayer::aimAt(%this, %object)
{
    %this.target = %object;
    %this.setAimObject(%object, %offset);
    %this.nextTask();
}

function AIPlayer::attack(%this, %target)
{
    %this.target = %target;
    %this.pushTask("aimAt" TAB %target);
    %this.pushTask("fire" TAB true);
}

function AIPlayer::animate(%this,%seq)
{
   //%this.stopThread(0);
   //%this.playThread(0,%seq);
   %this.setActionThread(%seq);
}

// ----------------------------------------------------------------------------
// Some handy getDistance/nearestTarget functions for the AI to use
// ----------------------------------------------------------------------------

function AIPlayer::getTargetDistance(%this, %target)
{
   %tgtPos = %target.getPosition();
   %eyePoint = %this.getWorldBoxCenter();
   %distance = VectorDist(%tgtPos, %eyePoint);
   return %distance;
}

function AIPlayer::getNearestPlayerTarget(%this)
{
   %index = -1;
   %botPos = %this.getPosition();
   %count = ClientGroup.getCount();
   for(%i = 0; %i < %count; %i++)
   {
      %client = ClientGroup.getObject(%i);
      if (%client.player $= "" || %client.player == 0)
         return -1;
      %playerPos = %client.player.getPosition();

      %tempDist = VectorDist(%playerPos, %botPos);
      if (%i == 0)
      {
         %dist = %tempDist;
         %index = %i;
      }
      else
      {
         if (%dist > %tempDist)
         {
            %dist = %tempDist;
            %index = %i;
         }
      }
   }
   return %index;
}

function AIPlayer::findTargetInMissionGroup(%this, %radius)
{
    %dist = %radius;
    %botPos = %this.getPosition();
    %count = MissionGroup.getCount();
    for(%i = 0; %i < %count; %i++)
    {
        %object = MissionGroup.getObject(%i);
        if (%object.getClassName() !$= "AIPlayer")
            continue;
        if (%object.team == %this.team)
            continue;
        %playerPos = %object.getPosition();

        %tempDist = VectorDist(%playerPos, %botPos);
        if (%dist > %tempDist && %dist <= %radius)
        {
            %dist = %tempDist;
            %target = %object;
        }
    }
    return %target;
}

function AIPlayer::getNearestTarget(%this, %radius)
{
    %team = %this.team;
    %dist = %radius;
    %botPos = %this.getPosition();
    %count = ClientGroup.getCount();
    for(%i = 0; %i <= %count; %i++)
    {
        if (%i == %team)
            continue;
        %teamList =  "Team"@%i@"List";
        if (!isObject(%teamList))
            continue;
        %teamCount = %teamList.getCount();
        %teamIndex = 0;
        while (%teamIndex < %teamCount)
        {
            %target = %teamList.getObject(%teamIndex);
            %teamIndex++;
            %playerPos = %target.getPosition();

            %tempDist = VectorDist(%playerPos, %botPos);
            if (%dist > %tempDist && %dist < %radius)
            {
                %dist = %tempDist;
                %nearestTarget = %target;
            }
        }
    }
    return %nearestTarget;
}

function AIPlayer::think(%this)
{
    if (!isObject(%this.target))
    {
        %target = %this.getNearestTarget(100.0);
        if (isObject(%target))
        {
            if (%this.seeTarget(%this.getPosition(), %target.getPosition(), 80.0))
                %this.target = %this.getNearestTarget(100.0);
        }
    }
    if (!isObject(%this.target))
        %this.target = %this.findTargetInMissionGroup(25.0);
    if (isObject(%this.target) && %this.target.getState() !$= "dead")
        %this.pushTask("attack" TAB %this.target);
    if (isObject(%this.target) && %this.target.getState() $= "dead")
        %this.pushTask("fire" TAB false);
}

//-----------------------------------------------------------------------------
$AIManager::PriorityTime = 200;
$AIManager::IdleTime = 1000;
$AIManager::PriorityRadius = 75;
$AIManager::SleepRadius = 250;

if (!isObject(AIManager))
    new ScriptObject(AIManager);

function AIManager::start(%this, %priorityTime, %idleTime, %priorityRadius, %sleepRadius)
{
    %this.priorityRadius = (%priorityRadius !$= "" ? %priorityRadius : $AIManager::PriorityRadius);
    %this.sleepRadius = (%sleepRadius !$= "" ? %sleepRadius : $AIManager::SleepRadius);
    %this.priorityTime = (%priorityTime !$= "" ? %priorityTime : $AIManager::PriorityTime);
    %this.idleTime = (%idleTime !$= "" ? %idleTime : $AIManager::IdleTime);

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

function AIManager::addUnit(%this, %name, %spawnLocation, %datablock, %priority, %onPath)
{
    %newUnit = %this.spawn(%name, %spawnLocation, %datablock, %priority, %onPath);
    %this.loadOutUnit(%newUnit);
    %this.priorityGroup.add(%newUnit);
    return %newUnit;
}

function AIManager::think(%this)
{
    if (isObject(%this.client))
    {
        %clientCamLoc = LocalClientConnection.camera.getPosition();
        %hCount = %this.priorityGroup.getCount();
        %index = 0;
        while (%index < %hCount)
        {
            %unit = %this.priorityGroup.getObject(%index);
            if (!isObject(%unit))
                %this.priorityGroup.remove(%unit);
            %range = VectorDist( %clientCamLoc, %unit.getPosition() );
            if (%this.priorityRadius < %range)
            {
                %this.priorityGroup.remove(%unit);
                %this.idleGroup.add(%unit);
                %hCount--;
                echo(" @@@ Moved " @ %unit @ " to idle group : " @ %range);
            }
            %index++;
        }
        %hCount = %this.idleGroup.getCount();
        %index = 0;
        while (%index < %hCount)
        {
            %unit = %this.idleGroup.getObject(%index);
            if (!isObject(%unit))
                %this.idleGroup.remove(%unit);
            %range = VectorDist( %clientCamLoc, %unit.getPosition() );
            if (%this.sleepRadius < %range)
            {
                %this.idleGroup.remove(%unit);
                %this.sleepGroup.add(%unit);
                %hCount--;
                echo(" @@@ Moved " @ %unit @ " to sleep group : " @ %range);
            }
            if (%this.priorityRadius > %range && %unit.priority > 0)
            {
                %this.idleGroup.remove(%unit);
                %this.priorityGroup.add(%unit);
                %hCount--;
                echo(" @@@ Moved " @ %unit @ " to priority group : " @ %range);
            }
            %index++;
        }
        %hCount = %this.sleepGroup.getCount();
        %index = 0;
        while (%index < %hCount)
        {
            %unit = %this.sleepGroup.getObject(%index);
            if (!isObject(%unit))
                %this.sleepGroup.remove(%unit);
            %range = VectorDist( %clientCamLoc, %unit.getPosition() );
            if (%this.sleepRadius > %range)
            {
                %this.sleepGroup.remove(%unit);
                %this.idleGroup.add(%unit);
                %hCount--;
                echo(" @@@ Moved " @ %unit @ " to idle group : " @ %range);
            }
            %index++;
        }
    }
    %this.schedule(500, "think");
}

function AIManager::priorityThink(%this)
{
    %count = %this.priorityGroup.getCount();
    %index = 0;
    while (%index < %count)
    {
        %unit = %this.priorityGroup.getObject(%index);
        %unit.think();
        %index++;
    }
    %this.schedule(%this.priorityTime, "priorityThink");
}

function AIManager::idleThink(%this)
{
    %count = %this.idleGroup.getCount();
    %index = 0;
    while (%index < %count)
    {
        %unit = %this.idleGroup.getObject(%index);
        %unit.think();
        %index++;
    }
    %this.schedule(%this.idleTime, "idleThink");
}

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

function AIManager::loadOutUnit(%this, %unit)
{
    %unit.clearWeaponCycle();
    switch$(%unit.getDatablock().getName())
    {
        case "DemoPlayerData":
            %unit.setInventory(Ryder, 1);
            %unit.setInventory(RyderClip, %unit.maxInventory(RyderClip));
            %unit.setInventory(RyderAmmo, %unit.maxInventory(RyderAmmo));    // Start the gun loaded
            %unit.addToWeaponCycle(Ryder);

        case "AssaultUnitData":
            %unit.setInventory(Lurker, 1);
            %unit.setInventory(LurkerClip, %unit.maxInventory(LurkerClip));
            %unit.setInventory(LurkerAmmo, %unit.maxInventory(LurkerAmmo));  // Start the gun loaded
            %unit.addToWeaponCycle(Lurker);

        case "GrenadierUnitData":
            %unit.setInventory(LurkerGrenadeLauncher, 1);
            %unit.setInventory(LurkerGrenadeAmmo, %unit.maxInventory(LurkerGrenadeAmmo));
            %unit.addToWeaponCycle(LurkerGrenadeLauncher);
    }
    if (%unit.getDatablock().mainWeapon.image !$= "")
        %unit.mountImage(%unit.getDatablock().mainWeapon.image, 0);
    else
        %unit.mountImage(Lurker, 0);
}