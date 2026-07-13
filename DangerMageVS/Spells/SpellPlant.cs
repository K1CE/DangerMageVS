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
			//TODO: make vines spread even without a target
			//TODO: metal wand cuts vines
			//TODO: gib if damage kills, and vine all debris
			float bufferedDamage = 0;
			int split = 0;
			public override void affect(Cast sender, IObject target, Vector2 vector, float powerMod)
			{
				float effectivePower = spellPower * powerMod;

				if (target == null) return;

				if (target.IsBurning) effectivePower /= 2f;

				Vector2 impactPos = sender.position;
				if (target is IPlayer || target.GetSizeFactor().X + target.GetSizeFactor().Y < 4) impactPos = target.GetWorldPosition();

				if (target is IPlayer)
                {
                    PlayerData data = dataFromPlayer((IPlayer)target);
					if (data != null)
					{
						effectivePower = effectivePower * data.toxinDamageTaken;
					}
				}



				for (int i = 0; i < splash / 2f; i++)
				{
					randomVine(target, impactPos, effectivePower);
				}

                dealElementalDamage(target, effectivePower * 0.5f);

                bufferedDamage = effectivePower * 0.5f;

				
            }

			public override void explode(Cast sender, IObject alreadyHit, Vector2 position) 
            {
				if (alreadyHit != null) return;
				//base.explode(sender, alreadyHit, position);
				IObject core = Game.CreateObject("InvisibleBlockNoCollision", position);
				core.CustomID = "weakJoint";
				core.SetMass(0.00001f);
				core.SetBodyType(BodyType.Dynamic);
				Game.PlaySound("ItemSpawn", position);
				for(int i = 0; i < splash / 4; i++)
				{
					randomVine(core, core.GetWorldPosition(), spellPower);
				}

            }

			protected override void setUpStats()
			{
				spellPower = 9f;
				cooldown = 2900;
				speed = 5f;
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

			private void randomVine(IObject target, Vector2 fromVec, float power)
			{
                for (int j = 0; j < 5; j++) //do 5 attempts to find a tie vector
                {
                    float range = splash * 3;

                    Vector2 throwVec = fromVec + new Vector2((float)Math.Cos(rnd.NextDouble() * Math.PI * 2) * range,
                        (float)Math.Sin(rnd.NextDouble() * Math.PI * 2) * range);

                    Game.DrawLine(fromVec, throwVec);
                    //Game.PlayEffect("GLM", throwVec);

                    if (tieObject(target, fromVec, throwVec, power)) break;

                }
            }

			private void createTimedTether(IObject target, Vector2 tieFrom, Vector2 tieTo, IObject anchor, float power)
			{
				bool weakTether = rnd.NextDouble() < 0.5;
				bool cut = false;
				bool burning = false;
				string effect = elementEffects[(int)element];
				float initialFireDamage = 0;


				//Check for fire
				if(anchor.IsBurning)
				{
					power /= 1.5f;
					burning = true;
                }
				else if (target.IsBurning)
				{
					burning = true;
				}



				//create vines
                IObjectPullJoint tether = (IObjectPullJoint)Game.CreateObject("PullJoint", tieFrom);
				IObjectTargetObjectJoint targetJoint = (IObjectTargetObjectJoint)Game.CreateObject("TargetObjectJoint", tieTo);

				targetJoint.SetTargetObject(anchor);

				tether.SetTargetObject(target);
				tether.SetTargetObjectJoint(targetJoint);

				tether.SetLineVisual(LineVisual.DJVine);
				tether.SetForcePerDistance(0.0001f + power/500f * (weakTether? 0.5f : 1) * ((target is IPlayer) ? 1 : 5f));
				tether.SetForce(0.0002f);

				split++;

               // messageRoss("tied to " + anchor.Name);



				//keep track of fire damage overall to see if they caught fire during the trap
				if(target is IPlayer)
				{
					initialFireDamage += ((IPlayer)target).Statistics.TotalFireDamageTaken;
				}
				if(anchor is IPlayer)
				{
					initialFireDamage += ((IPlayer)anchor).Statistics.TotalFireDamageTaken;
				}


				Action deleteVine = () =>
				{
                    if (tether != null) Game.PlaySound("MeleeHitSharp", tether.GetWorldPosition(), 0.25f);
                    if (tether != null) tether.Remove();
                    if (targetJoint != null) targetJoint.Remove();
                };


				//detach vines after some time
                Events.UpdateCallback vineDespawn = null;
				Events.PlayerMeleeActionCallback vineCut = null;
				Events.ObjectCreatedCallback vineBroke = null;
                vineDespawn = Events.UpdateCallback.Start(e => {
					float newFireDamage = 0;
					vineCut.Stop();
					vineDespawn.Stop();
					vineBroke.Stop();

                    if (target == null ||target.IsRemoved || anchor == null || anchor.IsRemoved || tether == null || tether.IsRemoved || targetJoint == null || targetJoint.IsRemoved)
					{
						deleteVine();
						return;
					}
					if (target is IPlayer)
					{
						newFireDamage += ((IPlayer)target).Statistics.TotalFireDamageTaken;
                    }
					if (anchor is IPlayer)
					{
						newFireDamage += ((IPlayer)anchor).Statistics.TotalFireDamageTaken;
                    }

					if (newFireDamage > initialFireDamage)
					{
						burning = true; //this means the player burned at some point during the tether
						Game.PlaySound("Flamethrower", target.GetWorldPosition());
					}

					if (burning) effect = "FIRE";



					for (int i = 0; i < (int)(Vector2.Distance(tether.GetWorldPosition(), targetJoint.GetWorldPosition()) / 17) + 1; i++)
						Game.PlayEffect(effect, tether.GetWorldPosition() + (targetJoint.GetWorldPosition() - tether.GetWorldPosition()) * ((float)rnd.NextDouble()));



					//deal damage
					if (!cut && !burning)
					{
                        dealElementalDamage(anchor, bufferedDamage / split);
                        dealElementalDamage(target, bufferedDamage / split);
						if (target.GetHealth() <= 0) target.Destroy();
					}

					deleteVine();

                }, (uint)(400 * power * (weakTether? (rnd.NextDouble()*2 + 2) : 1) * ((target is IPlayer || anchor is IPlayer) ? 1 : 5f) * ((target.CustomID == "weakJoint") ? 0.15f : 1f)));
				if (target.CustomID == "weakJoint") messageRoss("is weak joint");
				//check if object was destroyed and needs to be revined
				vineBroke = Events.ObjectCreatedCallback.Start((IObject[] objs) =>
				{
					
					if ((target == null || target.IsRemoved) && anchor != null && !anchor.RemovalInitiated) { 
						if(!(anchor is IPlayer)) foreach (IObject obj in objs)
						{
                            if (obj.GetBodyType() == BodyType.Dynamic && obj.GetMaxHealth() > 1f && obj.CustomID != "vined" && !(obj is IPlayer) && Vector2.Distance(obj.GetWorldPosition(), tether.GetWorldPosition()) < RETETHER_REACH)
                            {//try to get the target debris
                                messageRoss("found vining, " + obj.UniqueID + ", " + obj.Name);

                                tether.SetForcePerDistance(tether.GetForcePerDistance() * 8f);
                                tether.SetWorldPosition(obj.GetWorldPosition());
                                tether.SetTargetObject(obj);
                                obj.CustomID = "vined";
								break;
                            }
						}

                        vineCut.Stop();
                        vineDespawn.Stop();
                        vineBroke.Stop();
                    }
				});

                //check if the vines are being cut
                vineCut = Events.PlayerMeleeActionCallback.Start((IPlayer ply, PlayerMeleeHitArg[] args) => 
				{
					if(ply.IsMeleeAttacking || ply.IsJumpAttacking)
					{
						if(ply.CurrentWeaponDrawn == WeaponItemType.Melee && checkSharpWeapon(ply.CurrentMeleeWeapon.WeaponItem))
                        {
							


                            Vector2 comparePos = ply.GetWorldPosition() + new Vector2(ply.FacingDirection * 15f, ply.IsCrouching? 0f : 8f);
							Vector2 vineCenter = (targetJoint.GetWorldPosition() - tether.GetWorldPosition())/2f + tether.GetWorldPosition();
							//messageRoss(vineCenter.ToString());
							Game.DrawCircle(vineCenter, 13f);
							Game.DrawLine(comparePos, vineCenter);
							Game.DrawLine(targetJoint.GetWorldPosition(), tether.GetWorldPosition(), Color.Red);
							if((ply.UniqueId == target.UniqueID && ply.FacingDirection * (vineCenter.X - ply.GetWorldPosition().X)> 0) ||
							Vector2.Distance(comparePos, vineCenter) < 13f)
                            {
                                Game.PlaySound("MeleeHitSharp", tether.GetWorldPosition(), 2f);
								cut = true;
                                vineDespawn.Invoke(100);
								return;
                            }
						}
					}

				});
            }
			public static bool checkSharpWeapon(WeaponItem weapon)
			{
				return weapon == WeaponItem.KATANA || weapon == WeaponItem.MACHETE || weapon == WeaponItem.AXE || weapon == WeaponItem.BROKEN_BOTTLE || weapon == WeaponItem.KNIFE || weapon == WeaponItem.CHAINSAW;

            }

			private const float RETETHER_REACH = 20f;
			private static bool reattachVine(IObjectPullJoint vine)
			{
				Vector2 pos = vine.GetWorldPosition();
				int highestID = getHighestID() - 5;
				messageRoss("highest ID: " + highestID);

                foreach (IObject obj in Game.GetObjectsByArea(new Area(pos - Vector2.One * RETETHER_REACH, pos + Vector2.One * RETETHER_REACH)))
				{
					if(obj.GetBodyType() == BodyType.Dynamic && obj.GetMaxHealth() > 1f && obj.CustomID != "vined" && !(obj is IPlayer) && obj.UniqueID > highestID) {//try to get the target debris
						messageRoss("found vining, " + obj.UniqueID + ", " + obj.Name);
						
						vine.SetForcePerDistance(vine.GetForcePerDistance() * 2f);
						vine.SetWorldPosition(obj.GetWorldPosition());
                        vine.SetTargetObject(obj);
                        obj.CustomID = "vined";
						return true;
					}
				}
				return false;
			}
			private bool tieObject(IObject target, Vector2 impactVec, Vector2 shootAt, float power){
                //messageRoss("tying " + target.Name);
                RayCastInput input = new RayCastInput();
				input.AbsorbProjectile = RayCastFilterMode.True;
				input.ClosestHitOnly = true;
				input.IncludeOverlap = false;
				input.ProjectileHit = RayCastFilterMode.True;
                RayCastResult[] results = Game.RayCast(target.GetWorldPosition(), shootAt, input);
                if (results.Length > 0 && results[0].Hit && results[0].HitObject != target){
                    createTimedTether(target, impactVec, results[0].Position, results[0].HitObject, power);
					dealElementalDamage(results[0].HitObject, Spell.damageDropOff(Vector2.Distance(target.GetWorldPosition(), results[0].Position), this.splash) * power / 2f);//divided by 2 for reduced vine damage
					
                    return true;
				}

				return false;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
