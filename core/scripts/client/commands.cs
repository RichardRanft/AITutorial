//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// Sync the Camera and the EditorGui
function clientCmdSyncEditorGui()
{
   if (isObject(EditorGui))
      EditorGui.syncCameraGui();
}