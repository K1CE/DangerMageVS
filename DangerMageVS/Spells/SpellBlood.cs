using SFDGameScriptInterface;
using System;
using static System.Net.Mime.MediaTypeNames;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellBlood : Spell
        {
            public override Element element { get { return Element.BLOOD; } }

            //TODO: add flying giblets that need to be picked up to heal like in excesses script
            public SpellBlood(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellBlood(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{
				if(!ply.IsStrengthBoostActive) ply.DealDamage(spellPower/3f);
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


                        PlayerData casterData = dataFromPlayer(caster);
                        PlayerModifiers casterMod = caster.GetModifiers();
                        bool fullHp = casterMod.CurrentHealth > casterMod.MaxHealth - 0.5f - spellPower / 3f;

						//bloodfrenzy
						if (ply.IsDead)
						{
							ply.Gib();
							Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 10f - 5f, 15f), "BLOODFRENZY", elementColors1[(int)Element.BLOOD], 1500f, 1.1f, true);
                            Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 20f - 5f, 20f), "+5", elementColors2[(int)Element.BLOOD], 1500f, 0.8f, true);

                            casterMod.MaxHealth += 5;
                            casterMod.CurrentHealth += 50;
                            caster.SetStrengthBoostTime(10000f);

                            for (int i = 0; i < casterData.lastSpellCasts.Length; i++)
							{
								casterData.cooldowns[i] = 0;
							}
						}
						else
						//bloodthirst
						if (fullHp || rnd.NextDouble() < 0.5f)
						{
							Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 10f - 5f, 15f), "BLOODTHIRST", elementColors1[(int)Element.BLOOD], 1500f, 1.1f, true);
							caster.SetStrengthBoostTime(2200f);
							if (casterData != null)
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
							}

							if (fullHp)
							{
								int extraHealth = (int)(casterMod.MaxHealth + damage / 10f);
								casterMod.MaxHealth = extraHealth;
								Game.PlayEffect("CFTXT", caster.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 20f - 5f, 20f), "+" + (int)(damage / 10f), elementColors2[(int)Element.BLOOD], 1500f, 0.8f, true);

							}

						}
						casterMod.CurrentHealth += damage;
						caster.SetModifiers(casterMod);

                    }
					else
					{
						target.DealDamage(effectivePower * 1.2f, caster.UniqueID);
                        //	if (target.GetHealth() <= effectivePower) target.Destroy();
                        //	else target.SetHealth(target.GetHealth() - effectivePower);
                    }


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

			IObjectWeldJoint bloodWeld;
			IObject bldBall;
			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);

                bldBall = Game.CreateObject("Giblet01", position);
                bloodWeld = (IObjectWeldJoint)Game.CreateObject("WeldJoint");
                bloodWeld.AddTargetObject(bldBall);
                ((CastProjectile)cast).attach(bldBall);
			}

            public override void explode(Cast sender, IObject alreadyHit, Vector2 position)
            {
                int blacklistID = 0;
                if (alreadyHit != null) blacklistID = alreadyHit.UniqueID;
                if (splash <= 0) return;
                particleExplosion("HIT_S", position, 14 + (int)(splash/2), splash);
                Area area = new Area(position.Y + splash, position.X - splash, position.Y - splash, position.X + splash);
                foreach (IObject obj in Game.GetObjectsByArea(area))
                {
                    float distance = Vector2.Distance(position, obj.GetWorldPosition());
                    if (obj.GetBodyType() == BodyType.Dynamic && obj.UniqueID != blacklistID && distance <= splash)
                    {
                        float powerMod = (float)Math.Sin(distance * Math.PI / 2 + Math.PI / 2);

                        affect(sender, obj, Vector2.Normalize(obj.GetWorldPosition() - position), powerMod);
                    }
                }

				foreach(IObject giblet in bloodWeld.GetTargetObjects())
				{
					giblet.Destroy();
				}
            }

            public override void passive(Cast sender, IObject target, Vector2 vector)
            {
                if (target is IPlayer && ((IPlayer)target).IsDead)
				{
                    ((IPlayer)target).Gib();
				}
                if (target.Name.Contains("Giblet"))
				{
					target.SetWorldPosition(new Vector2((float)rnd.NextDouble() * 10f - 5f, (float)rnd.NextDouble() * 10f - 5f) + bldBall.GetWorldPosition());
					bloodWeld.AddTargetObject(target);
					spellPower += 5f;
					splash += 5f;
				}
            }

        }

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
