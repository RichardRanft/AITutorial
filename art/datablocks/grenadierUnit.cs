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

datablock PlayerData(GrenadierUnitData : DefaultPlayerData)
{
    shapeFile = "~/shapes/actors/boombot/boombot.dts";
    shootingDelay = 3000;
    corpseFadeTime = 2000;
    mainWeapon = "LurkerGrenadeLauncher";
    moveTolerance = 1.0;
};

// ----------------------------------------------------------------------------
// These call AIPlayer methods that provide common versions.  For custom
// behavior just provide the callback script here instead.
// ----------------------------------------------------------------------------

function GrenadierUnitData::onReachDestination(%this,%obj)
{
    %obj.ReachDestination();
}

function GrenadierUnitData::onMoveStuck(%this,%obj)
{
   %obj.MoveStuck();
}

function GrenadierUnitData::onTargetExitLOS(%this,%obj)
{
    %obj.TargetExitLOS();
}

function GrenadierUnitData::onTargetEnterLOS(%this,%obj)
{
    %obj.TargetEnterLOS();
}

function GrenadierUnitData::onEndOfPath(%this,%obj,%path)
{
   %obj.EndOfPath(%path);
}

function GrenadierUnitData::onEndSequence(%this,%obj,%slot)
{
    %obj.EndSequence(%slot);
}

function GrenadierUnitData::fire(%this, %obj)
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

function GrenadierUnitData::think(%this, %obj)
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
        if (%obj.canFire)
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

/// <summary>
/// This function handles the unitUnderAttack event for GrenadierUnitData, overriding
/// the default AIPlayer version.
/// </summary>
function GrenadierUnitData::underAttack(%this, %obj, %msgData)
{
    %unit = getField(%msgData, 0);
    %unitPos = %unit.getPosition();
    %objPos = %obj.getPosition();
    %source = getField(%msgData, 3);
    if (%source.sourceObject.team == %obj.team)
        return;
    %dist = VectorDist(%unitPos, %objPos) - $AIEventManager::GrenadierAttackResponseDist;
    %distWeight = (1/(%dist > 0 ? %dist : 1));
    if (%distWeight > 0.1)
    {
        %obj.target = %source.sourceObject;
        // if we can't see the target, move closer to ally
        // otherwise just "evade" (ok, run around randomly).
        if (!%obj.getLOS(%obj.target))
            %dest = %unitPos;
        else
            %dest = %objPos;
        %offsetX = getRandom(-20, 20);
        %offsetY = getRandom(-20, 20);
        %dest.x += %offsetX;
        %dest.y += %offsetY;
        %obj.setMoveDestination(%dest);
        %unit.notifyAttackResponse();
        %obj.respondedTo = %unit;
    }
}
