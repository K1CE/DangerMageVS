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
			//TODO: check if vined target catches fire during effect?
			//TODO: make vines spread even without a target
			//TODO: metal wand cuts vines
			//TODO: make wand heal if vines are on self
			//TODO: fix sometimes vines break at 0,0 when object breaks during
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
						effectivePower = effectivePower * data.acidDamageTaken;
					}
				}

				for (int i = 0; i < splash / 2f; i++)
				{
					for(int j = 0; j < 5; j++) //do 5 attempts to find a tie vector
					{
						float range = splash * 3;

						Vector2 throwVec = impactPos + new Vector2((float)Math.Cos(rnd.NextDouble() * Math.PI*2) * range, 
							(float)Math.Sin(rnd.NextDouble() * Math.PI * 2) * range);

						Game.DrawLine(impactPos, throwVec);
						//Game.PlayEffect("GLM", throwVec);

						if( tieObject(target, impactPos, throwVec, effectivePower)) break;
						
					}
				}

                if (!cantMeleeDamage(target))
                    target.DealDamage(effectivePower * (2f/3f) * ((target is IPlayer) ? 1 : 4f), caster.UniqueID);//1/3 of the damage is done by vines

				bufferedDamage = effectivePower * (1f / 3f);


            }

			public override void explode(Cast sender, IObject alreadyHit, Vector2 position) 
            {
				//base.explode(sender, alreadyHit, position);


            }

			protected override void setUpStats()
			{
				spellPower = 9f;
				cooldown = 2900;
				speed = 6.7f;
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




				//detach vines after some time
                Events.UpdateCallback despawn = null;
				Events.PlayerMeleeActionCallback vineCut = null;
                despawn = Events.UpdateCallback.Start(e => {
					float newFireDamage = 0;
                    if (target is IPlayer)
                    {
                        newFireDamage += ((IPlayer)target).Statistics.TotalFireDamageTaken;
                    }
                    if (anchor is IPlayer)
                    {
                        newFireDamage += ((IPlayer)anchor).Statistics.TotalFireDamageTaken;
                    }

					if(newFireDamage > initialFireDamage)
					{
						burning = true; //this means the player burned at some point during the tether
						Game.PlaySound("Flamethrower", target.GetWorldPosition());
					}

                    if (burning) effect = "FIRE";
                    

					if(tether != null && !tether.RemovalInitiated && targetJoint != null && !targetJoint.RemovalInitiated)
						for (int i = 0; i < (int)(Vector2.Distance(tether.GetWorldPosition(), targetJoint.GetWorldPosition())/17) + 1; i++)
							Game.PlayEffect(effect, tether.GetWorldPosition() + (targetJoint.GetWorldPosition() - tether.GetWorldPosition())*((float)rnd.NextDouble()));



                    //deal damage
                    if (!cut && !burning)
					{
						if (target != null && !cantMeleeDamage(target))
						{
							target.DealDamage(bufferedDamage / split * ((target is IPlayer)? 1 : 4f));
						}
						if (anchor != null && !cantMeleeDamage(anchor))
						{
							anchor.DealDamage(bufferedDamage / split * ((target is IPlayer) ? 1 : 4f));
						}
					}


                    if(tether!=null) Game.PlaySound("MeleeHitSharp", tether.GetWorldPosition(), 0.25f);
                    if(tether != null)tether.Remove();
                    if(targetJoint != null)targetJoint.Remove();
                    vineCut.Stop();
                    despawn.Stop();
                }, (uint)(400 * power * (weakTether? (rnd.NextDouble()*2 + 2) : 1) * ((target is IPlayer || anchor is IPlayer) ? 1 : 5f)));

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
                                despawn.Invoke(100);
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
					if (!cantMeleeDamage(results[0].HitObject)){

                        results[0].HitObject.DealDamage(Spell.damageDropOff(Vector2.Distance(target.GetWorldPosition(), results[0].Position), this.splash) * power/2f, caster.UniqueID); //divided by 2 for reduced vine damage
					}
                    return true;
				}

				return false;
			}

		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
