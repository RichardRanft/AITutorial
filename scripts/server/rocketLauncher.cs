//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// Support methods used to track our number of ready shells for a charged shot.
// ----------------------------------------------------------------------------
function RocketLauncherImage::readyLoad(%this)
{
   //echo("\c4RocketLauncherImage::readyLoad("@ %this.getName()@")");
   %this.loadCount = 1;
   //echo("\c4 loadCount = "@ %this.loadCount);
}

function RocketLauncherImage::incLoad(%this)
{
   //echo("\c4RocketLauncherImage::incLoad("@ %this.getName()@")");
   %this.loadCount++;
   //echo("\c4 loadCount = "@ %this.loadCount);
}

// ----------------------------------------------------------------------------
// The fire method that does all of the work
// ----------------------------------------------------------------------------

function RocketLauncherImage::onAltFire(%this, %obj, %slot)
{
   //echo("\c4RocketLauncherImage::onFire("@ %this.getName() @", "@ %obj.client.nameBase @", "@ %slot@")");

   //echo("\c4 #in the pipe = "@ %this.loadCount);
   //echo("");

   // Let's check amount of ammo, if it's less than the loadCount then only
   // fire the number of shots equal to the ammount of remaining ammo.
   %currentAmmo = %obj.getInventory(%this.ammo);
   if(%currentAmmo < %this.loadCount)
      %this.loadCount = %currentAmmo;

   for(%shotCount = 0; %shotCount < %this.loadCount; %shotCount++)
   {
      // Decrement inventory ammo. The image's ammo state is updated
      // automatically by the ammo inventory hooks.
      %obj.decInventory(%this.ammo, 1);

      // We fire our weapon using the straight ahead aiming point of the gun
      //%muzzleVector = %obj.getMuzzleVector(%slot);

      // We'll need to "skew" the projectile a little bit.  We start by getting
      // the straight ahead aiming point of the gun
      %vec = %obj.getMuzzleVector(%slot);

      // Then we'll create a spread matrix by randomly generating x, y, and z
      // points in a circle
      %matrix = "";
      for(%i = 0; %i < 3; %i++)
         %matrix = %matrix @ (getRandom() - 0.5) * 2 * 3.1415926 * 0.008 @ " ";
      %mat = MatrixCreateFromEuler(%matrix);

      // Which we'll use to alter the projectile's initial vector with
      %muzzleVector = MatrixMulVector(%mat, %vec);

      // Get the player's velocity, we'll then add it to that of the projectile
      %objectVelocity = %obj.getVelocity();
      %muzzleVelocity = VectorAdd(
         VectorScale(%muzzleVector, %this.projectile.muzzleVelocity),
         VectorScale(%objectVelocity, %this.projectile.velInheritFactor));

      // Create the projectile object
      %p = new (%this.projectileType)()
      {
         dataBlock = %this.projectile;
         initialVelocity = %muzzleVelocity;
         initialPosition = %obj.getMuzzlePoint(%slot);
         sourceObject = %obj;
         sourceSlot = %slot;
         client = %obj.client;
      };
      MissionCleanup.add(%p);
   }
   return %p;
}
