using SFDGameScriptInterface;
using System;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellIce : Spell
		{
			private static Element element = Element.ICE;

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
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);
						bool dataNull = data == null;
						float damage = effectivePower;

						if (!dataNull) damage *= (data.coldDamageTaken * ((data.cold) ? 1.5f : 1f)); //cold damage does more damage if target is cold
						else damage *= ((data.cold) ? 1.5f : 1f);
						ply.DealDamage(damage, caster.UniqueID);
						//if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						//else ply.SetHealth(ply.GetHealth() - damage);

						PlayerModifiers pmod = ply.GetModifiers();
						if (pmod.CurrentEnergy > damage * 4)
							pmod.CurrentEnergy = pmod.CurrentEnergy - (damage * 4f);
						else pmod.CurrentEnergy = 0f;
						if (!dataNull && !data.cold)
						{
							data.savedEnergyRecharge = pmod.EnergyRechargeModifier;
							data.savedRunSpeed = pmod.RunSpeedModifier;
							data.savedMeleeDamage = pmod.MeleeDamageDealtModifier;
						}
						pmod.EnergyRechargeModifier = pmod.EnergyRechargeModifier - (damage * 0.04f);
						pmod.RunSpeedModifier = pmod.RunSpeedModifier - (damage * 0.02f);
						pmod.MeleeDamageDealtModifier = (float)(9f / effectivePower);

						ply.SetModifiers(pmod);

						if (!dataNull) data.cold = true;

					}
					else
					{
						target.DealDamage(effectivePower, caster.UniqueID);
						//if (target.GetHealth() <= effectivePower) target.Destroy();
						//else target.SetHealth(target.GetHealth() - effectivePower);
					}

				Spell.particleExplosion("GLM", sender.position, 1, (int)splash);


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

					//TODO: fix glass sheet breaking weld when broken
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


                }
				//make it freeze objects
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
