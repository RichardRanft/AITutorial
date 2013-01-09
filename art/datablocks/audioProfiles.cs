//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// Always declare audio Descriptions (the type of sound) before Profiles (the
// sound itself) when creating new ones.

// ----------------------------------------------------------------------------
// Now for the profiles - these are the usable sounds
// ----------------------------------------------------------------------------

datablock SFXProfile(ThrowSnd)
{
   filename = "art/sound/throw";
   description = AudioClose3d;
   preload = false;
};

datablock SFXProfile(OOBWarningSnd)
{
   filename = "art/sound/orc_pain";
   description = "AudioLoop2D";
   preload = false;
};
