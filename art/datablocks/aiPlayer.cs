//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

$AIPlayer::GrenadierRange = 40.0;
$AIPlayer::GrenadeGravityModifier = 0.86;
$AIPlayer::DefaultPriority = 1;
$AIEventManager::DefaultAttackResponseDist = 30;

exec("./demoPlayer.cs");
exec("./defaultPlayer.cs");
exec("./assaultUnit.cs");
exec("./grenadierUnit.cs");
