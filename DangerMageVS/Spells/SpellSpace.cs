using SFDGameScriptInterface;
using System;


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
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                if (target != null)
				{
					Vector2 pos = sender.position;

					IProjectile prj = Game.SpawnProjectile(ProjectileItem.PISTOL, target.GetWorldPosition() - vector, vector);
					prj.CritChanceDealtModifier = 100f;
					float damage = (effectivePower / 8);
					prj.DamageDealtModifier = damage;
				}

				Game.PlaySound("ImpactMetal", sender.position, 1f);
				particleExplosion("S_P", sender.position, 3, 8f);
			}

			//make it switch objects of similar mass around
			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				/*
				Vector2 pos = sender.position;
				RayCastInput input = new RayCastInput(true);
				input.ProjectileHit = RayCastFilterMode.True;
				input.IncludeOverlap = true;
				RayCastResult outPut = Game.RayCast(pos, pos + vector, input)[0];
				if (outPut.Hit && Vector2.Distance(outPut.Position, pos) < 2f){*/

				//}
			}

			private const int MAX_PARTICLES = 16;
			private const int ARM_LENGTH = 4;
            protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject darkBall = Game.CreateObject("InvisibleBlockNoCollision", position);
				IObject anchor = Game.CreateObject("InvisibleBlockNoCollision", position);
				anchor.SetBodyType(BodyType.Dynamic);
                IObjectWeldJoint weldJoint = (IObjectWeldJoint)Game.CreateObject("WeldJoint", position);
                IObjectRevoluteJoint revolute = (IObjectRevoluteJoint)Game.CreateObject("RevoluteJoint", position);
                revolute.SetTargetObjectA(darkBall);
                revolute.SetTargetObjectB(anchor);
                revolute.SetMotorEnabled(true);
				revolute.SetMotorSpeed(-3f);
				weldJoint.AddTargetObject(anchor);

                IObject anchor2 = Game.CreateObject("InvisibleBlockNoCollision", position + new Vector2(0, 1000));
                IObjectTargetObjectJoint target = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", position);
				target.SetTargetObject(anchor2);
                //int halfWay = MAX_PARTICLES / 2;
				//float offset = (1f / halfWay) / 2f;
				for(int p = 0; p < MAX_PARTICLES; p++)
                {
                    IObjectText particle = (IObjectText)Game.CreateObject("Text", position);
					/*
					IObjectPullJoint antiGravity = (IObjectPullJoint)Game.CreateObject("PullJoint", position);
					antiGravity.SetForcePerDistance(0);
					antiGravity.SetForce(0.001f);
					antiGravity.SetTargetObject(particle);
					antiGravity.SetTargetObjectJoint(target);*/

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
				((CastProjectile)cast).attach(darkBall);
			}

			protected override void setUpStats()
			{
				spellPower = 18f;
				cooldown = 3700;
				speed = 1f; //used to be 2
				range = 4f;
			}
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
