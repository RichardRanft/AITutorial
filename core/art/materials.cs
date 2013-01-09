//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

singleton Material( DefaultDecalRoadMaterial )
{
   diffuseMap[0] = "core/art/defaultRoadTextureTop.png";
   mapTo = "unmapped_mat";
   materialTag0 = "RoadAndPath";
};

singleton Material( BlankWhite )
{
   diffuseMap[0] = "core/art/white";
   mapTo = "white";
   materialTag0 = "Miscellaneous";
};

singleton Material( Empty )
{
};

singleton Material(DefaultRoadMaterialTop)
{
   mapTo = "unmapped_mat";
   diffuseMap[0] = "core/art/defaultRoadTextureTop.png";
   materialTag0 = "RoadAndPath";
};

singleton Material(DefaultRoadMaterialOther)
{
   mapTo = "unmapped_mat";
   diffuseMap[0] = "core/art/defaultRoadTextureOther.png";
   materialTag0 = "RoadAndPath";
};

