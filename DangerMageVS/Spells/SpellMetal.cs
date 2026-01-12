using System;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellMetal : Spell
		{
			private static Element element = Element.METAL;

			public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellMetal(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
			public override void affect(Cast sender, IObject target, Vector2 vector)
			{

				if (target != null)
				{

					Vector2 pos = sender.position;
					IProjectile prj = Game.SpawnProjectile(ProjectileItem.PISTOL, target.GetWorldPosition() - vector, vector);
					prj.CritChanceDealtModifier = 100f;
					float damage = (spellPower / 8f);
					prj.DamageDealtModifier = damage;

					
				}
				Game.PlaySound("ImpactMetal", sender.position, 1f);
				particleExplosion("S_P", sender.position, 3, 8f);
			}

			public override void passive(Cast sender, IObject target, Vector2 vector)
			{
				/*
				Vector2 pos = sender.position;
				RayCastInput input = new RayCastInput(true);
				input.ProjectileHit = RayCastFilterMode.True;
				input.IncludeOverlap = true;
				RayCastResult outPut = Game.RayCast(pos, pos + vector, input)[0];
				if (outPut.Hit && Vector2.Distance(outPut.Position, pos) < 2f){*/
				if (target.GetCollisionFilter().BlockMelee && Math.Abs(target.GetWorldPosition().Y - (sender.position.Y - 1f)) < 5f)
					cast.impact(target);
				//}
			}

			protected override void projectile(Vector2 position, Vector2 direction)
			{
				cast = new CastProjectile(position, direction + position, speed, this);
				((CastProjectile)cast).attach(Game.CreateObject("Pulley00", position));
			}
			protected override void setUpStats()
			{
				spellPower = 19f;
				cooldown = 3700;
				speed = 7.5f;
				range = 2f;
			}
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
