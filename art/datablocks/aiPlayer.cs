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
