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
    %unit.AIClientMan = "";
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
        %i = 2;
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

function AIClientManager::getNearestAllyList(%this, %unit, %num, %range)
{
    %allyList = new SimSet();
    %ally = AIManager.findNearestUnit(%unit, %range);
    if (!isObject(%ally) || %ally == 0)
    {
        %allyList.delete();
        return 0;
    }
    %allyList.add(%ally);
    %count = 1;
    while (%count < %num)
    {
        %ally = AIManager.findNearestUnit(%allyList.getObject(%count - 1), %range);
        if(!isObject(%ally) || %ally == 0)
            break;
        %curNum = %allyList.getCount();
        %duplicate = false;
        for (%i = 0; %i < %curNum; %i++)
        {
            if (%ally == %allyList.getObject(%i))
            {
                %duplicate = true;
                break;
            }
        }
        if (%duplicate)
            break;
        %allyList.add(%ally);
        %count++;
    }
    return %allyList;
}

//-----------------------------------------------------------------------------
// Message handlers
//-----------------------------------------------------------------------------
// Message handlers should be written with the unit as the first non-this parameter
// and other parameters as needed.  See the standard message que handle caller:
//
// eval("%this."@%unitMessage@"("@%unit@", "@%data@");");
//
// So the message is the handler message to call, the unit is the orignator and 
// data is assembled from additional fields passed in the message.
//
// A message is sent by an AI unit from it's datablock think method like so:
//
// %obj.AIClientMan.sendMessage(%obj TAB "underAttack" TAB %damageLvl TAB %obj.damageSourceObj);
//
// %obj is the unit.  It's AIClientManager is assigned to it when it is spawned in
// AIClient::addUnit().  Wherever you send a message from you can use this to do it.
// The message is a tab-delimited string that is expected to be compose as so:
//
// <sending unit> TAB <message name> TAB <tab-delimited handler parameters>
//
// At the moment messages are simply handled in the order received.

/// <summary>
/// A simple message to friendly units that %unit is under attack
/// </summary>
/// <param name="unit">The unit that sent the message.</param>
/// <param name="damage"><unit>'s current damage level.</param>
/// <param name="source">The object that damaged <unit>.</param>
function AIClientManager::underAttack(%this, %unit, %damage, %source)
{
    // %source is most likely a projectile, but whatever it is it should carry a sourceObject
    // field on it that should hold the originating unit (the unit that fired the projectile).
    //echo(" @@@ AI Unit " @ %unit @ " sent message underAttack with data " @ %damage @ ":" @ %source @":"@%source.sourceObject);
    if (%unit.getState() $= "dead")
        return;
    if (%source.sourceObject.team == %unit.team)
    {
        //echo(" @@@ Friendly Fire!");
        return;
    }
    if (isEventPending(%unit.waitForHelp))
        return;

    %unit.waitForHelp = %unit.schedule(2000, think);

    %allyList = %this.getNearestAllyList(%unit, 3, 250);
    if(isObject(%allyList))
    {
        %allyCount = %allyList.getCount();
        if (%allyCount > 0)
        {
            for (%i = 0; %i < %allyCount; %i++)
            {
                %ally = %allyList.getObject(%i);
                if (!isObject(%ally))
                    continue;
                %offsetX = getRandom(-20, 20);
                %offsetY = getRandom(-20, 20);
                %dest = %unit.getPosition();
                %dest.x += %offsetX;
                %dest.y += %offsetY;
                %ally.target = %source.sourceObject;
                %ally.setMoveDestination(%dest);
            }
        }
        %allyList.delete();
    }
}