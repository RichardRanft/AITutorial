//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------


$LightRayPostFX::brightScalar = 0.75;
$LightRayPostFX::numSamples = 40;
$LightRayPostFX::density = 0.94;
$LightRayPostFX::weight = 5.65;
$LightRayPostFX::decay = 1.0;
$LightRayPostFX::exposure = 0.0005;
$LightRayPostFX::resolutionScale = 1.0;


singleton ShaderData( LightRayOccludeShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/lightRay/lightRayOccludeP.hlsl";

   pixVersion = 3.0;   
};

singleton ShaderData( LightRayShader )
{
   DXVertexShaderFile 	= "shaders/common/postFx/postFxV.hlsl";
   DXPixelShaderFile 	= "shaders/common/postFx/lightRay/lightRayP.hlsl";

   pixVersion = 3.0;   
};

singleton GFXStateBlockData( LightRayStateBlock : PFX_DefaultStateBlock )
{
   samplersDefined = true;
   samplerStates[0] = SamplerClampLinear;
   samplerStates[1] = SamplerClampLinear;     
};

singleton PostEffect( LightRayPostFX )
{
   isEnabled = false;
   allowReflectPass = false;
        
   renderTime = "PFXAfterDiffuse";
   renderPriority = 0.1;
      
   shader = LightRayOccludeShader;
   stateBlock = LightRayStateBlock;
   texture[0] = "$backBuffer";
   texture[1] = "#prepass";
   target = "$outTex";
   targetFormat = "GFXFormatR16G16B16A16F";
      
   new PostEffect()
   {
      shader = LightRayShader;
      stateBlock = LightRayStateBlock;
      internalName = "final";
      texture[0] = "$inTex";
      texture[1] = "$backBuffer";
      target = "$backBuffer";
   };
};

function LightRayPostFX::preProcess( %this )
{   
   %this.targetScale = $LightRayPostFX::resolutionScale SPC $LightRayPostFX::resolutionScale;
}

function LightRayPostFX::setShaderConsts( %this )
{
   %this.setShaderConst( "$brightScalar", $LightRayPostFX::brightScalar );
   
   %pfx = %this-->final;
   %pfx.setShaderConst( "$numSamples", $LightRayPostFX::numSamples );
   %pfx.setShaderConst( "$density", $LightRayPostFX::density );
   %pfx.setShaderConst( "$weight", $LightRayPostFX::weight );
   %pfx.setShaderConst( "$decay", $LightRayPostFX::decay );
   %pfx.setShaderConst( "$exposure", $LightRayPostFX::exposure );
}
