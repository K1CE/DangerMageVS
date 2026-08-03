using SFDGameScriptInterface;
using System;
using System.Reflection.Metadata;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellIce : Spell
        {
            public override Element element { get { return Element.ICE; } }

            public SpellIce(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellIce(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

				

                Game.PlaySound("DestroyStone", sender.position, 10f);

				if (target != null)
                {
                    target.ClearFire();

                    //weather damage bonus
                    if (Game.GetWeatherType() > 0 && target.GetHealth() > 1f)
					{
						RayCastInput skyRay = new RayCastInput(true);
						skyRay.AbsorbProjectile = RayCastFilterMode.True;
						skyRay.ProjectileHit = RayCastFilterMode.True;
						skyRay.IncludeOverlap = false;
						RayCastResult result = Game.RayCast(target.GetWorldPosition(), target.GetWorldPosition() + new Vector2(0, 400), skyRay)[0];
						if (!result.Hit) 
						{

							effectivePower *= 1.25f;

						}

                    }

					if (target is IPlayer) {
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);


						float damage = effectivePower;
						//damage *= (data.coldDamageTaken * ((data.cold) ? 1.5f : 1f)); //cold damage does more damage if target is cold
						damage = dealElementalDamage(target, effectivePower);
						//if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						//else ply.SetHealth(ply.GetHealth() - damage);

						if(data != null && ply.IsDead)
						{
							messageRoss("frozen!");
							freeze(ply, vector);
							return;
						}

						//note: cant give speed buff for recolor because it gives infinite stamina
						PlayerModifiers pmod = ply.GetModifiers();
						if (pmod.CurrentEnergy > damage * 4)
							pmod.CurrentEnergy = pmod.CurrentEnergy - (damage * 4f);
						else pmod.CurrentEnergy = 0f;
						if (!data.cold) 
						{
							data.savedEnergyRecharge = pmod.EnergyRechargeModifier;
							data.savedRunSpeed = pmod.RunSpeedModifier;
							data.savedMeleeDamage = pmod.MeleeDamageDealtModifier;
							data.savedClimbingSpeed = pmod.ClimbingSpeed;
						}
						pmod.EnergyRechargeModifier = pmod.EnergyRechargeModifier - (damage * 0.04f);
						pmod.RunSpeedModifier = pmod.RunSpeedModifier - (damage * 0.02f);
						pmod.MeleeDamageDealtModifier = (float)(9f / effectivePower);
						pmod.ClimbingSpeed = pmod.ClimbingSpeed - (damage * 0.02f);


                        ply.SetModifiers(pmod);

						data.cold = true;

                    }
					else {
						if (cantMeleeDamage(target) && !target.Name.Contains("Bg"))
						{
							Game.CreateObject("ReinforcedGlass00A", target.GetWorldPosition(), target.GetAngle()).SetBodyType(BodyType.Dynamic);
							target.Remove();

						}
						else dealElementalDamage(target, effectivePower);


						//if (target.GetHealth() <= effectivePower) target.Destroy();
						//else target.SetHealth(target.GetHealth() - effectivePower);
					}


				}

				Spell.particleExplosion("GLM", sender.position, 1, (int)splash);


			}

			//add icicles
			private void freeze(IPlayer ply, Vector2 direction)
			{
				direction.Normalize(); //redundant but safe
				Vector2 position = new Vector2(ply.FacingDirection * 1.5f, 8) + ply.GetWorldPosition();

				IProfile playerProfile = ply.GetProfile();

				IObjectWeldJoint weld = (IObjectWeldJoint)Game.CreateObject("WeldJoint");

				IObject invisibleBlock = Game.CreateObject("InvisibleBlock", position + new Vector2(0, 8));
				invisibleBlock.SetWorldPosition(position);
				invisibleBlock.SetBodyType(BodyType.Dynamic);
				invisibleBlock.SetSizeFactor(new Point(0,2));
				invisibleBlock.SetMass(0.001f);
				weld.AddTargetObject(invisibleBlock);



                IObjectPlayerProfileInfo profileInfo = (IObjectPlayerProfileInfo)Game.CreateObject("PlayerProfileInfo");
				IProfile objectProfile = profileInfo.GetProfile();
                objectProfile.Feet = playerProfile.Feet;
                objectProfile.Accessory = playerProfile.Accessory;
                objectProfile.Skin = playerProfile.Skin;
                objectProfile.Gender = playerProfile.Gender;
                objectProfile.ChestOver = playerProfile.ChestOver;
                objectProfile.Hands = playerProfile.Hands;
                objectProfile.ChestUnder = playerProfile.ChestUnder;
                objectProfile.Legs = playerProfile.Legs;
                objectProfile.Head = playerProfile.Head;
                objectProfile.Waist = playerProfile.Waist;

                IObjectPlayerPortrait skin = (IObjectPlayerPortrait)Game.CreateObject("BgPlayerPortrait00", position);
				skin.SetFaceDirection(ply.FacingDirection);
				skin.SetProfileInfo(profileInfo);
				skin.SetBodyType(BodyType.Dynamic);
                skin.SetMass(0.001f);
                weld.AddTargetObject(skin);


				for (int icicle = rnd.Next(4) + 7; icicle > 0; icicle--)
				{
					Vector2 startPos = ply.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 14 - 7, (float)rnd.NextDouble() * 14 - 4);
					float startScale = 1 + rnd.Next(9) / 5f;
                    Game.PlayEffect("GLM", startPos);
                    int segments = rnd.Next(1) + 4;

                    for (int i = 0; i < segments; i++)
                    {
						float scale = startScale - (startScale / (segments + 2)) * i;
						Vector2 segmentPos = startPos + direction * i * (startScale/1.2f);

						IObjectText pixel = (IObjectText)Game.CreateObject("Text", segmentPos + textPixelOffset * scale);
						pixel.SetTextScale(scale);
						pixel.SetText(".");
						pixel.SetTextColor(Color.White);

						pixel.SetBodyType(BodyType.Dynamic);
						weld.AddTargetObject(pixel);

					} 

				}


					ply.Remove();

            }

			public override void explode(Cast sender, IObject alreadyHit, Vector2 position) 
            {
				base.explode(sender, alreadyHit, position);

				//freeze air
				Vector2 normalizedDirection = Vector2.Normalize(sender.direction);
				IObject ice = Game.CreateObject("GlassSheet00A", sender.position, (float)Math.Acos(normalizedDirection.X));

            }

			protected override void setUpStats()
			{
				spellPower = 13.5f;
				cooldown = 3000;
				speed = 5f;
				range = 0.8f;
				splash = 17;
				particleEffect = "TR_D";
			}

			public static IObjectWeldJoint frozen;
			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				if (target.GetBodyType() == BodyType.Dynamic)
				{
					if(target.CustomID != "iceBit") frozen.AddTargetObject(target);
					//unfreezer.Trigger();

					//NOTE: glass shards will still sometimes break welds
					IObject iceBit = Game.CreateObject((rnd.Next(5) == 1)? "GlassShard00A" : "BgDirt00B", target.GetWorldPosition() + new Vector2(rnd.Next(-10, 10), rnd.Next(-10, 10)));
					if (iceBit.Name == "GlassShard00A") iceBit.SetAngle((float)(rnd.NextDouble() * Math.PI * 2));
					if (rnd.Next(2) == 1)
						iceBit.SetColor1("BgCyan");
					else
						iceBit.SetColor1("BgLightCyan");

                    iceBit.SetBodyType(BodyType.Dynamic);
                    iceBit.SetMass(0.00005f);
                    iceBit.CustomID = "iceBit";
                    frozen.AddTargetObject(iceBit);

                    target.ClearFire();
                }
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				frozen = (IObjectWeldJoint)Game.CreateObject("WeldJoint");
				frozen.SetBodyType(BodyType.Static);

				cast = new CastProjectile(position, direction + position, speed, this);
				IObject iceBall = Game.CreateObject("BgLamp01A", position);
				iceBall.SetColor2("LightBlue");
				((CastProjectile)cast).attach(iceBall);
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
