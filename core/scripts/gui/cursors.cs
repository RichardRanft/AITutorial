//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

if($platform $= "macos")
{
   new GuiCursor(DefaultCursor)
   {
      hotSpot = "4 4";
      renderOffset = "0 0";
      bitmapName = "~/art/gui/images/macCursor";
   };
} 
else 
{
   new GuiCursor(DefaultCursor)
   {
      hotSpot = "1 1";
      renderOffset = "0 0";
      bitmapName = "~/art/gui/images/defaultCursor";
   };
}