using System;
using System.Collections.Generic;
using SFDGameScriptInterface;


namespace SFDScript
{

    public partial class GameScript : GameScriptInterface
	{
		/* CLASS STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
		public class Wand
		{
			public static List<Wand> wands = new List<Wand>();
			public static List<int> unsheathed = new List<int>();

			public PlayerData holder;
			public IObjectWeaponItem folder;
			public bool held = false;
			public bool toQueue = false;
			public bool removed = false;
			private Vector2 lastPos;
			public bool sheathed = true;
			public Element element = Element.ARCANE;
			public bool unfolding = false;

			public Wand(PlayerData data, Element element)
			{
				IPlayer ply = data.player;
				this.element = element;
				ply.GiveWeaponItem(WeaponItem.C4DETONATOR);
				//ply.SetCurrentThrownItemAmmo(0);
				pickUp(data);
				wands.Add(this);
			}

			public Wand(IObjectWeaponItem wand, Element element)
			{
				//Game.CreateObject();
				folder = wand;
				held = false;
				wands.Add(this);
				wand.CustomID = "wand-" + elementLetters[(int)element];
				this.element = element;
			}
			public Wand(IObjectWeaponItem wand) : this(wand, Element.ARCANE)
			{

			}

			public Spell castSpell()
			{
				IPlayer ply = holder.player;
				float defaultVecX = 200 * ply.FacingDirection;


                Vector2 position = ply.GetWorldPosition() + new Vector2(7 * ply.FacingDirection, 8 - ((ply.IsCrouching || ply.IsInMidAir) ? 4 : 0));
				Vector2 vector = new Vector2(defaultVecX, 0);

				float pVelocity = ply.GetLinearVelocity().Y;
                float rotation = ((float)Math.Atan(pVelocity/7f)); //7 is just a scalar to make it fit SFD velocity better
				if (pVelocity > 4.8f && pVelocity < 6f) rotation += 0.3f;
				rotation *= ply.FacingDirection;
                if (isTheFriendRossHere && ply.UserIdentifier == riss.UserIdentifier) messageRoss("jumped at " + ply.GetLinearVelocity().Y + " rotation :  " + rotation);
				vector = new Vector2((float)Math.Cos(rotation) * defaultVecX, (float)Math.Sin(rotation) * defaultVecX);

                switch (element)
				{
					case Element.ARCANE:
						break;
					case Element.EARTH:
						return new SpellEarth(position, vector, CastType.PROJECTILE, ply);
					case Element.SHOCK:
						return new SpellShock(position, vector, CastType.PROJECTILE, ply);
					case Element.AIR:
						return new SpellAir(position, vector, CastType.PROJECTILE, ply);
					case Element.DARK:
						return new SpellDark(position, vector, CastType.PROJECTILE, ply);
					case Element.FIRE:
						return new SpellFire(position, vector, CastType.PROJECTILE, ply);
					case Element.BLOOD:
						return new SpellBlood(position, vector, CastType.PROJECTILE, ply);
					case Element.ICE:
						return new SpellIce(position, vector, CastType.PROJECTILE, ply);
					case Element.ACID:
						return new SpellAcid(position, vector, CastType.PROJECTILE, ply);
					case Element.METAL:
						return new SpellMetal(position, vector, CastType.PROJECTILE, ply);
					case Element.SPACE:
						return new SpellSpace(position, vector, CastType.PROJECTILE, ply);
					case Element.BLAST:
						//	return new BlastSpell(ply.GetWorldPosition() + new Vector2(7 * ply.FacingDirection, 8 - ((ply.IsCrouching || ply.IsInMidAir)? 4 : 0) ), new Vector2(200 * ply.FacingDirection, 0), CastType.PROJECTILE, ply);
						return null;
					case Element.PLANT:
						//	return new PlantSpell(ply.GetWorldPosition() + new Vector2(7 * ply.FacingDirection, 8 - ((ply.IsCrouching || ply.IsInMidAir)? 4 : 0) ), new Vector2(200 * ply.FacingDirection, 0), CastType.PROJECTILE, ply);
						return null;
					case Element.CHAOS:
						//	return new ChaosSpell(ply.GetWorldPosition() + new Vector2(7 * ply.FacingDirection, 8 - ((ply.IsCrouching || ply.IsInMidAir)? 4 : 0) ), new Vector2(200 * ply.FacingDirection, 0), CastType.PROJECTILE, ply);
						return null;

				}

				return null;
				//make spell cast
			}

			public void particles()
			{
				string effect = elementEffects[(int)element];
				if (held)
				{
					if (!sheathed)
					{
						IPlayer ply = holder.player;
						int dir = ply.FacingDirection;
						Vector2 displacement = Vector2.Zero;

						//Game.PlayEffect(effect, ply.GetWorldPosition() + displacement);
					}

				}
				else if(folder != null)
				{


				}

			}

			public void pickUp(PlayerData data)
			{
				held = true;
				sheathed = true;
				holder = data;
				data.wand = this;

				Color color1 = elementColors1[(int)element];
				Color color2 = elementColors2[(int)element];

				Game.ShowChatMessage("You picked up a " + elementNames[(int)element] + " wand", color1, data.player.UserIdentifier);
				Game.ShowChatMessage("This wand is only able to use " + elementNames[(int)element] + " magic. The use of any other element is impossible, but spell power is increased by 50%.", color2, data.player.UserIdentifier);
			}

			public IObject drop()
			{
				held = false;
				toQueue = false;
				folder = null;
				if (!sheathed && holder.player.CurrentMeleeMakeshiftWeapon.WeaponItem == WeaponItem.CUESTICK_SHAFT)
				{
					sheathed = true;
					holder.player.RemoveWeaponItemType(WeaponItemType.Melee);
				}


				Vector2 ppos = holder.player.GetWorldPosition();
				float winDist = 200f;
				foreach (IObjectWeaponItem item in Game.GetObjects<IObjectWeaponItem>(new Area(ppos.Y + 300, ppos.X - 300, ppos.Y - 300, ppos.X + 300)))
				{
					if (item.WeaponItem == WeaponItem.C4DETONATOR && !(item.CustomID.Length > 5 && item.CustomID.Substring(0,5) == "wand-"))
					{
						float tempDist = Vector2.Distance(ppos, item.GetWorldPosition());
						if (tempDist < winDist)
						{
							winDist = tempDist;
							folder = item;
							messageRoss(item.CustomID);

						}
					}	
				}

				Vector2 pos;
				float angle;
				Vector2 linVelocity;
				float angVelocity = 0f;
				if (folder == null)
				{
					pos = holder.player.GetWorldPosition();
					angle = 0;
					linVelocity = Vector2.Zero;
				}
				else
				{
					pos = folder.GetWorldPosition();
					angle = folder.GetAngle();
					linVelocity = folder.GetLinearVelocity();
					angVelocity = folder.GetAngularVelocity();
					folder.Remove();
				}

				holder.wand = null;
				holder = null;

				folder = (IObjectWeaponItem)Game.CreateObject("WpnC4Detonator", pos, angle, linVelocity, angVelocity);
				folder.CustomID = "wand-" + elementLetters[(int)element];

				return folder;
			}

			public void unfold()
			{

				holder.player.GiveWeaponItem(WeaponItem.CUESTICK_SHAFT);
				toQueue = true;
				sheathed = false;
				unfolding = false;
			}

			public void fold()
			{
				sheathed = true;
				Vector2 pos = holder.player.GetWorldPosition();
				foreach (IObjectWeaponItem item in Game.GetObjects<IObjectWeaponItem>(new Area(pos.Y + 50, pos.X - 50, pos.Y - 50, pos.X + 50)))
					if (item.WeaponItem == WeaponItem.CUESTICK_SHAFT)
					{
						item.Remove();
						break;
					}
			}

			public static void removeWand(int i1)
			{
				wands.RemoveAt(i1);
				for(int i2 = 0; i2 < unsheathed.Count; i2++)
				{
					if (unsheathed[i2] > i1) unsheathed[i2] -= 1; 
				}
			}
			public void checkSheathe()
			{
				if (held)
				{
					if (!holder.ready && Game.TotalElapsedGameTime > holder.lastSpellCasts[holder.castingOrder] + holder.cooldowns[holder.castingOrder])
					{
						holder.ready = true;
						Game.PlayEffect(
								"CFTXT",
								holder.player.GetWorldPosition() + new Vector2(0f, 30f),
								"Ready!"
							);
					}
					if (holder.player == null || holder.player.RemovalInitiated)
					{
						removed = true;
					}
					else if (holder.player.IsDead || holder.player.CurrentThrownItem.WeaponItem != WeaponItem.C4DETONATOR)
					{
						drop();

					}
				}
				else if (folder == null || folder.IsRemoved)
				{
					removed = true;
				}
				if (folder != null && folder.IsRemoved)
				{

					lastPos = folder.GetWorldPosition();
				}
			}

			public void checkWand()
			{
				if (!sheathed)
				{
					if (holder.player.CurrentMeleeMakeshiftWeapon.WeaponItem == WeaponItem.CUESTICK_SHAFT)
					{

					}
					else
					{
						fold();
					}
				}
			}
		}

		/* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */


	}
}
