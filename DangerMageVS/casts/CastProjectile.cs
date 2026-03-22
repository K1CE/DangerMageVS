using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{

		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class CastProjectile : Cast
		{
			public static int nextID = 0;
			public string ID;
			public IObjectAreaTrigger hitBox;
			public IObjectWeldJoint weld;
			public IObjectTargetObjectJoint targetJoint;
			public IObjectRailJoint rail;
			public IObjectRailAttachmentJoint railAttachment;
			public List<IObjectRailAttachmentJoint> railAttachments = new List<IObjectRailAttachmentJoint>();
			public Vector2 targetVector;

			public CastProjectile(Vector2 pos, Vector2 target, float speed, Spell spell) : base(spell)
			{


				targetVector = target - pos;
				direction = targetVector;
				direction.Normalize();

				ID = "PC-" + nextID;
				nextID++;

				weld = (IObjectWeldJoint)Game.CreateObject("WeldJoint");

				hitBox = (IObjectAreaTrigger)Game.CreateObject("AreaTrigger", pos); //+ new Vector2(0,-2));
				hitBox.SetSizeFactor(new Point(1, 2));
				hitBox.CustomID = ID;
				hitBox.SetScriptMethod("hitBoxImpact");
				hitBox.SetBodyType(BodyType.Dynamic);
				hitBox.SetMass(0.005f);

				weld.AddTargetObject(hitBox);

				targetJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", target);

				rail = (IObjectRailJoint)Game.CreateObject("RailJoint", pos);
				rail.SetTargetObjectJoint(targetJoint);

				railAttachment = (IObjectRailAttachmentJoint)Game.CreateObject("RailAttachmentJoint", pos);
				railAttachment.SetTargetObject(hitBox);
				railAttachment.SetRailJoint(rail);
				railAttachment.SetMotorEnabled(true);
				railAttachment.SetMotorSpeed(speed);
				railAttachment.SetOnDestinationReachedMethod("rangedOut");
				railAttachment.CustomID = ID;

				projectiles.Add(this);

			}

			public List<IObject> bits = new List<IObject>();
			public void attach(IObject obj)
			{

				obj.CustomID = attachmentID;

				
				CollisionFilter filt = obj.GetCollisionFilter();
				filt.MaskBits = 0000;
				filt.CategoryBits = 0;
				filt.AboveBits = 0;
				filt.BlockMelee = true;
				obj.SetCollisionFilter(filt);
				obj.SetBodyType(BodyType.Dynamic);
				obj.SetMass(0.005f);

				bits.Add(obj);

				IObjectRailAttachmentJoint attachment = (IObjectRailAttachmentJoint)Game.CreateObject("RailAttachmentJoint", hitBox.GetWorldPosition());
				railAttachments.Add(attachment);
				attachment.SetTargetObject(obj);
				attachment.SetRailJoint(rail);
				attachment.SetMotorEnabled(true);
				attachment.SetMotorSpeed(spell.speed);
				attachment.CustomID = ID;

				bits.Add(attachment);



				//weld.AddTargetObject(obj);
			}

			public static CastProjectile projectileFromID(string input)
			{
				foreach (CastProjectile prj in projectiles)
				{
					if (prj.ID.Equals(input)) return prj;
				}
				return null;
			}

			public override void particleTick()
			{
				particleTickProxy(hitBox.GetWorldPosition(), 2, 4.0f);
			}

            public override void intervalTick()
            {
				intervalTickProxy(position);
            }

            protected override void updatePosition()
			{
				position = hitBox.GetWorldPosition();
			}

			public override void destroy()
			{
				foreach (IObject bit in bits) if (bit != null) bit.Remove();
				if (hitBox != null) hitBox.Remove();
				weld.Remove();
				targetJoint.Remove();
				rail.Remove();
				railAttachment.Remove();
				projectiles.Remove(this);
				casts.Remove(this);

				foreach(IObject obj in cleanUp)
				{
					obj.Remove();
				}
				foreach(IObjectRailAttachmentJoint attachment in railAttachments)
				{
					attachment.Remove();
				}
			}

			protected override void onSpeedChange()
			{
				if(railAttachment != null)
                {
                    railAttachment.SetMotorSpeed(spell.speed);
					foreach(IObjectRailAttachmentJoint rail in railAttachments)
					{
						rail.SetMotorSpeed(spell.speed);
					}
                }

            }
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
