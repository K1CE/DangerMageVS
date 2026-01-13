using System;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class SpellEarth : Spell
{
		private static Element element = Element.EARTH;

			public SpellEarth(Vector2 position, Vector2 direction, CastType castType, IPlayer caster) : this(position, direction, castType, caster, null) { }

			public SpellEarth(Vector2 position, Vector2 direction, CastType castType, IPlayer ply, SpellArguments args) : base(position, direction, castType, ply, args)
			{

			}
			//TODO: add impact particle effects
		public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
		{
			float effectivePower = spellPower * powerMod;

			Vector2 pos = sender.position;
			IObject attacker = Game.CreateObject("StoneDebris00A", pos, rnd.Next(628) / 100f, Vector2.Normalize(vector) * 7, 0f);
			//if(target == null) messageRoss("" + vector.X + " ," + vector.Y);
			attacker.TrackAsMissile(true);
			if (target != null)
				if (target is IPlayer)
				{
					IPlayer ply = (IPlayer)target;
					if (!(ply.IsBlocking || ply.IsMeleeAttacking))
					{ //|| (ply.FacingDirection > 0) == (vector.X > 0)){
						float damage = effectivePower * ply.GetModifiers().MeleeDamageTakenModifier;
						if (ply.GetHealth() <= damage && !ply.IsStrengthBoostActive) ply.Kill();
						else ply.SetHealth(ply.GetHealth() - damage);
					} /*else {
			CollisionFilter filt = attacker.GetCollisionFilter();
			filt.BlockMelee = false;
			attacker.SetCollisionFilter(filt);
		}*/
				}
				else
				{
					if (!cantMeleeDamage(target))
						if (target.GetHealth() <= effectivePower) target.Destroy();
						else target.SetHealth(target.GetHealth() - effectivePower);
				}

			particleExplosion("DestroyDefault", pos, 3, 5f);
			//if(target is IPlayer){
			//	target.
			//}
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
		((CastProjectile)cast).attach(Game.CreateObject("StoneDebris00A", position));
	}
	protected override void setUpStats()
	{
		spellPower = 17;
		cooldown = 3700;
		speed = 9f;
		range = 1f;
	}
}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
