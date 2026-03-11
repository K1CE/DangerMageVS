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
		public class SpellBlast : Spell
		{
			private static Element element = Element.AIR;
			public const int EXPLOSION_DAMAGE = 50;
			public SpellBlast(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellBlast(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;
				for(int i = 0; i < effectivePower % EXPLOSION_DAMAGE; i++)
                {
                    Game.TriggerExplosion(vector + new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble()));

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

                target.ClearFire();
            }

			protected override void setUpStats()
			{
				spellPower = EXPLOSION_DAMAGE; //explosion damage
				cooldown = 20000;
				speed = 4f;
				range = 1f;
				splash = 40f;
				particleEffect = "STM";
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
