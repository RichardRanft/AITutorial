//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// AIPlayer default overrides for datablock callbacks
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
// all take place in the PlayerData namespace.  Below are implemented standard 
// versions of these callbacks so that the datablock callbacks have a common 
// fallback.
//-----------------------------------------------------------------------------

/// <summary>
/// This function is called by the unit's datablock onReachDestination() callback to 
/// act as a default callback handler.
/// </summary>
function AIPlayer::ReachDestination(%this)
{
    // Moves to the next node on the path.
    // Override for all player.  Normally we'd override this for only
    // a specific player datablock or class of players.
    if (%this.path !$= "")
    {
        if (%this.currentNode == %this.targetNode)
            %this.onEndOfPath(%this,%this.path);
        else
            %this.moveToNextNode();
    }
    else
    {
        if (!isObject(%this.target) || %this.target.getState() $= "dead")
        {
            if (!%this.destReached)
            {
                %this.destReached = true;
                %x = getRandom(-5, 5);
                %y = getRandom(-5, 5);
                %vec = %x SPC %y SPC "0";

                %this.setMoveDestination(VectorAdd(%this.getPosition(), %vec));
            }
            else
                %this.destReached = false;

            %this.nextTask();
        }
    }
    if(isObject(%this.target) && %this.target.getState() !$= "dead")
        %this.pushTask("attack" TAB %this.target);
}

/// <summary>
/// This function is called by the unit's datablock onMoveStuck() callback to 
/// act as a default callback handler.
/// </summary>
function AIPlayer::MoveStuck(%this)
{
    %x = getRandom(-5, 5);
    %y = getRandom(-5, 5);
    %vec = %x SPC %y SPC "0";
    %dest = VectorAdd(%this.getPosition(), %vec);
    %this.setMoveDestination(%dest);
    echo(" @@@ unit " @ %this @ " stuck - attempting to redirect to " @ %dest);
}

/// <summary>
/// This function is called by the unit's datablock onTargetExitLOS() callback to 
/// act as a default callback handler.
/// </summary>
function AIPlayer::TargetExitLOS(%this)
{
    %this.pushTask("fire" TAB %this TAB false);
    %this.setMoveDestination(%this.targetLKP);
}

/// <summary>
/// This function is called by the unit's datablock onTargetEnterLOS() callback to 
/// act as a default callback handler.
/// </summary>
function AIPlayer::TargetEnterLOS(%this)
{
    if ( %this.target != %this && isObject(%this.target) )
        %this.targetLKP = %this.target.getPosition();
    %this.pushTask("attack" TAB %this.target);
}

/// <summary>
/// This function is called by the unit's datablock onEndOfPath() callback to 
/// act as a default callback handler.
/// </summary>
/// <param name="path">The path the unit is assigned to.</param>
function AIPlayer::EndOfPath(%this,%path)
{
   %this.nextTask();
}

/// <summary>
/// This function stops the unit's current animation sequence.
/// </summary>
/// <param name="slot">The animation slot to stop.</param>
function AIPlayer::EndSequence(%this,%slot)
{
   echo("Sequence Done!");
   %this.stopThread(%slot);
   %this.nextTask();
}

//-----------------------------------------------------------------------------
// AIPlayer static functions
//-----------------------------------------------------------------------------

/// <summary>
/// This function removes the calling unit from its AIClientManager and from
/// its team list if it is part of a multi-select group.
/// </summary>
function AIPlayer::removeFromTeam(%this)
{
    %this.AIClientMan.removeUnit(%this);
    %teamList = "Team"@%this.team@"List";
    if (isObject(%teamList))
    {
        if (%teamList.isMember(%this))
            %teamList.remove(%this);
    }
}

