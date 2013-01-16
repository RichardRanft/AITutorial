//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
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

    if (!AIManager.started)
        AIManager.start(200, 1000);
    if (!AIClientManager.started)
        AIClientManager.start(500);
    AIClientManager.client = 0;

    if (%client.team $= "")
        %client.team = %client.getId();

    %client.AIMan = new ScriptObject()
    {
        class = AIClientManager;
    };
    %client.AIMan.client = %client;
    %client.AIMan.start(500);
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
