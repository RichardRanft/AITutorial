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

datablock PlayerData(AssaultUnitData : DefaultPlayerData)
{
    shapeFile = "~/shapes/actors/boombot/boombot.dts";
    shootingDelay = 500;
    corpseFadeTime = 2000;
    mainWeapon = "Lurker";
    moveTolerance = 1.0;
};

// ----------------------------------------------------------------------------
// These call AIPlayer methods that provide common versions.  For custom
// behavior just provide the callback script here instead.
// ----------------------------------------------------------------------------

function AssaultUnitData::onReachDestination(%this,%obj)
{
    %obj.ReachDestination();
}

function AssaultUnitData::onMoveStuck(%this,%obj)
{
    %obj.MoveStuck();
}

function AssaultUnitData::onTargetExitLOS(%this,%obj)
{
    %obj.TargetExitLOS();
}

function AssaultUnitData::onTargetEnterLOS(%this,%obj)
{
    %obj.TargetEnterLOS();
}

function AssaultUnitData::onEndOfPath(%this,%obj,%path)
{
   %obj.EndOfPath(%path);
}

function AssaultUnitData::onEndSequence(%this,%obj,%slot)
{
    %obj.EndSequence(%slot);
}

function AssaultUnitData::fire(%this, %obj)
{
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead" || %obj.getState() $= "dead")
    {
        if (%obj.trigger)
            cancel(%obj.trigger);
        %obj.trigger = "";
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

function AssaultUnitData::think(%this, %obj)
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
    if (!%obj.canFire)
        return;

    if (!isObject(%target))
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
            %obj.stop();
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