/// <summary>
/// This function spawns a new AI unit.
/// </summary>
/// <param name="name">The name to assign to the unit.  Should be "" for none.</param>
/// <param name="spawnPoint">The spawn point to spawn the unit at.  Can also be a vector location.</param>
/// <param name="datablock">The datablock to use for the new unit.</param>
/// <param name="priority">The priority to assign to the unit.  Defaults to 1.</param>
/// <return>Returns the new unit.</return>
function AIPlayer::spawn(%name, %spawnPoint, %datablock, %priority)
{
    // Create the demo player object
    %player = new AiPlayer()
    {
        dataBlock = (%datablock !$= "" ? %datablock : DemoPlayer);
    };
    %player.priority = (%priority !$= "" ? %priority : $AIPlayer::DefaultPriority);

    %player.shootingDelay = %datablock.shootingDelay;
    %player.damageLvl = 0;
    %player.moveTolerance = (%datablock.moveTolerance !$= "" ? %datablock.moveTolerance : 0.25);
    MissionCleanup.add(%player);
    %player.setShapeName(%name);
    if (isObject(%spawnPoint) && getWordCount(%spawnPoint) < 2)
        %player.setTransform(%spawnPoint.getPosition());
    else
        %player.setTransform(%spawnPoint);
    return %player;
}

/// <summary>
/// This function spawns an AI unit on a path.
/// </summary>
/// <param name="name">The name to assign to the player. Should be "" for none.</param>
/// <param name="path">The path object to assign the unit to.</param>
/// <param name="datablock">The datablock to use for the unit.</param>
/// <param name="priority">The priority to assign to the unit.  Defaults to 1.</param>
/// <return>Returns the new unit.</return>
function AIPlayer::spawnOnPath(%name, %path, %datablock, %priority)
{
   // Spawn a player and place him on the first node of the path
   if (!isObject(%path))
      return 0;
   %node = %path.getObject(0);
   %player = AIPlayer::spawn(%name, %node.getTransform(), %datablock, %priority);
   return %player;
}

/// <summary>
/// This function determines if the target is within the unit's line of sight.  It
/// sets a variable to indicate the point at which our line of sight is obstructed
/// in the event there is no clear view so that the unit can move toward the target.
/// </summary>
/// <param name="target">The target of our visual search.</param>
/// <return>True if target can be seen, false if not.</return>
function AIPlayer::getLOS(%this, %target)
{
    if (!isObject(%target))
    {
        %this.canFire = true;
        return false;
    }
    %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
        $TypeMasks::InteriorObjectType | $TypeMasks::StaticObjectType;

    // Search!
    %objPos = %this.getEyePoint();
    %targetPos = %target.getWorldBoxCenter();
    %scanTarg = ContainerRayCast( %objPos, %targetPos, %searchMasks);
    if (%scanTarg)
    {
        %this.intersectPos = getWords(%scanTarg, 1, 3);
        %this.canFire = false;
        return false;
    }
    %this.canFire = true;
    return true;
}

/// <summary>
/// This function handles the unitUnderAttack event.  It determines if there is
/// a datablock-specific response to the event and calls it, or calls the default
/// AIPlayer response.
/// The format of <msgData> is assumed to be <originatingUnit>TAB<messageHandler>TAB<tab-delimited data>
/// </summary>
/// <param name="msgData">Data to pass to the actual event handler.
function AIPlayer::unitUnderAttack(%this, %msgData)
{
    %unit = getField(%msgData, 0);
    if (%this.team != %unit.team || %this.respondedTo == %unit)
        return;
    %method  = getField(%msgData, 1);
    %datablock = %this.getDataBlock();
    if (%datablock.isMethod(%method))
        eval(%datablock.getName()@"."@%method@"(%this, \""@%msgData@"\");");
    else if (%this.isMethod(%method))
    {
        eval("%this."@%method@"(\""@%msgData@"\");");
    }
}

