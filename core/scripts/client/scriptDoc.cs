//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// Writes out all script functions to a file.
function writeOutFunctions()
{
   new ConsoleLogger(logger, "scriptFunctions.txt", false);
   dumpConsoleFunctions();
   logger.delete();
}

// Writes out all script classes to a file.
function writeOutClasses()
{
   new ConsoleLogger(logger, "scriptClasses.txt", false);
   dumpConsoleClasses();
   logger.delete();
}
