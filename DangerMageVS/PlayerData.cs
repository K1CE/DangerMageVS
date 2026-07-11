using SFDGameScriptInterface;
using System;
using static SFDScript.GameScript;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class PlayerData
        {
			//TODO: maybe move spellcharges from playerdata to wand?
			//TODO: maybe make shield have elemental resistance
			//TODO: deactivate mana shield if wand is removed


			//player data

			private const float COOLDOWN_MULTIPLIER = 0.6f;
            public int id;
			public IPlayer player;
			public IUser user;
			public Wand wand;
			public ManaShield shield;
			public float[] cooldowns = {0, 0};
            public float[] lastSpellCasts = {0, 0};
			public float GCD = 0;
            public int castingOrder = 0;
			public float savedMeleeDamage = 1f;
			public float savedRunSpeed = 1f;
			public float savedEnergyRecharge = 1f;
			public float savedClimbingSpeed = 1f;
            public bool ready = true;
			public bool recovering = false;
			public bool cold = false;
			public float lastHealth = 100;
			public float cursedDamage = 0f;
			public double corruption = 1.0;
			//add spell list

			//player modifiers
			public float shockDamageTaken = 1f;
			public float deathDamageTaken = 1f;
			public float coldDamageTaken = 1f;
			public float toxinDamageTaken = 1f;
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
				unfoldPause = CreateTimer(300, 1, "delayedUnfold", "2");
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
			private void findCastingOrder()
			{
				int bestCooldownIndex = -1;
				float bestTime = 10000000;
				for (int i = 0; i < cooldowns.Length; i++)
				{
					if (lastSpellCasts[i] + cooldowns[i] < bestTime)
					{
						bestTime = lastSpellCasts[i] + cooldowns[i];
						bestCooldownIndex = i;
					}
				}
				if (bestCooldownIndex > -1) castingOrder = bestCooldownIndex;
			}
			private void expendCharge(float cooldown)
			{
                GCD = Game.TotalElapsedGameTime + 350;
                cooldowns[castingOrder] = cooldown + cooldown * (cooldowns.Length - 1) / 1.1f;
                //cooldowns[castingOrder] /= 2.5f;

                lastSpellCasts[castingOrder] = Game.TotalElapsedGameTime;
                ready = false;

            }
			public void useWand()
			{
				float cooldown = cooldowns[castingOrder];

				if (Game.TotalElapsedGameTime > lastSpellCasts[castingOrder] + cooldown && Game.TotalElapsedGameTime > GCD)
				{

					Spell spell = wand.castSpell();
					if (spell != null)
					{

						expendCharge(spell.cooldown * COOLDOWN_MULTIPLIER);

					}
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
				findCastingOrder();
			}

			//if moved to wand color parameter can be removed
			public void castManaShield(Color color)
			{
                float cooldown = cooldowns[castingOrder];

				if (shield != null && shield.Enabled) return;

                if (Game.TotalElapsedGameTime > lastSpellCasts[castingOrder] + cooldown && Game.TotalElapsedGameTime > GCD){
					shield = new ManaShield(player);
					shield.setColor(color);
					expendCharge(10000);
				}
				findCastingOrder();
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
						pmod.ClimbingSpeed = savedClimbingSpeed;
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

            public static bool checkData(PlayerData data)
            {
				if (!(data.player == null || data.player.RemovalInitiated)) return true;
				else
				{
					players.Remove(data);
					return false;
				}
            }
        }
		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