/// <summary>
/// This function handles the unitUnderAttack event for AIPlayer if there is no
/// datablock-specific handler.
/// </summary>
function AIPlayer::underAttack(%this, %msgData)
{
    %unit = getField(%msgData, 0);
    %unitPos = %unit.getPosition();
    %thisPos = %this.getPosition();
    %source = getField(%msgData, 3);
    if (%source.sourceObject.team == %this.team)
        return;
    %dist = VectorDist(%unitPos, %thisPos) - $AIEventManager::DefaultAttackResponseDist;
    %distWeight = (1/(%dist > 0 ? %dist : 1));
    if (%distWeight > 0.1)
    {
        %this.target = %source.sourceObject;
        // if we can't see the target, move closer to ally
        // otherwise just "evade" (ok, run around randomly).
        if (!%this.getLOS(%this.target))
            %dest = %unitPos;
        else
            %dest = %thisPos;
        %offsetX = getRandom(-20, 20);
        %offsetY = getRandom(-20, 20);
        %dest.x += %offsetX;
        %dest.y += %offsetY;
        %this.setMoveDestination(%dest);
        %unit.notifyAttackResponse();
        %this.respondedTo = %unit;
    }
}

/// <summary>
/// This function sets a flag to prevent the unit for repeatedly crying for help.
/// </summary>
function AIPlayer::notifyAttackResponse(%this)
{
    %this.receivedAttackResponse = true;
}

//-----------------------------------------------------------------------------
// AIPlayer methods
//-----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// Some handy getDistance/nearestTarget functions for the AI to use
// ----------------------------------------------------------------------------

/// <summary>
/// This function calculates a firing offset for ballistic projectiles.  Thanks to 
/// Bryce for the vast majority of this function -
/// https://www.garagegames.com/community/resources/view/19739
/// <summary>
/// <param name="pos">The target position.</param>
/// <param name="roundVel">The muzzle velocity of the projectile.</param>
/// <param name="mortarAim">True to use high arc aim, false to use flat arc aim.</param>
/// <param name="gMod">The amount of gravity to apply to the projectile.</param>
/// <return>Returns the z axis aim offset position - or how high above the target to aim.</return>
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

/// <summary>
/// This function calculates the distance from the unit to a target object.
/// <summary>
/// <param name="target">The target to get the distance to.</param>
/// <return>Returns the distance from the unit to the target.</return>
function AIPlayer::getTargetDistance(%this, %target)
{
   %tgtPos = %target.getPosition();
   %eyePoint = %this.getWorldBoxCenter();
   %distance = VectorDist(%tgtPos, %eyePoint);
   return %distance;
}

/// <summary>
/// This function calculates the angle between two vectors.  Note! does not
/// take a %this parameter, so must be called scoped: 
/// %angle = AIPlayer::getAngle(%vec1, %vec2);
/// <summary>
/// <param name="vec1">The first vector.</param>
/// <param name="vec2">The second vector.</param>
/// <return>Returns a scalar angle in degrees between the two vectors.</return>
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

/// <summary>
/// This function calculates the angle between eye vector and %pos
/// <summary>
/// <param name="pos">The target position.</param>
/// <return>Returns a scalar angle in degrees.</return>
function AIPlayer::getAngleTo(%this, %pos)
{
    return AIPlayer::getAngle(%this.getVectorTo(%pos), %this.getEyeVector());
}

// Return position vector to a position
/// <summary>
/// This function calculates the vector to %pos from eye point
/// <summary>
/// <param name="pos">The target position.</param>
/// <return>Returns a 3D vector from eye pos to target pos (not normalized).</return>
function AIPlayer::getVectorTo(%this, %pos)
{
    if (getWordCount(%pos) < 2 && isObject(%pos))
        %pos = %pos.getPosition();
    return VectorSub(%pos, %this.getPosition());
}

/// <summary>
/// This function gets the direction from the unit to the target.
/// <summary>
/// <param name="target">The target object.</param>
/// <return>Returns a scalar angle in degrees.</return>
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

