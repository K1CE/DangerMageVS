using SFDGameScriptInterface;
using System;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellPlant : Spell
        {
            public override Element element { get { return Element.PLANT; } }

            public SpellPlant(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellPlant(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
                float effectivePower = spellPower * powerMod;


			}

			public override void explode(Cast sender, IObject alreadyHit, Vector2 position) 
            {
				base.explode(sender, alreadyHit, position);



            }

			protected override void setUpStats()
			{
				spellPower = 14f;
				cooldown = 3000;
				speed = 7f;
				range = 0.6f;
				splash = 25;
				particleEffect = elementEffects[(int)element];
			}

			public static IObjectWeldJoint frozen;
			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				//TODO: tie objects together
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				IObject leaf = Game.CreateObject("ItemDebrisFlamethrower00", position);
				((CastProjectile)cast).attach(leaf);
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
