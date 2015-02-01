//-----------------------------------------------------------------------------
// Copyright (c) 2012 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// DeathmatchGame
// ----------------------------------------------------------------------------
// Depends on methods found in gameCore.cs.  Those added here are specific to
// this game type and/or over-ride the "default" game functionaliy.
//
// The desired Game Type must be added to each mission's LevelInfo object.
//   - gameType = "Deathmatch";
// If this information is missing then the GameCore will default to Deathmatch.
// ----------------------------------------------------------------------------

function DeathMatchGame::onMissionLoaded(%game)
{
   //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::onMissionLoaded");

   $Server::MissionType = "DeathMatch";
   parent::onMissionLoaded(%game);
}

function DeathMatchGame::initGameVars(%game)
{
   //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::initGameVars");

   //-----------------------------------------------------------------------------
   // What kind of "player" is spawned is either controlled directly by the
   // SpawnSphere or it defaults back to the values set here. This also controls
   // which SimGroups to attempt to select the spawn sphere's from by walking down
   // the list of SpawnGroups till it finds a valid spawn object.
   // These override the values set in core/scripts/server/spawn.cs
   //-----------------------------------------------------------------------------
   
   // Leave $Game::defaultPlayerClass and $Game::defaultPlayerDataBlock as empty strings ("")
   // to spawn a the $Game::defaultCameraClass as the control object.
   $Game::defaultPlayerClass = "AIPlayer";
   $Game::defaultPlayerDataBlock = "DemoPlayerData";
   $Game::defaultPlayerSpawnGroups = "PlayerSpawnPoints PlayerDropPoints";

   //-----------------------------------------------------------------------------
   // What kind of "camera" is spawned is either controlled directly by the
   // SpawnSphere or it defaults back to the values set here. This also controls
   // which SimGroups to attempt to select the spawn sphere's from by walking down
   // the list of SpawnGroups till it finds a valid spawn object.
   // These override the values set in core/scripts/server/spawn.cs
   //-----------------------------------------------------------------------------
   $Game::defaultCameraClass = "Camera";
   $Game::defaultCameraDataBlock = "Observer";
   $Game::defaultCameraSpawnGroups = "CameraSpawnPoints PlayerSpawnPoints PlayerDropPoints";

   // Set the gameplay parameters
   %game.duration = 30 * 60;
   %game.endgameScore = 20;
   %game.endgamePause = 10;
   %game.allowCycling = false;   // Is mission cycling allowed?
}

function DeathMatchGame::startGame(%game)
{
    //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::startGame");

    parent::startGame(%game);
    exec("./aiEventManger.cs");

    initializeAIEventManager();
}

function DeathMatchGame::endGame(%game)
{
    //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::endGame");

    destroyAIEventManager();
    parent::endGame(%game);
}

function DeathMatchGame::onGameDurationEnd(%game)
{
   //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::onGameDurationEnd");

   parent::onGameDurationEnd(%game);
}

function DeathMatchGame::onClientEnterGame(%game, %client)
{
    //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::onClientEnterGame");

    parent::onClientEnterGame(%game, %client);

    // Start the AIManager if it hasn't been already
    if (!AIManager.started)
        AIManager.start($AIManager::PriorityTime, $AIManager::IdleTime);

    // Start an "AIClientManager" for the server
    if (!AIClientManager.started)
    {
        AIClientManager.start($AIClientManager::ThinkTime);
        AIClientManager.client = 0;
    }

    // Create a team ID for use by the AI Units for sorting friend from foe
    if (%client.team $= "")
        %client.team = %client.getId();

    // Create an AIClientManager for the incomming client
    %client.AIMan = new ScriptObject()
    {
        class = AIClientManager;
    };
    %client.AIMan.client = %client;
    %client.AIMan.start($AIClientManager::ThinkTime);
}

function DeathMatchGame::preparePlayer(%game, %client)
{
   echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::preparePlayer");

   // Find a spawn point for the player
   // This function currently relies on some helper functions defined in
   // core/scripts/spawn.cs. For custom spawn behaviors one can either
   // override the properties on the SpawnSphere's or directly override the
   // functions themselves.
   %playerSpawnPoint = %game.pickPlayerSpawnPoint($Game::DefaultPlayerSpawnGroups);
   // Spawn a camera for this client using the found %spawnPoint
   //%client.spawnPlayer(%playerSpawnPoint);
   %game.spawnPlayer(%client, %playerSpawnPoint, false);
   
   commandToServer('overheadCam');
}

