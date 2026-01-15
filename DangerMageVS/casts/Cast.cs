using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{

		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public abstract class Cast
		{
			public delegate void EffectHandler(Cast sender, IObject affected, Vector2 vector, float powerMod);
			public event EffectHandler onImpactEvent;

			public delegate void PassiveHandler(Cast sender, IObject affected, Vector2 vector);
			public event PassiveHandler onPassiveEvent;

			public delegate void ParticleHandler(Cast sender, Vector2 position, int count, float radius);
			public event ParticleHandler onParticleEvent;

            public delegate void ParticleExplosionHandler(Cast sender, Vector2 position, int density, float radius);
            public event ParticleExplosionHandler onParticleExplosionEvent;

            public Vector2 position = Vector2.Zero;
			public Vector2 direction = Vector2.Zero;
			public Spell spell;

			public Cast(Spell spell)
			{
				this.spell = spell;
				casts.Add(this);
			}
			
			public void splash(IObject alreadyHit) 
			{
				int blacklistID = 0;
				if (alreadyHit != null) blacklistID = alreadyHit.UniqueID;
                if (spell.splash <= 0) return;
				onParticleExplosionEvent(this, position, 14, spell.splash);

				Area area = new Area(position.Y + spell.splash, position.X - spell.splash, position.Y - spell.splash, position.X + spell.splash);
				foreach (IObject obj in Game.GetObjectsByArea(area))
				{
					if (obj.GetBodyType() == BodyType.Dynamic && obj.UniqueID != blacklistID && Vector2.Distance(position, obj.GetWorldPosition()) <= spell.splash) 
					{
						onImpactEvent(this, obj, Vector2.Normalize(obj.GetWorldPosition() - position), 0.5f); //TODO: variable powerMod
					}
				}
            }
			
			public void impact(IObject affected)
			{

				updatePosition();
				Vector2 vector = Vector2.Zero;
				if (affected != null)
				{
					vector = -(position - affected.GetWorldPosition());
					messageRoss(affected.Name + " was blasted for " + spell.spellPower);
				}
				else vector = direction;
				onImpactEvent(this, affected, vector, 1);
				splash(affected);

				destroy();

			}

			public void passiveEffect(IObject affected)
			{
				updatePosition();
				Vector2 vector = Vector2.Zero;
				if (affected != null) vector = -(position - affected.GetWorldPosition());
				onPassiveEvent(this, affected, vector);
			}

			protected void particleTickProxy(Vector2 pos, int count, float radius)
			{
				onParticleEvent(this, pos, count, radius);
			}



			protected abstract void updatePosition();

			public abstract void particleTick();

			public abstract void destroy();
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
