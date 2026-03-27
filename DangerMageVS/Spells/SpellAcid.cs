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

		public class SpellAcid : Spell
        {
            public override Element element { get { return Element.ACID; } }
            private bool triggerDebounce = false;
            Events.UpdateCallback acidDelay = null;
            private List<IObject> acidTagged = new List<IObject>();
			private List<float> acidTag = new List<float>();

            private float triggerTime = 0;

            public SpellAcid(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null)
			{ 
			
			}

			public SpellAcid(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

            }

			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                Vector2 pos = sender.position;
				Game.PlaySound("BreakGlass", sender.position, 10f);

				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;

					}
					else
					{
					}

                tryAddTag(target, effectivePower);
                if (!triggerDebounce) {
                    Events.UpdateCallback delay = null;
                    delay = Events.UpdateCallback.Start((Action<float>)delayedDamage, (uint)(5000 * (effectivePower / 21.5f)));
					triggerDebounce = true;
				}
            }

			private bool tryAddTag(IObject target, float power) {
				if (target == null || target.DestructionInitiated || target.IsRemoved) return false;
				acidTagged.Add(target);
				acidTag.Add(power);
				return true;

            }

			public void delayedDamage(float a)
			{

				for (int i = 0; i < acidTagged.Count; i++)
				{
					IObject obj = acidTagged[i];
					float damage = acidTag[i];
					if (obj == null || obj.DestructionInitiated || obj.IsRemoved) continue;
					if (obj is IPlayer) obj.DealDamage(damage);
					else {
						damage = damage * 1.8f;
						obj.DealDamage(damage);
						if (damage > 30f && obj.GetHealth() == 1f && obj.GetBodyType() == BodyType.Dynamic) obj.Destroy(); //unique acid effect
					}
					particleExplosion("ACS", obj.GetWorldPosition(), 3, 10f);
					Game.PlaySound("BreakGlass", obj.GetWorldPosition(), 0.2f);
				}
				triggerDebounce = false;
				acidTagged.Clear();
			}


			protected override void setUpStats()
			{
				spellPower = 21.5f;
				cooldown = 5000;
				speed = 5f;
				range = 0.6f;
				splash = 19f;
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


