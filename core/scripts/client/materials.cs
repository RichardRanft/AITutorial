//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

singleton Material( WarningMaterial )
{
   diffuseMap[0] = "~/art/warnMat";
   emissive[0] = false;
   translucent = false;
};


singleton CubemapData( WarnMatCubeMap )
{
   cubeFace[0] = "~/art/warnMat";
   cubeFace[1] = "~/art/warnMat";
   cubeFace[2] = "~/art/warnMat";
   cubeFace[3] = "~/art/warnMat";
   cubeFace[4] = "~/art/warnMat";
   cubeFace[5] = "~/art/warnMat";
};
