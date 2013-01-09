//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Server Admin Commands
//-----------------------------------------------------------------------------

function SAD(%password)
{
   if (%password !$= "")
      commandToServer('SAD', %password);
}

function SADSetPassword(%password)
{
   commandToServer('SADSetPassword', %password);
}

//----------------------------------------------------------------------------
// Misc server commands
//----------------------------------------------------------------------------

function clientCmdSyncClock(%time)
{
   // Time update from the server, this is only sent at the start of a mission
   // or when a client joins a game in progress.
}

//-----------------------------------------------------------------------------
// Numerical Health Counter
//-----------------------------------------------------------------------------

function clientCmdSetNumericalHealthHUD(%curHealth)
{
   // Skip if the hud is missing.
   if (!isObject(numericalHealthHUD))
      return;

   // The server has sent us our current health, display it on the HUD
   numericalHealthHUD.setValue(%curHealth);

   // Ensure the HUD is set to visible while we have health / are alive
   if (%curHealth)
      HealthHUD.setVisible(true);
   else
      HealthHUD.setVisible(false);
}

//-----------------------------------------------------------------------------
// Damage Direction Indicator
//-----------------------------------------------------------------------------

function clientCmdSetDamageDirection(%direction)
{
   eval("%ctrl = DamageHUD-->damage_" @ %direction @ ";");
   if (isObject(%ctrl))
   {
      // Show the indicator, and schedule an event to hide it again
      cancelAll(%ctrl);
      %ctrl.setVisible(true);
      %ctrl.schedule(500, setVisible, false);
   }
}

//-----------------------------------------------------------------------------
// Teleporter visual effect
//-----------------------------------------------------------------------------

function clientCmdPlayTeleportEffect(%position, %effectDataBlock)
{
   if (isObject(%effectDataBlock))
   {
      new Explosion()
      {
         position = %position;
         dataBlock = %effectDataBlock;
      };
   }
}

// ----------------------------------------------------------------------------
// WeaponHUD
// ----------------------------------------------------------------------------

// Update the Ammo Counter with current ammo, if not any then hide the counter.
function clientCmdSetAmmoAmountHud(%amount, %amountInClips)
{
   if (!%amount)
      AmmoAmount.setVisible(false);
   else
   {
      AmmoAmount.setVisible(true);
      AmmoAmount.setText("Ammo: " @ %amount @ "/" @ %amountInClips);
   }
}

// Here we update the Weapon Preview image & reticle for each weapon.  We also
// update the Ammo Counter (just so we don't have to call it separately).
// Passing an empty parameter ("") hides the HUD component.

function clientCmdRefreshWeaponHUD(%amount, %preview, %ret, %zoomRet, %amountInClips)
{
   if (!%amount)
      AmmoAmount.setVisible(false);
   else
   {
      AmmoAmount.setVisible(true);
      AmmoAmount.setText("Ammo: " @ %amount @ "/" @ %amountInClips);
   }

   if (%preview $= "")
      WeaponHUD.setVisible(false);//PreviewImage.setVisible(false);
   else
   {
      WeaponHUD.setVisible(true);//PreviewImage.setVisible(true);
      PreviewImage.setbitmap("art/gui/weaponHud/"@ detag(%preview));
   }

   if (%ret $= "")
      Reticle.setVisible(false);
   else
   {
      Reticle.setVisible(true);
      Reticle.setbitmap("art/gui/weaponHud/"@ detag(%ret));
   }

   if (isObject(ZoomReticle))
   {
      if (%zoomRet $= "")
      {
         ZoomReticle.setBitmap("");
      }
      else
      {
         ZoomReticle.setBitmap("art/gui/weaponHud/"@ detag(%zoomRet));
      }
   }
}

// ----------------------------------------------------------------------------
// Vehicle Support
// ----------------------------------------------------------------------------

function clientCmdtoggleVehicleMap(%toggle)
{
   if(%toggle)
   {
      moveMap.pop();
      vehicleMap.push();
   }
   else
   {
      vehicleMap.pop();
      moveMap.push();
   }
}

// ----------------------------------------------------------------------------
// Turret Support
// ----------------------------------------------------------------------------

// Call by the Turret class when a player mounts or unmounts it.
// %turret = The turret that was mounted
// %player = The player doing the mounting
// %mounted = True if the turret was mounted, false if it was unmounted
function turretMountCallback(%turret, %player, %mounted)
{
   //echo ( "\c4turretMountCallback -> " @ %mounted );

   if (%mounted)
   {
      // Push the action map
      turretMap.push();
   }
   else
   {
      // Pop the action map
      turretMap.pop();
   }
}

// ----------------------------------------------------------------------------
// Player commands
// ----------------------------------------------------------------------------

function clientCmdsetDecal(%ai, %pos, %norm)
{
   // If the AI player already has a decal (0 or greater)
   // tell the decal manager to delete the instance of the gg_decal
   if( %ai.decal > -1 )
      decalManagerRemoveDecal( %ai.decal );

   // Create a new decal using the decal manager
   // arguments are (Position, Normal, Rotation, Scale, Datablock, Permanent)
   // AddDecal will return an ID of the new decal, which we will
   // store in the player
   %ai.decal = decalManagerAddDecal( %pos, %norm, 0, 1, gg_decal, true );
}

function clientCmddisplayPlacementDecal()
{
    echo(" *** Displaying building placement marker on client");
   // If the AI player already has a decal (0 or greater)
   // tell the decal manager to delete the instance of the gg_decal
   if( PlayGui.decal > -1 )
      decalManagerRemoveDecal( PlayGui.decal );

   // Create a new decal using the decal manager
   // arguments are (Position, Normal, Rotation, Scale, Datablock, Permanent)
   // AddDecal will return an ID of the new decal, which we will
   // store in the player
    PlayGui.placingBuilding = true;
}