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

			public Vector2 position = Vector2.Zero;
			public Vector2 direction = Vector2.Zero;
			public Spell spell;

			public Cast(Spell spell)
			{
				this.spell = spell;
				casts.Add(this);
			}
			/*
			public void splash() 
			{
				if (spell.splash <= 0) return;

			}
			*/
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
