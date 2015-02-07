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

// Some AI globals
$AIPlayer::GrenadierRange = 40.0;
$AIPlayer::GrenadeGravityModifier = 0.84;
$AIPlayer::DefaultPriority = 1;
$AIPlayer::UrgentDamageThreshold = 0.8;
$AIEventManager::DefaultAttackResponseDist = 30;
$AIEventManager::GrenadierAttackResponseDist = 60;

// These contain the datablocks and/or datablock-scoped methods for the AI 
// units.  Some are overrides from the AIPlayer scope and some call back to 
// the AIPlayer scope for default behavior.
exec("./defaultPlayer.cs");
exec("./demoPlayer.cs");
exec("./assaultUnit.cs");
exec("./grenadierUnit.cs");
