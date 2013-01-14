//-----------------------------------------------------------------------------
// Client AI Manager system - coordinates groups of AI units
//-----------------------------------------------------------------------------
// This system should handle messages from units that belong to the client that
// it is assigned to.
$AIClientManager::ThinkTime = 500;
if (!isObject(AIClientManager))
    new ScriptObject(AIClientManager);

function AIClientManager::start(%this, %thinkTime)
{
    MissionCleanup.add(%this);
    %this.thinkTime = (%thinkTime !$= "" ? %thinkTime : $AIClientManager::ThinkTime);

    if (!isObject(%this.messageQue))
        %this.messageQue = new SimSet();
    else
        %this.messageQue.clear();

    if (!isObject(%this.unitList))
        %this.unitList = new SimSet();
    else
        %this.unitList.clear();

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
    %newUnit.team = (%this.client !$= "" ? %this.client : 0);
    %newUnit.AIClientMan = %this;
    %this.unitList.add(%newUnit);
    return %newUnit;
}

function AIClientManager::removeUnit(%this, %unit)
{
    if (%this.unitList.isMember(%unit))
        %this.unitList.remove(%unit);
    %index = %this.messageQue.getCount() - 1;
    while(%index >= 0)
    {
        %msg = %this.messageQue.getObject(%index);
        %sender = getField(%msg.message, 0);
        if (%sender == %unit)
        {
            %this.messageQue.remove(%msg);
            %msg.delete();
        }
        %index--;
    }
}

/// <summary>
/// This function handles messages from AI Units in the same team
/// </summary>
function AIClientManager::think(%this)
{
    if (%this.client $= "0" || isObject(%this.client))
    {
        %index = %this.messageQue.getCount() - 1;
        while (%index >= 0)
        {
        	// message is no longer valid but somehow did not get cleaned up,
        	// so clean it up.
            %message = %this.messageQue.getObject(%index);
            if (!isObject(%message))
            {
                if (%this.messageQue.isMember(%message))
                {
                    %this.messageQue.remove(%message);
                }
            }
            %this.handleMessage(%message);
            %index--;
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
    %this.messageQue.add(%msgObj);
    %this.messageQue.bringToFront(%msgObj);
}

function AIClientManager::handleMessage(%this, %message)
{
    %unit = getField(%message.message, 0);
    %unitMessage = getField(%message.message, 1);
    %dataCount = getFieldCount(%message.message);
    if (%dataCount > 1)
    {
        %i = %dataCount - 1;
        while (%i < %dataCount)
        {
            if (%i == 2)
                %data = getField(%message.message, %i);
            else
                %data = %data @ ", " @ getField(%message.message, %i);
            %i++;
        }
    }
    if (%this.isMethod(%unitMessage))
    {
        eval("%this."@%unitMessage@"("@%unit@", "@%data@");");
    }
    %this.messageQue.remove(%message);
    %message.delete();
}

//-----------------------------------------------------------------------------
// Message handlers
//-----------------------------------------------------------------------------

function AIClientManager::underAttack(%this, %unit, %damage)
{
    echo(" @@@ AI Unit " @ %unit @ " sent message underAttack with data " @ %damage);
}