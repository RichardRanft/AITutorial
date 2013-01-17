//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// Some AI globals
$AIPlayer::GrenadierRange = 40.0;
$AIPlayer::GrenadeGravityModifier = 0.86;
$AIPlayer::DefaultPriority = 1;
$AIEventManager::DefaultAttackResponseDist = 30;
$AIEventManager::GrenadierAttackResponseDist = 60;

// These contain the datablocks and/or datablock-scoped methods for the AI 
// units.  Some are overrides from the AIPlayer scope and some call back to 
// the AIPlayer scope for default behavior.
exec("./demoPlayer.cs");
exec("./defaultPlayer.cs");
exec("./assaultUnit.cs");
exec("./grenadierUnit.cs");
