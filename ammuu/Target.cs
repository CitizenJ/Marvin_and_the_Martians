using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Assets;

public class Target : PhysicsObject 
{
    private IntMeter hP = new IntMeter(3, 0, 3);
    public IntMeter HP { get { return hP; } }

    public Target (double width, double height, string tag) : base(width, height)
    {
        this.Tag = tag;
        IgnoresPhysicsLogics = true;
        CanRotate = false;
        hP.LowerLimit += delegate { this.Destroy(); };
    }
}

