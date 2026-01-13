using SFDGameScriptInterface;


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


			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject darkBall = Game.CreateObject("BgValve00D", position);
				darkBall.SetColor1("Black");
				((CastProjectile)cast).attach(darkBall);
			}

			protected override void setUpStats()
			{
				spellPower = 18f;
				cooldown = 3700;
				speed = 2f;
				range = 4f;
			}
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
