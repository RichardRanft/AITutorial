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

datablock PlayerData(DemoPlayerData : DefaultPlayerData)
{
    shapeFile = "~/shapes/actors/boombot/boombot.dts";
    shootingDelay = 1000;
    corpseFadeTime = 2000;
    mainWeapon = "Ryder";
};

// ----------------------------------------------------------------------------
// These call AIPlayer methods that provide common versions.  For custom
// behavior just provide the callback script here instead.
// ----------------------------------------------------------------------------

function DemoPlayer::onReachDestination(%this,%obj)
{
    %obj.ReachDestination();
}

function DemoPlayer::onMoveStuck(%this,%obj)
{
    %obj.MoveStuch();
}

function DemoPlayer::onTargetExitLOS(%this,%obj)
{
    %obj.TargetExitLOS();
}

function DemoPlayer::onTargetEnterLOS(%this,%obj)
{
    %obj.TargetEnterLOS();
}

function DemoPlayer::onEndOfPath(%this,%obj,%path)
{
    %obj.EndOfPath(%path);
}

function DemoPlayer::onEndSequence(%this,%obj,%slot)
{
    %obj.EndSequence(%slot);
}

function DemoPlayerData::fire(%this, %obj)
{
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead" || %obj.getState() $= "dead")
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

function DemoPlayerData::think(%this, %obj)
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
