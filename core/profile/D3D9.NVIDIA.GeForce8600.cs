//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// nVidia Vendor Profile Script
//
// This script is responsible for setting global
// capability strings based on the nVidia vendor.

if(GFXCardProfiler::getVersion() < 1.2)
{
   $GFX::OutdatedDrivers = true;
   $GFX::OutdatedDriversLink = "<a:www.nvidia.com>You can get newer drivers here.</a>.";
}
else
{
   $GFX::OutdatedDrivers = false;
}
