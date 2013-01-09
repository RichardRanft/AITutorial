//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

singleton CubemapData( NewLevelSkyCubemap )
{
   cubeFace[0] = "./cubemap/skybox_1";
   cubeFace[1] = "./cubemap/skybox_2";
   cubeFace[2] = "./cubemap/skybox_3";
   cubeFace[3] = "./cubemap/skybox_4";
   cubeFace[4] = "./cubemap/skybox_5";
   cubeFace[5] = "./cubemap/skybox_6";
};

singleton Material( NewLevelSkyMat )
{
   cubemap = NewLevelSkyCubemap;
};
