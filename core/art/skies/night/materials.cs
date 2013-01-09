//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

singleton CubemapData( NightCubemap )
{
   cubeFace[0] = "./skybox_1";
   cubeFace[1] = "./skybox_2";
   cubeFace[2] = "./skybox_3";
   cubeFace[3] = "./skybox_4";
   cubeFace[4] = "./skybox_5";
   cubeFace[5] = "./skybox_6";
};

singleton Material( Moon_Glow_Mat )
{
   baseTex = "./moon_wglow.png";
   emissive = true;
   translucent = true;
   vertColor[ 0 ] = true;
};

singleton Material( Moon_Mat )
{
   baseTex = "./moon_noglow.png";
   emissive = true;
   translucent = true;
   vertColor[ 0 ] = true;
};
