//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
//  This file contains shader data necessary for various engine utility functions
//-----------------------------------------------------------------------------


singleton ShaderData( _DebugInterior_ )
{
   DXVertexShaderFile   = "shaders/common/debugInteriorsV.hlsl";
   DXPixelShaderFile    = "shaders/common/debugInteriorsP.hlsl";
   
   OGLVertexShaderFile   = "shaders/common/gl/debugInteriorsV.glsl";
   OGLPixelShaderFile    = "shaders/common/gl/debugInteriorsP.glsl";
   
   samplerNames[0] = "$diffuseMap";
   
   pixVersion = 1.1;
};

singleton ShaderData( ParticlesShaderData )
{
   DXVertexShaderFile     = "shaders/common/particlesV.hlsl";
   DXPixelShaderFile      = "shaders/common/particlesP.hlsl";   
   
   OGLVertexShaderFile     = "shaders/common/gl/particlesV.glsl";
   OGLPixelShaderFile      = "shaders/common/gl/particlesP.glsl";
   
   pixVersion = 2.0;
};

singleton ShaderData( OffscreenParticleCompositeShaderData )
{
   DXVertexShaderFile     = "shaders/common/particleCompositeV.hlsl";
   DXPixelShaderFile      = "shaders/common/particleCompositeP.hlsl";
   
   OGLVertexShaderFile     = "shaders/common/gl/particleCompositeV.glsl";
   OGLPixelShaderFile      = "shaders/common/gl/particleCompositeP.glsl";
   
   pixVersion = 2.0;
};

//-----------------------------------------------------------------------------
// Planar Reflection
//-----------------------------------------------------------------------------
new ShaderData( ReflectBump )
{
   DXVertexShaderFile 	= "shaders/common/planarReflectBumpV.hlsl";
   DXPixelShaderFile 	= "shaders/common/planarReflectBumpP.hlsl";
   
   OGLVertexShaderFile 	= "shaders/common/gl/planarReflectBumpV.glsl";
   OGLPixelShaderFile 	= "shaders/common/gl/planarReflectBumpP.glsl";
              
   samplerNames[0] = "$diffuseMap";
   samplerNames[1] = "$refractMap";
   samplerNames[2] = "$bumpMap";
   
   pixVersion = 2.0;
};

new ShaderData( Reflect )
{
   DXVertexShaderFile 	= "shaders/common/planarReflectV.hlsl";
   DXPixelShaderFile 	= "shaders/common/planarReflectP.hlsl";
   
   OGLVertexShaderFile 	= "shaders/common/gl/planarReflectV.glsl";
   OGLPixelShaderFile 	= "shaders/common/gl/planarReflectP.glsl";
   
   samplerNames[0] = "$diffuseMap";
   samplerNames[1] = "$refractMap";
   
   pixVersion = 1.4;
};

//-----------------------------------------------------------------------------
// fxFoliageReplicator
//-----------------------------------------------------------------------------
new ShaderData( fxFoliageReplicatorShader )
{
   DXVertexShaderFile 	= "shaders/common/fxFoliageReplicatorV.hlsl";
   DXPixelShaderFile 	= "shaders/common/fxFoliageReplicatorP.hlsl";
   
   OGLVertexShaderFile  = "shaders/common/gl/fxFoliageReplicatorV.glsl";
   OGLPixelShaderFile   = "shaders/common/gl/fxFoliageReplicatorP.glsl";

   samplerNames[0] = "$diffuseMap";
   samplerNames[1] = "$alphaMap";
   
   pixVersion = 1.4;
};