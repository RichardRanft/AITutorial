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
$AIPlayer::GrenadierRange = 40.0;
$AIPlayer::GrenadeGravityModifier = 0.86;
$AIPlayer::DefaultPriority = 1;

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
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead")
    {
        cancel(%obj.trigger);
        %obj.pushTask("clearTarget");
        return;
    }

    %obj.aimOffset = 0;

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
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead")
    {
        cancel(%obj.trigger);
        %obj.pushTask("clearTarget");
        return;
    }

    %obj.aimOffset = 0;
    %fireState = %obj.getImageTrigger(0);

    if (!%fireState)
        %obj.setImageTrigger(0, 1);

    %obj.pushTask("checkTargetStatus");
    %obj.nextTask();
}

function GrenadierUnitData::fire(%this, %obj)
{
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead")
    {
        cancel(%obj.trigger);
        %obj.pushTask("clearTarget");
        return;
    }

    // ok, here we need to calculate offset by figuring the angle that we need to aim
    // upwards to reach our target and move closer if we can't reach.
    %objWeapon = %obj.getMountedImage(0);
    %velocity = %objWeapon.projectile.muzzleVelocity;
    %range = %obj.getTargetDistance(%obj.target);
    if (%range > $AIPlayer::GrenadierRange)
        %offset = %obj.getBallisticAimPos(%obj.target.getPosition(), %velocity, true, $AIPlayer::GrenadeGravityModifier);
    else
        %offset = %obj.getBallisticAimPos(%obj.target.getPosition(), %velocity, false, $AIPlayer::GrenadeGravityModifier * 0.85);
    if ( %offset == -1 )
    {
        %obj.schedule(32, pushTask, "closeOnTarget");
        %obj.schedule(64, pushTask, "fire" TAB true);
        return;
    }

    %obj.aimOffset = %offset;

    // Tell our AI object to fire its weapon
    %obj.setImageTrigger(0, 1);
    %obj.schedule(64, setImageTrigger, 0, 0);
    %obj.trigger = %obj.schedule(%obj.shootingDelay, pushTask, "checkTargetStatus");
    %obj.nextTask();
}
//-----------------------------------------------------------------------------
// AIPlayer static functions
//-----------------------------------------------------------------------------

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
function AIPlayer::GetBallisticAimPos(%this, %pos, %roundVel, %mortarAim, %gMod)  
{  
    %posFlat = %pos;
    %thisPos = %this.getPosition();
    %posFlat.z = %thisPos.z;
    %x = VectorDist(%thisPos, %posFlat);  
    %y = %pos.z - %thisPos.z;  
    //error("X delta: " @ %x @ " -- Y delta: " @ %y);   

    %g = 9.82 * %gMod;  
    %r1 = mSqrt(mPow(%roundVel,4.0) - %g * (%g * (%x * %x) + ((%y / 4) * (%roundVel * %roundVel))));  

    if (%r1 $= "-1.#IND") // If not a real number, it's not possible to hit %pos, return -1  
        return -1;  

    %a1 = ((%roundVel*%roundVel) - %r1) / (%g * %x);  
    %a1 = mASin(%a1 / mSqrt((%a1 * %a1) + 1));  
    %angleOfReach = mRadToDeg(%a1);  
    if (%mortarAim)  
        %angleOfReach = 90 - %angleOfReach;  
    //error("Angle of reach is " @ %angleOfReach);  

    %offsetHeight = mTan(mDegToRad(%angleOfReach)) * %x;  

    //error(%this @ ": aim offset for gravity = " @ %offsetHeight);  
    return %offsetHeight;
}  

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
{
    if (getWordCount(%pos) < 2 && isObject(%pos))
        %pos = %pos.getPosition();
    return VectorSub(%pos, %this.getPosition());
}

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
        %weapon = %this.getMountedImage(0);
        %velocity = %weapon.projectile.muzzleVelocity;
        %offset = %this.getBallisticAimPos(%this.target.getPosition(), %velocity, true, 1.0);
        if ( %offset == -1 )
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
    %this.setAimObject(0);
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
//  Task system
//-----------------------------------------------------------------------------
// The system needs a prioritization method to "float" higher-priority tasks 
// toward the front of the list.

