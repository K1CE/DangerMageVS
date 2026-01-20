using System;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellMetal : Spell
		{
			private static Element element = Element.METAL;

			public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
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
					float damage = (effectivePower / 8f);
					prj.DamageDealtModifier = damage;

					
				}
				Game.PlaySound("ImpactMetal", sender.position, 1f);
				particleExplosion("S_P", sender.position, 3, 8f);
            }
            public override void explode(Cast sender, IObject alreadyHit, Vector2 position) {
				float bullets = splash;
				for (int i = 0; i < bullets; i++) {
					double rotation = rnd.NextDouble() * Math.PI * 2;
					Vector2 vector = new Vector2((float)Math.Cos(rotation) / 10f, (float)Math.Sin(rotation) / 10f);

                    IProjectile shrapnel = Game.SpawnProjectile(ProjectileItem.PISTOL, position + vector * 15f, vector);
					shrapnel.DamageDealtModifier = 0.3f;
					shrapnel.CritChanceDealtModifier = 0f;
					//shrapnel.Velocity = vector;
                }
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



			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				((CastProjectile)cast).attach(Game.CreateObject("Pulley00", position));
			}
			protected override void setUpStats()
			{
				spellPower = 16f;
				cooldown = 3700;
				speed = 7.5f;
				range = 2f;
				splash = 18;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
