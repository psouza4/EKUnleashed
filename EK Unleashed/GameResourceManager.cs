using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json.Linq;
using System.Drawing.Text;

namespace EKUnleashed
{
    class GameResourceManager
    {
        private GameResourceManager() { }

        private static double _ImageScalingSize = -1.0;
        private static double ImageScalingSize
        {
            get
            {
                if (_ImageScalingSize != -1)
                    return _ImageScalingSize;

                _ImageScalingSize = Utils.CDbl(Utils.GetAppSetting("Dev_ImageScalingSize"));

                if (_ImageScalingSize == 0.0)
                    _ImageScalingSize = 75.0;

                return _ImageScalingSize;
            }
        }

        public static Image LoadResource_FullName(string name)
        {
            Image i = Utils.LoadImageFromResource("Resources." + name);
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_Common(string name)
        {
            Image i = Utils.LoadImageFromResource("Resources.common.CommonComposite_" + name + ".png");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_StarImage()
        {
            return LoadResource_Common("Star");
        }

        public static Image LoadResource_SilverNumber(int digit)
        {
            return LoadResource_Common("NumberSilver" + digit.ToString());
        }

        public static Image LoadResource_GoldNumber(int digit)
        {
            return LoadResource_Common("NumberGold" + digit.ToString());
        }

        public static Image LoadResource_CardBlank()
        {
            Image i = Utils.LoadImageFromResource("Resources.common.Card_Blank.jpg");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_RuneBlank()
        {
            Image i = Utils.LoadImageFromResource("Resources.common.Rune_Blank.jpg");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_CommonShader()
        {
            return LoadResource_Common("ShaderFadeLeft");
        }

        public static Image LoadResource_CardATK()
        {
            Image i = Utils.LoadImageFromResource("Resources." + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CardComposite_ATK.png");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_CardHP()
        {
            Image i = Utils.LoadImageFromResource("Resources." + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CardComposite_HP.png");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadResource_CardWaitHourglass()
        {
            Image i = Utils.LoadImageFromResource("Resources." + ((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CardComposite_WaitHourglass.png");
            if (i != null)
            {
                Image b_cloned = (Image)i.Clone();
                i.Dispose();

                return b_cloned;
            }

            return null;
        }

        public static Image LoadCardArtwork(int id)
        {
            try
            {
                if (!Directory.Exists(Utils.AppFolder + "\\ResourceCache"))
                    Directory.CreateDirectory(Utils.AppFolder + "\\ResourceCache");

                if (File.Exists(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_CardArtwork_" + id.ToString("0000") + ".jpg"))
                    return Utils.LoadImageFromDisk(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_CardArtwork_" + id.ToString("0000") + ".jpg");

                byte[] bImageData = new byte[] { };

                using (Image iDownloadNewImage = Utils.DownloadImage(GameClient.Current.Game_CDN_URL + "public/swf/card/370_570/img_maxCard_" + id.ToString() + ".jpg", ref bImageData))
                {
                    if (bImageData.Length >= 100)
                    {
                        File.WriteAllBytes(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_CardArtwork_" + id.ToString("0000") + ".jpg", bImageData);

                        if (File.Exists(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_CardArtwork_" + id.ToString("0000") + ".jpg"))
                            return Utils.LoadImageFromDisk(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_CardArtwork_" + id.ToString("0000") + ".jpg");
                    }
                }
            }
            catch { }

            Image iBadImage = new Bitmap(370, 530, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(iBadImage))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.FillRectangle(new SolidBrush(Color.FromArgb(18, 29, 33)), 0, 0, iBadImage.Width, iBadImage.Height);
            }

            return iBadImage;
        }

        public static Image LoadRuneArtwork(int id)
        {
            try
            {
                if (!Directory.Exists(Utils.AppFolder + "\\ResourceCache"))
                    Directory.CreateDirectory(Utils.AppFolder + "\\ResourceCache");

                if (File.Exists(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_RuneArtwork_" + id.ToString("0000") + ".png"))
                    return Utils.LoadImageFromDisk(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_RuneArtwork_" + id.ToString("0000") + ".png");

                byte[] bImageData = new byte[] { };

                Image iDownloadNewImage = Utils.DownloadImage(GameClient.Current.Game_CDN_URL + "public/swf/rune/220_220/rune_" + id.ToString() + ".png", ref bImageData);

                if (bImageData.Length >= 100)
                {
                    File.WriteAllBytes(Utils.AppFolder + "\\ResourceCache\\" + GameClient.Current.CurrentGame + "_RuneArtwork_" + id.ToString("0000") + ".png", bImageData);

                    return iDownloadNewImage;
                }
            }
            catch { }

            Image iBadImage = new Bitmap(220, 220, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(iBadImage))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.FillRectangle(new SolidBrush(Color.FromArgb(18, 29, 33)), 0, 0, iBadImage.Width, iBadImage.Height);
            }

            return iBadImage;
        }

        public static void OutputSkill(Graphics g, string skill_name, float x, float y, float width, float height)
        {
            using (Font f = Utils.GetFontThatWillFit(g, skill_name, (int)width, (int)height, "Arial", FontStyle.Bold, 18.0f))
            {
                SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, skill_name);
                float height_adjustment = (height - height_adjuster.Height) / 2f;

                g.DrawString(skill_name, f, Brushes.White, new PointF(x + (width - Utils.SizeOfDrawnText(g, f, skill_name).Width), y + height_adjustment));
            }
        }

        public static Image GenerateFakeCardImage(Image iBaseImageFile, int card_level, string card_name = "", int element = 0, int stars = 0, int cost = 0, int wait = 0, List<JObject> skills = null, List<int> attack_progression = null, List<int> hp_progression = null)
        {
            return GenerateCardImage(0, card_level, card_name, element, stars, cost, wait, skills, attack_progression, hp_progression, iBaseImageFile);
        }

        public static Image GenerateCardImage(int card_id, int card_level, string card_name = "", int element = 0, int stars = 0, int cost = 0, int wait = 0, List<JObject> skills = null, List<int> attack_progression = null, List<int> hp_progression = null, Image iBaseImageFile = null, int evolved_times = 0)
        {
            Image i = null;

            try
            {
                i = new Bitmap(385 + 350, 580, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(i))
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(18, 29, 33)), 0, 0, i.Width, i.Height);
                }

                Image iStar = Utils.ImageResizer(LoadResource_StarImage(), 38, 38);

                // base
                if (iBaseImageFile != null)
                    i = Utils.ImageOverlayer(i, iBaseImageFile, new Point(7, 34));
                else
                    i = Utils.ImageOverlayer(i, GameResourceManager.LoadCardArtwork(card_id), new Point(7, 34));

                // wait shader
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CommonShader(), 70, 30), new Point(28, 459));

                // skills shader
                List<JObject> skills_card_has = new List<JObject>();
                try { if (Utils.JObjectValid(skills[0])) skills_card_has.Add(skills[0]); } catch { }
                try { if (Utils.JObjectValid(skills[1])) if (card_level >= 5) skills_card_has.Add(skills[1]); } catch { }
                try { if (card_level >= 10) { for (int j = 2; j < skills.Count; j++) if (Utils.JObjectValid(skills[j])) skills_card_has.Add(skills[j]); } } catch { }

                if (skills_card_has.Count > 0)
                {
                    // max supported: 4 skills
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CommonShader(), 180, 40 * skills_card_has.Count), new Point(190, 280 + ((4 - skills_card_has.Count) * 40)));
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CommonShader(), 180, 40 * skills_card_has.Count), new Point(190, 280 + ((4 - skills_card_has.Count) * 40)));
                }

                // frame
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (element.ToString() == "1") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameTundra.png"), new Point(0, 0));
                    if (element.ToString() == "1") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameTundra.png"), new Point(0, 0));
                    if (element.ToString() == "2") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameForest.png"), new Point(0, 0));
                    if (element.ToString() == "2") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameForest.png"), new Point(0, 0));
                    if (element.ToString() == "3") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameSwamp.png"), new Point(0, 0));
                    if (element.ToString() == "3") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameSwamp.png"), new Point(0, 0));
                    if (element.ToString() == "4") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameMountain.png"), new Point(0, 0));
                    if (element.ToString() == "4") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameMountain.png"), new Point(0, 0));
                    if (element.ToString() == "95") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "95") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "96") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "96") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "97") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameBoss.png"), new Point(0, 0));
                    if (element.ToString() == "97") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameBoss.png"), new Point(0, 0));
                    if (element.ToString() == "99") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "99") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameActivity.PNG"), new Point(0, 0));
                    if (element.ToString() == "100") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameBoss.png"), new Point(0, 0));
                    if (element.ToString() == "100") i = Utils.ImageOverlayer(i, LoadResource_FullName("EK_SW.CardComposite_FrameBoss.png"), new Point(0, 0));
                }
                else
                {
                    if (element.ToString() == "1") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameKingdom.png"), new Point(0, 0));
                    if (element.ToString() == "1") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameKingdom.png"), new Point(0, 0));
                    if (element.ToString() == "2") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameForest.png"), new Point(0, 0));
                    if (element.ToString() == "2") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameForest.png"), new Point(0, 0));
                    if (element.ToString() == "3") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameWilderness.png"), new Point(0, 0));
                    if (element.ToString() == "3") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameWilderness.png"), new Point(0, 0));
                    if (element.ToString() == "4") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameHell.png"), new Point(0, 0));
                    if (element.ToString() == "4") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameHell.png"), new Point(0, 0));
                    if (element.ToString() == "95") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "95") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "96") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "96") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "99") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "99") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameResource.png"), new Point(0, 0));
                    if (element.ToString() == "100") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameDemon.png"), new Point(0, 0));
                    if (element.ToString() == "100") i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CardComposite_FrameDemon.png"), new Point(0, 0));
                }

