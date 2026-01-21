using SFDGameScriptInterface;
using static System.Net.Mime.MediaTypeNames;


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
				ply.DealDamage(spellPower/3f);
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
						PlayerData victimData = dataFromPlayer(ply);

						float damage = effectivePower;

						if (victimData != null) damage *= victimData.darkDamageTaken;

						ply.DealDamage(damage, caster.UniqueID);

                        PlayerModifiers casterMod = caster.GetModifiers();


						//bloodthirst
                        messageRoss("hp: " + (-casterMod.CurrentHealth + casterMod.MaxHealth));
						bool fullHp = casterMod.CurrentHealth > casterMod.MaxHealth - 0.5f - spellPower/3f;
                        if (fullHp || rnd.NextDouble() < 0.5f) 
						{
							Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 10f - 5f, 15f), "BLOODTHIRST", elementColors1[(int)Element.BLOOD], 1500f, 1.1f, true);
							caster.SetStrengthBoostTime(2200f);
							PlayerData casterData = dataFromPlayer(caster);
							if(casterData != null) 
							{

								int longestCooldownIndex = 0;
								float longestCooldown = 100000000f;
								for (int i = 0; i < casterData.lastSpellCasts.Length; i++) 
								{
									float currentCooldown = casterData.cooldowns[i] - casterData.lastSpellCasts[i];

									if (currentCooldown < longestCooldown) 
									{
										longestCooldown = currentCooldown;
										longestCooldownIndex = i;
									}
								}

								casterData.cooldowns[longestCooldownIndex] = 0;
								casterData.castingOrder = longestCooldownIndex;
								messageRoss("removed cooldown " + longestCooldownIndex);
							}

							if (fullHp)
							{
								int extraHealth = (int)(casterMod.MaxHealth + damage / 10f);
                                casterMod.MaxHealth = extraHealth;
                                Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 20f - 5f, 20f), "+"+(damage / 10f), elementColors2[(int)Element.BLOOD], 1500f,0.8f, true);

                            }

                        }
						casterMod.CurrentHealth = casterMod.CurrentHealth + damage;
						caster.SetModifiers(casterMod);

                    }
					else
					{
						target.DealDamage(effectivePower * 1.2f, caster.UniqueID);
                        //	if (target.GetHealth() <= effectivePower) target.Destroy();
                        //	else target.SetHealth(target.GetHealth() - effectivePower);
                    }


                particleExplosion("BLD", pos, 10, 13f);
			}
			protected override void setUpStats()
			{
				spellPower = 10;
				cooldown = 4000;
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
