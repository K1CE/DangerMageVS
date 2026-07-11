using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
    {

        /* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */


        /// <summary>
        /// Represents a base power-up that can be activated and updated over time.
        /// </summary>
        public class ManaShield : Powerup // 1: CHANGE YOUR CLASS' NAMESPACE
        {


            private List<IObject> allItems = new List<IObject>();
            private IObjectText[] effects = new IObjectText[8];
            private IObjectText[] damageEffect = new IObjectText[3];
            private Events.CallbackDelegate[] handlers = new Events.CallbackDelegate[2];
            private const string centerobj = "InvisibleBlockNoCollision";

            private IObject bird;

            private float health = 100;

            private bool queueDisable = false;

            private float preservedHealth;

            private Vector2 offset;

            private string shieldEffect = "GLM";

            private static Random shieldRandom = new Random();
            private const int MAX_TIME = 30000;
            private const int RADIUS = 25;

            private const byte COLOR_R = 123;
            private const byte COLOR_G = 244;
            private const byte COLOR_B = 244;

            private const int X_OFFSET = 0;
            private const int Y_OFFSET = 10;

            private IObjectText[] coloredPixels = new IObjectText[4];

            public override string Name
            {
                get
                {
                    return "Mana Shield";
                }
            }

            public override string Author
            {
                get { return "Danger Ross"; }
            }

            // Interval for the main update callback event

            public ManaShield(IPlayer player) : base(player){
                Time = MAX_TIME;
            }

            private Vector2 randomPoint(float radius)
            {
                float distance = (float)Math.Pow(shieldRandom.NextDouble(), 0.25) * radius;
                return Vector2Helper.Rotated(new Vector2(distance, 0), (float)(shieldRandom.NextDouble() * Math.PI * 2));
            }

            public void setColor(Color color)
            {
                foreach(IObjectText obj in coloredPixels)
                    obj.SetTextColor(color);

            }

            public void setEffect(string effect)
            {
                shieldEffect = effect;
            }
            private void breakShield()
            {
                List<IObject> toFade = new List<IObject>();

                Game.PlaySound("BreakGlass", Player.GetWorldPosition(), 5f);
                Game.PlaySound("BreakGlass", Player.GetWorldPosition(), 5f);
                Game.PlaySound("BreakGlass", Player.GetWorldPosition(), 5f);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 dir = randomPoint(RADIUS);
                    if (shieldRandom.Next(0, 2) == 0)
                    {
                        IObject debris = Game.CreateObject("GlassShard00A", Player.GetWorldPosition() + new Vector2(X_OFFSET, Y_OFFSET) + dir);
                        debris.SetHealth(1f);
                        debris.SetLinearVelocity(dir * 0.3f + new Vector2(0, 4));
                        debris.SetAngle((float)(shieldRandom.NextDouble() * (Math.PI * 2)));
                        debris.SetAngularVelocity(((float)shieldRandom.NextDouble() - 0.5f) * 20f);
                        toFade.Add(debris);
                    }
                    else
                    {
                        Game.PlayEffect("DestroyGlass", dir + Player.GetWorldPosition());
                    }
                }




                Events.UpdateCallback cleanUp = null;
                cleanUp = Events.UpdateCallback.Start(a =>
                {
                    if (toFade.Count() > 0)
                    {
                        Game.PlaySound("GlassShard", toFade[toFade.Count() - 1].GetWorldPosition(), 5f);
                        Game.PlayEffect(shieldEffect, toFade[toFade.Count() - 1].GetWorldPosition());
                        toFade[toFade.Count() - 1].Remove();
                        toFade.RemoveAt(toFade.Count() - 1);

                    }
                    else
                    {
                        cleanUp.Stop();
                    }
                }, 100);
            }


            /// <summary>
            /// Virtual method for actions upon activating the power-up.
            /// </summary>
            protected override void Activate()
            {
                offset = new Vector2(X_OFFSET, Y_OFFSET);

                Game.PlaySound("StrengthBoostStart", Player.GetWorldPosition(), 5f);
                PlayerModifiers modify = Player.GetModifiers();
                modify.MeleeStunImmunity = 1;
                preservedHealth = modify.CurrentHealth;
                Player.SetModifiers(modify);

                IObjectWeldJoint weld1 = (IObjectWeldJoint)Game.CreateObject("WeldJoint", Player.GetWorldPosition() + offset); //Direct attachment to player by center1
                IObjectWeldJoint weld2 = (IObjectWeldJoint)Game.CreateObject("WeldJoint", Player.GetWorldPosition() + offset); //Rotating attachments around center1, by center2
                IObjectWeldJoint weld3 = (IObjectWeldJoint)Game.CreateObject("WeldJoint", Player.GetWorldPosition() + offset); //attached to player by proxy through center1
                allItems.Add(weld1);
                allItems.Add(weld2);
                allItems.Add(weld3);

                IObject center1 = (IObject)Game.CreateObject(centerobj, Player.GetWorldPosition() + offset); //HINGE FOR ROTATING PART TO ATTACH TO, WELDED ONTO PLAYER
                center1.SetBodyType(BodyType.Dynamic);
                center1.SetMass(0.0001f);
                weld1.AddTargetObject(center1);
                allItems.Add(center1);

                IObjectPullJoint force = (IObjectPullJoint)Game.CreateObject("PullJoint", center1.GetWorldPosition() + new Vector2(0, 200));
                //force.SetLineVisual(LineVisual.DJRope);
                force.SetForcePerDistance(0.009f);
                allItems.Add(force);

                bird = Game.CreateObject(centerobj, center1.GetWorldPosition() + new Vector2(0, 200));
                force.SetTargetObject(bird);
                allItems.Add(bird);

                IObjectTargetObjectJoint target = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", center1.GetWorldPosition());
                target.SetTargetObject(center1);
                force.SetTargetObjectJoint(target);
                allItems.Add(target);

                IObject center2 = (IObject)Game.CreateObject(centerobj, Player.GetWorldPosition() + offset);
                center2.SetBodyType(BodyType.Dynamic);
                center2.SetMass(0.001f);
                weld2.AddTargetObject(center2);
                weld2.AddTargetObject(Player);
                allItems.Add(center2);

                IObjectRevoluteJoint revolute = (IObjectRevoluteJoint)Game.CreateObject("RevoluteJoint", Player.GetWorldPosition() + offset);
                revolute.SetTargetObjectA(center2);
                revolute.SetTargetObjectB(center1);
                revolute.SetMotorEnabled(true);
                revolute.SetMotorSpeed(0.7f);
                allItems.Add(revolute);

                //revolute.SetBodyType(BodyType.Dynamic);
                //revolute.SetMass(0.0001f);

                for (int i = 0; i < 4; i++)
                {
                    IObjectText obj = (IObjectText)Game.CreateObject("Text", center1.GetWorldPosition() + Vector2Helper.Rotated(new Vector2(-22, 2), (float)(Math.PI / 2) * i));
                    obj.SetTextColor(new Color(COLOR_R, COLOR_G, COLOR_B));
                    coloredPixels[i] = obj;
                    obj.SetTextScale(4f);
                    obj.SetText("(");
                    obj.CustomID = "(";
                    obj.SetAngle((float)(Math.PI / 2) * i);
                    obj.SetBodyType(BodyType.Dynamic);
                    obj.SetMass(0.000001f);
                    weld1.AddTargetObject(obj);
                    allItems.Add(obj);
                    effects[i] = obj;
                }

                for (int i = 0; i < 4; i++)
                {
                    IObjectText obj = (IObjectText)Game.CreateObject("Text", center1.GetWorldPosition() + Vector2Helper.Rotated(new Vector2(-22, 2), (float)(Math.PI / 2) * i));
                    obj.SetTextColor(Color.White);
                    obj.SetTextScale(4f);
                    obj.SetText("{");
                    obj.CustomID = "{";
                    obj.SetAngle((float)(Math.PI / 2) * i);
                    obj.SetBodyType(BodyType.Dynamic);
                    obj.SetMass(0.000001f);
                    weld1.AddTargetObject(obj);
                    allItems.Add(obj);
                    effects[i + 4] = obj;
                }




                CollisionFilter filter = new CollisionFilter();
                filter.ProjectileHit = true;
                filter.AbsorbProjectile = false;
                filter.BlockFire = true;


                IObject deflector = Game.CreateObject("InvisibleBlockNoCollision", Player.GetWorldPosition() + new Vector2(-17, 2.3f));
                deflector.CustomID = "deflector";
                deflector.SetBodyType(BodyType.Dynamic);
                deflector.SetCollisionFilter(filter);
                deflector.SetAngle((float)Math.PI / 4);
                deflector.SetSizeFactor(new Point(4, 4)); //setmass doesnt come into effect if called too early
                deflector.SetMass(0.000001f);
                weld2.AddTargetObject(deflector);
                allItems.Add(deflector);


                IObjectText shine = (IObjectText)Game.CreateObject("Text", new Vector2(-5, -1) + center1.GetWorldPosition());
                shine.SetTextColor(Color.White);
                shine.SetTextScale(3f);
                shine.SetText(",");
                shine.SetAngle((float)(Math.PI / 2f));
                shine.SetBodyType(BodyType.Dynamic);
                shine.SetMass(0.000001f);
                weld2.AddTargetObject(shine);
                allItems.Add(shine);

                IObjectText crack1 = (IObjectText)Game.CreateObject("Text", center1.GetWorldPosition() + new Vector2(-8.6f, 14.5f));
                crack1.SetTextColor(Color.White);
                crack1.SetTextScale(3f);
                crack1.SetText("");
                crack1.SetAngle(5.22f);
                crack1.SetBodyType(BodyType.Dynamic);
                crack1.SetMass(0.000001f);
                weld2.AddTargetObject(crack1);
                allItems.Add(crack1);

                IObjectText crack2 = (IObjectText)Game.CreateObject("Text", center1.GetWorldPosition() + new Vector2(12.8f, 6.9f));
                crack2.SetTextColor(Color.White);
                crack2.SetTextScale(3f);
                crack2.SetText("");
                crack2.SetAngle(4.45f);
                crack2.SetBodyType(BodyType.Dynamic);
                crack2.SetMass(0.000001f);
                weld2.AddTargetObject(crack2);
                allItems.Add(crack2);

                IObjectText crack3 = (IObjectText)Game.CreateObject("Text", center1.GetWorldPosition() + new Vector2(6f, -5.3f));
                crack3.SetTextColor(Color.White);
                crack3.SetTextScale(3f);
                crack3.SetText("");
                crack3.SetAngle(3.93f);
                crack3.SetBodyType(BodyType.Dynamic);
                crack3.SetMass(0.000001f);
                weld2.AddTargetObject(crack3);
                allItems.Add(crack3);


                //Player.SetCollisionFilter(filter);

                Events.ProjectileHitCallback onHit = null;
                onHit = Events.ProjectileHitCallback.Start((projectile, args) => {
                    if (Game.GetObject(args.HitObjectID).CustomID == "deflector")
                    { //remove getobject
                        Vector2 normal = Vector2Helper.Rotated(Vector2.Normalize(projectile.Position - center1.GetWorldPosition()), (float)Math.PI);


                        double angleDifference = Math.Abs(Vector2Helper.AngleTo(normal, projectile.Velocity));//Math.Abs(Vector2Helper.Angle(normal) - Vector2Helper.Angle(projectile.Velocity));

                        if (angleDifference < Math.PI / 2)
                        {

                            if (projectile.ProjectileItem == ProjectileItem.GRENADE_LAUNCHER || projectile.ProjectileItem == ProjectileItem.BAZOOKA || projectile.ProjectileItem == ProjectileItem.FLAKCANNON)
                            {
                                Game.TriggerExplosion(projectile.Position);
                                projectile.FlagForRemoval();
                                return;
                            }

                            Game.PlaySound("GrenadeBounce", projectile.Position);
                            Game.PlayEffect("S_P", projectile.Position);
                            projectile.Velocity = Vector2Helper.Bounce(projectile.Velocity, normal);
                            projectile.Position = projectile.Position + (normal * 2);

                            health -= (projectile.GetProperties().ObjectDamage * (float)(angleDifference / Math.PI)) + (projectile.GetProperties().ObjectDamage) / 3f;

                            if (health > 50 && health < 75)
                            {
                                crack1.SetText("X");
                                Game.PlaySound("ImpactGlass", crack1.GetWorldPosition(), 5f);
                            }
                            else if (health > 25 && health < 50)
                            {
                                crack2.SetText("X");
                                Game.PlaySound("ImpactGlass", crack2.GetWorldPosition(), 5f);
                            }
                            else if (health > 0 && health < 25)
                            {
                                crack3.SetText("X");
                                Game.PlaySound("ImpactGlass", crack3.GetWorldPosition(), 5f);

                            }
                            else if (health <= 0)
                            {
                                queueDisable = true;
                            }
                            //onHeadshot.Stop();
                            //return;
                        }

                    }
                });
                handlers[0] = onHit;



                Events.PlayerDamageCallback onDamage = null;
                onDamage = Events.PlayerDamageCallback.Start((IPlayer hitPlayer, PlayerDamageArgs args) => {
                    if (args.DamageType == PlayerDamageEventType.Fire)
                    {
                        preservedHealth = Player.GetModifiers().CurrentHealth;
                        if (preservedHealth > 0) return;
                    }
                    if (hitPlayer.UniqueID == Player.UniqueID)
                    {
                        PlayerModifiers modhp = Player.GetModifiers();
                        if (modhp.CurrentHealth == 0)
                            modhp.CurrentHealth = preservedHealth;
                        else
                            modhp.CurrentHealth = modhp.CurrentHealth + args.Damage; //THIS DOESNT BLOCK ALL DAMAGE
                        Player.SetModifiers(modhp);
                        queueDisable = true;
                    }
                });
                handlers[1] = onDamage;


                /*
                Events.ProjectileHitCallback onHit = null;
                onHit = Events.ProjectileHitCallback.Start((IProjectile projectile, PlayerDamageArgs args) => {
                    if (Game.GetObject(args.HitObjectID).CustomID == "deflector"){ //remove getobject
                        Vector2 normal = Vector2.Normalize(projectile.Position - center1.GetWorldPosition());

                        if (Math.Abs(Vector2Helper.Angle(normal) - Vector2Helper.Angle(projectile.Velocity)) % 6 > Math.PI/2){


                            Game.PlaySound("GrenadeBounce", projectile.Position);
                            Game.PlayEffect("S_P", projectile.Position);
                            projectile.Velocity = Vector2Helper.Bounce(projectile.Velocity, normal);
                            projectile.Position = projectile.Position + (normal * 2);

                            //onHeadshot.Stop();
                            //return;
                        }

                    }			
                });
                handlers[1] = */

                weld2.AddTargetObject(Player);

            }
            public void ToggleEffect()
            {
                for (int i = 0; i < effects.Length; i++)
                {

                    if (effects[i].GetText() == "")
                    {
                        effects[i].SetText(effects[i].CustomID);
                    }
                    else
                    {
                        effects[i].SetText("");
                    }

                }
            }

            public void TurnOnEffect()
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    effects[i].SetText(effects[i].CustomID); //setting to effect at the last sec
                }
            }

            public void TurnOffEffect()
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    effects[i].SetText(effects[i].CustomID); //setting to effect at the last sec
                }
            }

            /// <summary>
            /// Virtual method for updating the power-up.
            /// </summary>
            /// <param name="dlt">The time delta since the last update.</param>
            /// <param name="dltSecs">The time delta in seconds since the last
            /// update.</param>
            private bool delayUpdate = true;
            public override void Update(float dlt, float dltSecs)
            {

                if (queueDisable)
                {
                    if (!delayUpdate)
                    {
                        Enabled = false;
                        return;
                    }
                    delayUpdate = false;
                }

                if (Time > MAX_TIME - 1200 || Time < 2000)
                {
                    if ((Time < MAX_TIME - 1000 && Time > 2000))
                    {
                        TurnOnEffect(); //setting to effect at the last sec
                    }
                    else
                    {
                        ToggleEffect(); //blinking
                    }
                }


                if (Time % 50 == 0)
                {
                    if (shieldRandom.Next(0, 6) == 1)
                    {
                        Game.PlayEffect("GLM", randomPoint(RADIUS - 6) + Player.GetWorldPosition() + offset);
                    }
                }



                bird.SetWorldPosition(Player.GetWorldPosition() + new Vector2(0, 202));
            }

            /// <summary>
            /// Virtual method called when the power-up times out.
            /// </summary>
            public override void TimeOut()
            {

            }

            /// <summary>
            /// Virtual method called when the power-up is enabled or disabled. Called
            /// by the constructor and on timeout.
            /// </summary>
            public override void OnEnabled(bool enabled)
            {

                PlayerModifiers modify = Player.GetModifiers();
                modify.MeleeStunImmunity = 0;
                Player.SetModifiers(modify);

                if (!enabled)
                {
                    foreach (IObject obj in allItems)
                    {
                        obj.Remove();
                    }

                    for (int i = 0; i < handlers.Length; i++)
                    {
                        handlers[i].Stop();
                    }

                    if (Time > 0)
                    {
                        breakShield();
                    }
                }
            }
        }

            //==================================================================//
            //==================< DO NOT CHANGE ANYTHING BELOW >================//
            //==============< IF YOU'RE NOT SURE WHAT YOU'RE DOING >============//
            //==================================================================//


        //		   USE  >>>> /test <player>  <<<< TO TEST YOUR SUBMISSION
        /*
        private void OnUserMessage(UserMessageCallbackArgs args)
        {
            if (args.IsCommand && args.User.IsModerator)
            {
                if (args.Command == "TEST")
                {
                    string[] argsPieces = args.CommandArguments.ToLower().Split(' ');

                    IUser reciever;

                    if (argsPieces.Length > 0 && argsPieces[0].Length > 0)
                    {
                        reciever = GetUser(argsPieces[0]);
                    }
                    else
                    {
                        reciever = args.User;
                    }

                    if (reciever != null && reciever.GetPlayer() != null)
                    {
                        IPlayer ply = reciever.GetPlayer();

                        Activator.CreateInstance(GetPowerup(), ply);

                        Game.ShowChatMessage(string.Format("{0} recieved ability", ply.Name), new Color(34, 134, 34));
                    }
                }
            }
        }*/

        //==================================================================//
        //============================< HELPERS >===========================//
        //==================================================================//

        //              Use these to help you create your powerups

        /// <summary>
        /// Contains methods for creating various point shapes.
        /// </summary>
        public static class PointShape
        {
            /// <summary>
            /// Creates a trail of points between two points.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="start">The starting point of the trail.</param>
            /// <param name="end">The ending point of the trail.</param>
            /// <param name="pointDistance">The distance between each point on the
            /// trail.</param>
            public static void Trail(Action<Vector2> func, Vector2 start, Vector2 end,
                float pointDistance = 0.1f)
            {
                int count
                    = (int)Math.Ceiling(Vector2.Distance(start, end) / pointDistance);

                for (int i = 0; i < count; i++)
                {
                    Vector2 pos = Vector2.Lerp(start, end, (float)i / (count - 1));
                    func(pos);
                }
            }

            /// <summary>
            /// Creates a circle of points around a center point.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="centerPoint">The center point of the circle.</param>
            /// <param name="radius">The radius of the circle.</param>
            /// <param name="separationAngle">The angle between each point on the
            /// circle.</param>
            public static void Circle(Action<Vector2> func, Vector2 centerPoint,
                float radius, float separationAngle = 1)
            {
                int pointCount = (int)Math.Ceiling(360f / separationAngle);

                for (int i = 0; i < pointCount; i++)
                {
                    float angle = DegreesToRadians(i * separationAngle);
                    Vector2 pos
                        = new Vector2(centerPoint.X + radius * (float)Math.Cos(angle),
                            centerPoint.Y + radius * (float)Math.Sin(angle));
                    func(pos);
                }
            }

            /// <summary>
            /// Creates a square of points within a specified area.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="area">The area defining the square.</param>
            /// <param name="pointDistance">The distance between each point on the
            /// square.</param>
            public static void Square(
                Action<Vector2> func, Area area, float pointDistance = 0.1f)
            {
                Vector2[] vertices = new Vector2[]{ area.BottomLeft, area.BottomRight,
      area.TopRight, area.TopLeft };

                Polygon(func, vertices, pointDistance);
            }

            /// <summary>
            /// Creates a polygon of points using the provided vertices.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="points">The vertices of the polygon.</param>
            /// <param name="pointDistance">The distance between each point on the
            /// polygon.</param>
            public static void Polygon(
                Action<Vector2> func, Vector2[] points, float pointDistance = 0.1f)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Trail(func, points[i], points[i + 1], pointDistance);
                }

                Trail(func, points[points.Length - 1], points[0], pointDistance);
            }

            /// <summary>
            /// Creates a swirl of points around a center point.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="centerPoint">The center point of the swirl.</param>
            /// <param name="startRadius">The starting radius of the swirl.</param>
            /// <param name="endRadius">The ending radius of the swirl.</param>
            /// <param name="revolutions">The number of revolutions for the swirl.</param>
            /// <param name="pointsPerRevolution">The number of points per
            /// revolution.</param>
            public static void Swirl(Action<Vector2> func, Vector2 centerPoint,
                float startRadius, float endRadius, int revolutions = 1,
                int pointsPerRevolution = 360)
            {
                int totalPoints = revolutions * pointsPerRevolution;

                float angleIncrement = 360f / pointsPerRevolution;
                float radiusIncrement = (endRadius - startRadius) / totalPoints;

                for (int i = 0; i < totalPoints; i++)
                {
                    float angle = DegreesToRadians(i * angleIncrement);
                    float radius = startRadius + i * radiusIncrement;
                    Vector2 pos
                        = new Vector2(centerPoint.X + radius * (float)Math.Cos(angle),
                            centerPoint.Y + radius * (float)Math.Sin(angle));
                    func(pos);
                }
            }

            /// <summary>
            /// Creates a wave of points between two points.
            /// </summary>
            /// <param name="func">The action to perform on each point.</param>
            /// <param name="start">The starting point of the wave.</param>
            /// <param name="end">The ending point of the wave.</param>
            /// <param name="amplitude">The amplitude of the wave.</param>
            /// <param name="frequency">The frequency of the wave.</param>
            /// <param name="pointDistance">The distance between each point on the
            /// wave.</param>
            public static void Wave(Action<Vector2> func, Vector2 start, Vector2 end,
                float amplitude = 1, float frequency = 1, float pointDistance = 0.1f)
            {
                float totalDistance = Vector2.Distance(start, end);
                int count = (int)Math.Ceiling(totalDistance / pointDistance);
                float adjustedFrequency = frequency * (totalDistance / count);

                for (int i = 0; i < count; i++)
                {
                    Vector2 pos = Vector2.Lerp(start, end, (float)i / (count - 1));
                    float offsetY = amplitude * ((float)Math.Sin(adjustedFrequency * pos.X));
                    func(pos + new Vector2(0, offsetY));
                }
            }

            /// <summary>
            /// Generates a random Vector2 point inside the specified Area.
            /// </summary>
            /// <param name="func">Function to be called with the generated random Vector2
            /// point.</param> <param name="area">The Area in which to generate the random
            /// point.</param> <param name="random">A Random instance for generating
            /// random numbers.</param> <returns>The generated random Vector2
            /// point.</returns>
            public static Vector2 Random(Action<Vector2> func, Area area, Random random)
            {
                // Generate random coordinates within the bounds of the area
                float randomX = (float)random.NextDouble() * area.Width + area.Left;
                float randomY = (float)random.NextDouble() * area.Height + area.Bottom;

                Vector2 randomV = new Vector2(randomX, randomY);

                // Return the random point as a tuple
                func(randomV);

                return randomV;
            }

            private static float DegreesToRadians(float degrees)
            {
                return degrees * MathHelper.PI / 180f;
            }
        }

        /// <summary>
        /// A helper class for performing various operations on Vector2 objects.
        /// </summary>
        public static class Vector2Helper
        {
            private static readonly Vector2 _up = new Vector2(0, 1);
            private static readonly Vector2 _down = new Vector2(0, -1);
            private static readonly Vector2 _right = new Vector2(1, 0);
            private static readonly Vector2 _left = new Vector2(-1, 0);

            /// <summary>
            /// Gets the Vector2 representing upward direction.
            /// </summary>
            public static Vector2 Up
            {
                get { return _up; }
            }

            /// <summary>
            /// Gets the Vector2 representing downward direction.
            /// </summary>
            public static Vector2 Down
            {
                get { return _down; }
            }

            /// <summary>
            /// Gets the Vector2 representing rightward direction.
            /// </summary>
            public static Vector2 Right
            {
                get { return _right; }
            }

            /// <summary>
            /// Gets the Vector2 representing leftward direction.
            /// </summary>
            public static Vector2 Left
            {
                get { return _left; }
            }

            /// <summary>
            /// Returns the absolute value of each component of the specified vector.
            /// </summary>
            public static Vector2 Abs(Vector2 v)
            {
                return new Vector2(Math.Abs(v.X), Math.Abs(v.Y));
            }

            /// <summary>
            /// Returns the angle (in radians) of the specified vector.
            /// </summary>
            public static float Angle(Vector2 v) { return (float)Math.Atan2(v.Y, v.X); }

            /// <summary>
            /// Returns the angle (in radians) between two vectors.
            /// </summary>
            public static float AngleTo(Vector2 v, Vector2 to)
            {
                return (float)Math.Atan2(Cross(v, to), Vector2.Dot(to, v));
            }

            /// <summary>
            /// Returns the angle (in radians) from one vector to another point.
            /// </summary>
            public static float AngleToPoint(Vector2 v, Vector2 to)
            {
                return (float)Math.Atan2(to.Y - v.Y, to.X - v.X);
            }

            /// <summary>
            /// Returns the aspect ratio of the specified vector (X / Y).
            /// </summary>
            public static float Aspect(Vector2 v) { return v.X / v.Y; }

            /// <summary>
            /// Reflects a vector off the specified normal vector.
            /// </summary>
            public static Vector2 Bounce(Vector2 v, Vector2 normal)
            {
                return -Reflect(v, normal);
            }

            /// <summary>
            /// Returns the ceiling of each component of the specified vector.
            /// </summary>
            public static Vector2 Ceiling(Vector2 v)
            {
                return new Vector2((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y));
            }

            /// <summary>
            /// Restricts each component of the specified vector to the specified range.
            /// </summary>
            public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
            {
                return new Vector2(MathHelper.Clamp(v.X, min.X, max.X),
                    MathHelper.Clamp(v.Y, min.Y, min.Y));
            }

            /// <summary>
            /// Calculates the cross product of two vectors.
            /// </summary>
            public static float Cross(Vector2 v, Vector2 with)
            {
                return (v.X * with.Y) - (v.Y * with.X);
            }

            /// <summary>
            /// Returns a unit vector pointing from one vector to another.
            /// </summary>
            public static Vector2 DirectionTo(Vector2 v, Vector2 to)
            {
                return Vector2.Normalize(new Vector2(to.X - v.X, to.Y - v.Y));
            }

            /// <summary>
            /// Returns the floor of each component of the specified vector.
            /// </summary>
            public static Vector2 Floor(Vector2 v)
            {
                return new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));
            }

            /// <summary>
            /// Returns the inverse of each component of the specified vector.
            /// </summary>
            public static Vector2 Inverse(Vector2 v)
            {
                return new Vector2(1 / v.X, 1 / v.Y);
            }

            /// <summary>
            /// Determines whether the specified vector is normalized.
            /// </summary>
            public static bool IsNormalized(Vector2 v)
            {
                return Math.Abs(v.LengthSquared() - 1) < float.Epsilon;
            }

            /// <summary>
            /// Restricts the length of the specified vector to a maximum value.
            /// </summary>
            public static Vector2 LimitLength(Vector2 v, float length = 1)
            {
                float l = v.Length();

                if (l > 0 && length < l)
                {
                    v /= l;
                    v *= length;
                }

                return v;
            }

            /// <summary>
            /// Moves a vector towards a target vector by a specified delta.
            /// </summary>
            public static Vector2 MoveToward(Vector2 v, Vector2 to, float delta)
            {
                Vector2 vd = to - v;
                float len = vd.Length();

                if (len <= delta || len < float.Epsilon)
                    return to;

                return v + (vd / len * delta);
            }

            /// <summary>
            /// Projects a vector onto a specified normal vector.
            /// </summary>
            public static Vector2 Project(Vector2 v, Vector2 onNormal)
            {
                return onNormal * (Vector2.Dot(onNormal, v) / onNormal.LengthSquared());
            }

            /// <summary>
            /// Reflects a vector off the specified normal vector.
            /// </summary>
            public static Vector2 Reflect(Vector2 v, Vector2 normal)
            {
                normal.Normalize();

                return 2 * normal * Vector2.Dot(normal, v) - v;
            }

            /// <summary>
            /// Rotates the specified vector by the specified angle (in radians).
            /// </summary>
            public static Vector2 Rotated(Vector2 v, float angle)
            {
                float sin = (float)Math.Sin(angle);
                float cos = (float)Math.Cos(angle);

                return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
            }

            /// <summary>
            /// Returns the rounded value of each component of the specified vector.
            /// </summary>
            public static Vector2 Round(Vector2 v)
            {
                return new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
            }

            /// <summary>
            /// Returns a vector with the sign of each component of the specified vector.
            /// </summary>
            public static Vector2 Sign(Vector2 v)
            {
                v.X = Math.Sign(v.X);
                v.Y = Math.Sign(v.Y);

                return v;
            }

            /// <summary>
            /// Slides a vector along the specified normal vector.
            /// </summary>
            public static Vector2 Slide(Vector2 v, Vector2 normal)
            {
                return v - (normal * Vector2.Dot(normal, v));
            }

            /// <summary>
            /// Returns an orthogonal vector to the specified vector.
            /// </summary>
            public static Vector2 Orthogonal(Vector2 v) { return new Vector2(v.Y, -v.X); }

            /// <summary>
            /// Returns a unit vector rotated by the specified angle (in radians).
            /// </summary>
            public static Vector2 FromAngle(Vector2 v, float angle)
            {
                float sin = (float)Math.Sin(angle);
                float cos = (float)Math.Cos(angle);

                return new Vector2(cos, sin);
            }
        }

        public IUser GetUser(string arg)
        {
            return Game.GetActiveUsers().FirstOrDefault(u => u.AccountName == arg
                    || u.Name == arg
                    || (arg.All(char.IsDigit) ? u.GameSlotIndex == int.Parse(arg)
                                              : false));
        }

        public static Type GetPowerup()
        {

            Type[] nestedPowerups = { typeof(ManaShield)};

            Type[] instantiableTypes
                = nestedPowerups
                      .Where(t =>
                          // t.BaseType == typeof(Powerup) &&
                          t.GetConstructors().Any(c => c.GetParameters().Length == 1
                                  && c.GetParameters()[0].ParameterType
                                      == typeof(IPlayer)))
                      .ToArray();

            if (instantiableTypes.Length == 0)
                throw new InvalidOperationException("No instantiable types found.");

            return instantiableTypes[0];
        }

        //=================================================================//
        //==========================< BASE CLASS >=========================//
        //=================================================================//

        /// <summary>
        /// Represents a base power-up that can be activated and updated over time.
        /// </summary>
        public abstract class Powerup
        {
            // Interval for the main update callback event
            private const uint COOLDOWN = 0;

            public abstract string Name { get; }

            public abstract string Author { get; }

            // Main update callback event
            private Events.UpdateCallback _updateCallback = null;

            // Time left for the power-up to be active
            public float Time = 1000;

            // used for calculating delta time
            private float lastUpdate;

            // The player associated with this power-up
            public IPlayer Player;

            /// <summary>
            /// Gets or sets whether the power-up is enabled.
            /// </summary>
            public bool Enabled
            {
                get { return _updateCallback != null; }
                set
                {
                    if (value != Enabled)
                    {
                        if (value)
                        {
                            _updateCallback = Events.UpdateCallback.Start(Update, COOLDOWN);

                            OnEnabled(true);
                        }
                        else
                        {
                            _updateCallback.Stop();
                            _updateCallback = null;

                            OnEnabled(false);
                        }
                    }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Powerup"/> class.
            /// </summary>
            /// <param name="player">The player associated with this power-up.</param>
            public Powerup(IPlayer player)
            {
                Player = player;
                lastUpdate = Game.TotalElapsedGameTime;
                Enabled = true;
                Activate();
            }

            /// <summary>
            /// Updates the power-up with the specified time delta.
            /// </summary>
            /// <param name="dlt">The time delta since the last update.</param>
            private void Update(float dlt)
            {
                dlt = Game.TotalElapsedGameTime - lastUpdate;
                lastUpdate = Game.TotalElapsedGameTime;

                // Check if the player is still valid
                if (Player == null)
                {
                    Enabled = false;

                    return;
                }

                // Check if the player is dead or removed
                if (Player.IsDead || Player.IsRemoved)
                {
                    Enabled = false;

                    return;
                }

                // Check if the power-up has timed out
                if (Time <= 0)
                {
                    TimeOut();

                    Enabled = false;

                    return;
                }

                // Update the time left for the power-up
                Time -= dlt;

                // Invoke the virtual Update method
                Update(dlt, dlt / 1000);
            }

            /// <summary>
            /// Virtual method for actions upon activating the power-up.
            /// </summary>
            protected abstract void Activate();

            /// <summary>
            /// Virtual method for updating the power-up.
            /// </summary>
            /// <param name="dlt">The time delta since the last update.</param>
            /// <param name="dltSecs">The time delta in seconds since the last
            /// update.</param>
            public virtual void Update(float dlt, float dltSecs)
            {
                // Implement in derived classes
            }

            /// <summary>
            /// Virtual method called when the power-up times out.
            /// </summary>
            public virtual void TimeOut()
            {
                // Implement in derived classes
            }

            /// <summary>
            /// Virtual method called when the power-up is enabled or disabled. Called by
            /// the constructor.
            /// </summary>
            public virtual void OnEnabled(bool enabled)
            {
                // Implement in derived classes
            }
        }
        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
