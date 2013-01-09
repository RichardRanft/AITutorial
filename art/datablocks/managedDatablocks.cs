//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// This is the default save location for any Datablocks created in the
// Datablock Editor (this script is executed from onServerCreated())

datablock TriggerData(CameraRoomTrigger : DefaultTrigger)
{
};

datablock ItemData(NavMarkerData : HealthKitPatch)
{
   elasticity = "0.298143";
   shapeFile = "art/shapes/station/pylon.dae";
   cameraMaxDist = "1";
   lightType = "PulsingLight";
   lightColor = "0 1 0 1";
   category = "Marker";
   class = "NavMarker";
   pickupName = "A Navigation Marker";
   shadowEnable = "0";
};
