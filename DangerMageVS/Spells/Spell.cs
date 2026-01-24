using System;
using SFDGameScriptInterface;
using static SFDScript.GameScript;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public abstract class Spell
		{
			protected Cast cast;
			public string particleEffect;

			public float spellPower = 0;
			public int cooldown = 0;
			public float speed = 0f;
			public float range = 0f;
			public float splash = 0f;

			public IPlayer caster;


			public Spell(Vector2 position, Vector2 direction, CastType castType, IPlayer caster, SpellArguments args)
			{
				setUpStats();
				switch (castType)
				{
					case CastType.TOUCH:
						break;
					case CastType.GRENADE:

						break;
					case CastType.DISCHARGE:

						break;
					case CastType.PROJECTILE://try using BgLamp01A
						projectile(position, direction * range);
						break;
					case CastType.SELF:

						break;
					case CastType.HOVER:

						break;
					case CastType.CURSE:

						break;
					case CastType.RAIN:

						break;
					case CastType.AREA:

						break;
					case CastType.BULLET:

						break;
					case CastType.COORDINATE:

						break;
					case CastType.SHOTGUN:

						break;
					case CastType.SYNERGY:

						break;
					case CastType.PILLARS:

						break;
					case CastType.CHEAT:
						cheat(args.argObject);
						break;
				}

				this.caster = caster;

				cast.onImpactEvent += new Cast.EffectHandler(affect);
				cast.onPassiveEvent += new Cast.PassiveHandler(passive);
				cast.onParticleEvent += new Cast.ParticleHandler(particles);
                cast.onParticleExplosionEvent += new Cast.ParticleExplosionHandler(particleExplosion);
				cast.onExplodeEvent += new Cast.ExplosionHandler(explode);
            }

			protected abstract void setUpStats();

			public abstract void affect(Cast sender, IObject target, Vector2 vector, float powerMod);

			public abstract void passive(Cast sender, IObject target, Vector2 vector);

            public virtual void explode(Cast sender, IObject alreadyHit, Vector2 position)
			{
                int blacklistID = 0;
                if (alreadyHit != null) blacklistID = alreadyHit.UniqueID;
                if (splash <= 0) return;
                particleExplosion(sender, position, (int)(3 * (splash / 10)), splash);
                Area area = new Area(position.Y + splash, position.X - splash, position.Y - splash, position.X + splash);
                foreach (IObject obj in Game.GetObjectsByArea(area)) {
                    float distance = Vector2.Distance(position, obj.GetWorldPosition());
                    if (obj.GetBodyType() == BodyType.Dynamic && obj.UniqueID != blacklistID && distance <= splash) {
                        float powerMod = (float)Math.Sin(distance * Math.PI / 2 + Math.PI / 2);

                        affect(sender, obj, Vector2.Normalize(obj.GetWorldPosition() - position), powerMod);
                    }
                }
            }

            public virtual void particles(Cast sender, Vector2 position, int count, float radius)
			{
				if (particleEffect != "")
					while (count > 0)
					{
						float distance = (float)(rnd.Next(0, 50) * radius) / 50f;
						float angle = (float)(rnd.Next(50) * 2 * Math.PI) / 50f;
						Vector2 point = new Vector2((float)(distance * Math.Sin(angle)), (float)(distance * Math.Cos(angle)));
						Game.PlayEffect(particleEffect, position + point);
						count--;
					}
			}

			protected virtual void grenade() { }
			protected virtual void discharge() { }
			protected virtual void projectile(Vector2 position, Vector2 direction)
			{
				//add directional casting here
				cast = new CastProjectile(position, direction + position, speed, this);
			}
			protected virtual void self() { }
			protected virtual void hover() { }
			protected virtual void curse() { }
			protected virtual void rain() { }
			protected virtual void area() { }
			protected virtual void bullet() { }
			protected virtual void coordinate() { }
			protected virtual void shotgun() { }
			protected virtual void synergy() { }
			protected virtual void pillars() { }

			protected virtual void cheat(IObject target) 
			{
				cast = new CastCheat(target, this);
			}
			public static void particleExplosion(string effect, Vector2 pos, int density, float radius)
			{
				for (int i = 0; i < density; i++)
				{
					float dist = (rnd.Next(30) / 30f) * radius;
					float angle = (float)(Math.PI * 2 * (rnd.Next(30) / 30f));
					Game.PlayEffect(effect, pos + new Vector2((float)(Math.Cos(angle) * dist), (float)(Math.Sin(angle) * dist)));
				}
            }
            public void particleExplosion(Cast sender, Vector2 pos, int density, float radius) {
                for (int i = 0; i < density; i++) {
                    float dist = (rnd.Next(30) / 30f) * radius;
                    float angle = (float)(Math.PI * 2 * (rnd.Next(30) / 30f));
                    Game.PlayEffect(particleEffect, pos + new Vector2((float)(Math.Cos(angle) * dist), (float)(Math.Sin(angle) * dist)));
                }
            }

        }

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