function DeathMatchGame::spawnPlayer(%game, %client, %spawnPoint, %noControl)
{
    echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::spawnPlayer");
}

function DeathMatchGame::onClientLeaveGame(%game, %client)
{
   //echo (%game @"\c4 -> "@ %game.class @" -> DeathMatchGame::onClientLeaveGame");

   parent::onClientLeaveGame(%game, %client);

}

function DeathMatchGame::pickPlayerSpawnPoint(%game, %spawnGroups)
{
    %point = ClientGroup.getCount() % 4;
    if (%point = 0)
        %point = 4;

    %spawnPoint = "Player"@%point@"Spawn";
    if (isObject(%spawnPoint))
        return %spawnPoint;

    // Didn't find a spawn point by looking for the groups
    // so let's return the "default" SpawnSphere
    // First create it if it doesn't already exist
    if (!isObject(DefaultPlayerSpawnSphere))
    {
        %spawn = new SpawnSphere(DefaultPlayerSpawnSphere)
        {
            dataBlock      = "SpawnSphereMarker";
            spawnClass     = $Game::DefaultCameraClass;
            spawnDatablock = $Game::DefaultCameraDataBlock;
        };

        // Add it to the MissionCleanup group so that it
        // doesn't get saved to the Mission (and gets cleaned
        // up of course)
        MissionCleanup.add(%spawn);
    }

    return DefaultPlayerSpawnSphere;
}

function DeathMatchGame::pickPointInSpawnSphere(%objectToSpawn, %spawnSphere)
{
   %SpawnLocationFound = false;
   %attemptsToSpawn = 0;
   while(!%SpawnLocationFound && (%attemptsToSpawn < 5))
   {
      %sphereLocation = %spawnSphere.getTransform();
      
      // Attempt to spawn the player within the bounds of the spawnsphere.
      %angleY = mDegToRad(getRandom(0, 100) * m2Pi());
      %angleXZ = mDegToRad(getRandom(0, 100) * m2Pi());

      %sphereLocation = setWord( %sphereLocation, 0, getWord(%sphereLocation, 0) + (mCos(%angleY) * mSin(%angleXZ) * getRandom(-%spawnSphere.radius, %spawnSphere.radius)));
      %sphereLocation = setWord( %sphereLocation, 1, getWord(%sphereLocation, 1) + (mCos(%angleXZ) * getRandom(-%spawnSphere.radius, %spawnSphere.radius)));
      
      %SpawnLocationFound = true;

      // Now have to check that another object doesn't already exist at this spot.
      // Use the bounding box of the object to check if where we are about to spawn in is
      // clear.
      %boundingBoxSize = %objectToSpawn.getDatablock().boundingBox;
      %searchRadius = getWord(%boundingBoxSize, 0);
      %boxSizeY = getWord(%boundingBoxSize, 1);
      
      // Use the larger dimention as the radius to search
      if (%boxSizeY > %searchRadius)
         %searchRadius = %boxSizeY;
         
      // Search a radius about the area we're about to spawn for players.
      initContainerRadiusSearch( %sphereLocation, %searchRadius, $TypeMasks::PlayerObjectType );
      while ( (%objectNearExit = containerSearchNext()) != 0 && isObject(%objectNearExit) && %objectNearExit != %objectToSpawn)
      {
         // If any player is found within this radius, mark that we need to look
         // for another spot.
         %SpawnLocationFound = false;
         break;
      }
         
      // If the attempt at finding a clear spawn location failed
      // try no more than 5 times.
      %attemptsToSpawn++;
   }
      
   // If we couldn't find a spawn location after 5 tries, spawn the object
   // At the center of the sphere and give a warning.
   if (!%SpawnLocationFound)
   {
      %sphereLocation = %spawnSphere.getTransform();
      warn("WARNING: Could not spawn player after" SPC %attemptsToSpawn 
      SPC "tries in spawnsphere" SPC %spawnSphere SPC "without overlapping another player. Attempting spawn in center of sphere.");
   }
   
   return %sphereLocation;
}
