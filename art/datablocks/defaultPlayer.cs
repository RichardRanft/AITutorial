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

function DefaultPlayerData::onReachDestination(%this,%obj)
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
   else
   	%obj.nextTask();
}

function DefaultPlayerData::onMoveStuck(%this,%obj)
{
   //echo( %obj @ " onMoveStuck" );
}

function DefaultPlayerData::onTargetExitLOS(%this,%obj)
{
}

function DefaultPlayerData::onTargetEnterLOS(%this,%obj)
{
}

function DefaultPlayerData::onEndOfPath(%this,%obj,%path)
{
   %obj.nextTask();
}

function DefaultPlayerData::onEndSequence(%this,%obj,%slot)
{
   echo("Sequence Done!");
   %obj.stopThread(%slot);
   %obj.nextTask();
}

function DefaultPlayerData::think(%this, %obj)
{
    if(%obj.getState() $= "dead")
        return;
	
	%damageLvl = %obj.getDamageLevel();
	if (%damageLvl > 0)
	{
		if (%damageLvl > %obj.damageLvl)
		{
			%obj.damageLvl = %damageLvl;
			%obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl TAB %obj.damageSourceObj);
		}
	}
    %canFire = (%obj.trigger !$= "" ? !isEventPending(%obj.trigger) : true);
    // bail - can't attack anything right now anyway, why bother with the search?
    if (!%canFire)
        return;

    if (!isObject(%obj.target))
    {
        %target = %obj.getNearestTarget(100.0);
        if (isObject(%target))
        {
            if (%obj.seeTarget(%obj.getPosition(), %target.getPosition(), %obj.getAngleTo(%target.getPosition(), 80.0)))
                %obj.target = %target;
        }
    }
    if (!isObject(%obj.target))
        %obj.target = %obj.findTargetInMissionGroup(25.0);

    if (isObject(%obj.target) && %obj.target.getState() !$= "dead")
    {
        if (%canFire)
            %obj.pushTask("attack" TAB %obj.target);

        return;
    }
    if (isObject(%obj.target) && %obj.target.getState() $= "dead" || !isObject(%obj.target))
    {
        %obj.pushTask("fire" TAB false);
        %obj.pushTask("evaluateCondition");
        return;
    }
    %obj.pushTask("clearTarget");
}

function DefaultPlayerData::fire(%this, %obj)
{
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead" || %obj.getState() $= "dead")
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
