using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;
using System.Diagnostics;


namespace SFDScript
{

	public partial class GameScript : GameScriptInterface
	{

		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

		public class SpellToxic : Spell
		{
			private static Element element = Element.TOXIC;
			private bool triggerDebounce = false;
			//private IObjectTimerTrigger acidDelay = null;
            Events.UpdateCallback acidDelay = null;
            //private Dictionary<IObject, float> acidTagged = new Dictionary<IObject, float>();
            private List<IObject> acidTagged = new List<IObject>();
			private List<float> acidTag = new List<float>();

            private float triggerTime = 0;
            //private List<IPlayer> queuedPlayers = new List<IPlayer>();

            public SpellToxic(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null)
			{ 
			
			}

			public SpellToxic(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{
			//	acidDelay = CreateTimer((int)(5000), 1, "toxinTimer", "2");
			//	acidDelay.SetWorldPosition(new Vector2(0, -16));
            }
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                Vector2 pos = sender.position;
				Game.PlaySound("BreakGlass", sender.position, 10f);

				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						//queuedPlayer = ply;

					}
					else
					{
						//target.DealDamage(effectivePower);
						//if (target.GetHealth() <= effectivePower) target.Destroy();
						//else target.SetHealth(target.GetHealth() - effectivePower);
					}

                //TODO: USE TOXIN QUEUE AGAIN
                tryAddTag(target, effectivePower);
                //CreateTimer((int)(5000 * (effectivePower/21.5f)), 1, "toxinTimer", "2").SetWorldPosition(new Vector2(0, -16));
                if (!triggerDebounce) {
					messageRoss("toxintimer for " + (int)(5000 * (effectivePower / 21.5)));
					toxinQueue.Add(this);
                    Events.UpdateCallback delay = null;
                    delay = Events.UpdateCallback.Start((Action<float>)delayedDamage, (uint)(5000 * (effectivePower / 21.5f)));
                    //acidDelay.SetIntervalTime((int)(5000 * (effectivePower / 21.5)));
                    //acidDelay.Trigger();
                    //triggerTime = Game.TotalElapsedGameTime;
                    messageRoss("triggered at" + triggerTime);
					triggerDebounce = true;
				}
                //particleExplosion("ACS", pos, 10, 9f);
                /*
                Events.UpdateCallback delay = null;
                delay = Events.UpdateCallback.Start(e => {
                    if (target != null)
                        target.DealDamage(effectivePower);
					delay.Stop();
                }, (uint)(5000 * (effectivePower / 21.5f)));
				*/
            }

			private bool tryAddTag(IObject target, float power) {
				if (target == null || target.DestructionInitiated || target.IsRemoved) return false;
				//else acidTagged.Add(target, power);
				acidTagged.Add(target);
				acidTag.Add(power);
				return true;

            }

			public void delayedDamage(float a)
			{
				/*
				if (acidDelay == null) messageRoss("nullacid");

                if (acidDelay == null || acidDelay.GetIntervalTime() - 200 > Game.TotalElapsedGameTime - triggerTime) 
				{

					messageRoss("elapsed time" + (Game.TotalElapsedGameTime - triggerTime));
					return false;
				}
				*/
				//IPlayer ply = queuedPlayer;

				//messageRoss("toxindmg " + damage);

				/*
				if (ply != null)
				{

					PlayerData data = dataFromPlayer(ply);
					if (data != null) damage *= data.acidDamageTaken;

					if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
					else ply.SetHealth(ply.GetHealth() - damage);



					Game.PlaySound("BreakGlass", ply.GetWorldPosition(), 10f);
					//particleExplosion("ACS", ply.GetWorldPosition(), 4, 8f);
				}
				*/

				for (int i = 0; i < acidTagged.Count; i++)
				{
					//if (!acidTagged.Conta(target.Key)) continue;
					IObject obj = acidTagged[i];
					float damage = acidTag[i];
					if (obj == null || obj.DestructionInitiated || obj.IsRemoved) continue;
					if (obj is IPlayer) obj.DealDamage(damage);
					else obj.DealDamage(damage * 1.8f);
					/*
					if (!obj.DestructionInitiated && obj != null)
						if (obj.GetHealth() <= damage * 1.5f) obj.Destroy();
						else obj.SetHealth(obj.GetHealth() - (damage * 1.5f));
					*/
					messageRoss("acid damage to " + obj.Name + " for " + damage);
					particleExplosion("ACS", obj.GetWorldPosition(), 3, 10f);
					Game.PlaySound("BreakGlass", obj.GetWorldPosition(), 0.2f);
				}
				triggerDebounce = false;
				acidTagged.Clear();

				//return true;
			}


			protected override void setUpStats()
			{
				spellPower = 21.5f;
				cooldown = 5000;
				speed = 5f;
				range = 0.6f;
				splash = 20f;
				particleEffect = "ACS";
			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				Game.PlayEffect("ACS", target.GetWorldPosition());
                tryAddTag(target, spellPower/1.5f);
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject toxicBall = Game.CreateObject("BgLamp01A", position);
				toxicBall.SetColor2("LightGreen");
				((CastProjectile)cast).attach(toxicBall);
			}
		}


		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}


