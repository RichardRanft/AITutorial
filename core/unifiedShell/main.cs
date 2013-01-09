//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

function loadShell()
{
   exec("./profiles.cs");

   exec("./MainMenuGui.cs");
   exec("./MainMenuGui.gui");

   exec("./OptionsGui.cs");
   exec("./OptionsGui.gui");

   exec("./GamepadButtonsGui.cs");
   exec("./GamepadButtonsGui.gui");

   exec("./ObjectPickerGui.cs");
   exec("./ObjectPickerGui.gui");
}

loadShell();
