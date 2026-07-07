using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellMetal : Spell
        {
            public override Element element { get { return Element.METAL; } }

            public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

            }
			//TODO: make it shoot twice
			//TODO: fix blob falling apart
			//TODO: deflect with melee
			//TODO: add more compatible metal
			private bool hitPlayer = false;
            public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                if (target != null)
				{
					if (target is IPlayer)
					{
						hitPlayer = true;
                        Game.PlaySound("MeleeHitSharp", sender.position, 1f);
                    }
					Vector2 pos = sender.position;
					IProjectile prj = Game.SpawnProjectile(ProjectileItem.PISTOL, target.GetWorldPosition() - vector, vector);
					prj.CritChanceDealtModifier = 100f;
					float damage = (effectivePower / 8f);
					prj.DamageDealtModifier = damage;

					
				}
				Game.PlaySound("ImpactMetal", sender.position, 1f);
				particleExplosion("S_P", sender.position, 3, 8f);
            }
			public const float PISTOL_DAMAGE = 3.33f;
			private const float MAGNET_DISTANCE = 23f;
			private const float CHUNK_CLOSENESS = 2.4f;
            public override void explode(Cast sender, IObject alreadyHit, Vector2 position) {
				if (hitPlayer) return;
				bool fat = spellPower > 6 || splash > 12;
				List<IObject> chunks = new List<IObject>();

                if (sawblade != null) {
					sawblade.Destroy();
				}



                Game.PlaySound("MeleeBlockMetal", position, 1f);
                if (fat)
				{
					float rotation = (float)(rnd.NextDouble() * Math.PI * 2);
					for(int i = 0; i < 3; i++)
					{
						float localRotation = (float)(rotation + (i * Math.PI * 2 / 3));
						float facingRotation = (float)(Math.PI / 2 + localRotation);
						Vector2 relativePos = new Vector2((float)Math.Cos(localRotation) * CHUNK_CLOSENESS, (float)Math.Sin(localRotation) * CHUNK_CLOSENESS);
						chunks.Add(Game.CreateObject("MetalDebris00" + (char)(rnd.Next(3) + 65), relativePos + position, facingRotation));
						chunks[i].SetBodyType(BodyType.Static);
                        chunks[i].CustomID = "magnetized";
                    }
				} else
				{
                    chunks[0] = Game.CreateObject("MetalDebris00A", position, (float)(rnd.NextDouble() * Math.PI * 2));
                    chunks[0].SetBodyType(BodyType.Static);
                    chunks[0].CustomID = "magnetized";
                }

				//Game.PlayEffect("ACS", -1 * MAGNET_DISTANCE * Vector2.One + position);
                //Game.PlayEffect("ACS", MAGNET_DISTANCE * Vector2.One + position);

                foreach (IObject metal in Game.GetObjects<IObject>(new Area(-1 * MAGNET_DISTANCE * Vector2.One + position, MAGNET_DISTANCE * Vector2.One + position))){
					//messageRoss(metal.Name);
					if (!isScrap(metal)) continue;
                    float rotation = (float)(rnd.NextDouble() * Math.PI * 2);

                    chunks.Add(metal);
                    Vector2 relativePos = new Vector2((float)Math.Cos(rotation) * (CHUNK_CLOSENESS + 4f) , (float)Math.Sin(rotation) * (CHUNK_CLOSENESS + 4f));
                    float facingRotation = (float)(Math.PI / 2 + rotation);
                    metal.SetWorldPosition(relativePos + position);
                    metal.SetBodyType(BodyType.Static);
					metal.SetAngle(facingRotation);
					metal.ClearFire();
                }


					Events.UpdateCallback delay = null;
                delay = Events.UpdateCallback.Start(e => {
					bool spellBroke = false;
					if (chunks.Count > 3){
						splash *= (chunks.Count - 3);
						spellPower *= 2f;
					}
					foreach(IObject chunk in chunks)
					{
						if(chunk == null || chunk.RemovalInitiated || chunk.IsRemoved){
							spellBroke = true;
						}
						if (spellBroke) chunk.SetBodyType(BodyType.Dynamic);
						else chunk.Destroy();
						//messageRoss(spellBroke? "spellBroke" : "not spellBroke");
					}
					if (spellBroke){
						//messageRoss("stopping blast");
						delay.Stop();
						return;
					}


                    float bullets = splash * 1.5f;
                    for (int i = 0; i < bullets; i++)
                    {
                        double rotation = rnd.NextDouble() * Math.PI * 2;
                        Vector2 vector = new Vector2((float)Math.Cos(rotation) / 10f, (float)Math.Sin(rotation) / 10f);
                        vector.Normalize();
                        IProjectile shrapnel = Game.SpawnProjectile(ProjectileItem.PISTOL, position + vector * 10f, vector);
                        shrapnel.DamageDealtModifier = (spellPower / 10f) / PISTOL_DAMAGE;
                        shrapnel.CritChanceDealtModifier = 0f;
						shrapnel.Velocity = shrapnel.Velocity * ((float)rnd.NextDouble() * 0.5f + 0.5f);
						if(i > 30 && rnd.NextDouble() < 0.1f) shrapnel.PowerupBounceActive = true;
                        //shrapnel.Velocity = vector;
                    }

                    delay.Stop();
                }, 1300);
                
            }

            public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				/*
				Vector2 pos = sender.position;
				RayCastInput input = new RayCastInput(true);
				input.ProjectileHit = RayCastFilterMode.True;
				input.IncludeOverlap = true;
				RayCastResult outPut = Game.RayCast(pos, pos + vector, input)[0];
				if (outPut.Hit && Vector2.Distance(outPut.Position, pos) < 2f){*/
				if (target.GetCollisionFilter().AbsorbProjectile && Math.Abs(target.GetWorldPosition().Y - (sender.position.Y - 1f)) < 5f)
					cast.hit(target);
				//}

				if (isScrap(target) && target.CustomID != "magnetized2"){
                    IObjectPullJoint pullJoint = (IObjectPullJoint)Game.CreateObject("pullJoint", target.GetWorldPosition());
                    ((CastProjectile)cast).attach(pullJoint);
                    //pullJoint.SetLineVisual(LineVisual.DJSteelWire);
                    pullJoint.SetTargetObject(target);
					pullJoint.SetTargetObjectJoint(targetJoint);
					pullJoint.SetForce(0.3f);
                    cast.addForCleanup(pullJoint);

					target.SetMass(0.05f);
					target.CustomID = "magnetized2";

					//cast.addForCleanup(target);
				}
			}


			private IObject sawblade;
			private IObjectTargetObjectJoint targetJoint;
			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);

				sawblade = Game.CreateObject("Pulley00", position);
                ((CastProjectile)cast).attach(sawblade);
                Game.PlaySound("Sawblade", position, 1f);
				cast.addForCleanup(sawblade);

                targetJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", position + new Vector2(0, 16));
                targetJoint.SetTargetObject(sawblade);
                cast.addForCleanup(targetJoint);


            }

			private bool isScrap(IObject obj)
			{
				//if (obj.CustomID == "magnetized") messageRoss("magnetized");
				string name = obj.Name;
                    return obj.CustomID != "magnetized" &&
					(name.Contains("MetalDebris")) 
					|| name == "WoodBarrelDebris00A"
					|| name.Contains("ItemDebris")
					|| name == "StreetsweeperCratePart"
					|| name == "CrabCan00_D"
					|| name == "Cage00_D";
			}
            protected override void setUpStats()
			{
				spellPower = 12f;
				cooldown = 3000;
				speed = 7.5f;
				range = 0.75f;
				splash = 24;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
