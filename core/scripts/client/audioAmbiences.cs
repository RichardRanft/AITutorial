//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------


singleton SFXAmbience( AudioAmbienceDefault )
{
   environment = AudioEnvOff;
};

singleton SFXAmbience( AudioAmbienceOutside )
{
   environment = AudioEnvPlain;
   states[ 0 ] = AudioLocationOutside;
};

singleton SFXAmbience( AudioAmbienceInside )
{
   environment = AudioEnvRoom;
   states[ 0 ] = AudioLocationInside;
};

singleton SFXAmbience( AudioAmbienceUnderwater )
{
   environment = AudioEnvUnderwater;
   states[ 0 ] = AudioLocationUnderwater;
};
