using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class PlayerData
		{
			//player data
			public int id;
			public IPlayer player;
			public IUser user;
			public Wand wand;
			public float[] cooldowns = {0, 0};
            public float[] lastSpellCasts = {0, 0};
			public float GCD = 0;
            public int castingOrder = 0;
			public float savedMeleeDamage = 1f;
			public float savedRunSpeed = 1f;
			public float savedEnergyRecharge = 1f;
			public bool ready = true;
			public bool recovering = false;
			public bool cold = false;
			public float lastHealth = 100;
			public float cursedDamage = 0f;
			public double corruption = 1.0;
			//add spell list

			//player modifiers
			public float shockDamageTaken = 1f;
			public float darkDamageTaken = 1f;
			public float coldDamageTaken = 1f;
			public float acidDamageTaken = 1f;
			public float distortionDamageTaken = 1f;

			//player timers
			public IObjectTimerTrigger unfoldPause;
			public IObjectTimerTrigger spellCast;
			public IObjectTimerTrigger recovery;
			public IObjectTimerTrigger fireRecovery;
			public IObjectTimerTrigger toxin;

			public PlayerData(IPlayer ply)
			{
				player = ply;
				user = ply.GetUser();
				id = ply.UniqueID;
				unfoldPause = CreateTimer(500, 1, "delayedUnfold", "2");
				spellCast = CreateTimer(500, 2, "delayedCast", "2");
				recovery = CreateTimer(1000, 3, "recoveryTimer", "2");
				fireRecovery = CreateTimer(0, 1, "fireRecovery", "2");

				unfoldPause.SetActivateOnStartup(false);
				spellCast.SetActivateOnStartup(false);
				recovery.SetActivateOnStartup(false);
				fireRecovery.SetActivateOnStartup(false);

				players.Add(this);
			}

			public void unfoldWand()
			{
				if (wand != null)
				{
					buttonQueue.Add(wand);
					unfoldPause.Trigger();
				}
			}

			public void castSpell()
			{
				float cooldown = cooldowns[castingOrder];

				if (Game.TotalElapsedGameTime > lastSpellCasts[castingOrder] + cooldown && Game.TotalElapsedGameTime > GCD)
				{

					Spell spell = wand.castSpell();
					if (spell != null)
					{

						GCD = Game.TotalElapsedGameTime + 350;
                        cooldowns[castingOrder] = spell.cooldown + spell.cooldown * (cooldowns.Length - 1)/1.1f;
						//cooldowns[castingOrder] /= 2.5f;

                        lastSpellCasts[castingOrder] = Game.TotalElapsedGameTime;
						ready = false;
						//spellQueue.Add(this);

						Game.PlaySound(elementSounds[(int)wand.element], player.GetWorldPosition(), 10f);
						Game.PlaySound(elementSounds[(int)wand.element], player.GetWorldPosition(), 10f);
					}
					castingOrder++;
					castingOrder %= cooldowns.Length;

					//add spell list shuffle if it didnt work
				}
				else
				{
					Game.PlayEffect(
							"CFTXT",
							player.GetWorldPosition() + new Vector2(0f, 30f),
							(int)((lastSpellCasts[castingOrder] + cooldowns[castingOrder] - Game.TotalElapsedGameTime) / 1000) + "s"
						);
				}
			}


			public bool electrocuted = false;
			public void electrocute(int interval)
			{
				electrocuted = true;
				stun(interval);
			}
			public void stun(int interval)
			{
				stunQueue.Add(this);
				recovery.SetIntervalTime(interval);
				recovery.Trigger();
				recovering = true;
				lastHealth = player.GetHealth();
				player.SetInputEnabled(false);
			}

			public void recover()
			{
				player.AddCommand(new PlayerCommand(PlayerCommandType.StopDeathKneel));
				player.SetUser(user);
				player.SetInputEnabled(true);
				recovering = false;
				electrocuted = false;
			}

			public void coldCheck()
			{
				if (cold)
				{
					PlayerModifiers pmod = player.GetModifiers();
					if (pmod.CurrentEnergy == pmod.MaxEnergy || ( (player.IsBurning || player.IsBurningInferno) && rnd.NextDouble() < 0.2f))
					{
						pmod.EnergyRechargeModifier = savedEnergyRecharge;
						pmod.RunSpeedModifier = savedRunSpeed;
						pmod.MeleeDamageDealtModifier = savedMeleeDamage;
						player.SetModifiers(pmod);
						cold = false;
					}
					else
					{
						for (int i = 0; i < 2; i++)
							Game.PlayEffect("STM", player.GetWorldPosition() + new Vector2(rnd.Next(-8 - (3 * player.FacingDirection), 8 - (3 * player.FacingDirection)), rnd.Next(-5, 8)));
						for (int i = 0; i < 6; i++)
							Game.PlayEffect("GLM", player.GetWorldPosition() + new Vector2(rnd.Next(-8 - (3 * player.FacingDirection), 8 - (3 * player.FacingDirection)), rnd.Next(-5, 8)));
					}
				}
			}

			public void effectsBody(Element element)
			{
				int count = rnd.Next(-1, 3); //options are: 0, 0, 1, or 2
				Point area = player.GetSize();
				area = new Point(area.X, area.Y + 8);
				Vector2 position = player.GetWorldPosition();
				for (int i = 0; i < count; i++)
				{
					float x = (float)(rnd.NextDouble() * area.X) - area.X / 2;
					float y = -(float)(rnd.NextDouble() * area.Y) + area.Y / 2;

					Vector2 toEffect = new Vector2(x, y) + position;

					Game.PlayEffect(elementEffects[(int)element], toEffect);
					Game.PlaySound("ElectricSparks", toEffect);
				}
			}

			float totalHealthHealed;
			public void corruptionCheck()
			{
				if (player != null)
				{
					PlayerModifiers pMod = player.GetModifiers();
					float remaining = pMod.MaxHealth - cursedDamage;

					float totalDamage = player.Statistics.TotalDamageTaken;
					float expectedHealth = pMod.MaxHealth - totalDamage + totalHealthHealed;
					if (pMod.CurrentHealth > expectedHealth)
					{
						float healed = pMod.CurrentHealth - expectedHealth;
						healed = healed / 2f + (float)(healed * corruption) / 2f;
						pMod.CurrentHealth = expectedHealth + healed;
						totalHealthHealed += healed;
					}

					if (pMod.CurrentHealth > remaining)
					{
						float healing = (pMod.CurrentHealth - remaining)/3f;
						cursedDamage -= healing;
						pMod.CurrentHealth = pMod.MaxHealth - cursedDamage;
						
					}
					player.SetModifiers(pMod);

				}
			}
		}
		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
