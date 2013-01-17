//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Misc server commands avialable to clients
//-----------------------------------------------------------------------------

function serverCmdSuicide(%client)
{
   if (isObject(%client.player))
      %client.player.kill("Suicide");
}

function serverCmdPlayCel(%client,%anim)
{
   if (isObject(%client.player))
      %client.player.playCelAnimation(%anim);
}

function serverCmdTestAnimation(%client, %anim)
{
   if (isObject(%client.player))
      %client.player.playTestAnimation(%anim);
}

function serverCmdPlayDeath(%client)
{
   if (isObject(%client.player))
      %client.player.playDeathAnimation();
}

// ----------------------------------------------------------------------------
// Throw/Toss
// ----------------------------------------------------------------------------

function serverCmdThrow(%client, %data)
{
   %player = %client.player;
   if(!isObject(%player) || %player.getState() $= "Dead" || !$Game::Running)
      return;
   switch$ (%data)
   {
      case "Weapon":
         %item = (%player.getMountedImage($WeaponSlot) == 0) ? "" : %player.getMountedImage($WeaponSlot).item;
         if (%item !$="")
            %player.throw(%item);
      case "Ammo":
         %weapon = (%player.getMountedImage($WeaponSlot) == 0) ? "" : %player.getMountedImage($WeaponSlot);
         if (%weapon !$= "")
         {
            if(%weapon.ammo !$= "")
               %player.throw(%weapon.ammo);
         }
      default:
         if(%player.hasInventory(%data.getName()))
            %player.throw(%data);
   }
}

// ----------------------------------------------------------------------------
// Force game end and cycle
// Probably don't want this in a final game without some checks.  Anyone could
// restart a game.
// ----------------------------------------------------------------------------

function serverCmdFinishGame()
{
   cycleGame();
}

// ----------------------------------------------------------------------------
// Cycle weapons
// ----------------------------------------------------------------------------

function serverCmdCycleWeapon(%client, %direction)
{
   %client.getControlObject().cycleWeapon(%direction);
}

// ----------------------------------------------------------------------------
// Unmount current weapon
// ----------------------------------------------------------------------------

function serverCmdUnmountWeapon(%client)
{
   %client.getControlObject().unmountImage($WeaponSlot);
}

// ----------------------------------------------------------------------------
// Weapon reloading
// ----------------------------------------------------------------------------

function serverCmdReloadWeapon(%client)
{
   %player = %client.getControlObject();
   %image = %player.getMountedImage($WeaponSlot);
   
   // Don't reload if the weapon's full.
   if (%player.getInventory(%image.ammo) == %image.ammo.maxInventory)
      return;
      
   if (%image > 0)
      %image.clearAmmoClip(%player, $WeaponSlot);
}

// ----------------------------------------------------------------------------
// Camera commands
// ----------------------------------------------------------------------------

function serverCmdorbitCam(%client)
{
    %client.camera.setOrbitObject(%client.player, mDegToRad(20) @ "0 0", 0, 5.5, 5.5);
    %client.camera.camDist = 5.5;
    %client.camera.controlMode = "OrbitObject";
}

function serverCmdoverheadCam(%client)
{
    //%client.camera.position = VectorAdd(%client.player.position, "0 0 30");
    //%client.camera.lookAt(%client.player.position);
    %client.camera.controlMode = "Overhead"; 
}

function serverCmdactionCam(%client)
{
    %client.camera.setTrackObject(%client.player);
}

function serverCmdtoggleCamMode(%client)
{
    if(%client.camera.controlMode $= "Overhead")
    {
        %client.camera.setOrbitObject(%client.player, mDegToRad(20) @ "0 0", 0, 5.5, 5.5);
        %client.camera.camDist = 5.5;
        %client.camera.controlMode = "OrbitObject";
    }
    else if(%client.camera.controlMode $= "OrbitObject")
    {
        %client.camera.controlMode = "Overhead"; 
        %client.camera.position = VectorAdd(%client.player.position, "0 0 30");
        %client.camera.lookAt(%client.player.position);
    }
}

function serverCmdadjustCamera(%client, %adjustment)
{
    if(%client.camera.controlMode $= "OrbitObject")
    {
        if(%adjustment == 1)
            %n = %client.camera.camDist + 0.5;
        else
            %n = %client.camera.camDist - 0.5;

        if(%n < 0.5)
            %n = 0.5;

        if(%n > 15)
            %n = 15.0;

        %client.camera.setOrbitObject(%client.player, %client.camera.getRotation(), 0, %n, %n);
        %client.camera.camDist = %n;
    }
    if(%client.camera.controlMode $= "Overhead")
    {
        %client.camera.position = VectorAdd(%client.camera.position, "0 0 " @ %adjustment);
    }
}

function serverCmdspawnBaddie(%client)
{
    // Create a new, generic AI Player
    // Position will be at the camera's location
    // Datablock will determine the type of actor
    %enemy = AIClientManager.addUnit("", %client.camera.getTransform(), "DefaultPlayerData", 0);
    
    if (!isObject(Team0List))
        new SimSet(Team0List){};

    Team0List.add(%enemy);
    MissionGroup.add(%enemy);
}