/// <summary>
/// This function determines if the target is within the unit's sight cone.
/// <summary>
/// <param name="objPos">The unit's position.</param>
/// <param name="targetPos">The target's position.</param>
/// <param name="angle">The target's angle off of direct front.</param>
/// <param name="viewAngle">The number of degrees off of forward that the cone extends (1/2 view width).</param>
/// <return>Returns true if the target is in "sight" or false if not.</return>
function AIPlayer::seeTarget(%this, %objPos, %targetPos, %angle, %viewAngle)
{
    if (%viewAngle $= "")
        %viewAngle = 80;
    if ( %angle <= %viewAngle )
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

/// <summary>
/// This function tells the unit to follow a path object.
/// <summary>
/// <param name="path">The path object to follow.</param>
/// <param name="node">The path node to move to.</param>
function AIPlayer::followPath(%this, %path, %node)
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

/// <summary>
/// This function tells the unit to move to the next node in it's assigned path.
/// <summary>
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

/// <summary>
/// This function tells the unit to move to a specific node location on its assigned
/// path object.
/// <summary>
/// <param name="index">The index of the node to move to.</param>
function AIPlayer::moveToNode(%this,%index)
{
   // Move to the given path node index
   %this.currentNode = %index;
   %node = %this.path.getObject(%index);
   %this.setMoveDestination(%node.getTransform(), %index == %this.targetNode);
}

/// <summary>
/// This function determines the distance from the unit to a target.
/// <summary>
/// <param name="target">The target object.</param>
/// <return>Returns the distance from the unit to the target.</return>
function AIPlayer::getTargetDistance(%this, %target)
{
   %tgtPos = %target.getPosition();
   %eyePoint = %this.getWorldBoxCenter();
   %distance = VectorDist(%tgtPos, %eyePoint);
   return %distance;
}

/// <summary>
/// This function finds the nearest "player" object to the unit.
/// <summary>
/// <return>Returns the nearest "player" or -1 if none is found.</return>
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

/// <summary>
/// This function finds the nearest AI unit in the MissionGroup to the unit and
/// within the specified radius.
/// <summary>
/// <param name="radius">The search radius.</param>
/// <return>Returns the nearest AI unit or a blank string if none are within range.</return>
function AIPlayer::findTargetInMissionGroup(%this, %radius)
{
    %target = "";
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

/// <summary>
/// This function finds the nearest AI unit that is not on this unit's team within the
/// given radius.
/// <summary>
/// <param name="radius">The search radius.</param>
/// <return>Returns the nearest non-team unit or a blank string if none are in range.</return>
function AIPlayer::getNearestTarget(%this, %radius)
{
    %nearestTarget = AIManager.findNearestTarget(%this, %radius);
    return %nearestTarget;
}

/// <summary>
/// This function destroys the unit after the number of milliseconds specified.
/// <summary>
/// <param name="time">The wait time in milliseconds.</param>
function AIPlayer::done(%this, %time)
{
    if (%time $= "")
        %time = 32;
    %this.schedule(%time, "delete");
}

/// <summary>
/// This function instructs the unit to attack a target.
/// <summary>
/// <param name="target">The target object to attack.</param>
function AIPlayer::attack(%this, %target)
{
    %this.target = %target;
    %this.pushTask("aimAt" TAB %target);
    %this.schedule(128, pushTask, "fire" TAB true);
}

/// <summary>
/// This function instructs the unit to play a specified animation sequence.
/// <summary>
/// <param name="seq">The animation sequence to play.</param>
function AIPlayer::animate(%this,%seq)
{
   %this.setActionThread(%seq);
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
    if (!isObject(%this.taskList))
        %this.taskList = new SimSet();
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
    if (!isObject(%this.taskList))
        %this.taskList = new SimSet();
    %this.executeTask();
}

/// <summary>
/// This function gets the next task in the unit's list and parses out the 
/// method and parameters for the task method, then removes the task from 
/// the list and calls the method.
/// </summary>
function AIPlayer::executeTask(%this)
{
    if (!isObject(%this.taskList))
        %this.taskList = new SimSet();
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
//  Tasks
//-----------------------------------------------------------------------------
// Task methods should end in %this.nextTask()

/// <summary>
/// This function causes the AI to attempt to move toward its target until it's
/// within weapon range of the target.  Used only by the grenadier at the moment, 
/// but this should be revised to be more general.
/// <summary>
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
            %this.stop();
    }
    %this.nextTask();
}

/// <summary>
/// This function checks to see if the current target is dead.  If it is, the unit
/// clears its current target, otherwise it reschedules to check again.
/// <summary>
function AIPlayer::checkTargetStatus(%this)
{
    if (isObject(%this.target))
    {
        if (%this.target.getState() $= "dead")
            %this.pushTask("clearTarget");
        else
            %this.schedule(%this.shootingDelay, "pushTask", "checkTargetStatus");
    }
    %this.nextTask();
}

/// <summary>
/// This function evaluates the unit's current health and ammo levels to determine
/// if we should look for healing or resupply.  This can be overridden by the unit's
/// datablock.
/// </summary>
function AIPlayer::evaluateCondition(%this)
{
    %datablock = %this.getDataBlock();
    if (%datablock.isMethod("evaluateCondition"))
        %datablock.evaluateCondition(%this);
    else
    {
        %maxDamage = %this.getDataBlock().maxDamage;
        %damageThreshold = %maxDamage * 0.8;
        %health = %maxDamage - %this.getDamageLevel();

        if (%health < %damageThreshold)
            %this.pushTask("findHealing");

        %ammo = %this.getDataBlock().mainWeapon.image.ammo;
        %currentAmmo = %this.getInventory(%ammo);
        %maxAmmo = %this.maxInventory(%ammo);
        %ammoThreshold = %maxAmmo * 0.3;

        if (%currentAmmo < %ammoThreshold)
            %this.pushTask("findAmmo");
    }
    %this.nextTask();
}

/// <summary>
/// This function searches first for health kits and then for a barracks.
/// </summary>
function AIPlayer::findHealing(%this)
{
    %obj = %this.findHealthKit();
    if (isObject(%obj))
        %dest = %obj.getPosition();
    if (!isObject(%obj))
    {
            %obj = %this.findBarracks();
        if (isObject(%obj))
            %dest = %obj.spawnPoint.getPosition();
    }
    if (isObject(%obj))
        %this.setMoveDestination(%dest);
}

/// <summary>
/// This function returns the first health kit to turn up in a container search.
/// May need to filter for distance....
/// </summary>
function AIPlayer::findHealthKit(%this)
{
    initContainerRadiusSearch(%this.getPosition(), 100.0, $TypeMasks::ItemObjectType);
    %obj = containerSearchNext();
    if (!isObject(%obj) || %obj $= "")
    {
        %this.nextTask();
        return 0;
    }
    while (isObject(%obj) && %obj.getDatablock().getName() !$= "HealthKitPatch")
    {
        %obj = containerSearchNext();
        if (!isObject(%obj) || %obj $= "")
            continue;
    }
    return %obj;
}

/// <summary>
/// This function searches first for ammo drops and then for a barracks to rearm.
/// </summary>
function AIPlayer::findAmmo(%this)
{
    %obj = %this.findAmmoDrop();
    if (isObject(%obj))
        %dest = %obj.getPosition();
    if (!isObject(%obj))
    {
            %obj = %this.findBarracks();
        if (isObject(%obj))
            %dest = %obj.spawnPoint.getPosition();
    }
    if (isObject(%obj))
        %this.setMoveDestination(%dest);
}

/// <summary>
/// This function returns the first ammo item to turn up in a container search.
/// May need to filter for distance....
/// </summary>
function AIPlayer::findAmmoDrop(%this)
{
    initContainerRadiusSearch(%this.getPosition(), 100.0, $TypeMasks::ItemObjectType);
    %weapon = %this.getDataBlock().mainWeapon.image;
    %ammoType = %weapon.ammo;
    %clipType = %weapon.clip;
    %obj = containerSearchNext();
    if (!isObject(%obj) || %obj $= "")
    {
        %this.nextTask();
        return 0;
    }
    %datablock = %obj.getDatablock().getName();
    while (isObject(%obj) && (%datablock !$= %ammoType && %datablock !$= %clipType))
    {
        %obj = containerSearchNext();
        if (!isObject(%obj) || %obj $= "")
            continue;
        %datablock = %obj.getDatablock().getName();
    }
    return %obj;
}

/// <summary>
/// This function returns the first barracks to turn up in a container search.
/// May need to filter for distance....
/// </summary>
function AIPlayer::findBarracks(%this)
{
    initContainerRadiusSearch(%this.getPosition(), 100.0, $TypeMasks::StaticObjectType);
    %obj = containerSearchNext();
    if (!isObject(%obj))
        %this.nextTask();
    while (isObject(%obj) && (%obj.class !$= "Barracks" || %obj.team != %this.team))
    {
        %obj = containerSearchNext();
        if (!isObject(%obj))
            continue;
    }
    return %obj;
}

/// <summary>
/// This function clears the unit's current target.
/// <summary>
function AIPlayer::clearTarget(%this)
{
    %this.setAimObject(0);
    %this.target = "";
    if (%this.trigger)
    {
        cancel(%this.trigger);
        %this.trigger = "";
    }
    %this.canFire = true;
    %this.receivedAttackResponse = false;
    %this.respondedTo = "";
    %this.schedule(32, "setImageTrigger", 0, 0);
    %this.nextTask();
}


/// <summary>
/// This function tells the unit to continue pulsing the trigger using the it's 
/// shooting delay as frequency.
/// <summary>
function AIPlayer::singleShot(%this)
{
    // The shooting delay is used to pulse the trigger
    %this.setImageTrigger(0, true);
    %this.schedule(64, setImageTrigger, 0, false);

    if (%this.target !$= "" && isObject(%this.target) && %this.target.getState() !$= "dead")
        %this.trigger = %this.schedule(%this.shootingDelay, singleShot);
}

/// <summary>
/// This function tells the unit to stand by for the specified number of seconds.
/// <summary>
/// <param name="time">The wait time in seconds.</param>
function AIPlayer::wait(%this, %time)
{
    if (%time $= "")
        %time = 1;
    %this.schedule(%time * 1000, "nextTask");
}

/// <summary>
/// This function tells the unit to begin or stop firing its weapon.
/// <summary>
/// <param name="bool">Start firing if true, stop firing and clear target if false.</param>
function AIPlayer::fire(%this, %bool)
{
    if (!isObject(%this.target))
        %bool = false;
    if (%this.target.team == %this.team)
    {
        %this.target = "";
        %this.nextTask();
    }
    %bool = %this.getLOS(%this.target);

    %this.canFire = (%this.trigger !$= "" ? !isEventPending(%this.trigger) : true);
    %datablock = %this.getDatablock();
    if (%bool)
    {
        if (%this.canFire)
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

/// <summary>
/// This function tells the unit to aim at the specified object
/// <summary>
/// <param name="object">The object to aim at.</param>
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

//-----------------------------------------------------------------------------
//  Goal system
//-----------------------------------------------------------------------------
// This system should handle more high-level goals such as get repairs or find
// ammo that contain task lists to accomplish the goal.

//-----------------------------------------------------------------------------

/// <summary>
/// This function handles the unit's thinking.  Simple minds and all that....
/// </summary>
function AIPlayer::think(%this)
{
    %datablock = %this.getDataBlock();
    %datablock.think(%this);
}
