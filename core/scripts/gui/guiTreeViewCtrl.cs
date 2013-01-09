//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

function GuiTreeViewCtrl::onDefineIcons( %this )
{
   %icons = "core/art/gui/images/treeview/default:" @
            "core/art/gui/images/treeview/simgroup:" @
            "core/art/gui/images/treeview/simgroup_closed:" @
            "core/art/gui/images/treeview/simgroup_selected:" @
            "core/art/gui/images/treeview/simgroup_selected_closed:" @      
            "core/art/gui/images/treeview/hidden:" @      
            "core/art/gui/images/treeview/shll_icon_passworded_hi:" @
            "core/art/gui/images/treeview/shll_icon_passworded:" @      
            "core/art/gui/images/treeview/default";
              
   %this.buildIconTable(%icons);   
}

function GuiTreeViewCtrl::handleRenameObject( %this, %name, %obj )
{
   %inspector = GuiInspector::findByObject( %obj );
   
   if( isObject( %inspector ) )   
   {
      %field = ( %this.renameInternal ) ? "internalName" : "name";      
      %inspector.setObjectField( %field, %name );
      return true;
   }
   
   return false;   
}
