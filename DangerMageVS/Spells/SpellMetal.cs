using System;
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
			//TODO make it shoot twice
			//TODO: add impact particle effects
			//TODO: add impact sounds
			//TODO: make explosion only trigger on miss
			//TODO: delayed shrapnel explosion with cool effect and sounds
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
			private const float chunkCloseness = 2.4f;
            public override void explode(Cast sender, IObject alreadyHit, Vector2 position) {
				if (hitPlayer) return;
				bool fat = spellPower > 6 || splash > 12;
				IObject[] chunks;
				if (sawblade != null) {
					sawblade.Destroy();
				}

                Game.PlaySound("MeleeBlockMetal", position, 1f);
                if (fat)
				{
					chunks = new IObject[3];
					float rotation = (float)(rnd.NextDouble() * Math.PI * 2);
					for(int i = 0; i < 3; i++)
					{
						float localRotation = (float)(rotation + (i * Math.PI * 2 / 3));
						float facingRotation = (float)(Math.PI / 2 + localRotation);
						Vector2 relativePos = new Vector2((float)Math.Cos(localRotation) * chunkCloseness, (float)Math.Sin(localRotation) * chunkCloseness);
						chunks[i] = Game.CreateObject("MetalDebris00" + (char)(rnd.Next(3) + 65), relativePos + position, facingRotation);
						chunks[i].SetBodyType(BodyType.Static);
					}
				} else
				{
                    chunks = new IObject[1];
                    chunks[0] = Game.CreateObject("MetalDebris00A", position, (float)(rnd.NextDouble() * Math.PI * 2));
                    chunks[0].SetBodyType(BodyType.Static);
                }


					Events.UpdateCallback delay = null;
                delay = Events.UpdateCallback.Start(e => {
					bool spellBroke = false;
					foreach(IObject chunk in chunks)
					{
						if(chunk == null || chunk.DestructionInitiated || chunk.IsRemoved){
							spellBroke = true;
						}
						if (spellBroke) chunk.SetBodyType(BodyType.Dynamic);
						else chunk.Destroy();
						messageRoss(spellBroke? "spellBroke" : "not spellBroke");
					}
					if (spellBroke){
						messageRoss("stopping blast");
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
			}


			private IObject sawblade;
			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				sawblade = Game.CreateObject("Pulley00", position);
                ((CastProjectile)cast).attach(sawblade);
                Game.PlaySound("Sawblade", position, 1f);
            }
            protected override void setUpStats()
			{
				spellPower = 12f;
				cooldown = 3000;
				speed = 7.5f;
				range = 0.9f;
				splash = 24;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
