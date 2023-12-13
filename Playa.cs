using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

//pelaajaluokka
public class Playa: PhysicsObject
{

    public Playa(double width, double height) : base(width, height)
    {
        //jättää fysiikkamoottorin huomiotta
        this.IgnoresPhysicsLogics = true;
        this.Mass = 10000;
        this.CanRotate = false;
        this.Tag = "player";
    }
}

