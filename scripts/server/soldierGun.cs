function Soldier_gunImage::onMount(%this, %obj, %slot)
{
   // Make it ready
   Parent::onMount(%this, %obj, %slot);
}

function Soldier_gunImage::onAltFire(%this, %obj, %slot)
{
   echo("Fire Grenade!");
}