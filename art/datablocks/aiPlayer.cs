//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Demo Pathed AIPlayer.
//-----------------------------------------------------------------------------

datablock PlayerData(DemoPlayerData : DefaultPlayerData)
{
   shapeFile = "~/shapes/actors/boombot/boombot.dts";
   shootingDelay = 1000;
   mainWeapon = "Ryder";
};

datablock PlayerData(AssaultUnitData : DefaultPlayerData)
{
   shapeFile = "~/shapes/actors/boombot/boombot.dts";
   shootingDelay = 500;
   mainWeapon = "Lurker";
};

datablock PlayerData(GrenadierUnitData : DefaultPlayerData)
{
   shapeFile = "~/shapes/actors/boombot/boombot.dts";
   shootingDelay = 3000;
   mainWeapon = "LurkerGrenadeLauncher";
};

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
			%obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl);
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
    if (isObject(%obj.target) && %obj.target.getState() $= "dead")
    {
        %obj.pushTask("fire" TAB false);
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
			%obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl);
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
    if (isObject(%obj.target) && %obj.target.getState() $= "dead")
    {
        %obj.pushTask("fire" TAB false);
        return;
    }
    %obj.pushTask("clearTarget");
}

function AssaultUnitData::fire(%this, %obj)
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
			%obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl);
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
    if (isObject(%obj.target) && %obj.target.getState() $= "dead")
    {
        %obj.pushTask("fire" TAB false);
        return;
    }
    %obj.pushTask("clearTarget");
}

function GrenadierUnitData::fire(%this, %obj)
{
    %validTarget = isObject(%obj.target);
    if (!%validTarget || %obj.target.getState() $= "dead" || %obj.getState() $= "dead")
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
			%obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl);
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
    if (isObject(%obj.target) && %obj.target.getState() $= "dead")
    {
        %obj.pushTask("fire" TAB false);
        return;
    }
    %obj.pushTask("clearTarget");
}
