//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// global vars
//------------------------------------------------------------------------------

$BUTTON_A   =  0;
$BUTTON_B   =  1;
$BUTTON_X   =  2;
$BUTTON_Y   =  3;

//------------------------------------------------------------------------------
// GamepadButtonsGui methods
//------------------------------------------------------------------------------

/// Callback when this control wakes up. All buttons are set to invisible and
/// disabled.
function GamepadButtonsGui::onWake(%this)
{
   %this.setButton($BUTTON_A);
   %this.setButton($BUTTON_B);
   %this.setButton($BUTTON_X);
   %this.setButton($BUTTON_Y);
}

/// Sets the command and text for the specified button. If %text and %command
/// are left empty, the button will be disabled and hidden.
/// Note: This command is not executed when the A button is pressed. That
/// command is executed directly from the GuiGameList___Ctrl. This command is
/// for the graphical hint and to allow a mouse equivalent.
///
/// \param %button (constant) The button to set. See: $BUTTON_A, _B, _X, _Y
/// \param %text (string) The text to display next to the A button graphic.
/// \param %command (string) The command executed when the A button is pressed.
function GamepadButtonsGui::setButton(%this, %button, %text, %command)
{
   switch (%button)
   {
      case $BUTTON_A :
         %labelCtrl = ButtonALabel;
         %buttonCtrl = ButtonAButton;
         %imgCtrl = ButtonAImg;

      case $BUTTON_B :
         %labelCtrl = ButtonBLabel;
         %buttonCtrl = ButtonBButton;
         %imgCtrl = ButtonBImg;

      case $BUTTON_X :
         %labelCtrl = ButtonXLabel;
         %buttonCtrl = ButtonXButton;
         %imgCtrl = ButtonXImg;

      case $BUTTON_Y :
         %labelCtrl = ButtonYLabel;
         %buttonCtrl = ButtonYButton;
         %imgCtrl = ButtonYImg;

      default:
         error("GamepadButtonsGui::setButton(" @ %button @ ", " @ %text @ ", " @ %command @ "). No valid button was specified. Please pass one of the $BUTTON_ globals for this parameter.");
         return "";
   }

   %set = (! ((%text $= "") && (%command $= "")));
   %labelCtrl.setText(%text);
   %labelCtrl.setActive(%set);
   %labelCtrl.setVisible(%set);

   %buttonCtrl.Command = %command;
   %buttonCtrl.setActive(%set);
   %buttonCtrl.setVisible(%set);

   %imgCtrl.setActive(%set);
   %imgCtrl.setVisible(%set);
}
