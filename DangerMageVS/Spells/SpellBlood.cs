using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellBlood : Spell
		{
			private static Element element = Element.BLOOD;

			//TODO: add flying giblets that need to be picked up to heal like in excesses script
			public SpellBlood(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellBlood(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;

				Vector2 pos = sender.position;
				Game.PlaySound("PlayerGib", pos, 10f);
				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);

						float damage = effectivePower;

						if (data != null) damage *= data.darkDamageTaken;

						ply.DealDamage(damage, caster.UniqueID);
						//if (ply.GetHealth() <= damage) ply.Kill();
						//else ply.SetHealth(ply.GetHealth() - damage);

						caster.SetHealth(caster.GetHealth() + damage);

					}
					else
					{
						target.DealDamage(effectivePower, caster.UniqueID);
					//	if (target.GetHealth() <= effectivePower) target.Destroy();
					//	else target.SetHealth(target.GetHealth() - effectivePower);
					}

				particleExplosion("BLD", pos, 10, 13f);
			}
			protected override void setUpStats()
			{
				spellPower = 10;
				cooldown = 3850;
				speed = 5.6f;
				range = 1.0f;
				splash = 6;
				particleEffect = "BLD";
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject bldBall = Game.CreateObject("Giblet01", position);
				((CastProjectile)cast).attach(bldBall);
			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