// ----------------------------------------------------------------------------
// Player commands
// ----------------------------------------------------------------------------

// Request that the server move the AI unit or units in question to the position
// we clicked.
function serverCmdmovePlayer(%client, %pos, %start, %ray)
{
    //echo(" -- " @ %client @ ":" @ %client.player @ " moving");

    // Get access to the AI player we control
    %ai = findTeamLeader(%client.team);

    // If we don't have a unit selected we don't need to be here....
    if (!isObject(%ai))
        return;

    %ray = VectorScale(%ray, 1000);
    %end = VectorAdd(%start, %ray);

    // only care about terrain objects
    %searchMasks = $TypeMasks::TerrainObjectType | $TypeMasks::StaticTSObjectType | 
    $TypeMasks::InteriorObjectType | $TypeMasks::ShapeBaseObjectType | $TypeMasks::StaticObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If the terrain object was found in the scan
    if( %scanTarg )
    {
        %pos = getWords(%scanTarg, 1, 3);
        // Get the normal of the location we clicked on
        %norm = getWords(%scanTarg, 4, 6);

        // Set the destination for the AI player to
        // make him move
        %teamList = "Team"@%client.team@"List";
        if (isObject(%teamList))
        {
            %c = 0;
            %end = %teamList.getCount();
            %unit = %teamList.getObject(0);
            while (isObject(%unit))
            {
                if (%unit.isSelected)
                {
                    %dest = VectorSub(%pos, %unit.destOffset);
                    %unit.setMoveDestination( %dest );
                }
                %c++;
                if (%c < %end)
                    %unit = %teamList.getObject(%c);
                else
                    %unit = 0;
            }
        }
        else
            %ai.setMoveDestination( %pos );
    }
    // tell the client to draw our destination decal
    commandToClient(%client, 'completeMove', %pos, %start, %ray);
}

// Find out what we clicked on and act accordingly
function serverCmdcheckTarget(%client, %pos, %start, %ray)
{
    %ray = VectorScale(%ray, 1000);
    %end = VectorAdd(%start, %ray);

    // Only care about players this time
    %searchMasks = $TypeMasks::PlayerObjectType | $TypeMasks::StaticTSObjectType
         | $TypeMasks::StaticObjectType;

    // Search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If an enemy AI object was found in the scan
    if( %scanTarg )
    {
        // Get the enemy ID
        %target = firstWord(%scanTarg);
        if (%target.class $= "barracks" && %target.team == %client.team)
        {
            serverCmdspawnTeammate(%client, %target);
        }
        else if (%target.getClassName() $= "AIPlayer")
        {
            if (%target.team != %client.team)
            {
                // Cause our AI object to aim at the target
                // offset (0, 0, 1) so you don't aim at the target's feet
                %teamList = "Team"@%client.team@"List";
                if (isObject(%teamList))
                {
                    %c = 0;
                    %unitCount = %teamList.getCount();
                    while (%c < %unitCount)
                    {
                        %unit = %teamList.getObject(%c);
                        if (%unit.isSelected)
                        {
                            attack(%unit, %target);
                        }
                        %c++;
                    }
                }
            }
            else
            {
                if ($SelectToggled)
                {
                    multiSelect(%target, %client.team);
                }
                else
                {
                    cleanupSelectGroup(%client.team);
                    %target.isSelected = true;
                    %target.isLeader = true;
                }
            }
        }
        else
        {
            serverCmdstopAttack(%client);
            if (!$SelectToggled)
                cleanupSelectGroup(%client.team);
        }
    }
    else
    {
        serverCmdstopAttack(%client);
        if (!$SelectToggled)
            cleanupSelectGroup(%client.team);
    }
}

// cancels the current units' attack.
function serverCmdstopAttack(%client)
{
    // If no valid target was found, or left mouse
    // clicked again on terrain, stop firing and aiming
    %teamList = "Team"@%client.team@"List";
    if (!isObject(%teamList))
        return;
    for (%c = 0; %c < %teamList.getCount(); %c++)
    {
        %unit = %teamList.getObject(%c);
        %unit.pushTask("clearTarget");
    }
}

