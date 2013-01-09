//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// Load up all datablocks.  This function is called when
// a server is constructed.

// Do the sounds first -- later scripts/datablocks may need them
exec("./audioProfiles.cs");

// Do the various effects next -- later scripts/datablocks may need them
exec("./particles.cs");

exec("./environment.cs");
exec("./triggers.cs");

// Add a rigid example
exec("./rigidShape.cs");

exec("./health.cs");

// Load our supporting weapon datablocks
exec("./weapon.cs");

// Load the weapon datablocks
exec("./weapons/grenadeLauncher.cs");
exec("./weapons/grenade.cs");
exec("./weapons/rocketLauncher.cs");
exec("./weapons/Lurker.cs");
exec("./weapons/Ryder.cs");
exec("./weapons/ProxMine.cs");
exec("./weapons/Turret.cs");

exec("./teleporter.cs");

// Load the default player datablocks
exec("./player.cs");

// Load our other player datablocks
exec("./aiPlayer.cs");

// Load the vehicle datablocks
exec("./vehicles/defaultCar.cs");
exec("./vehicles/cheetahCar.cs");