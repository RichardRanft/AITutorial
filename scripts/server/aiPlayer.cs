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

//-----------------------------------------------------------------------------
// AIPlayer static functions
//-----------------------------------------------------------------------------

function AIPlayer::spawn(%name, %spawnPoint, %datablock)
{
    // Create the demo player object
    %player = new AiPlayer()
    {
        dataBlock = (%datablock !$= "" ? %datablock : DemoPlayer);
    };
    MissionCleanup.add(%player);
    %player.setShapeName(%name);
    if (isObject(%spawnPoint))
        %player.setPosition(%spawnPoint.getPosition());
    else
        %player.setTransform(%spawnPoint);
    return %player;
}

function AIPlayer::spawnOnPath(%name, %path, %datablock)
{
   // Spawn a player and place him on the first node of the path
   if (!isObject(%path))
      return 0;
   %node = %path.getObject(0);
   %player = AIPlayer::spawn(%name, %node.getTransform());
   return %player;
}

//-----------------------------------------------------------------------------
// AIPlayer methods
//-----------------------------------------------------------------------------

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
   if (%this.taskIndex $= "")
   {
      %this.taskIndex = 0;
      %this.taskCurrent = -1;
   }
   %this.task[%this.taskIndex] = %method;
   %this.taskIndex++;
   if (%this.taskCurrent == -1)
      %this.executeTask(%this.taskIndex - 1);
}

function AIPlayer::clearTasks(%this)
{
   %this.taskIndex = 0;
   %this.taskCurrent = -1;
}

function AIPlayer::nextTask(%this)
{
   if (%this.taskCurrent != -1)
      if (%this.taskCurrent < %this.taskIndex - 1)
         %this.executeTask(%this.taskCurrent++);
      else
         %this.taskCurrent = -1;
}

function AIPlayer::executeTask(%this,%index)
{
   %this.taskCurrent = %index;
   eval(%this.getId() @"."@ %this.task[%index] @";");
}

//-----------------------------------------------------------------------------

function AIPlayer::singleShot(%this)
{
   // The shooting delay is used to pulse the trigger
   %this.setImageTrigger(0, true);
   %this.setImageTrigger(0, false);
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

function AIPlayer::fire(%this,%bool)
{
   if (%bool)
   {
      cancel(%this.trigger);
      %this.singleShot();
   }
   else
      cancel(%this.trigger);
   %this.nextTask();
}

function AIPlayer::aimAt(%this,%object)
{
   echo("Aim: "@ %object);
   %this.setAimObject(%object);
   %this.nextTask();
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
   echo("\c4AIPlayer::getTargetDistance("@ %this @", "@ %target @")");
   $tgt = %target;
   %tgtPos = %target.getPosition();
   %eyePoint = %this.getWorldBoxCenter();
   %distance = VectorDist(%tgtPos, %eyePoint);
   echo("Distance to target = "@ %distance);
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
    %this.target = %this.getNearestTarget(100.0);
    if (!isObject(%this.target))
        %this.target = %this.findTargetInMissionGroup(100.0);
    if (isObject(%this.target))
        echo(" @@@ Unit " @ %this @ " nearest enemy is : " @ %this.target @ " : Range = " @ VectorDist(%this.target.getPosition(), %this.getPosition()));
}

//-----------------------------------------------------------------------------
new ScriptObject(AIManager);

function AIManager::start(%this, %priorityTime, %idleTime, %priorityRadius)
{
    %this.priorityGroup = new SimSet();
    %this.idleGroup = new SimSet();
    %this.think();
    %this.priorityRadius = %priorityRadius;
    %this.priorityTime = %priorityTime;
    %this.idleTime = %idleTime;
    %this.priorityThink();
    %this.idleThink();
    %this.started = true;
}

function AIManager::addUnit(%this, %name, %spawnLocation, %datablock, %onPath)
{
    %newUnit = %this.spawn(%name, %spawnLocation, %datablock, %onPath);
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
            %range = VectorDist( %clientCamLoc, %unit.getPosition() );
            if (%this.priorityRadius < %range)
            {
                if (%unit.priority > 0)
                {
                    %this.priorityGroup.remove(%unit);
                    %this.idleGroup.add(%unit);
                    %hCount--;
                }
                echo(" @@@ Moved " @ %unit @ " to idle group : " @ %range);
            }
            %index++;
        }
        %hCount = %this.idleGroup.getCount();
        %index = 0;
        while (%index < %hCount)
        {
            %unit = %this.idleGroup.getObject(%index);
            %range = VectorDist( %clientCamLoc, %unit.getPosition() );
            if (%this.priorityRadius > %range)
            {
                if (%unit.priority < 1)
                {
                    %this.idleGroup.remove(%unit);
                    %this.priorityGroup.add(%unit);
                    %hCount--;
                }
                echo(" @@@ Moved " @ %unit @ " to priority group : " @ %range);
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

function AIManager::spawn(%this, %name, %spawnLocation, %datablock, %onPath)
{
    if (%onPath)
    {
        %path = (%spawnLocation !$= "" ? %spawnLocation : "MissionGroup/Paths/Path1");
        %player = AIPlayer::spawnOnPath(%name, %path, %datablock);

        if (isObject(%player))
        {
            %player.followPath(%path, -1);

            // slow this sucker down, I'm tired of chasing him!
            %player.setMoveSpeed(0.5);

            return %player;
        }
        else
            return 0;
    }
    else
    {
        if (%spawnLocation $= "")
        {
            %count = PlayerDropPoints.getCount();
            %index = getRandom(0, %count - 1);
            %location = PlayerDropPoints.getObject(%index);
        }
        %player = AIPlayer::spawn(%name, %location, %datablock);

        if (isObject(%player))
        {
            // slow this sucker down, I'm tired of chasing him!
            %player.setMoveSpeed(0.5);

            return %player;
        }
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