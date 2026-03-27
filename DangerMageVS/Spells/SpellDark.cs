using SFDGameScriptInterface;
using System;
using System.Runtime.InteropServices;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		//make some sort of benefit to killing players with the dark spell
		public class SpellDark : Spell
        {
            public override Element element { get { return Element.DARK; } }

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


						float spellPowerPlusMissingHealth = effectivePower + effectivePower * ((pmod.MaxHealth - pmod.CurrentHealth)/100f);

						float victimHealth = ply.GetHealth();

						float damage = spellPowerPlusMissingHealth;

						if (data != null) damage *= data.darkDamageTaken;

						double roll;
						double maxChange = 0.8f * (effectivePower / 12.5f);
						if (!ply.IsDead)
						{

							if (data != null)
							{
								double input = (rnd.NextDouble()); 


								double curve = (-maxChange) * (Math.Pow(input - 1, 2));
								double line = maxChange * (input - 1);
								double percentageChance = (spellPowerPlusMissingHealth - 5) / 40;

								double subtractor = (curve * (1 - percentageChance)) + (line * percentageChance);

                                roll = data.corruption + subtractor;
                                //limited curve function:
                                roll = Math.Pow(10, roll) / 10;

                                if (data.corruption > roll) data.corruption = data.corruption - (data.corruption - roll) /3f;


								if (data.corruption > 1.0) data.corruption = 1.0;
								else if (data.corruption < 0) data.corruption = 0;
								messageRoss(ply.Name + "'s corruption level changed to " + (((int)(data.corruption * 1000)) / 1000D));
                                messageRoss("you rolled " + (((int)(roll * 1000)) / 1000D));

                            }
							else roll = rnd.NextDouble(); 

							double chance = (0.25 * (effectivePower / 15)) * ((100 - victimHealth)) / 100;
							if (chance > roll)
							{ // smash bros formulas, 
								Game.ShowChatMessage("YOU HAD A SUDDEN HEART ATTACK", elementColors1[(int)element], ply.UserIdentifier);
								Game.ShowChatMessage("thwakc", new Color(250, 0, 0));
								ply.Kill();
							}
							else if (roll < chance * (3 + spellPowerPlusMissingHealth / 10))
							{
								Game.ShowChatMessage("YOU FELT YOUR HEART STOP FOR A MOMENT", elementColors1[(int)element], ply.UserIdentifier);
								damage += 1f;

								messageRoss(ply.Name + "'s limit is " + chance);
							}
						}



						//if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						//else ply.SetHealth(ply.GetHealth() - damage);
						ply.DealDamage(damage, caster.UniqueID);

						data.cursedDamage += (damage);

					}
					else
					{
						//if (target.GetHealth() <= effectivePower) target.Destroy();
						//else target.SetHealth(target.GetHealth() - effectivePower);
						target.DealDamage(effectivePower, caster.UniqueID);
					}

				particleExplosion("TR_S", pos, 10, 13f);
			}
			protected override void setUpStats()
			{
				spellPower = 11.5f; //starts at 10 and rises to 200% depending on playerhealth
				cooldown = 3700;
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
