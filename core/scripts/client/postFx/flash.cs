//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

singleton ShaderData( PFX_FlashShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/flashP.hlsl";

   defines = "WHITE_COLOR=float4(1.0,1.0,1.0,0.0);MUL_COLOR=float4(1.0,0.25,0.25,0.0)";

   pixVersion = 2.0;
};
 
singleton PostEffect( FlashFx )
{
   isEnabled = false;    
   allowReflectPass = false;  

   renderTime = "PFXAfterDiffuse";  

   shader = PFX_FlashShader;   
   texture[0] = "$backBuffer";  
   renderPriority = 10;
   stateBlock = PFX_DefaultStateBlock;  
};

function FlashFx::setShaderConsts( %this )
{
   if ( isObject( ServerConnection ) )
   {
      %this.setShaderConst( "$damageFlash", ServerConnection.getDamageFlash() );
      %this.setShaderConst( "$whiteOut", ServerConnection.getWhiteOut() );
   }
   else
   {
      %this.setShaderConst( "$damageFlash", 0 );
      %this.setShaderConst( "$whiteOut", 0 );
   }
}
