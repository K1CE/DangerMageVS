using SFDGameScriptInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellSpace : Spell
		{
			private static Element element = Element.SPACE;

			public SpellSpace(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellSpace(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod * (1 / Game.SlowmotionModifier);

				if (Game.SlowmotionModifier < 1)
				{
					Game.PlaySound("Madness", target.GetWorldPosition());
					Game.PlayEffect("Electric", target.GetWorldPosition());
				}

                if (target != null)
				{
					target.DealDamage(effectivePower);
					if (target.GetBodyType() == BodyType.Dynamic)
					{

						if(target is IPlayer)
						{
							IPlayer vic = (IPlayer)target;
                            foreach (int i in Enumerable.Range(0, 5).OrderBy(x => rnd.Next()))
                            {
								WeaponItemType slot = convertIndexToSlot(vic, i);

								if (slot != WeaponItemType.NONE) {
									vic.Disarm(slot);
									break;
								}
                            }
                            vic.SetFaceDirection(-((IPlayer)target).FacingDirection);
							if (vic.IsDead) target.Remove();
						}

						Vector2 displaceTo = Vector2.Zero;
						double rDistance;
						double rAngle;
						RayCastInput rayIn = new RayCastInput(true);
						rayIn.AbsorbProjectile = RayCastFilterMode.True;
						rayIn.BlockFire = RayCastFilterMode.True;
						rayIn.IncludeOverlap = true;
						for (int v = 0; v < 30; v++)
						{
							rDistance = rnd.NextDouble() * effectivePower * 5f;
							rAngle = rnd.NextDouble() * 2 * Math.PI;
							displaceTo = target.GetWorldPosition() + new Vector2((float)(rDistance * Math.Cos(rAngle)), (float)(rDistance * Math.Sin(rAngle)));

							if (!Game.RayCast(displaceTo, displaceTo + new Vector2(0, 2f), rayIn)[0].Hit) break;
							else displaceTo = Vector2.Zero;
						}
						if (!displaceTo.Equals(Vector2.Zero))
						{
							target.SetWorldPosition(displaceTo);
                           // particleExplosion(elementEffects[(int)Element.SPACE], displaceTo, 1, 7);
                        }
					}
					
				}

				Game.PlaySound("GetSlomo", sender.position, 1f);
				particleExplosion(elementEffects[(int)Element.SPACE], sender.position, 3, splash);
			}

			//make it switch objects of similar mass around
			IObject cachedObject;
			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				if(cachedObject != target)
				{
					if (cachedObject == null || cachedObject.IsRemoved) 
					{
						cachedObject = target;
						return;
					}

					if (Math.Abs(cachedObject.GetMass() - target.GetMass()) < 0.05)
					{
						Vector2 tempV = cachedObject.GetWorldPosition();
						cachedObject.SetWorldPosition(target.GetWorldPosition());
						target.SetWorldPosition(tempV);
						cachedObject = null;
					}
				}
			}

			private const int MAX_PARTICLES = 16;
			private const int ARM_LENGTH = 4;


            protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject darkBall = Game.CreateObject("InvisibleBlockNoCollision", position);
				cast.addForCleanup(darkBall);
				IObject anchor = Game.CreateObject("InvisibleBlockNoCollision", position);
                cast.addForCleanup(anchor);
                anchor.SetBodyType(BodyType.Dynamic);
                IObjectWeldJoint weldJoint = (IObjectWeldJoint)Game.CreateObject("WeldJoint", position);
                cast.addForCleanup(weldJoint);
                IObjectRevoluteJoint revolute = (IObjectRevoluteJoint)Game.CreateObject("RevoluteJoint", position);
                cast.addForCleanup(revolute);
                revolute.SetTargetObjectA(darkBall);
                revolute.SetTargetObjectB(anchor);
                revolute.SetMotorEnabled(true);
				revolute.SetMotorSpeed(-3f);
				weldJoint.AddTargetObject(anchor);

                IObject anchor2 = Game.CreateObject("InvisibleBlockNoCollision", position + new Vector2(0, 1000));
                cast.addForCleanup(anchor2);
                IObjectTargetObjectJoint target = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", anchor2.GetWorldPosition());
                cast.addForCleanup(target);
                target.SetTargetObject(anchor2);
                //int halfWay = MAX_PARTICLES / 2;
				//float offset = (1f / halfWay) / 2f;
				for(int p = 0; p < MAX_PARTICLES; p++)
                {
                    IObjectText particle = (IObjectText)Game.CreateObject("Text", position);
                    cast.addForCleanup(particle);

                    IObjectPullJoint antiGravity = (IObjectPullJoint)Game.CreateObject("PullJoint", position);
                    cast.addForCleanup(antiGravity);
                    antiGravity.SetForcePerDistance(0);
					antiGravity.SetForce(0.000006f);
					antiGravity.SetTargetObject(particle);
					//antiGravity.SetLineVisual(LineVisual.DJRope);
					antiGravity.SetTargetObjectJoint(target);

					int localIndex = p % ARM_LENGTH;
                    particle.SetTextScale((MAX_SPACE_PARTICLE_SIZE) * (((float)localIndex + 1) / ARM_LENGTH));
                    particle.SetAngle((float)((2 * Math.PI) * ((float)p / MAX_PARTICLES)));
					/*
                    particle.SetAngle((float)(
						(2 * Math.PI) / (MAX_PARTICLES / ARM_LENGTH) * (localIndex / ARM_LENGTH) 
						+ ((2 * Math.PI) / (MAX_PARTICLES / ARM_LENGTH)) * (p / ARM_LENGTH)
						));
					*/


					/*
                    messageRoss("angles: " + ((2 * Math.PI) * ((float)p / halfWay)));

                    for (int a = 0; a < ARM_LENGTH; )
					if(p < halfWay)
                    {
                        particle.SetTextScale(4f);
						particle.SetAngle((float)((2 * Math.PI) * ((float)p /halfWay)));
						messageRoss("angles: " + ((2 * Math.PI) * ((float)p / halfWay)));
                    }
					else if(p >= halfWay)
					{
                        particle.SetTextScale(2f);
                        particle.SetAngle((float)((2 * Math.PI) * ((float)p / halfWay)) + offset);
                    }
					*/

                    particle.SetText(".");
                    particle.SetBodyType(BodyType.Dynamic);
                    weldJoint.AddTargetObject(particle);
                    spaceParticleQueue.Add(particle);

                }

				//darkBall.SetColor1("Black");
				speedUpLoop();
                ((CastProjectile)cast).attach(darkBall);
			}

			private void speedUpLoop()
			{
                Events.UpdateCallback delay = null;
				int speedUps = 0;
                delay = Events.UpdateCallback.Start(e => {
					speed = speed + speed * 0.18f;
					speedUps++;
					if (speedUps > 50) delay.Stop();
                }, 100);
            }

			protected override void setUpStats()
			{
				spellPower = 18f;
				cooldown = 3700;
				speed = 1f; //used to be 2
				range = 4f;
				splash = 25;
			}
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
