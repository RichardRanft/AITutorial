//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------


function imposterMetricsCallback()
{
   return "  | IMPOSTER |" @ 
          "   Rendered: " @ $ImposterStats::rendered @
          "   Batches: " @ $ImposterStats::batches @
          "   DrawCalls: " @ $ImposterStats::drawCalls @
          "   Polys: " @ $ImposterStats::polyCount @
          "   RtChanges: " @ $ImposterStats::rtChanges;
}