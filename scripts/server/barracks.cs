function BarracksTrigger::onEnterTrigger(%this,%trigger,%obj)
{
    // Apply health to colliding object if it needs it.
    // Works for all shapebase objects.
    %damageLevel = %obj.getDamageLevel();
    %maxDamage = %obj.getDatablock().maxDamage;
    %state = %obj.getState();
    echo(" @@@ " @ %obj @ " : " @ %maxDamage @"/"@ %damageLevel@" : " @ %state);
    AIManager.loadOutUnit(%obj);
    if (%damageLevel != 0 && %state !$= "Dead" && %obj.team == %trigger.owner.team)
    {
        %obj.applyRepair(%maxDamage - %damageLevel);

        serverPlay3D(HealthUseSound, %trigger.owner.getTransform());
    }
    %obj.setMoveDestination(VectorAdd(%trigger.position, "0 15 0"));

    %obj.nextTask();
}
