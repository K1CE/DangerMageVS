using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellFire : Spell
		{
			private static Element element = Element.FIRE;

			public SpellFire(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellFire(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                Game.PlaySound("PlayerRoll", sender.position, 10f);
				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);


						float damage = effectivePower;

						if (data != null) damage *= (data.player.GetModifiers().FireDamageTakenModifier) * ((ply.IsBurningInferno) ? 1.5f : 1f);

						float hdamage = damage / 3f;

						if (ply.GetHealth() <= hdamage && !ply.IsStrengthBoostActive) ply.Kill();
						else ply.SetHealth(ply.GetHealth() - hdamage);

						//add fire extinguisher
						if (data != null && !ply.IsBurningInferno)
						{
							data.fireRecovery.SetIntervalTime((int)(((hdamage * 2) / FIRE_PER_SECOND) * 1000));
							data.fireRecovery.Trigger();
							fireQueue.Add(data);
						}

						target.SetMaxFire();

					}
					else
					{
						if (target.GetHealth() <= effectivePower) target.Destroy();
						else target.SetHealth(target.GetHealth() - effectivePower);
						target.SetMaxFire();
					}

				Game.SpawnFireNodes(sender.position, 4, 1f, FireNodeType.Default);

			}
			protected override void setUpStats()
			{
				spellPower = 24;
				cooldown = 3650;
				speed = 5f;
				range = 0.65f;
				splash = 6;
				particleEffect = "TR_F";
			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				if (target.CanBurn && rnd.Next(8) == 1)
					target.SetMaxFire();
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