/// <summary>
/// This function creates a task for the AI unit to carry out.  The <method>
/// parameter is the name of the method to call when carrying out the task plus
/// any method parameters in a TAB separated list.
///
/// For example:
/// %unit.pushTask("method1"); // calls a method with no parameters
/// %unit.pushTask("method2" TAB true); // calls a method and a parameter
/// </summary>
/// <param name="method">The unit method to call, plus method parameters in a TAB separated string list.</param>
function AIPlayer::pushTask(%this, %method)
{
    if (!isObject(%this.taskList))
        %this.taskList = new SimSet();
    %task = new ScriptObject();
    %task.method = %method;
    %this.taskList.add(%task);
    %this.executeTask();
}

/// <summary>
/// This function clears the unit's task list.
/// </summary>
function AIPlayer::clearTasks(%this)
{
    if (isObject(%this.taskList))
        %this.taskList.clear();
    else
        %this.taskList = new SimSet();
}

/// <summary>
/// This function begins execution of the next task in the unit's list.
/// </summary>
function AIPlayer::nextTask(%this)
{
    %this.executeTask();
}

/// <summary>
/// This function gets the next task in the unit's list and parses out the 
/// method and parameters for the task method, then removes the task from 
/// the list and calls the method.
/// </summary>
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
//  Goal system
//-----------------------------------------------------------------------------
// This system should handle more high-level goals such as get repairs or find
// ammo that contain task lists to accomplish the goal.

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

    %canFire = (%this.trigger !$= "" ? !isEventPending(%this.trigger) : true);
    %datablock = %this.getDatablock();
    if (%bool)
    {
        if (%canFire)
            %datablock.fire(%this);
    }
    else
    {
        %fireState = %this.getImageTrigger(0);
        if (%fireState)
            %this.setImageTrigger(0, 0);
        cancel(%this.trigger);
        %this.pushTask("clearTarget");
    }
    %this.nextTask();
}

function AIPlayer::aimAt(%this, %object)
{
    if (isObject(%object))
    {
        %this.target = %object;
        %datablock = %object.getDatablock();
        %offset = "0 0 "@%datablock.boundingBox.z / 2;
        %this.setAimObject(%object, %offset);
    }
    %this.nextTask();
}

function AIPlayer::attack(%this, %target)
{
    %this.target = %target;
    %this.pushTask("aimAt" TAB %target);
    %this.schedule(128, pushTask, "fire" TAB true);
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

/// <summary>
/// This function handles the unit's thinking.  Simple minds and all that....
/// </summary>
function AIPlayer::think(%this)
{
    %canFire = (%this.trigger !$= "" ? !isEventPending(%this.trigger) : true);
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
    {
        if (%canFire)
        {
            %this.pushTask("attack" TAB %this.target);
            return;
        }
    }
    if (isObject(%this.target) && %this.target.getState() $= "dead")
    {
        %this.pushTask("fire" TAB false);
        return;
    }
    %this.pushTask("clearTarget");
}

//-----------------------------------------------------------------------------
// AI Manager system - coordinates groups of AI units
//-----------------------------------------------------------------------------
$AIManager::PriorityTime = 200;
$AIManager::IdleTime = 1000;
$AIManager::PriorityRadius = 75;
$AIManager::SleepRadius = 250;

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
function AIManager::start(%this, %priorityTime, %idleTime, %priorityRadius, %sleepRadius)
{
    MissionCleanup.add(%this);
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
    %this.loadOutUnit(%newUnit, true);
    %this.priorityGroup.add(%newUnit);
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
    if (isObject(%this.client))
    {
        %hCount = %this.priorityGroup.getCount();
        %index = 0;
        while (%index < %hCount)
        {
            %unit = %this.priorityGroup.getObject(%index);
            if (!isObject(%unit))
                %this.priorityGroup.remove(%unit);
            %unitPosition = %unit.getPosition();
            %clientCamLoc = %this.findNearestClientPosition(%unitPosition);
            if (%clientCamLoc $= "")
                continue;
            %range = VectorDist( %clientCamLoc, %unitPosition );
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
            %unitPosition = %unit.getPosition();
            %clientCamLoc = %this.findNearestClientPosition(%unitPosition);
            if (%clientCamLoc $= "")
                continue;
            %range = VectorDist( %clientCamLoc, %unitPosition );
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
            %unitPosition = %unit.getPosition();
            %clientCamLoc = %this.findNearestClientPosition(%unitPosition);
            if (%clientCamLoc $= "")
                continue;
            %range = VectorDist( %clientCamLoc, %unitPosition );
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
        %unit.think();
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
        %unit.think();
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