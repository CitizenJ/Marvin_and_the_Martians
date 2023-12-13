using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class ammuu : PhysicsGame
{
Playa player1;
AssaultRifle rifle;
Bombhand bombhand;
Timer slidewindow;
Key lastKey;
Vector up = new Vector(0, 400);
Vector left = new Vector(-400, 0);
Vector down = new Vector(0, -400);
Vector right = new Vector(400, 0);
Vector aim = Vector.Zero;

    public override void Begin()
    {
        Level.Background.Color = Color.Black;
        Mouse.IsCursorVisible = true;
        System.Windows.Forms.Cursor custom = NativeMethods.LoadCustomCursor(@"cursor/aim.cur");
        System.Windows.Forms.Form form = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(this.Window.Handle);
        form.Cursor = custom;

        slidewindow = new Timer();
        slidewindow.Interval = 0.15;
        slidewindow.Timeout += EndWindow;
        lastKey = new Key();
        
        player1 = new Playa(40.0, 40.0);
        player1.Image = LoadImage("soldier");
        Add(player1);

        rifle = new AssaultRifle(0, 0);
        rifle.InfiniteAmmo = true;
        rifle.CanHitOwner = false;
        rifle.Image = null;
        rifle.ProjectileCollision = BulletHit;
        player1.Add(rifle);
        bombhand = new Bombhand(0, 0);
        player1.Add(bombhand);

        Timer spawner = new Timer();
        spawner.Interval = 5;

        spawner.Timeout += delegate { CreateTargets(SideSelect); };

        CreateTargets(Level.Left + 200.0, -400.0, "target");
        CreateTargets(Level.Right - 200.0, 400.0, "target");
        CreateTargets(Level.Left + 200.0, 400.0, "target");
        CreateTargets(Level.Right - 200.0, -400.0, "target");
        CreateTargets(Level.Left - 500.0, 0, "target2");
        CreateTargets(Level.Right + 500.0, 0, "target2");

        Kontrols();
    }

    public void CreateTargets(double x, double y, string tag)
    {
        Target target = new Target(40.0, 40.0, tag);
            if (tag.Equals("target"))
            {
            target.Image = LoadImage("martian1");
            }
            if (tag.Equals("target2"))
            {
            target.Image = LoadImage("martian2");

            }
        target.X = x;
        target.Y = y;
        GiveAI(target);
        Add(target);
    }

    public void GiveAI(Target target)
    {
        if (target.Tag.Equals("target"))
        {
            FollowerBrain hunt = new FollowerBrain(player1);
            hunt.TurnWhileMoving = true;
            hunt.Speed = 200;
            target.Brain = hunt;
        }
        if (target.Tag.Equals("target2"))
        {
            FollowerBrain shoot = new FollowerBrain(player1);
            shoot.TurnWhileMoving = true;
            shoot.Speed = 100;
            target.Brain = shoot;

            LaserGun lazer = new LaserGun(0, 0);
            lazer.Image = null;
            lazer.AttackSound = LoadSoundEffect("Blaster");
            lazer.InfiniteAmmo = true;
            lazer.CanHitOwner = false;
            target.Add(lazer);

            Timer shot = new Timer();
            shot.Interval = 3;
            shot.Timeout += delegate { Incoming(target, lazer); };
            shot.Start();
        }
    }

    public void Kontrols()
    {
        Mouse.ListenMovement(0.1, Aim, "Aim weapon");
        Mouse.Listen(MouseButton.Left, ButtonState.Down, Fire, "Fire weapon");
        Mouse.Listen(MouseButton.Right, ButtonState.Pressed, BombToss, "Throw firebomb");

        Keyboard.Listen(Key.W, ButtonState.Down, Move, "Move north. 2x to dodge", up, false, Key.W);
        Keyboard.Listen(Key.W, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.W);
        Keyboard.Listen(Key.A, ButtonState.Down, Move, "Move west. 2x to dodge", left, false, Key.A);
        Keyboard.Listen(Key.A, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.A);
        Keyboard.Listen(Key.S, ButtonState.Down, Move, "Move south. 2x to dodge", down, false, Key.S);
        Keyboard.Listen(Key.S, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.S);
        Keyboard.Listen(Key.D, ButtonState.Down, Move, "Move east. 2x to dodge", right, false, Key.D);
        Keyboard.Listen(Key.D, ButtonState.Released, Move, "Stop", Vector.Zero, true, Key.D);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Shows this bullshit");
        Keyboard.Listen(Key.F2, ButtonState.Pressed, Unjam, "Manually re-enable controls");

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Quit game");
    }

    public void Move(Vector dir, bool slideok, Key k)
    {
        if (slidewindow.Enabled == false && slideok == false)
        {
            player1.Move(dir);
            Orientate();
        }
        else if (slidewindow.Enabled == false && slideok == true)
        {
            player1.Move(dir);
            slidewindow.Start();
            lastKey = k;
            Orientate();
        }
        else if (slidewindow.Enabled == true)
        {
            if (k == lastKey)
            {
            Keyboard.Disable(Key.W);
            Keyboard.Disable(Key.A);
            Keyboard.Disable(Key.S);
            Keyboard.Disable(Key.D);
            Mouse.DisableAll();
            player1.MoveTo(new Vector(player1.X + (dir.X/2), player1.Y + (dir.Y/2)), 1600, SlideStop);
            }
            else if (slideok == false)
            {
            player1.Move(dir);
            slidewindow.Stop();
            Orientate();
            }
            else if (slideok == true)
            {
            player1.Move(dir);
            slidewindow.Start();
            lastKey = k;
            Orientate();
            }
        }
    }

    private void SlideStop()
    {
        Keyboard.EnableAll();
        Mouse.EnableAll();
        Orientate();
    }

    public void EndWindow()
    {
        slidewindow.Stop();
    }

    public void Unjam()
    {
        Keyboard.EnableAll();
        Mouse.EnableAll();
    }

    public void Aim(AnalogState mouseMove)
    {
        Orientate();
    }

    public void Orientate()
    {
        aim = (Mouse.PositionOnWorld - player1.AbsolutePosition).Normalize();
        player1.Angle = aim.Angle;
    }

    public void Fire()
    {
        PhysicsObject bullet = rifle.Shoot();
        if (bullet != null)
        {
            bullet.Image = LoadImage("bullet");
            bullet.IgnoresExplosions = true;
            bullet.CollisionIgnoreGroup = 1;
        }
    }

    public void BulletHit(PhysicsObject bullet, PhysicsObject target)
    {
        string st = (String)target.Tag;
        if (st.Contains("target") == true)
        {
            bullet.Destroy();
            ((Target)target).HP.Value--;
        }
    }

    public void BombToss()
    {
        PhysicsObject bomb = bombhand.Shoot();
        if (bomb != null)
        {
            bomb.CollisionIgnoreGroup = 1;
            ((Grenade)bomb).FuseTime = TimeSpan.FromSeconds(0.5);
            ((Grenade)bomb).Explosion.Speed = 500;
            ((Grenade)bomb).Explosion.ShockwaveReachesObject += BombHit;
        }
    }

    public void BombHit(IPhysicsObject target, Vector v)
    {
        string st = (String)target.Tag;
        if (st.Contains("target") == true)
        {
            ((Target)target).HP.Value = -3;
        }
    }

    public void Incoming(Target target, LaserGun lazer)
    {
        if (target.IsDestroyed == false)
        {
            Vector inc = (player1.Position - target.Position).Normalize();
            target.Angle = inc.Angle;
            PhysicsObject ray = lazer.Shoot();
            if (ray != null)
            {
                ray.IgnoresExplosions = true;
            }
        }
    }

    public Vector SideSelect()
    {
    Vector xp;
    string side = RandomGen.SelectOne<string>("left", "right");
    
    if (side.Equals("left")
    {

    }
    return xp;
    }

}
