using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;


//hirviöluokka
public class Target : PhysicsObject 
{
    //antaa hirviölle kolme terveyspistettä
    private IntMeter hP = new IntMeter(3, 0, 3);
    public IntMeter HP { get { return hP; } }
    private ammuu peli;

    public Target (double width, double height, string tag, ammuu game) : base(width, height)
    {
        this.Tag = tag;
        this.peli = game;
        //olio jättää fysiikkamoottorin huomiotta
        IgnoresPhysicsLogics = true;
        CanRotate = false;
        //jos terveyspisteet menee nollaan, hirviö kuolee
        hP.LowerLimit += delegate { this.Death(); };
    }

    //kuolinfunktio. Kutsutaan hirviön menettäessä kaikki terveyspisteensä
    public void Death()
    {
        //jos "jahtaaja", annetaan sata pistettä
        if (this.Tag.Equals("target"))
        {
            this.peli.piste.Value += 100;
        }
        //jos "ampuja", annetaan 200 pistettä. Lapsioliot poistetaan. 
        if (this.Tag.Equals("target2"))
        {
            this.peli.piste.Value += 200;
            this.Clear();
        }
        //kuolinääni
        this.peli.deathsound.Play();
        //räjähdys
        this.peli.exdeath.AddEffect(this.X, this.Y, 20);
        //hirviö poistetaan pelistä
        this.Destroy();
    }
}

