using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class ammuu : PhysicsGame
{
//pisteet
public IntMeter piste = new IntMeter(0);
public ExplosionSystem exdeath = new ExplosionSystem(LoadImage("MarsD"), 100);
public SoundEffect deathsound = LoadSoundEffect("pop");
Playa player1;
AssaultRifle rifle;
Bombhand bombhand;
//bufferi kaksoisnäpäytykselle
Timer slidewindow;
Key lastKey;
Vector up = new Vector(0, 400);
Vector left = new Vector(-400, 0);
Vector down = new Vector(0, -400);
Vector right = new Vector(400, 0);
Vector aim = Vector.Zero;

    public override void Begin()
    {
        //Lataa taustan
        Level.Background.Image = LoadImage("floor");
        Level.Width = Screen.Width;
        Level.Height = Screen.Height;
        Level.Background.TileToLevel();

        //Lataa kustomoidun kursorin
        Mouse.IsCursorVisible = true;
        System.Windows.Forms.Cursor custom = NativeMethods.LoadCustomCursor(@"cursor/aim.cur");
        System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(this.Window.Handle);
        form.Cursor = custom;

        //lisää pistetaulukon
        Label pistenaytto = new Label();
        pistenaytto.X = Screen.Left + 50;
        pistenaytto.Y = Screen.Top - 50;
        pistenaytto.Color = Color.White;
        pistenaytto.BindTo(piste);
        Add(pistenaytto);

        //hirviöiden kuolinräjähdys
        exdeath.MaxLifetime = 0.01;
        exdeath.MaxScale = 0.1;
        exdeath.ScaleAmount = 0.1;
        Add(exdeath);

        //näppäinbufferin pituus, 1/4 sekuntia
        slidewindow = new Timer();
        slidewindow.Interval = 0.15;
        slidewindow.Timeout += EndWindow;
        lastKey = new Key();
        
        //lisää pelaajahahmon
        player1 = new Playa(40.0, 40.0);
        //lataa grafiikat pelaajahahmolle
        player1.Image = LoadImage("soldier");
        Add(player1);

        //lisää pelaajan aseen (hiiren vasen näppäin)
        rifle = new AssaultRifle(0, 0);
        rifle.InfiniteAmmo = true;
        //ei voi ampua itseään
        rifle.CanHitOwner = false;
        rifle.Image = null;
        //lisää luotien törmäystarkistuksen 
        rifle.ProjectileCollision = BulletHit;
        player1.Add(rifle);
        //lisää pelaajan kranaatit (hiiren oikea näppäin)
        bombhand = new Bombhand(0, 0);
        player1.Add(bombhand);

        //lisää ajastuksen hirviöiden luonnille, uusi hirviö joka sekunti
        Timer spawner = new Timer();
        spawner.Interval = 1;
        spawner.Timeout += delegate { CreateTargets(SideSelectX(), SideSelectY(), EnemySelect()); };

        //MUSIIKIN COPYRIGHT TOMI RUUSKA
        MediaPlayer.Volume = 1.0;
        MediaPlayer.Play("Giuseppe");
        MediaPlayer.IsRepeating = true;

        //alustaa ohjauksen
        Kontrols();
        //alustaa hirviöiden luonnin
        spawner.Start();
    }

    //funktio luo ja sijoittaa hirviöitä 
    public void CreateTargets(double x, double y, string tag)
    {
        Target target = new Target(40.0, 40.0, tag, this);
            //antaa hirviölle sen tyyppiä vastaavan spriten
            if (tag.Equals("target"))
            {
            target.Image = LoadImage("martian1");
            }
            if (tag.Equals("target2"))
            {
            target.Image = LoadImage("martian2");
            }
        //sijoittaa hirviön
        target.X = x;
        target.Y = y;
        //lataa hirviön tekoälyn
        GiveAI(target);
        Add(target);
    }

    //funktio antaa hirviöille tekoälyrutiinin
    public void GiveAI(Target target)
    {
        //valitsee tekoälyn hirviön tyypin perusteella
        if (target.Tag.Equals("target"))
        {
            //"metsäjä/jahtaaja"
            FollowerBrain hunt = new FollowerBrain(player1);
            hunt.TurnWhileMoving = true;
            //hirviön nopeus
            hunt.Speed = 200;
            target.Brain = hunt;
        }
        if (target.Tag.Equals("target2"))
        {
            //"ampuja"
            FollowerBrain shoot = new FollowerBrain(player1);
            shoot.TurnWhileMoving = true;
            //hirviön nopeus
            shoot.Speed = 100;
            target.Brain = shoot;

            //hirviön ase
            LaserGun lazer = new LaserGun(0, 0);
            lazer.Image = null;
            lazer.AttackSound = LoadSoundEffect("Blaster");
            lazer.InfiniteAmmo = true;
            lazer.CanHitOwner = false;
            //törmäystarkistus
            lazer.ProjectileCollision = LaserHit;
            target.Add(lazer);

            //ampuu joka kolmas sekunti
            Timer shot = new Timer();
            shot.Interval = 3;
            shot.Timeout += delegate { Incoming(target, lazer); };
            shot.Start();
        }
    }

    //pelaajan ohjaus
    public void Kontrols()
    {
        Mouse.ListenMovement(0.1, Aim, "Aim weapon");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, Fire, "Fire weapon");
        Mouse.Listen(MouseButton.Right, ButtonState.Pressed, BombToss, "Throw bomb");

        Keyboard.Listen(Key.W, ButtonState.Down, Move, "Move north. 2x to dodge", up, false, Key.W);
        Keyboard.Listen(Key.W, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.W);
        Keyboard.Listen(Key.A, ButtonState.Down, Move, "Move west. 2x to dodge", left, false, Key.A);
        Keyboard.Listen(Key.A, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.A);
        Keyboard.Listen(Key.S, ButtonState.Down, Move, "Move south. 2x to dodge", down, false, Key.S);
        Keyboard.Listen(Key.S, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.S);
        Keyboard.Listen(Key.D, ButtonState.Down, Move, "Move east. 2x to dodge", right, false, Key.D);
        Keyboard.Listen(Key.D, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.D);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Shows this bullshit");
        Keyboard.Listen(Key.P, ButtonState.Pressed, Pause, "Pauses the game");
        Keyboard.Listen(Key.Q, ButtonState.Pressed, Unjam, "Manually re-enable controls");

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Quit game");
    }

    //pelaajan liikuttaminen
    public void Move(Vector dir, bool slideok, Key k)
    {
        //liikuttaa pelaajaa haluttuun suuntaan
        if (slidewindow.Enabled == false && slideok == false)
        {
            player1.Move(dir);
            Orientate();
        }
        //sama, mutta bufferoi suuntanäppäimen
        else if (slidewindow.Enabled == false && slideok == true)
        {
            player1.Move(dir);
            slidewindow.Start();
            lastKey = k;
            Orientate();
        }
        //liuku, buginen
        else if (slidewindow.Enabled == true)
        {
            //suorittaa liu'un, jos suunta sama kuin bufferoitu
            if (k == lastKey)
            {
            //ei voi liikkua muulla tavalla liu'un aikana
            Keyboard.Disable(Key.W);
            Keyboard.Disable(Key.A);
            Keyboard.Disable(Key.S);
            Keyboard.Disable(Key.D);
            //ei voi ampua liu'un aikana
            Mouse.DisableAll();
            //itse liuku
            player1.MoveTo(new Vector(player1.X + (dir.X/2), player1.Y + (dir.Y/2)), 1600, SlideStop);
            }
            //liikkuu normaalisti, pysäyttää bufferin kellon
            else if (slideok == false)
            {
            player1.Move(dir);
            slidewindow.Stop();
            Orientate();
            }
            //liikkuu normaalisti, bufferoi viimeisen suunnan
            else if (slideok == true)
            {
            player1.Move(dir);
            slidewindow.Start();
            lastKey = k;
            Orientate();
            }
        }
    }

    //liu'n loppu, re-aktivoi muut kontrollit
    private void SlideStop()
    {
        Keyboard.EnableAll();
        Mouse.EnableAll();
        Orientate();
    }

    //lopettaa liu'un näppäinbufferin kellon
    public void EndWindow()
    {
        slidewindow.Stop();
    }

    //funktio, joka resetoi kontrollit, jos liuku bugittaa
    public void Unjam()
    {
        Keyboard.EnableAll();
        Mouse.EnableAll();
    }

    //tähtäys, kutsutaan aina kuin hiiri liikkuu
    public void Aim(AnalogState mouseMove)
    {
        Orientate();
    }

    //funktio, joka määrittää pelaajan suunnan kursoria kohti
    public void Orientate()
    {
        aim = (Mouse.PositionOnWorld - player1.AbsolutePosition).Normalize();
        player1.Angle = aim.Angle;
    }

    //ampuu aseella, kutsutaan kuin vasen hiirennappi on painettuna
    public void Fire()
    {
        PhysicsObject bullet = rifle.Shoot();
        if (bullet != null)
        {
            //lataa kuvan
            bullet.Image = LoadImage("bullet");
            bullet.IgnoresExplosions = true;
            bullet.CollisionIgnoreGroup = 1;
        }
    }

    //luotien törmäystarkistus
    public void BulletHit(PhysicsObject bullet, PhysicsObject target)
    {
        string st = (String)target.Tag;
        //jos hirviö
        if (st.Contains("target") == true)
        {
            //poistaa luodin pelistä
            bullet.Destroy();
            //hirviö ottaa yhden pisteen vahinkoa
            ((Target)target).HP.Value--;
        }
    }

    //heittää pommin, kutsutaan kuin oikeaa hiirennappia painetaan kerran
    public void BombToss()
    {
        PhysicsObject bomb = bombhand.Shoot();
        if (bomb != null)
        {
            //pommit ei välitä pelaajan luodeista
            bomb.CollisionIgnoreGroup = 1;
            //puolen sekunnin sytytinlanka
            ((Grenade)bomb).FuseTime = TimeSpan.FromSeconds(0.5);
            ((Grenade)bomb).Explosion.Speed = 500;
            //törmäystarkistus
            ((Grenade)bomb).Explosion.ShockwaveReachesObject += BombHit;
        }
    }

    //pommin törmäystarkistus
    public void BombHit(IPhysicsObject target, Vector v)
    {
        string st = (String)target.Tag;
        //jos hirviö
        if (st.Contains("target") == true)
        {
            //hirviö ottaa 3 pistettä vahinkoa
            ((Target)target).HP.Value = -3;
        }
    }

    //ampuja-hirviön laser
    public void Incoming(Target target, LaserGun lazer)
    {
        //tarkistaa, että hirviö on elossa ja ruudun sisällä, muuten ei anneta ampua
        if (target.IsDestroyed == false && target.X >= Screen.Left && target.X <= Screen.Right && target.Y <= Screen.Top && target.Y >= Screen.Bottom)
        {
            //ampuu pelaajan suuntaan
            Vector inc = (player1.Position - target.Position).Normalize();
            target.Angle = inc.Angle;
            //itse ammus
            PhysicsObject ray = lazer.Shoot();
            if (ray != null)
            {
                ray.Color = Color.BrightGreen;
                ray.IgnoresExplosions = true;
            }
        }
    }

    //laserin törmäystarkistus
    public void LaserHit(PhysicsObject bullet, PhysicsObject target)
    {
        string st = (String)target.Tag;
        if (st.Contains("player") == true)
        {
            bullet.Destroy();
        }
    }

    //hirviö-generaattorin funktio. Valitsee satunnaisesti kummalta puolelta ruutua hirviö tulee
    public Double SideSelectX()
    {
    Double xp = 0.0;
    //satunnaisesti valitsee puolen
    string side = RandomGen.SelectOne<string>("left", "right");
    
    if (side.Equals("left"))
    {
        xp = Screen.Left - RandomGen.NextDouble(100.0, 1000.0);    
    }
    if (side.Equals("right"))
    {
        xp = Screen.Right + RandomGen.NextDouble(100.0, 1000.0);
    }
    return xp;
    }

    //hirviögeneraattorin funktio. Valitsee satunnaisesti tuleeko hirviö ylhäältä vai alhaalta
    public Double SideSelectY()
    {
    Double yp = 0.0;
    //satunnaisesti valitsee puolen
    string side = RandomGen.SelectOne<string>("up", "down");

    if (side.Equals("up"))
    {
        yp = Screen.Top + RandomGen.NextDouble(100.0, 1000.0);
    }
    if (side.Equals("down"))
    {
        yp = Screen.Bottom + RandomGen.NextDouble(100.0, 1000.0);
    }
    return yp;
    }

    //hirviögeneraattorin funktio. Valitsee satunnaisesti hirviön tyypin 
    public String EnemySelect()
    {
        //luo merkkijonon hirviön tyypille
        String st = "";
        //Satunnaisesti valitsee hirviön tyypin. 1/3 mahdollisuus, että hirviö on ampuja
        st = RandomGen.SelectOne<string>("target", "target", "target2");
        //palauttaa hirviön tyypin
        return st;
    }
}
