using System;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellShock : Spell
		{
			private static Element element = Element.SHOCK;

			public SpellShock(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellShock(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;

                Vector2 pos = sender.position;

				Game.PlaySound("DestroyMetal", pos, 10f);
				Game.PlaySound("DestroyMetal", pos, 10f);

				if (target != null)
					if (target is IPlayer)
					{
						IPlayer ply = (IPlayer)target;
						PlayerData data = dataFromPlayer(ply);

						float damage = effectivePower;

						if (data != null) damage *= data.shockDamageTaken;

						if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						else ply.SetHealth(ply.GetHealth() - damage); //add stun effect

						data.electricute((int)(6.4f * effectivePower * effectivePower));
						ply.AddCommand(new PlayerCommand(PlayerCommandType.DeathKneelInfinite));
						Game.PlayEffect("Electric", target.GetWorldPosition());
						Game.PlayEffect("Electric", target.GetWorldPosition() + new Vector2(0, 8));

					}
					else if (target.Name == "Streetsweeper")
					{
						if (target.GetHealth() <= effectivePower + 9f) target.Destroy();
						else target.SetHealth(target.GetHealth() - (effectivePower + 14f));
					}
					else
					{
						if (target.GetHealth() <= effectivePower) target.Destroy();
						else target.SetHealth(target.GetHealth() - effectivePower);
					}

				for (int i = 0; i < 6; i++)
				{
					float dist = (rnd.Next(30) / 30f) * 15f;
					float angle = (float)(Math.PI * 2 * (rnd.Next(30) / 30f));
					Game.PlayEffect("Electric", pos + new Vector2((float)(Math.Cos(angle) * dist), (float)(Math.Sin(angle) * dist)));
				}
				particleExplosion("Electric", pos, 6, 17f);

			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				Game.PlayEffect("Electric", target.GetWorldPosition());
				vector.Normalize();
				Vector2 newdir = new Vector2(0, 5);
				newdir.Normalize();
				newdir *= 2f;
				target.SetLinearVelocity(target.GetLinearVelocity() + newdir);
				target.SetAngularVelocity(newdir.X);

				if (target.Name == "Streetsweeper")
				{
					if (target.GetHealth() > spellPower) target.SetHealth(target.GetHealth() - (spellPower + 4));
					else target.Destroy();
				}
				else if (cantMeleeDamage(target))
					target.Destroy();
			}

			protected override void setUpStats()
			{
				spellPower = 14;
				cooldown = 4500;
				speed = 10f;
				particleEffect = "Electric";
				splash = 12;
				range = 0.45f;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
