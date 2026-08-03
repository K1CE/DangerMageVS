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

        class FrozenPlayer
        {

            private IObjectWeldJoint weld;
            private IObject invisibleBlock;
            private IObjectPlayerProfileInfo profileInfo;
            private IObjectPlayerPortrait skin;

            private List<IObjectText> pixels = new List<IObjectText>();
            public FrozenPlayer(IPlayer ply, Vector2 blastDirection)
            {


                blastDirection.Normalize(); //redundant but safe
                Vector2 position = new Vector2(ply.FacingDirection * 1.5f, 8) + ply.GetWorldPosition();

                IProfile playerProfile = ply.GetProfile();

                weld = (IObjectWeldJoint)Game.CreateObject("WeldJoint");

                invisibleBlock = Game.CreateObject("InvisibleBlock", position + new Vector2(0, 8));
                invisibleBlock.SetWorldPosition(position);
                invisibleBlock.SetBodyType(BodyType.Dynamic);
                invisibleBlock.SetSizeFactor(new Point(0, 2));
                invisibleBlock.SetMass(0.001f);
                weld.AddTargetObject(invisibleBlock);



                profileInfo = (IObjectPlayerProfileInfo)Game.CreateObject("PlayerProfileInfo");
                IProfile objectProfile = profileInfo.GetProfile();
                objectProfile.Feet = playerProfile.Feet;
                objectProfile.Accessory = playerProfile.Accessory;
                objectProfile.Skin = playerProfile.Skin;
                objectProfile.Gender = playerProfile.Gender;
                objectProfile.ChestOver = playerProfile.ChestOver;
                objectProfile.Hands = playerProfile.Hands;
                objectProfile.ChestUnder = playerProfile.ChestUnder;
                objectProfile.Legs = playerProfile.Legs;
                objectProfile.Head = playerProfile.Head;
                objectProfile.Waist = playerProfile.Waist;

                skin = (IObjectPlayerPortrait)Game.CreateObject("BgPlayerPortrait00", position);
                skin.SetFaceDirection(ply.FacingDirection);
                skin.SetProfileInfo(profileInfo);
                skin.SetBodyType(BodyType.Dynamic);
                skin.SetMass(0.001f);
                weld.AddTargetObject(skin);


                for (int icicle = rnd.Next(4) + 7; icicle > 0; icicle--)
                {
                    Vector2 startPos = ply.GetWorldPosition() + new Vector2((float)rnd.NextDouble() * 14 - 7, (float)rnd.NextDouble() * 14 - 4);
                    float startScale = 1 + rnd.Next(9) / 5f;
                    Game.PlayEffect("GLM", startPos);
                    int segments = rnd.Next(1) + 4;

                    for (int i = 0; i < segments; i++)
                    {
                        float scale = startScale - (startScale / (segments + 2)) * i;
                        Vector2 segmentPos = startPos + blastDirection * i * (startScale / 1.2f);

                        IObjectText pixel = (IObjectText)Game.CreateObject("Text", segmentPos + textPixelOffset * scale);
                        pixel.SetTextScale(scale);
                        pixel.SetText(".");
                        pixel.SetTextColor(Color.White);

                        pixel.SetBodyType(BodyType.Dynamic);
                        weld.AddTargetObject(pixel);

                        pixels.Add(pixel);

                    }

                }


                ply.Remove();
            }

            public void update()
            {
                //check for fire

                //check for tilt
            }

            public void destroy()
            {
                //spawn particles effects
                Vector2 pos = invisibleBlock.GetWorldPosition();
                for(int i = 0; i < 18; i++)
                {
                    Game.PlayEffect(rnd.Next(2) == 1 ? "WS" : "STM", pos + new Vector2((float)(10 * rnd.NextDouble() - 5)), (float)(10 * rnd.NextDouble() - 7));
                }

                remove();

            }

            public void thaw()
            {
                //release player from prison

                remove();
            }

            public void remove()
            {
                weld.Remove();
                invisibleBlock.Remove();
                profileInfo.Remove();
                skin.Remove();

                foreach (IObjectText pixel in pixels)
                {
                    pixel.Remove();
                }
            }

        }

        //TODO: add box in the sky to store players

        /* CLASS ENDS HERE - COPY ABOVE INTO THE SCRIPT WINDOW */

    }
}
