using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		//make some sort of benefit to killing players with the dark spell
		public class SpellDark : Spell
		{
			private static Element element = Element.DARK;

			public SpellDark(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellDark(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}

			//Mechanic concept: damage done with darkwand can carry over into future rounds
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;
				
				Vector2 pos = sender.position;
				Game.PlaySound("Heartbeat", pos, 10f);
				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);


						PlayerModifiers pmod = caster.GetModifiers();


						float spellPowerPlus = effectivePower + effectivePower * ((pmod.MaxHealth - pmod.CurrentHealth)/100f);

						float tt = ply.GetHealth();

						float damage = spellPowerPlus;

						if (data != null) damage *= data.darkDamageTaken;

						double chance;
						double maxChange = 0.4f * (effectivePower / 12);
						if (!ply.IsDead)
						{

							if (data != null)
							{
								double input = (rnd.NextDouble());


								double curve = (-maxChange) * ((input - 1) * (input - 1));
								double line = maxChange * input - maxChange;

								double percentage = (spellPowerPlus - 5) / 40;

								data.corruption += (curve * (1 - percentage)) + (line * percentage);
								chance = data.corruption;

								if (data.corruption > 1.0) data.corruption = 1.0;
								else if (data.corruption < 0) data.corruption = 0;
								messageRoss(ply.Name + "'s corruption level changed to " + (((int)(data.corruption * 1000)) / 1000D));

							}
							else chance = rnd.NextDouble();

							double scale = (0.25 * (effectivePower / 15)) * ((100 - tt)) / 100;
							if (chance < scale)
							{ // smash bros formulas, 
								Game.ShowChatMessage("YOU HAD A SUDDEN HEART ATTACK", elementColors1[(int)element], ply.UserIdentifier);
								Game.ShowChatMessage("thwakc", new Color(250, 0, 0));
								ply.Kill();
							}
							else if (chance < scale * (3 + spellPowerPlus / 10))
							{
								Game.ShowChatMessage("YOU FELT YOUR HEART STOP FOR A MOMENT", elementColors1[(int)element], ply.UserIdentifier);
								damage += 1f;

								messageRoss(ply.Name + "'s limit is " + scale);
							}
						}



						if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						else ply.SetHealth(ply.GetHealth() - damage);

						data.cursedDamage += (damage);

					}
					else
					{
						if (target.GetHealth() <= effectivePower) target.Destroy();
						else target.SetHealth(target.GetHealth() - effectivePower);
					}

				particleExplosion("TR_S", pos, 10, 13f);
			}
			protected override void setUpStats()
			{
				spellPower = 10; //starts at 10 and rises to 200% depending on playerhealth
				cooldown = 3500;
				speed = 5.3f;
				range = 1.2f;
				splash = 18;
				particleEffect = "TR_S";
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject darkBall = Game.CreateObject("BgValve00D", position);
				darkBall.SetColor1("Black");
				((CastProjectile)cast).attach(darkBall); 

			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				target.SetHealth(target.GetHealth() - 4f);
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