// create a building as indicated by the %type at the point we clicked.  Right
// now this only creates a "Barracks" building.
function serverCmdcreateBuilding(%client, %pos, %start, %ray, %type)
{
    // find end of search vector
    %ray = VectorScale(%ray, 2000);
    %end = VectorAdd(%start, %ray);

    %searchMasks = $TypeMasks::TerrainObjectType;

    // search!
    %scanTarg = ContainerRayCast( %start, %end, %searchMasks);

    // If the terrain object was found in the scan
    if( %scanTarg )
    {
        %obj = getWord(%scanTarg, 0);

        while (%obj.class $= "barrier")
        {
            // Get the X,Y,Z position of where we clicked
            %pos = getWords(%scanTarg, 1, 3);
            %restart = VectorNormalize(VectorSub(%end, %pos));
            %pos = VectorAdd(%pos, %restart);
            %scanTarg = ContainerRayCast( %pos, %end, %searchMasks);
            %obj = getWord(%scanTarg, 0);
        }

        %pos = getWords(%scanTarg, 1, 3);

        // spawn a new object at the intersection point
        %obj = new TSStatic()
        {
            position = %pos;
            shapeName = "art/shapes/building/orcburrow.dts";
            class = "barracks";
            collisionType = "Visible Mesh";
            scale = "0.5 0.5 0.5";
        };
        %obj.team = %client.getId();

        // Add the new object to the MissionCleanup group
        MissionCleanup.add(%obj);
        
        // Set up a spawn point for new troops to arrive at.
        %teamSpawnGroup = "Team"@%client.team@"SpawnGroup";
        if (!isObject(%teamSpawnGroup))
        {
            new SimGroup(%teamSpawnGroup)
            {
                canSave = "1";
                canSaveDynamicFields = "1";
                    enabled = "1";
            };

            MissionGroup.add(%teamSpawnGroup);
        }
        
        %spawnName = "team"@%client.team@"Spawn" @ %obj.getId();
        %datablock = (%type !$= "" ? %type : $Game::DefaultPlayerDataBlock);
        %point = new SpawnSphere(%spawnName)
        {
            radius = "1";
            dataBlock      = "SpawnSphereMarker";
            spawnClass     = $Game::DefaultPlayerClass;
            spawnDatablock = %datablock;
        };
        %point.position = VectorAdd(%obj.getPosition(), "0 6 2");
        %obj.spawnPoint = %point;
        %teamSpawnGroup.add(%point);
        MissionCleanup.add(%point);

        %triggerName = "team"@%client.team@"Trigger" @ %obj.getId();
        %box = %obj.getWorldBox();
        %rot = %obj.rotation;
        %pos = %obj.getPosition();
        %scalex = (getWord(%box, 3) - getWord(%box, 0)) * 1.2;
        %scaley = (getWord(%box, 4) - getWord(%box, 1)) * 1.2;
        %scalez = (getWord(%box, 5) - getWord(%box, 2)) * 10.0;
        %scale = %scalex SPC %scaley SPC %scalez;
        %trigger = new Trigger(%triggerName)
        {
            polyhedron = "-0.5 0.5 0.0 1.0 0.0 0.0 0.0 -1.0 0.0 0.0 0.0 1.0";
            dataBlock = "BarracksTrigger";
        	rotation = %rot;
        	position = %pos;
        	scale = %scale;
        };
        %trigger.owner = %obj;
        MissionCleanup.add(%trigger);
    }
}

function serverCmdspawnTeammate(%client, %source)
{
    // Create a new, generic AI Player
    // Datablock will determine the type of actor
    %spawnName = "team"@%client.team@"Spawn" @ %source.getId();

    %newBot = %client.AIMan.addUnit("", %spawnName, %spawnName.spawnDatablock);
    %spawnLocation = DeathMatchGame::pickPointInSpawnSphere(%newBot, %spawnName);
    %newBot.setTransform(%spawnLocation);

    %x = getRandom(-10, 10);
    %y = getRandom(4, 10);
    %vec = %x SPC %y SPC "0";
    %dest = VectorAdd(%newBot.getPosition(), %vec);
    %newBot.setMoveDestination(%dest);
}

function serverCmdtoggleMultiSelect(%client, %flag)
{
    if (%flag)
        $SelectToggled = true;
    else
        $SelectToggled = false;
}

function attack(%unit, %target)
{
    %unit.target = %target;
    %unit.attack(%target);
}

function multiSelect(%target, %team)
{
    %teamList = "Team"@%team@"List";    
    if (!isObject(%teamList))
    {
        new SimSet(%teamList);
        MissionCleanup.add(%teamList);
    }
    
    %leader = findTeamLeader(%team);
    if (isObject(%leader))
    {
        %target.destOffset = VectorSub(%leader.getPosition(), %target.getPosition());
    }
    else
    {
        %target.destOffset = "0 0 0";
        %target.isLeader = true;
    }

    %target.isSelected = true;
    %teamList.add(%target);
}

function findTeamLeader(%team)
{
    %listName = "Team"@%team@"List";
    if (!isObject(%listName))
    {
        new SimSet(%listName);
        MissionCleanup.add(%listName);
    }

    for (%c = 0; %c < %listName.getCount(); %c++)
    {
        %unit = %listName.getObject(%c);
        if (%unit.isLeader)
            return %unit;
    }

    return 0;
}

function addTeamBot(%bot, %team)
{
    %listName = "Team"@%team@"List";
    if (!isObject(%listName))
    {
        new SimSet(%listName);
        MissionCleanup.add(%listName);
    }
    %listName.add(%bot);
}

function cleanupSelectGroup(%team)
{
    %listName = "Team"@%team@"List";
    if (!isObject(%listName))
    {
        new SimSet(%listName);
        MissionCleanup.add(%listName);
    }
    
    for (%c = 0; %c < %listName.getCount(); %c++)
    {
        %temp = %listName.getObject(%c);
        %temp.isSelected = false;
        %temp.isLeader = false;
        %temp.destOffset = "0 0 0";
    }
}
