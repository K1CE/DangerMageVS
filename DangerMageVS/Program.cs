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
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public GameScript() : base(null) { }

        /* SCRIPT STARTS HERE - COPY BELOW INTO THE SCRIPT WINDOW */
        //TODO:
        /*
         * particle effects framework for space wand
         * fix ice freeze?
         * give ice speedup syringe for blue glow
         * make elements put out fire like ice and air
         * fix fire damage
         * add bloodlust and hunger for blood magic
         * gib on kill for blood magic
         * add flying giblets for blood magic
         * thwakc spinning animation
         * insane idea: sawblades move across surfaces
         * fix crash on death
         * 
         * 
         * */

        #region Startup Calls

        Events.UserMessageCallback m_userMessageCallback = null;

        static Random rnd = new Random();
        Events.PlayerKeyInputCallback m_playerKeyInputEvent = null;
        public static IObjectTimerTrigger unfreezer;
        public const string STARTWANDS_KEY = "START WANDS";

        public static string[] elementNames = new String[] { "", "earth", "shock", "air", "dark", "fire", "blood", "ice", "acid", "metal", "space", "blast", "plant", "chaos" };
        public static char[] elementLetters = new char[] { ' ', 'p', 'k', 'f', 'z', 'r', 'g', 's', 'l', 't', 'w', 'c', 'm', '&' };
        public static string[] elementSounds = new string[]{
        "BowNoAmmo", "BulletHitStone", "ElectricSparks", "Throw", "Madness", "Flamethrower", "PlayerGib", "DestroyGlass", "ChainSwing", "BulletHitMetal", "StrengthBoostStart",
        "Bazooka", "StrengthBoostStart", "Bazooka"
        };
        public static string[] elementEffects = new string[]{
        "GLM",//simple
        "BulletHitDirt",//earth
        "Electric",//shock
        "STM",//air
        "TR_S",//dark
        "FIRE",//fire
        "BLD",//blood
        "DestroyGlass",//ice
        "ACS",//acid
        "HIT_S",//metal
        "Block",//space
        "S_P",//blast
        "BulletHitMoney",//plant
        "CSW" //chaos

        };
        public static Color[] elementColors1 = new Color[]{
        new Color(132,132,255), new Color(255,220,135),new Color(0,255,255),new Color(89,255,177),new Color(100,100,100),new Color(255,0,0),new Color(255,0,127),
        new Color(86,131,255),new Color(90,255,0),new Color(194,194,255),new Color(182,0,255),new Color(255,110,0),new Color(0,255,42),new Color(255,0,203)
    };
        public static Color[] elementColors2 = new Color[]{
        new Color(75,75,150), new Color(84,66,45),new Color(45,84,84),new Color(45,84,66),new Color(64,64,64),new Color(84,45,45),new Color(84,45,55),
        new Color(52,61,81),new Color(60,81,44),new Color(82,82,104),new Color(73,45,84),new Color(84,59,45),new Color(45,84,48),new Color(255,89,66)
    };



        public static List<PlayerData> players = new List<PlayerData>();

        public enum Element
        {
            ARCANE,
            EARTH,
            SHOCK,
            AIR,
            DARK,
            FIRE,
            BLOOD,
            ICE,
            ACID,
            METAL,
            SPACE,
            BLAST,
            PLANT,
            CHAOS

        }

        public enum CastType
        {
            TOUCH,
            GRENADE,
            DISCHARGE,
            PROJECTILE,
            SELF,
            HOVER,
            CURSE,
            RAIN,
            AREA,
            BULLET,
            COORDINATE,
            SHOTGUN,
            SYNERGY,
            PILLARS,
            CHEAT
        }

        //add flame bullet after fire clear? (to add a smaller fire)
        const double FIRE_PER_SECOND = 27.60028;

        public static List<Cast> casts = new List<Cast>();
        public const string attachmentID = "att";
        public static List<CastProjectile> projectiles = new List<CastProjectile>();

        public static void hitBoxImpact(TriggerArgs args)
        {
            if (args.Sender is IObject && !((IObject)args.Caller).RemovalInitiated)
            {
                CastProjectile prj = CastProjectile.projectileFromID(((IObject)(args.Caller)).CustomID);
                IObject sent = (IObject)args.Sender;
                prj.position = prj.hitBox.GetWorldPosition();
                if (sent is IPlayer)
                {

                    if (!sent.Equals(prj.spell.caster) && !((IPlayer)sent).IsDead)
                    {
                        prj.position = prj.hitBox.GetWorldPosition();
                        prj.hit(sent);
                    }
                }
                else if (sent.GetCollisionFilter().BlockFire)
                {


                    Vector2 pos = prj.hitBox.GetWorldPosition();
                    RayCastInput input = new RayCastInput(true);
                    input.BlockExplosions = RayCastFilterMode.True;
                    input.IncludeOverlap = true;
                    RayCastResult outPut = Game.RayCast(pos, prj.targetJoint.GetWorldPosition(), input)[0];
                    if (outPut.Hit && Vector2.Distance(outPut.Position, pos) < 2f)
                    {
                        prj.hit(outPut.HitObject);
                       // if (outPut.HitObject.GetMaxHealth() != 1) 
                       // else prj.hit(null);


                    }
                }
                else if (sent.GetMaxHealth() != 1)
                {
                    prj.passiveEffect(sent);

                }
                if (sent.Name.Contains("GlassSheet")) sent.Destroy();
            }
        }

        public static void rangedOut(TriggerArgs args)
        {
            CastProjectile prj = CastProjectile.projectileFromID(((IObject)args.Caller).CustomID);
            if (prj != null)
            {
                IObject targ = null;
                Vector2 pos = (prj.hitBox.GetWorldPosition());
                foreach (IObject obj in Game.GetObjectsByArea(new Area(pos.Y + 4f, pos.X - 4f, pos.Y - 4f, pos.X + 4f)))
                    if (obj.CustomID != attachmentID && obj.GetMaxHealth() != 1)
                    {
                        targ = obj;
                        break;
                    }
                prj.hit(targ);
            }
        }

        /*
		PlayerModifiers modify = new PlayerModifiers();
				modify.CurrentHealth = 100;
				ply.SetModifiers(modify);
		*/


        public void OnStartup()
        {

            m_playerKeyInputEvent = Events.PlayerKeyInputCallback.Start(OnPlayerKeyInput);

            rossColor = new Color(255, 65, 49);

            int availableElements = 9;
            int cap = 0;

            int wands = rnd.Next(2) + (int)(Game.GetPlayers().Count()/2);
            wands = 0;
            if (wands <= 0) wands = 1;

            Game.RunCommand("/msg " + wands + " wands");

            //doesn't give wands unless the setting is enabled
            giveStartWands();

            while (wands > 0)
            {
                IObject[] areas = Game.GetObjects<IObjectSpawnWeaponArea>();
                IObject obj = areas[rnd.Next(areas.Count())];
                Vector2 spot = obj.GetWorldPosition() + new Vector2(rnd.Next(obj.GetSizeFactor().X) * 8, 0);
                new Wand((IObjectWeaponItem)Game.CreateObject("WpnC4Detonator", spot), (Element)(rnd.Next(availableElements - cap) + 1 + cap));
                wands--;
            }

            unfreezer = (IObjectTimerTrigger)CreateTimer(15000, 1, "unfreeze", "4");
            unfreezer.SetActivateOnStartup(false);

            foreach (IPlayer ply in Game.GetPlayers())
            {
                new PlayerData(ply);
                if (ply.GetUser().AccountID == "S125950250")
                {
                    riss = ply.GetUser();
                    isTheFriendRossHere = true;
                }
            }

            for (int i = 0; i < 14; i++)
                foreach (IObjectWeaponItem wand in Game.GetObjectsByCustomID<IObjectWeaponItem>("wand-" + elementLetters[i]))
                {
                    new Wand(wand, (Element)i);
                }


            CreateTimer(1000, 0, "slowTick", "0");
            CreateTimer(300, 0, "fastTick", "1");
            CreateTimer(50, 0, "effectTick", "3");




            m_userMessageCallback = Events.UserMessageCallback.Start(OnUserMessage);

        }

        public void slowTick(TriggerArgs args)
        {
            for (int i = 0; i < Wand.wands.Count; i++)
            {
                Wand wand = Wand.wands[i];
                wand.checkSheathe();
                if (wand.toQueue)
                {
                    IPlayer ply = wand.holder.player;
                    PlayerModifiers modify = ply.GetModifiers();
                    wand.holder.savedMeleeDamage = modify.MeleeDamageDealtModifier;
                    modify.MeleeDamageDealtModifier = 0.3f;
                    ply.SetModifiers(modify);


                    Wand.unsheathed.Add(i);
                    wand.toQueue = false;
                }
                if (wand.removed)
                {
                    Wand.wands.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < players.Count; i++)
            {
                PlayerData data = players[i];
                if (data.player == null || data.player.IsRemoved)
                {
                    if (data.wand != null) data.wand.removed = true;
                    players.RemoveAt(i);
                    i--;
                }
                else
                {
                    data.corruptionCheck();
                }

            }

        }

        public void fastTick(TriggerArgs args)
        {
            for (int i = 0; i < Wand.unsheathed.Count; i++)
            {
                Wand wand = Wand.wands[Wand.unsheathed[i]];
                if (!wand.sheathed)
                {
                    wand.checkWand();
                }
                else if (wand.held && wand.holder.player != null && !wand.holder.player.IsDead)
                {
                    if (wand.holder != null)
                    {
                        IPlayer ply = wand.holder.player;
                        PlayerModifiers modify = ply.GetModifiers();
                        modify.MeleeDamageDealtModifier = wand.holder.savedMeleeDamage;
                        ply.SetModifiers(modify);
                    }
                    Wand.unsheathed.RemoveAt(i);
                    i--;
                }
                else
                {
                    Wand.unsheathed.RemoveAt(i);
                    i--;
                }
            }
            foreach(Wand wand in Wand.wands)
            {
                if(wand != null)
                {
                    wand.particles();
                }
            }
            foreach (PlayerData data in players)
            {
                if (data.player != null)
                {
                    if (data.cold) data.coldCheck();
                    if (data.recovering && data.lastHealth > data.player.GetHealth()) data.recover(); //for shock

                    if(data.electrocuted) data.effectsBody(Element.SHOCK);
                      
                    
                }
            }
        }

        public void effectTick(TriggerArgs args)
        {
            foreach (Cast cast in casts)
            {
                cast.particleTick();
            }
        }

        //TODO: theres a chance for wands to duplicate

        public void OnPlayerKeyInput(IPlayer player, VirtualKeyInfo[] keyEvents)
        {
            PlayerData data = dataFromPlayer(player);
            for (int i = 0; i < keyEvents.Length; i++)
            {
                if (player.CurrentThrownItem.WeaponItem == WeaponItem.C4DETONATOR && keyEvents[i].Event == VirtualKeyEvent.Pressed && keyEvents[i].Key == VirtualKey.ACTIVATE)
                {
                    foreach (Wand wnd in Wand.wands)
                    {
                        if (!wnd.held && wnd.folder.IsRemoved)
                        {
                            Wand equip = data.wand;
                            if (equip != null)
                            {
                                if (!equip.sheathed)
                                {
                                    equip.sheathed = true;
                                    player.RemoveWeaponItemType(WeaponItemType.Melee);

                                    IPlayer ply = equip.holder.player;
                                    PlayerModifiers modify = ply.GetModifiers();
                                    modify.MeleeDamageDealtModifier = equip.holder.savedMeleeDamage;
                                    ply.SetModifiers(modify);
                                }
                                equip.drop();
                            }

                            wnd.pickUp(data);

                            break;
                        }
                    }
                }
                if (data.wand != null)
                {
                    if (keyEvents[i].Key == VirtualKey.ATTACK)
                    {
                        if (keyEvents[i].Event == VirtualKeyEvent.Released)
                        {
                            if (player.CurrentWeaponDrawn == WeaponItemType.Thrown
                                    && player.CurrentThrownItem.WeaponItem == WeaponItem.C4DETONATOR
                                    && !player.IsThrowing
                                    && !player.IsManualAiming)
                            {
                                Wand wnd = data.wand;

                                if (wnd.sheathed && !wnd.unfolding)
                                {
                                    wnd.unfolding = true;
                                    buttonQueue.Add(wnd);
                                    data.unfoldPause.Trigger();
                                }
                            }
                            if (!data.wand.sheathed && player.CurrentMeleeMakeshiftWeapon.WeaponItem == WeaponItem.CUESTICK_SHAFT && (player.IsMeleeAttacking || player.IsJumpAttacking))
                            {
                                if (player.IsThrowing)
                                {
                                    data.wand.sheathed = true;
                                    player.RemoveWeaponItemType(WeaponItemType.Melee);
                                }
                                else
                                {
                                    data.castSpell();
                                    player.SetCurrentMeleeMakeshiftDurability(1.0f);
                                }
                            }
                        }
                    }
                    if (!data.wand.sheathed)
                    {
                        if (player.CurrentMeleeMakeshiftWeapon.WeaponItem != WeaponItem.CUESTICK_SHAFT)
                        {
                            data.wand.fold();
                        }
                    }
                }
                Game.WriteToConsole(string.Format("Player {0} keyevent: {1}", player.UniqueID, keyEvents[i].ToString()));
            }
        }


        public static List<Wand> buttonQueue = new List<Wand>();
        public void delayedUnfold(TriggerArgs args)
        {
            if (buttonQueue.Count() > 0)
            {
                if (buttonQueue[0].held) buttonQueue[0].unfold();
                buttonQueue.RemoveAt(0);
            }
        }

        public static List<PlayerData> spellQueue = new List<PlayerData>();
        public void delayedCast(TriggerArgs args)
        {
            if (spellQueue.Count() > 0)
            {
                PlayerData data = spellQueue[0];
                spellQueue.RemoveAt(0);


            }
        }

        public static List<PlayerData> stunQueue = new List<PlayerData>();
        public void recoveryTimer(TriggerArgs args)
        {
            if (stunQueue.Count() > 0)
            {
                stunQueue[0].recover();
                stunQueue.RemoveAt(0);


            }
        }

        public static List<PlayerData> fireQueue = new List<PlayerData>();
        public void fireRecovery(TriggerArgs args)
        {
            if (fireQueue.Count() > 0)
            {
                IPlayer ply = fireQueue[0].player;
                ply.ClearFire();

                //messageRoss("fire damage after: " + ply.Statistics.TotalFireDamageTaken);

                if (!(ply.IsRolling || ply.IsDiving))
                {
                    IProjectile prj = Game.SpawnProjectile(ProjectileItem.PISTOL, ply.GetWorldPosition(), Vector2.Zero);
                    prj.CritChanceDealtModifier = 0f;
                    prj.DamageDealtModifier = 0f;
                    prj.PowerupFireActive = true;
                    fireQueue.RemoveAt(0);
                }

            }
        }


        public static List<IObjectWeldJoint> unfreezeQueue = new List<IObjectWeldJoint>();
        public void unfreeze(TriggerArgs args)
        {
            foreach (IObjectWeldJoint frozen in unfreezeQueue)
            {
                if (frozen.GetTargetObjects().Count() > 0)
                    foreach (IObject frzn in frozen.GetTargetObjects())
                    {
                        if (frzn.CustomID == "iceBit") frzn.Destroy();
                        frozen.RemoveTargetObject(frzn);

                    }
                frozen.Remove();
            }
            unfreezeQueue = new List<IObjectWeldJoint>();
        }

        public static PlayerData dataFromPlayer(IPlayer ply)
        {
            int idIn = ply.UniqueID;
            foreach (PlayerData data in players)
            {
                if (data.id == idIn) return data;
            }
            return null;
        }

        //taken from Odex. Used in Commands+
        public IUser Match(string cmds)
        {
            if (cmds != "")
            {
                if (cmds.All(char.IsDigit))
                {
                    foreach (IUser user in Game.GetActiveUsers())
                    {
                        if (user.GameSlotIndex == Convert.ToInt32(cmds))
                            return user;
                    }
                }
                for (int i = 0; i < Game.GetActiveUsers().Count(); i++)
                {
                    if (Game.GetActiveUsers()[i].Name.ToLowerInvariant().Contains(cmds))
                        return Game.GetActiveUsers()[i];
                }
            }
            return null;
        }

        public void setStartWands(bool startWands) {
            Game.SessionStorage.SetItem(STARTWANDS_KEY, startWands);
        }

        public void giveStartWands() {
            bool startWands = false;
            if (!(Game.SessionStorage.TryGetItemBool(STARTWANDS_KEY, out startWands) && startWands)) return;
            
            foreach (IPlayer ply in Game.GetPlayers()) {
                if (ply.GetUser().IsBot) continue;

                PlayerData data = dataFromPlayer(ply);
                if (data == null) {
                    data = new PlayerData(ply);
                }
                new Wand(data, (Element)rnd.Next(elementNames.Length - 5) + 1);
            }
        }

        public void OnUserMessage(UserMessageCallbackArgs args)
        {
            // user just said something in the chat.
            if (args.IsCommand && (args.User.IsHost || args.User.IsModerator))
            {
                switch (args.Command)
                {
                    case "SETSTARTWANDS": 
                        {
                            string[] argsPieces = args.CommandArguments.ToLower().Split(' ');

                            //messageRoss("len " + argsPieces.Length);
                            bool startWands;

                            if (argsPieces[0] == "") startWands = true;
                            else startWands = argsPieces[0].ToLower() != "false" && Convert.ToInt32(argsPieces[0]) != 0;

                            Game.ShowChatMessage("Start wands " + (startWands? "enabled" : "disabled"), new Color(33, 133, 33), args.User.UserIdentifier);

                            setStartWands(startWands);

                            break;
                        }

                    case "GIVEWAND":
                        {
                            string[] argsPieces = args.CommandArguments.ToLower().Split(' ');
                            if (argsPieces[0] == "") break;

                            IUser user = Match(argsPieces[0]);
                            if (user.GetPlayer() != null)
                            {
                                IPlayer ply = user.GetPlayer();
                                Element element = Element.ARCANE;
                                int wandType = 0;
                                //messageRoss(user.Name + "got wand");

                                int max = (argsPieces.Length > 3) ? 3 : argsPieces.Length;
                                for (int i = 1; i < max; i++)
                                {
                                    String piece = argsPieces[i];
                                    if (piece.All(char.IsDigit))
                                    {
                                        element = (Element)Convert.ToInt32(piece);

                                    }
                                    else
                                    {
                                        if (piece == "bulky") wandType = 1;
                                        else if (piece == "staff") wandType = 2;
                                        for (int a = 0; a < elementNames.Length; a++)
                                        {
                                            if (piece == elementNames[a])
                                            {
                                                element = (Element)a;
                                                break;
                                            }
                                        }
                                    }
                                }
                                PlayerData picker = dataFromPlayer(ply);
                                if (picker == null)
                                {
                                    picker = new PlayerData(ply);
                                }
                                else if (picker.wand != null) picker.wand.drop();
                                new Wand(picker, element);
                            }
                            break;
                        }

                    case "CAST":
                        {
                            string[] argsPieces = args.CommandArguments.ToLower().Split(' ');

                            if (argsPieces.Length < 2 || argsPieces.Length > 4)
                            {
                                break;
                            }

                            IUser user = Match(argsPieces[0]);
                            if (user.GetPlayer() != null)
                            {
                                IPlayer target = user.GetPlayer();
                                Element element = Element.ARCANE;
                                float power = 0;
                                bool useOriginal = false;


                                String piece = argsPieces[1];
                                if (piece.All(char.IsDigit))
                                {
                                    element = (Element)Convert.ToInt32(piece);
                                }
                                else
                                {
                                    for (int a = 0; a < elementNames.Length; a++)
                                    {
                                        if (piece == elementNames[a])
                                        {
                                            element = (Element)a;
                                            break;
                                        }
                                    }
                                }
                                if (element == Element.ARCANE) break;
                                if (argsPieces.Length > 2)
                                {
                                    piece = argsPieces[2];
                                    int splitAt = piece.IndexOf('.');
                                    int additional = 0;

                                    if (splitAt != -1)
                                    {
                                        string split = piece.Substring(splitAt + 1);
                                        piece = piece.Substring(0, splitAt);

                                        if (split.All(char.IsDigit)) additional = Convert.ToInt32(split);
                                    }
                                    if (piece.All(char.IsDigit))
                                    {
                                        float decimals = (float)(additional / Math.Pow(10, argsPieces[2].Length - piece.Length - 1));
                                        messageRoss("were your decimals: " + decimals + "?");
                                        power = Convert.ToInt32(piece) + decimals;
                                    }


                                }
                                else useOriginal = true;



                             
                                
                                IPlayer caster;
                                if (args.User.GetPlayer() != null && !args.User.GetPlayer().IsRemoved) caster = args.User.GetPlayer();
                                else caster = target;

                                SpellArguments spellArg = new SpellArguments();
                                spellArg.argObject = target;
                                Spell spell = null;
                                if (element == Element.EARTH)
                                    spell = new SpellEarth(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                
                                else
                                if (element == Element.SHOCK)
                                    spell = new SpellShock(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.AIR)
                                    spell = new SpellAir(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.DARK)
                                    spell = new SpellDark(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.FIRE)
                                    spell = new SpellFire(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.BLOOD)
                                    spell = new SpellBlood(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.ICE)
                                    spell = new SpellIce(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.ACID)
                                    spell = new SpellAcid(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.METAL)
                                    spell = new SpellMetal(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);
                                else
                                if (element == Element.SPACE)
                                    spell = new SpellSpace(Vector2.Zero, Vector2.Zero, CastType.CHEAT, caster, spellArg);

                                if (!useOriginal) spell.spellPower = power;
                            }
                           
                            break;
                        }
                }

                // Do something specific for ABC
                if (args.CommandArguments == "1")
                {
                    // Do stuff when user types "/ABC 1"
                }
                else if (args.CommandArguments == "2")
                {
                    // Do stuff when user types "/ABC 2"
                }
            }
        }

        public static bool cantMeleeDamage(IObject target)
        {
            return target.Name == "BarrelExplosive" || target.Name == "BarrelWreck" || target.Name == "Gascan00" || target.Name == "PropaneTank" || target.Name == "Spotlight00A" ||
                target.Name == "Spotlight00AWeak" || target.Name == "WpnMineThrown" || target.Name == "WpnGrenadesThrown" || target.Name == "WpnC4Thrown" || target.Name.Contains("Lamp") || target.Name.Contains("pulley") ||
                target.Name == "HangingCrateHolder";
        }

        static bool isTheFriendRossHere = false;
        static Color rossColor;
        static IUser riss;
        public static void messageRoss(String message)
        {
            if (isTheFriendRossHere)
                Game.ShowChatMessage(message, rossColor, riss.UserIdentifier);
        }


        public static void testHealth(IObject obj, int time)
        {
            CreateTimer(time, 1, "healthCheck", "5");
            healthCheckQueue.Add(obj);
            messageRoss("before: " + obj.GetHealth());

        }
        public static void testHealth(IObject obj)
        {
            testHealth(obj, 100);
        }

        public static List<IObject> healthCheckQueue = new List<IObject>();

        public void healthCheck(TriggerArgs args)
        {
            if (healthCheckQueue.Count() > 0)
            {
                IObject obj = healthCheckQueue[0];
                messageRoss("after: " + obj.GetHealth());
                healthCheckQueue.RemoveAt(0);


            }
        }

        public static IObjectTimerTrigger CreateTimer(int interval, int count, string method, string id)
        {
            IObjectTimerTrigger timerTrigger = (IObjectTimerTrigger)Game.CreateObject("TimerTrigger");
            timerTrigger.SetIntervalTime(interval);
            timerTrigger.SetRepeatCount(count);
            timerTrigger.SetScriptMethod(method);
            timerTrigger.CustomId = id;
            timerTrigger.Trigger();
            return timerTrigger;
        }

        public void AfterStartup()
        {

        }

        #endregion

        /* SCRIPT ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
