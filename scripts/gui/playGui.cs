//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// PlayGui is the main TSControl through which the game is viewed.
// The PlayGui also contains the hud controls.
//-----------------------------------------------------------------------------

function PlayGui::onWake(%this)
{
   // Turn off any shell sounds...
   // sfxStop( ... );

   $enableDirectInput = "1";
   activateDirectInput();

   // Message hud dialog
   if ( isObject( MainChatHud ) )
   {
      Canvas.pushDialog( MainChatHud );
      chatHud.attach(HudMessageVector);
   }      
   
   // just update the action map here
   moveMap.push();

   // hack city - these controls are floating around and need to be clamped
   if ( isFunction( "refreshCenterTextCtrl" ) )
      schedule(0, 0, "refreshCenterTextCtrl");
   if ( isFunction( "refreshBottomTextCtrl" ) )
      schedule(0, 0, "refreshBottomTextCtrl");
}

function PlayGui::onSleep(%this)
{
   if ( isObject( MainChatHud ) )
      Canvas.popDialog( MainChatHud );
   
   // pop the keymaps
   moveMap.pop();
}

function PlayGui::clearHud( %this )
{
   Canvas.popDialog( MainChatHud );

   while ( %this.getCount() > 0 )
      %this.getObject( 0 ).delete();
}

//-----------------------------------------------------------------------------

function refreshBottomTextCtrl()
{
   BottomPrintText.position = "0 0";
}

function refreshCenterTextCtrl()
{
   CenterPrintText.position = "0 0";
}

// onRightMouseDown is called when the right mouse
// button is clicked in the scene
// %pos is the screen (pixel) coordinates of the mouse click
// %start is the world coordinates of the camera
// %ray is a vector through the viewing 
// frustum corresponding to the clicked pixel
function PlayGui::onRightMouseDown(%this, %pos, %start, %ray)
{   
    commandToServer('movePlayer', %pos, %start, %ray);
    
    %ray = VectorScale(%ray, 1000);
    %end = VectorAdd(%start, %ray);

    // only care about terrain objects
    %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
    $TypeMasks::InteriorObjectType | $TypeMasks::ShapeBaseObjectType | $TypeMasks::StaticObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    if (%scanTarg)
    {
        %obj = getWord(%scanTarg, 0);

        while (%obj.class $= "barrier")
        {
            // Get the X,Y,Z position of where we clicked
            %pos = getWords(%scanTarg, 1, 3);
            %restart = VectorNormalize(VectorSub(%end, %pos));
            %pos = VectorAdd(%pos, %restart);
            %scanTarg = ContainerRayCast( %pos, %end, %searchMasks);
            %obj = getWord(%scanTarg, 0);
        }

        // Get the X,Y,Z position of where we clicked
        %pos = getWords(%scanTarg, 1, 3);

        // Get the normal of the location we clicked on
        %norm = getWords(%scanTarg, 4, 6);

        // Create a new decal using the decal manager
        // arguments are (Position, Normal, Rotation, Scale, Datablock, Permanent)
        // We are now just letting the decals clean up after themselves.
        decalManagerAddDecal(%pos, %norm, 0, 1, "gg_decal", false);
    }
}

// onMouseDown is called when the left mouse
// button is clicked in the scene
// %pos is the screen (pixel) coordinates of the mouse click
// %start is the world coordinates of the camera
// %ray is a vector through the viewing 
// frustum corresponding to the clicked pixel
function PlayGui::onMouseDown(%this, %pos, %start, %ray)
{
    // If we're in building placement mode ask the server to create a building for
    // us at the point that we clicked.
    if (%this.placingBuilding)
    {
        // Clear the building placement flag first.
        %this.placingBuilding = false;
        // Request a building at the clicked coordinates from the server.
        commandToServer('createBuilding', %pos, %start, %ray, %this.buildingType);
    }
    else
    {
        // Ask the server to let us attack a target at the clicked position
        // or spawn a team mate if the clicked object is a barracks.
        commandToServer('checkTarget', %pos, %start, %ray);
    }
}

// This function is the callback that handles our new button.  When you click it
// the button tells the PlayGui that we're now in building placement mode.
function orcBurrowButton::onClick(%this)
{
    PlayGui.placingBuilding = true;
    PlayGui.buildingType = %this.buildingType;
}

function orcBurrowButton2::onClick(%this)
{
    PlayGui.placingBuilding = true;
    PlayGui.buildingType = %this.buildingType;
}

function orcBurrowButton3::onClick(%this)
{
    PlayGui.placingBuilding = true;
    PlayGui.buildingType = %this.buildingType;
}