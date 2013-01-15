function BarracksTrigger::onEnterTrigger(%this,%trigger,%obj)
{
    echo(" @@@ " @ %obj @ " Entered trigger " @ %trigger.getName());
    // Apply health to colliding object if it needs it.
    // Works for all shapebase objects.
    if (%obj.getDamageLevel() != 0 && %obj.getState() !$= "Dead" && %obj.team == %this.owner.team)
    {
        %obj.applyRepair(%obj.getDatablock().maxDamage);

        // Update the Health GUI while repairing
        %this.doHealthUpdate(%obj);
        AIManager.loadOutUnit(%obj);
        %obj.setMoveDestination(VectorAdd(%this.position, "0 5 0"));

        serverPlay3D(HealthUseSound, %this.owner.getTransform());
        %obj.nextTask();
    }
}
