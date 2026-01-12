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



		static class CustomParticle
		{
			public static int MAX_STACK = 50;
			public static IObjectText[] particleStack = new IObjectText[MAX_STACK];
			public static IObjectText[] particleQueue = new IObjectText[MAX_STACK];
			private static int head = 0;
			private static int tail = 0;
			

			public static bool newParticle(Color color, Vector2 position)
			{
				if ((head + 1) % 50 != tail)
				{
					IObjectText particle = particleStack[head];
					particleQueue[head] = particle;
					head++;

					return true;
				}
				else return false;
				
			}
			/*
			public static bool removeParticle()
			{
				if (queueTail != queueHead)
				{
					particleStack[stackTail] = particleQueue[queueTail];
					queueTail++;
					return true;
				}
				else return false;
			}
			*/




		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

	}
}
