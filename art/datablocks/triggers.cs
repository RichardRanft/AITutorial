//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// DefaultTrigger is used by the mission editor.  This is also an example
// of trigger methods and callbacks.

datablock TriggerData(DefaultTrigger)
{
   // The period is value is used to control how often the console
   // onTriggerTick callback is called while there are any objects
   // in the trigger.  The default value is 100 MS.
   tickPeriodMS = 100;
};

datablock TriggerData(ClientTrigger : DefaultTrigger)
{
   clientSide = true;
};