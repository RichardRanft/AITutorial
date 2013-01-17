// ----------------------------------------------------------------------------
// DBArena
// 2 July 2012
// Event Manager
// ----------------------------------------------------------------------------

// Builds the event manager and script listener that will be responsible for
// handling important game system events.
function initializeAIEventManager()
{
    if (!isObject(AIEventManager))
    {
        $AIEventManager = new EventManager(AIEventManager)
        { 
            queue = "AIEventQue"; 
        };
        
        // Module related signals
        AIEventManager.registerEvent("_UnitUnderAttack");
    }
    
    if (!isObject(AIListener))
    {
        $AIListener = new ScriptMsgListener(AIListener) 
        { 
            class = "AIEventListener"; 
        };
        
        // Module related subscriptions
        AIEventManager.subscribe(AIListener, "_UnitUnderAttack", "unitUnderAttack");
    }
}

// Cleanup the event manager
function destroyAIEventManager()
{
    if (isObject(AIEventManager) && isObject(AIListener))
    {
        // Remove all the subscriptions
        AIEventManager.remove(AIListener, "_UnitUnderAttack");
        
        // Delete the actual objects
        AIEventManager.delete();
        AIListener.delete();
        
        // Clear the global variables, just in case
        $AIEventManager = "";
        $AIListener = "";
    }
}

function AIEventListener::unitUnderAttack(%this, %messageData)
{
}
