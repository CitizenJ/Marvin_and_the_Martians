using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;

public class Bombhand : Weapon
{
    public Bombhand(double width, double height) : base(width, height)
    {
        Power.DefaultValue = 8000;
        Ammo.DefaultValue = Int32.MaxValue;
        AmmoIgnoresExplosions = true;
        CanHitOwner = false;
        TimeBetweenUse = TimeSpan.FromSeconds(0);
    }

    protected override PhysicsObject CreateProjectile()
    {
        return new Grenade(8.0);
    }
}
