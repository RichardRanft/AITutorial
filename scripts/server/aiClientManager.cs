//-----------------------------------------------------------------------------
// Client AI Manager system - coordinates groups of AI units
//-----------------------------------------------------------------------------
// This system should handle messages from units that belong to the client that
// it is assigned to.
$AIClientManager::ThinkTime = 500;

function AIClientManager::start(%this, %thinkTime)
{
    MissionCleanup.add(%this);
    %this.thinkTime = (%thinkTime !$= "" ? %thinkTime : $AIClientManager::ThinkTime);

    if (!isObject(%this.messageQue))
        %this.messageQue = new SimSet();
    else
        %this.messageQue.clear();

    %this.think();

    %this.started = true;
}

/// <summary>
/// This function requests that the AIClientManager spawn a unit and add it to its group.
/// </summary>
/// <param name="name">The desired unit name - this is the SimName of the object and must be unique or "".</param>
/// <param name="spawnLocation">The position or object (spawnpoint, path object) to spawn the unit at.</param>
/// <param name="datablock">The datablock that the unit should use.</param>
/// <param name="priority">The priority of this unit. 0 to 2 from low to high priority.  Defaults to 1.</param>
/// <param name="onPath">If spawnLocation is a path, this should be true to get the unit to spawn on and follow the path.</param>
/// <return>Returns the new unit.</return>
function AIClientManager::addUnit(%this, %name, %spawnLocation, %datablock, %priority, %onPath)
{
	if (%this.client $= "")
	{
		echo(" !!!! AIClientManager is not assigned to a client - cannot add unit");
		return 0;
	}
    %newUnit = AIManager.addUnit(%name, %spawnLocation, %datablock, %priority, %onPath);
    %newUnit.team = %this.client;
    %newUnit.AIClientMan = %this;
    return %newUnit;
}

/// <summary>
/// This function handles sorting the AIClientManager's managed units by distance and
/// priority.
/// </summary>
function AIClientManager::think(%this)
{
    // The purpose here is to reduce overhead from AI for units that are
    // farther "from the action."  So I'm using sorting units from one 
    // list to another based on the unit's distance from the nearest 
    // player's camera, since any unit near any player's 
    // camera needs to be "thinking" at the correct priority.
    if (isObject(%this.client))
    {
        %hCount = %this.messageQue.getCount();
        %index = 0;
        while (%index < %hCount)
        {
        	// message is no longer valid but somehow did not get cleaned up,
        	// so clean it up.
            %mesage = %this.messageQue.getObject(%index);
            if (!isObject(%message))
            {
                if (%this.messageQue.isMember(%unit))
                {
                    %this.messageQue.remove(%unit);
                    %hCount--;
                }
                %index++;
                continue;
            }
            echo(%message.message);
            %index++;
        }
    }
    %this.schedule(%this.thinkTime, "think");
}

function AIClientManager::sendMessage(%this, %message)
{
    if (!isObject(%this.messageQue))
        %this.messageQue = new SimSet();
    %msgObj = new ScriptObject();
    %msgObj.message = %message;
    %this.messageQue.add(%task);
}