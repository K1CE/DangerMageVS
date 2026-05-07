using SFDGameScriptInterface;
using System;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellPlant : Spell
        {
            public override Element element { get { return Element.PLANT; } }

            public SpellPlant(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellPlant(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;

				for (int i = 0; i < splash / 2f; i++)
				{
					for(int j = 0; j < 5; j++)
					{
						Vector2 throwVec = new Vector2((float)Math.Cos(rnd.NextDouble() * Math.PI*2) * splash, 
							(float)Math.Sin(rnd.NextDouble() * Math.PI * 2) * splash);
						if( tieObject(target, throwVec, effectivePower)) break;
						
					}
				}


            }

			public override void explode(Cast sender, IObject alreadyHit, Vector2 position) 
            {
				base.explode(sender, alreadyHit, position);


            }

			protected override void setUpStats()
			{
				spellPower = 13f;
				cooldown = 3000;
				speed = 6.7f;
				range = 0.6f;
				splash = 25;
				particleEffect = elementEffects[(int)element];
			}

			public static IObjectWeldJoint frozen;
			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				//TODO: tie objects together
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject leaf = Game.CreateObject("ItemDebrisFlamethrower00", position);
				((CastProjectile)cast).attach(leaf);
			}

			private void createTimedTether(IObject target, Vector2 tieTo, IObject anchor, float power)
			{
				IObjectPullJoint tether = (IObjectPullJoint)Game.CreateObject("PullJoint", target.GetWorldPosition());
				IObjectTargetObjectJoint targetJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", tieTo);

				targetJoint.SetTargetObject(anchor);

				tether.SetTargetObject(target);
				tether.SetTargetObjectJoint(targetJoint);

				tether.SetLineVisual(LineVisual.DJVine);
				tether.SetForcePerDistance(1f + power/17f);


                Events.UpdateCallback despawn = null;
                despawn = Events.UpdateCallback.Start(e => {

					//Game.PlayEffect(elementEffects[(int)element], tether.GetWorldPosition() + (targetJoint.GetWorldPosition() - tether.GetWorldPosition())/2f);

					tether.Remove();
					targetJoint.Remove();

                    despawn.Stop();
                }, (uint)(100 * power));
            }
			private bool tieObject(IObject target, Vector2 shootAt, float power)
			{
				RayCastInput input = new RayCastInput();
				input.AbsorbProjectile = RayCastFilterMode.True;
				input.ClosestHitOnly = true;
				input.IncludeOverlap = false;
				RayCastResult[] results = Game.RayCast(target.GetWorldPosition(), shootAt, input);
				//messageRoss("made it here!!!");
				if (results.Length > 0 && results[0].Hit)
                {
                    createTimedTether(target, results[0].Position, results[0].HitObject, power);
					return true;
				}

				return false;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
