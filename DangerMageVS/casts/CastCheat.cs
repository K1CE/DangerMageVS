using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SFDGameScriptInterface;


namespace SFDScript
{

	public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		class CastCheat : Cast
		{ 
			public IObject target;
			private Events.UpdateCallback m_updateEvent = null;
			public CastCheat(IObject affected, Spell spell) : base(spell)
			{
				
				this.target = affected;

				m_updateEvent = Events.UpdateCallback.Start(OnUpdate, 0);
			}
			public void OnUpdate(float elapsed)
			{
				hit(target);
				m_updateEvent.Stop();
				m_updateEvent = null;
			}

			public override void destroy()
			{
				casts.Remove(this);
			}

			public override void particleTick()
			{
			}

            public override void intervalTick()
            {
                intervalTickProxy(position);
            }

            protected override void updatePosition()
			{
				position = target.GetWorldPosition();
			}

            protected override void onSpeedChange()
            {
                
            }
        }

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
