using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;


namespace SFDScript
{

	public partial class GameScript : GameScriptInterface
	{

		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

		public class SpellToxic : Spell
		{
			private static Element element = Element.TOXIC;
			private IPlayer queuedPlayer;

			public SpellToxic(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellToxic(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

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
						queuedPlayer = ply;

					}
					else
					{
						if (target.GetHealth() <= effectivePower) target.Destroy();
						else target.SetHealth(target.GetHealth() - effectivePower);
					}

				toxinQueue.Add(this);
				CreateTimer((int)(5000 * (effectivePower / 21.5)), 1, "toxinTimer", "2").SetWorldPosition(new Vector2(0, -16));

				particleExplosion("ACS", pos, 10, 9f);
				
			}

			public List<IObject> acidQueue = new List<IObject>();
			public void delayedDamage(float damage)
			{
				IPlayer ply = queuedPlayer;



				if (ply != null)
				{

					PlayerData data = dataFromPlayer(ply);
					if (data != null) damage *= data.acidDamageTaken;

					if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
					else ply.SetHealth(ply.GetHealth() - damage);



					Game.PlaySound("BreakGlass", ply.GetWorldPosition(), 10f);
					particleExplosion("ACS", ply.GetWorldPosition(), 4, 8f);
				}

				foreach (IObject obj in acidQueue)
				{
					if (!obj.DestructionInitiated && obj != null)
						if (obj.GetHealth() <= damage * 1.5f) obj.Destroy();
						else obj.SetHealth(obj.GetHealth() - (damage * 1.5f));
					particleExplosion("ACS", obj.GetWorldPosition(), 3, 10f);
					Game.PlaySound("BreakGlass", obj.GetWorldPosition(), 0.2f);
				}
			}


			protected override void setUpStats()
			{
				spellPower = 21.5f;
				cooldown = 5000;
				speed = 5f;
				range = 0.6f;
				particleEffect = "ACS";
			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				Game.PlayEffect("ACS", target.GetWorldPosition());
				acidQueue.Add(target);
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


