//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//------------------------------------------------------------------------------
// Gui Profiles for the unified shell
//------------------------------------------------------------------------------

singleton GuiGameListMenuProfile(DefaultListMenuProfile)
{
   fontType = "Arial Bold";
   fontSize = 20;
   fontColor = "120 120 120";
   fontColorSEL = "16 16 16";
   fontColorNA = "200 200 200";
   fontColorHL = "100 100 120";
   HitAreaUpperLeft = "16 20";
   HitAreaLowerRight = "503 74";
   IconOffset = "40 0";
   TextOffset = "100 0";
   RowSize = "525 93";
   bitmap = "./images/listMenuArray";
   canKeyFocus = true;
};

singleton GuiGameListOptionsProfile(DefaultOptionsMenuProfile)
{
   fontType = "Arial Bold";
   fontSize = 20;
   fontColor = "120 120 120";
   fontColorSEL = "16 16 16";
   fontColorNA = "200 200 200";
   fontColorHL = "100 100 120";
   HitAreaUpperLeft = "16 20";
   HitAreaLowerRight = "503 74";
   IconOffset = "40 0";
   TextOffset = "90 0";
   RowSize = "525 93";
   ColumnSplit = "220";
   RightPad = "20";
   bitmap = "./images/listMenuArray";
   canKeyFocus = true;
};

singleton GuiControlProfile(GamepadDefaultProfile)
{
   border = 0;
};

singleton GuiControlProfile(GamepadButtonTextLeft)
{
   fontType = "Arial Bold";
   fontSize = 20;
   fontColor = "40 40 40";
   justify = "left";
};

singleton GuiControlProfile(GamepadButtonTextRight : GamepadButtonTextLeft)
{
   justify = "right";
};
