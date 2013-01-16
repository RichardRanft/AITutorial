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

// ----------------------------------------------------------------------------
// These call AIPlayer methods that provide common versions.  For custom
// behavior just provide the callback script here instead.
// ----------------------------------------------------------------------------

function DefaultPlayerData::onReachDestination(%this,%obj)
{
    %obj.ReachDestination();
}

function DefaultPlayerData::onMoveStuck(%this,%obj)
{
   %obj.MoveStuck();
}

function DefaultPlayerData::onTargetExitLOS(%this,%obj)
{
    %obj.TargetExitLOS();
}

function DefaultPlayerData::onTargetEnterLOS(%this,%obj)
{
    %obj.TargetEnterLOS();
}

function DefaultPlayerData::onEndOfPath(%this,%obj,%path)
{
   %obj.EndOfPath(%path);
}

function DefaultPlayerData::onEndSequence(%this,%obj,%slot)
{
    %obj.EndSequence(%slot);
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
			%obj.target = %obj.damageSourceObj.sourceObject;
			if (!%obj.receivedAttackResponse)
			    AIEventManager.postEvent("_UnitUnderAttack", %obj TAB "underAttack" TAB %damageLvl TAB %obj.damageSourceObj);
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
        if (!%obj.getLOS(%obj.target))
        {
            %obj.pushTask("fire" TAB %obj TAB false);
            %obj.setMoveDestination(%obj.intersectPos);
        }
        else if (%obj.canFire)
        {
            %obj.setMoveDestination(%obj.getPosition());
            %obj.pushTask("attack" TAB %obj.target);
        }

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
