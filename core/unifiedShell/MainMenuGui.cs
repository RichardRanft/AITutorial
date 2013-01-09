//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// ListMenu methods
//------------------------------------------------------------------------------

/// Callback when this gui is added to the sim.
function ListMenu::onAdd(%this)
{
   %this.addRow("Play Game", "onSinglePlayer", 0);

   // Defunct for now
   //%this.addRow("Select Model", "onSelectModel", 4, -15);

   // No need for video options on Xbox 360
   if ( $platform $= "xenon" )
      %this.addRow("Exit Game", "onQuit", 4, -15);
   else
   {
      %this.addRow("Setup", "onOptions", 4, -15);
      %this.addRow("Exit Game", "onQuit", 6, -15);
   }
}

//------------------------------------------------------------------------------
// MainMenuButtonHolder methods
//------------------------------------------------------------------------------

function MainMenuButtonHolder::onWake(%this)
{
   %this.add(GamepadButtonsGui);

   GamepadButtonsGui.setButton($BUTTON_A, "Go", ListMenu.CallbackOnA);
}

//------------------------------------------------------------------------------
// global methods
//------------------------------------------------------------------------------

/// Callback from the shell button for triggering single player.
function onSinglePlayer()
{
   echo("Default implementation. Override onSinglePlayer() to add game specific functionality");

   if ( isObject( LoadingGui ) )
   {
      Canvas.setContent("LoadingGui");
      LoadingProgress.setValue(1);
      LoadingProgressTxt.setValue("LOADING MISSION FILE");
      Canvas.repaint();
   }
   
   // Grab the specified default level
   %mission = $DefaultLevelFile;

   // If the default level isn't loaded attempt a fallback
   if ( %mission $= "" )
      %mission = "levels/default.mis";

   %serverType = $AutoLoadLevelMode;

   if ( %serverType $= "" )
      %serverType = "SinglePlayer";

   createAndConnectToLocalServer( %serverType, %mission );
}

/// Callback from the shell button to bring up the object picker.
function onSelectModel()
{
   Canvas.setContent(ObjectPickerGui);
}

/// Callback from the shell button to bring up the options gui.
function onOptions()
{
   Canvas.setContent(OptionsGui);
}

/// Callback from the shell "quit" button.
function onQuit()
{
   echo("Default implementation. Override onQuit() to add game specific functionality");
   quit();
}