                // cost
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (cost >= 10)
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost / 10), new Point(306, 35));
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost % 10), new Point(328, 35));
                    }
                    else
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost), new Point(319, 35));
                    }
                }
                else
                {
                    if (cost >= 10)
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost / 10), new Point(299, 34));
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost % 10), new Point(321, 34));
                    }
                    else
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(cost), new Point(312, 34));
                    }
                }

                // wait
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    i = Utils.ImageOverlayer(i, LoadResource_CardWaitHourglass(), new Point(12, 457));
                    i = Utils.ImageOverlayer(i, LoadResource_SilverNumber(wait), new Point(49, 459));
                }
                else
                {
                    i = Utils.ImageOverlayer(i, LoadResource_FullName("LoA.CommonComposite_SecondaryShader.png"), new Point(33, 449));
                    i = Utils.ImageOverlayer(i, LoadResource_CardWaitHourglass(), new Point(12, 460));
                    i = Utils.ImageOverlayer(i, LoadResource_SilverNumber(wait), new Point(40, 463));
                }

                // level
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (card_level >= 10)
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level / 10), new Point(14, 524));
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level % 10), new Point(36, 524));
                    }
                    else
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level), new Point(27, 524));
                    }
                }
                else
                {
                    if (card_level >= 10)
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level / 10), new Point(20, 524));
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level % 10), new Point(42, 524));
                    }
                    else
                    {
                        i = Utils.ImageOverlayer(i, LoadResource_GoldNumber(card_level), new Point(33, 524));
                    }
                }

                // ATK graphic
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CardATK(), 0.67), new Point(133, 460));
                }
                else
                {
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CardATK(), 0.67), new Point(133, 443));
                }

                // HP graphic
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CardHP(), 0.70), new Point(88, 510));
                }
                else
                {
                    i = Utils.ImageOverlayer(i, Utils.ImageResizer(LoadResource_CardHP(), 0.70), new Point(88, 501));
                }

                // skill icons
                if (skills_card_has.Count == 1)
                {
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                }
                else if (skills_card_has.Count == 2)
                {
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[1]["SkillCategory"].ToString() + ".png"), new Point(323, 359));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                }
                else if (skills_card_has.Count == 3)
                {
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[2]["SkillCategory"].ToString() + ".png"), new Point(323, 320));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[1]["SkillCategory"].ToString() + ".png"), new Point(323, 359));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                }
                else if (skills_card_has.Count == 4)
                {
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[3]["SkillCategory"].ToString() + ".png"), new Point(323, 281));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[2]["SkillCategory"].ToString() + ".png"), new Point(323, 320));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[1]["SkillCategory"].ToString() + ".png"), new Point(323, 359));
                    i = Utils.ImageOverlayer(i, LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills_card_has[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                }

                // star rating
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (stars == 6)
                    {
                        i = Utils.ImageOverlayer(i, iStar, new Point(100, 57));
                        i = Utils.ImageOverlayer(i, iStar, new Point(133, 57));
                        i = Utils.ImageOverlayer(i, iStar, new Point(166, 57));
                        i = Utils.ImageOverlayer(i, iStar, new Point(199, 57));
                        i = Utils.ImageOverlayer(i, iStar, new Point(232, 57));
                        i = Utils.ImageOverlayer(i, iStar, new Point(265, 57));
                    }
                    else
                    {
                        if (stars >= 1) i = Utils.ImageOverlayer(i, iStar, new Point(105, 56));
                        if (stars >= 2) i = Utils.ImageOverlayer(i, iStar, new Point(143, 56));
                        if (stars >= 3) i = Utils.ImageOverlayer(i, iStar, new Point(181, 56));
                        if (stars >= 4) i = Utils.ImageOverlayer(i, iStar, new Point(219, 56));
                        if (stars >= 5) i = Utils.ImageOverlayer(i, iStar, new Point(257, 56));
                    }
                }
                else
                {
                    if (stars >= 1) i = Utils.ImageOverlayer(i, iStar, new Point(107, 51));
                    if (stars >= 2) i = Utils.ImageOverlayer(i, iStar, new Point(145, 51));
                    if (stars >= 3) i = Utils.ImageOverlayer(i, iStar, new Point(183, 51));
                    if (stars >= 4) i = Utils.ImageOverlayer(i, iStar, new Point(221, 51));
                    if (stars >= 5) i = Utils.ImageOverlayer(i, iStar, new Point(259, 51));
                }

                iStar.Dispose();

                // all text
                using (Graphics g = Graphics.FromImage(i))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    using (Brush b = new SolidBrush(Color.FromArgb(60, 60, 60)))
                    {
                        if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                        {
                            // title
                            using (Font f = Utils.GetFontThatWillFit(g, card_name, 190, 30, "Arial", FontStyle.Bold, 18.0f))
                            {
                                SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, card_name);
                                float height_adjustment = (30f - height_adjuster.Height) / 2f;
                                g.DrawString(card_name, f, b, new PointF(99f, 17f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(99f, 17f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(99f, 17f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(100f, 18f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(100f, 18f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 19f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 19f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 19f + height_adjustment)); // outline
                                g.DrawString(card_name, f, Brushes.White, new PointF(100f, 18f + height_adjustment));
                            }

                            // ATK
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 451f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 452f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(234f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(234f, 452f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 451f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 452f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, Brushes.White, new PointF(234f, 451f));

                            // HP
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 499f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 500f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 501f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(171f, 499f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(171f, 501f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 499f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 500f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 501f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, Brushes.White, new PointF(171f, 500f));
                        }
                        else
                        {
                            // title
                            using (Font f = Utils.GetFontThatWillFit(g, card_name, 190, 30, "Arial", FontStyle.Bold, 18.0f))
                            {
                                SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, card_name);
                                float height_adjustment = (30f - height_adjuster.Height) / 2f;
                                g.DrawString(card_name, f, b, new PointF(99f, 10f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(99f, 10f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(99f, 10f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(100f, 11f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(100f, 11f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 12f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 12f + height_adjustment)); // outline
                                g.DrawString(card_name, f, b, new PointF(101f, 12f + height_adjustment)); // outline
                                g.DrawString(card_name, f, Brushes.White, new PointF(100f, 11f + height_adjustment));
                            }

                            // ATK
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 448f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 449f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(233f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(234f, 448f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(234f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 448f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 449f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(235f, 450f)); // outline
                            g.DrawString(attack_progression[card_level].ToString(), frmMain.FONT__141CAI978, Brushes.White, new PointF(234f, 449f));

                            // HP
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 499f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 500f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(170f, 501f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(171f, 499f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(171f, 501f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 499f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 500f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, b, new PointF(172f, 501f + 9f)); // outline
                            g.DrawString(hp_progression[card_level].ToString(), frmMain.FONT__141CAI978, Brushes.White, new PointF(171f, 500f + 9f));
                        }
                    }



                    if (skills_card_has.Count == 1)
                    {
                        OutputSkill(g, skills_card_has[0]["Name"].ToString(), 164f, 406f, 162f, 28f);
                    }
                    else if (skills_card_has.Count == 2)
                    {
                        OutputSkill(g, skills_card_has[1]["Name"].ToString(), 164f, 367f, 162f, 28f);
                        OutputSkill(g, skills_card_has[0]["Name"].ToString(), 164f, 406f, 162f, 28f);
                    }
                    else if (skills_card_has.Count == 3)
                    {
                        OutputSkill(g, skills_card_has[2]["Name"].ToString(), 164f, 328f, 162f, 28f);
                        OutputSkill(g, skills_card_has[1]["Name"].ToString(), 164f, 367f, 162f, 28f);
                        OutputSkill(g, skills_card_has[0]["Name"].ToString(), 164f, 406f, 162f, 28f);
                    }
                    else if (skills_card_has.Count == 4)
                    {
                        OutputSkill(g, skills_card_has[3]["Name"].ToString(), 164f, 289f, 162f, 28f);
                        OutputSkill(g, skills_card_has[2]["Name"].ToString(), 164f, 328f, 162f, 28f);
                        OutputSkill(g, skills_card_has[1]["Name"].ToString(), 164f, 367f, 162f, 28f);
                        OutputSkill(g, skills_card_has[0]["Name"].ToString(), 164f, 406f, 162f, 28f);
                    }



                    Font fRightNormal = new Font("Tahoma", 17.0f, FontStyle.Bold);
                    Font fRightSmall = new Font("Tahoma", 13.0f, FontStyle.Bold);

                    StringFormat sfCenterAlignment = new StringFormat() { Alignment = StringAlignment.Center };

                    using (Font f = Utils.GetFontThatWillFit(g, card_name, 338, 44, "Tahoma", FontStyle.Bold, 21.0f))
                    {
                        SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, card_name);
                        float height_adjustment = (44f - height_adjuster.Height) / 2f;

                        g.DrawString(card_name, f, Brushes.White, new RectangleF(new PointF(386f, 20f + height_adjustment), new SizeF(348f, 44f)), sfCenterAlignment);
                    }

                    g.DrawString(GameClient.ConvertCardElementToText(element), fRightNormal, Brushes.White, new RectangleF(new PointF(400f, 70f), new SizeF(160f, 36f)));
                    g.DrawString("Lv. " + card_level.ToString(), fRightNormal, Brushes.White, new RectangleF(new PointF(546f, 70f), new SizeF(140f, 36f)));

                    if (evolved_times > 0)
                        g.DrawString("Times Evolved:  " + evolved_times.ToString(), fRightNormal, Brushes.Yellow, new RectangleF(new PointF(400f, 100f), new SizeF(314f, 36f)));
                    

                    float current_height_offset = 116f + ((evolved_times > 0) ? 10f : 0f);

                    for (int iSkill = 0; iSkill < skills.Count; iSkill++)
                    {
                        try
                        {
                            if (!Utils.JObjectValid(skills[iSkill]))
                                continue;

                            JObject skill = skills[iSkill];

                            #region Skill name
                            string skill_name = skill["Name"].ToString();
                            if (card_level < 5 && iSkill == 1) skill_name += "(Level 5)";
                            if (card_level < 10 && iSkill > 1) skill_name += "(Level 10)";

                            using (Font f = Utils.GetFontThatWillFit(g, skill_name, 314, 36, "Tahoma", FontStyle.Bold, 18.0f))
                            {
                                SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, skill_name);
                                float height_adjustment = (36f - height_adjuster.Height) / 2f;

                                g.DrawString(skill_name, f, Brushes.White, new PointF(400f, current_height_offset + height_adjustment));
                            }
                            #endregion
                            #region Skill desc
                            SizeF skill_size = g.MeasureString(skill["Desc"].ToString(), fRightSmall, new SizeF(320f, 1000f));
                            g.DrawString(skill["Desc"].ToString(), fRightSmall, Brushes.White, new RectangleF(new PointF(400f, current_height_offset + 35f), skill_size));
                            #endregion

                            current_height_offset += 35f + skill_size.Height + 20f;
                        }
                        catch { }
                    }

                    sfCenterAlignment.Dispose();
                    fRightNormal.Dispose();
                    fRightSmall.Dispose();
                }

                if (ImageScalingSize != 100.0)
                    i = Utils.ImageResizer(i,
                        (int)(((double)i.Width) * ImageScalingSize / 100.0),
                        (int)(((double)i.Height) * ImageScalingSize / 100.0));

                return i;
            }
            catch (Exception ex)
            {
                Utils.Chatter(Errors.GetAllErrorDetails(ex));
            }

            return i;
        }

        public static Image GenerateRuneImage(int rune_id, int rune_level, string rune_name = "", int element = 0, int stars = 0, JObject skill = null, string rune_activate_condition = "", string rune_activate_times = "")
        {
            Image iGeneratedImage = null;

            try
            {
                iGeneratedImage = new Bitmap(388 + 350, 546, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                List<JObject> skills = new List<JObject>();
                try
                {
                    if (skill != null)
                        skills.Add(skill);
                }
                catch { }

                using (Graphics g = Graphics.FromImage(iGeneratedImage))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    g.FillRectangle(new SolidBrush(Color.FromArgb(18, 29, 33)), 0, 0, iGeneratedImage.Width, iGeneratedImage.Height);
                }

                Image iStar = Utils.ImageResizer(GameResourceManager.LoadResource_StarImage(), 36, 36);

                // common background
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_Background.png"), new Point(0, 0));
                else
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_Background.png"), new Point(0, 0));

                // base
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadRuneArtwork(rune_id), new Point(80, 164));
                else
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadRuneArtwork(rune_id), new Point(90, 166));

                // skills shader
                if (skills.Count > 0)
                {
                    // max supported: 3 skills
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, Utils.ImageResizer(GameResourceManager.LoadResource_CommonShader(), 180, 40 * skills.Count), new Point(190, 320 + ((3 - skills.Count) * 40)));
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, Utils.ImageResizer(GameResourceManager.LoadResource_CommonShader(), 180, 40 * skills.Count), new Point(190, 320 + ((3 - skills.Count) * 40)));
                }

                // frame
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_Frame.png"), new Point(0, 0));
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_Frame.png"), new Point(0, 0));
                }
                else
                {
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_Frame.png"), new Point(0, 0));
                }

                // property/element icon
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (element.ToString() == "1") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_IconEarth.png"), new Point(21, 23));
                    if (element.ToString() == "2") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_IconWater.png"), new Point(21, 23));
                    if (element.ToString() == "3") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_IconAir.png"), new Point(21, 23));
                    if (element.ToString() == "4") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("EK_SW.RuneComposite_IconFire.png"), new Point(21, 23));
                }
                else
                {
                    if (element.ToString() == "1") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_IconEarth.png"), new Point(4, 4));
                    if (element.ToString() == "2") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_IconWater.png"), new Point(4, 4));
                    if (element.ToString() == "3") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_IconAir.png"), new Point(4, 4));
                    if (element.ToString() == "4") iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName("LoA.RuneComposite_IconFire.png"), new Point(4, 4));
                }

                // level
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_GoldNumber(rune_level), new Point(33, 480));
                else
                    iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_GoldNumber(rune_level), new Point(38, 487));

                // skill icons
                if (skills.Count > 0)
                {
                    if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                        iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                    else
                        iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, GameResourceManager.LoadResource_FullName(((GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm) ? "EK_SW" : "LoA") + ".CommonComposite_SkillSC" + skills[0]["SkillCategory"].ToString() + ".png"), new Point(323, 398));
                }

                // star rating
                if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                {
                    if (stars >= 1) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(115, 57));
                    if (stars >= 2) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(153, 57));
                    if (stars >= 3) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(191, 57));
                    if (stars >= 4) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(229, 57));
                    if (stars >= 5) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(267, 57));
                }
                else
                {
                    if (stars >= 1) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(128, 53));
                    if (stars >= 2) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(166, 53));
                    if (stars >= 3) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(204, 53));
                    if (stars >= 4) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(242, 53));
                    if (stars >= 5) iGeneratedImage = Utils.ImageOverlayer(iGeneratedImage, iStar, new Point(280, 53));
                }

                iStar.Dispose();

                // all text
                using (Graphics g = Graphics.FromImage(iGeneratedImage))
                {
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    // title
                    using (Font f = Utils.GetFontThatWillFit(g, rune_name, 190, 30, "Arial", FontStyle.Bold, 18.0f))
                    {
                        using (Brush b = new SolidBrush(Color.FromArgb(60, 60, 60)))
                        {
                            SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, rune_name);
                            float height_adjustment = (30f - height_adjuster.Height) / 2f, x, y;

                            if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                            {
                                x = 112f;
                                y = 17f;
                            }
                            else
                            {
                                x = 125f;
                                y = 11f;
                            }

                            g.DrawString(rune_name, f, b, new PointF(x - 1f, (y - 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x - 1f, y + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x - 1f, (y + 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x, (y - 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x, (y + 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x + 1f, (y - 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x + 1f, y + height_adjustment)); // outline
                            g.DrawString(rune_name, f, b, new PointF(x + 1f, (y + 1f) + height_adjustment)); // outline
                            g.DrawString(rune_name, f, Brushes.White, new PointF(x, y + height_adjustment));
                        }
                    }

                    // Skill
                    if (skills.Count > 0)
                    {
                        using (Font f = Utils.GetFontThatWillFit(g, skills[0]["Name"].ToString(), 162, 28, "Arial", FontStyle.Bold, 18.0f))
                        {
                            SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, skills[0]["Name"].ToString());
                            float height_adjustment = (28f - height_adjuster.Height) / 2f;

                            g.DrawString(skills[0]["Name"].ToString(), f, Brushes.White, new PointF(164f + (162f - Utils.SizeOfDrawnText(g, f, skills[0]["Name"].ToString()).Width), 406f + height_adjustment));
                        }
                    }





                    Font fRightNormal = new Font("Tahoma", 18.0f, FontStyle.Bold);
                    Font fRightSmall = new Font("Tahoma", 14.0f, FontStyle.Bold);

                    StringFormat sfCenterAlignment = new StringFormat() { Alignment = StringAlignment.Center };

                    using (Font f = Utils.GetFontThatWillFit(g, rune_name, 338, 44, "Tahoma", FontStyle.Bold, 21.0f))
                    {
                        SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, rune_name);
                        float height_adjustment = (44f - height_adjuster.Height) / 2f;

                        g.DrawString(rune_name, f, Brushes.White, new RectangleF(new PointF(386f, 20f + height_adjustment), new SizeF(348f, 44f)), sfCenterAlignment);
                    }

                    string element_name = "Unknown";
                    if (GameClient.Current.Service != GameClient.GameService.Lies_of_Astaroth && GameClient.Current.Service != GameClient.GameService.Elves_Realm)
                    {
                        if (element == 1) element_name = "Earth";
                        if (element == 2) element_name = "Water";
                        if (element == 3) element_name = "Air";
                        if (element == 4) element_name = "Fire";
                    }
                    else
                    {
                        if (element == 1) element_name = "Att:Eart";
                        if (element == 2) element_name = "Att:Wat";
                        if (element == 3) element_name = "Att:Win";
                        if (element == 4) element_name = "Att:Fire";
                    }
                    g.DrawString(element_name, fRightNormal, Brushes.White, new RectangleF(new PointF(400f, 70f), new SizeF(140f, 36f)));
                    g.DrawString("Lv. " + rune_level.ToString(), fRightNormal, Brushes.White, new RectangleF(new PointF(546f, 70f), new SizeF(140f, 36f)));

                    float current_height_offset = 116f;

                    if (skills.Count > 0)
                    {
                        for (int iPartOutputter = 1; iPartOutputter <= 2; iPartOutputter++)
                        {
                            try
                            {
                                if (iPartOutputter == 1)
                                {
                                    using (Font f = Utils.GetFontThatWillFit(g, "Activation Condition:", 314, 36, "Tahoma", FontStyle.Bold, 18.0f))
                                    {
                                        SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, "Activation Condition:");
                                        float height_adjustment = (36f - height_adjuster.Height) / 2f;

                                        g.DrawString("Activation Condition:", f, Brushes.White, new PointF(400f, current_height_offset + height_adjustment));
                                    }

                                    string activation_condition = string.Empty;

                                    activation_condition = "When " + rune_activate_condition + ", " + skill["Name"].ToString() + " will be activated.(Max " + rune_activate_times + " times per battle)";

                                    SizeF skill_size = g.MeasureString(activation_condition, fRightSmall, new SizeF(320f, 1000f));
                                    g.DrawString(activation_condition, fRightSmall, Brushes.White, new RectangleF(new PointF(400f, current_height_offset + 35f), skill_size));

                                    current_height_offset += 35f + skill_size.Height + 20f;
                                }
                                else if (iPartOutputter == 2)
                                {

                                    #region Skill name
                                    string skill_name = skill["Name"].ToString();

                                    using (Font f = Utils.GetFontThatWillFit(g, skill_name, 314, 36, "Tahoma", FontStyle.Bold, 18.0f))
                                    {
                                        SizeF height_adjuster = Utils.SizeOfDrawnText(g, f, skill_name);
                                        float height_adjustment = (36f - height_adjuster.Height) / 2f;

                                        g.DrawString(skill_name, f, Brushes.White, new PointF(400f, current_height_offset + height_adjustment));
                                    }
                                    #endregion
                                    #region Skill desc
                                    SizeF skill_size = g.MeasureString(skill["Desc"].ToString(), fRightSmall, new SizeF(320f, 1000f));
                                    g.DrawString(skill["Desc"].ToString(), fRightSmall, Brushes.White, new RectangleF(new PointF(400f, current_height_offset + 35f), skill_size));
                                    #endregion

                                    current_height_offset += 35f + skill_size.Height + 20f;
                                }
                            }
                            catch { }
                        }
                    }

                    sfCenterAlignment.Dispose();
                    fRightNormal.Dispose();
                    fRightSmall.Dispose();
                }

                if (ImageScalingSize != 100.0)
                    iGeneratedImage = Utils.ImageResizer(iGeneratedImage,
                        (int)(((double)iGeneratedImage.Width) * ImageScalingSize / 100.0),
                        (int)(((double)iGeneratedImage.Height) * ImageScalingSize / 100.0));

            }
            catch (Exception ex)
            {
                Utils.Chatter(Errors.GetAllErrorDetails(ex));
            }

            return iGeneratedImage;
        }
    }
}
