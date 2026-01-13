using SFDGameScriptInterface;


using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;




namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{

		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */

		//TODO: add passive effect
		public class SpellAir : Spell
		{
			private static Element element = Element.AIR;

			public SpellAir(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellAir(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;
				Vector2 pos = sender.position;

				Game.PlaySound("PlayerJump", pos, 10f);
				Game.PlaySound("PlayerJump", pos, 10f);
				Game.PlaySound("PlayerJump", pos, 10f);

				//particleExplosion("STM", pos, 14, 22f);

				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);

						float damage = effectivePower;

						if (data != null) damage *= data.player.GetModifiers().ImpactDamageTakenModifier;
						if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						else ply.SetHealth(ply.GetHealth() - damage);

						data.stun(100);
						ply.AddCommand(new PlayerCommand(PlayerCommandType.Fall));


						vector += new Vector2(0, 4);
						//Vector2 direction = sender.targetVector;
						//direction.Normalize();
						vector.Normalize();
						//vector += direction;
						//vector /= 2f;
						ply.SetLinearVelocity((vector * (effectivePower / 1.7f)) + ply.GetLinearVelocity());// + new Vector2(0,8));
						ply.SetWorldPosition(ply.GetWorldPosition() + new Vector2(0, 2.5f));

					}
					else
					{
						if (cantMeleeDamage(target))
							if (target.GetHealth() <= effectivePower) target.Destroy();
							else target.SetHealth(target.GetHealth() - effectivePower);

						vector += new Vector2(0, 4);
						vector.Normalize();

						target.SetLinearVelocity((vector * (effectivePower / 1.5f)) + target.GetLinearVelocity());// + new Vector2(0,8));
						target.SetWorldPosition(target.GetWorldPosition() + new Vector2(0, 2.5f));
					}

			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				vector.Normalize();
				Vector2 newdir = vector + new Vector2(0, 5);
				newdir.Normalize();
				newdir *= 4f;
				target.SetLinearVelocity(target.GetLinearVelocity() + ((newdir / (target.GetMass() * 100) + newdir) / 2)); //fix to make more realistic
				target.SetAngularVelocity(newdir.X * 2f);
			}

			protected override void setUpStats()
			{
				spellPower = 12;
				cooldown = 3400;
				speed = 7f;
				range = 1.4f;
				//splash = 10f;
				particleEffect = "STM";
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
