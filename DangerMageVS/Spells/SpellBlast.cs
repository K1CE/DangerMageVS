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
            public override Element element { get { return Element.BLAST; } }
            public const int EXPLOSION_DAMAGE = 50;
			public const int EXPLOSION_RADIUS = 40;
			public SpellBlast(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellBlast(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{
			}

			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
            {
                float effectivePower = spellPower * powerMod;
                float radius = (splash - EXPLOSION_RADIUS);
                radius = (radius < 0 ? 0 : radius);

                for (int i = 0; i < effectivePower / EXPLOSION_DAMAGE; i++)
                {
                    messageRoss("expl " + i);
                    Game.TriggerExplosion(sender.position + new Vector2((float)(rnd.NextDouble() - 0.5f) * radius, (float)(rnd.NextDouble() - 0.5f) * radius));
                    i += 1;
                }

            }
            public override void explode(Cast sender, IObject alreadyHit, Vector2 position)
            {
            }


            public override void passive(Cast sender, IObject target, Vector2 vector)
			{
            }


            //TODO: balance flak effect
            public static int BLAST_PROXIMITY = 70;
            protected override void interval(Cast sender, Vector2 pos)
            {
                foreach (IPlayer found in Game.GetObjectsByArea<IPlayer>(new Area(
                       (float)(pos.Y + (BLAST_PROXIMITY / 2)),
                       (float)(pos.X + (BLAST_PROXIMITY / -2)),
                       (float)(pos.Y + (BLAST_PROXIMITY / -2)),
                       (float)(pos.X + (BLAST_PROXIMITY / 2))

                       )))
                {
                    messageRoss("player found");
                    if (found.UniqueId != caster.UniqueID && !found.IsDead)
                    {
                        sender.hit(found);
                        messageRoss("proxy");
                        return;
                    }
                }
            }


            protected override void setUpStats()
			{
				spellPower = EXPLOSION_DAMAGE; //explosion damage
				cooldown = 9000;
				speed = 4f;
				range = 1f;
				splash = EXPLOSION_RADIUS;
				particleEffect = elementEffects[(int)element];

            }

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
