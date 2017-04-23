using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// enumeration to get named sound effects
    /// </summary>
    /// --------------------------------------------------------------------------
    public enum SoundID
    {
        PixelWhimsy2,  // Load this first
        PixelWhimsy3,  // Load this second
        PixelWhimsy5,  // Load this third
        Cheer3,
        Click00,
        Click01,
        Click02,
        Click03,
        Click_Camera1,
        Comma00,
        Comma01,
        Comma02,
        //Dot00,
        Dot01,
        Dot02,
        Dot03,
        Dot04,
        Dot05,
        Dot06,
        Dot07,
        Dot08,
        Dot09,
        Dot10,
        Dot11,
        Dot12,
        Dot13,
        Dot14,
        Dot15,
        //Dot16,
        Dot17,
        Dot18,
        Dot_Airgun,
        //Dot_Cough,
        Dot_Cowbell,
        Dot_Dinghigh,
        Dot_Dinglow,
        //Dot_Drip1,
        //Dot_Drip2,
        //Dot_Drip3,
        //Dot_Drip4,
        //Dot_Drip5,
        Dot_Juice,
        //Dot_Kettledrum,
        Dot_Laugh,
        Dot_Pow,
        Dot_Smack,
        Dot_Snare,
        Dot_Snuff,
        Dot_Thump,
        EdgeDetect,
        DTine,
        //Firework_Crack1,
        Firework_Crack2,
        //Firework_Crack3,
        //Firework_Crack4,
        //Firework_Fizzle,
        Firework_Fwoosh,
        Firework_Peep,
        //Firework_Spinner,
        //Firework_Whistle1,
        //Firework_Whistle2,
        //Firework_Whistle3,
        //Firework_Whistle4,
        Flabbergasted2,
        Fwop,
        Gradient,
        //Loop_Bongos,
        //Loop_Buzz,
        Loop_Buzz_High,
        Loop_Chiggers,
        //Loop_Cow,
        Loop_Draw,
        Loop_Gargle,
        Loop_Hiss,
        //Loop_Hum_High,
        Loop_Hum_Low,
        //Loop_Hum_Medium,
        Loop_Rain,
        Loop_Rumble,
        Loop_Rumble2,
        Loop_Squeak,
        Loop_Wahwah,
        Note_Whistle,
        Pop,
        RGBSwitch,
        Slide00,
        Slide01,
        //Slide02,
        //Slide03,
        Slide04,
        Slide05,
        Slide06,
        Slide07,
        Slide08,
        Slide_Birds1,
        Slide_Birds2,
        Slide_Birds3,
        //Slide_Achoo,
        Slide_Cymbalhat,
        Slide_DownBongos,
        //Slide_Dwoing,
        Slide_Enginerev,
        //Slide_Juiceding,
        Slide_KaBoom,
        Slide_Laugh,
        //Slide_Longbell,
        Slide_RulerTwang,
        Slide_Screech,
        Slide_SleighBells,
        //Slide_Telering,
        //Slide_Tinkerbell,
        //Slide_Tubering,
        Slide_VoiceBoom,
        Slide_VoiceSpitKaboom,
        Slide_Wow,
        Slide_Yawn,
        Spike,
        Thunder,
        Thx,
        Woon,
        NumberOfSounds
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Static class to manage sound effects for everyone
    /// </summary>
    /// --------------------------------------------------------------------------
    public class MediaBag
    {
        private static List<SoundPlayer.SoundEffect> effectList  = new List<SoundPlayer.SoundEffect>();
        public static SoundID LastSoundPlayed = SoundID.Click_Camera1;
        public static SoundPlayer Player = null;
        public static PixelBuffer.Sprite iconPics;
        public static int miniPicCount = 48;
        public static PixelBuffer.DVFont font_LogoPixel = new PixelBuffer.DVFont("Courier New", 35, System.Drawing.FontStyle.Bold, true);

        // uncomment this to re-serialize the logo font
        //public static PixelBuffer.DVFont font_LogoWhimsy = new PixelBuffer.DVFont("Bauhaus 93", 32, System.Drawing.FontStyle.Bold, true);
        public static DirectVarmint.PixelBuffer.DVFont font_LogoWhimsy = DirectVarmint.PixelBuffer.DVFont.FromFile(DVTools.GetStream("PixelWhimsy.OtherData.bauhaus93.dvfont"));
        public static DirectVarmint.PixelBuffer.DVFont font_Status = new PixelBuffer.DVFont("Arial", 8, FontStyle.Regular, false);
        public static DirectVarmint.PixelBuffer.DVFont font_Instructions = new PixelBuffer.DVFont("Arial", 10, FontStyle.Bold, true);
        public static DirectVarmint.PixelBuffer.DVFont font_Text = new PixelBuffer.DVFont("Arial", 16, FontStyle.Regular, true);
        public static DirectVarmint.PixelBuffer.DVFont font_Keys = new PixelBuffer.DVFont("Lucida Console", 10, FontStyle.Bold, true);
        public static DirectVarmint.PixelBuffer.DVFont font_Wingding = new PixelBuffer.DVFont("WingDings", 16, FontStyle.Regular, true);
        public static int WingdingSize = 16;

        public static List<string> InstalledFonts;
        public static MadLib madLib = new MadLib();

        public static ushort color_White = PixelBuffer.ColorConverters._5Bit((uint)Color.White.ToArgb());
        public static ushort color_DarkRed = PixelBuffer.ColorConverters._5Bit((uint)Color.DarkRed.ToArgb());
        public static ushort color_Blue = PixelBuffer.ColorConverters._5Bit((uint)Color.Blue.ToArgb());
        public static ushort color_LightBlue = PixelBuffer.ColorConverters._5Bit((uint)Color.LightBlue.ToArgb());
        public static ushort color_Yellow = PixelBuffer.ColorConverters._5Bit((uint)Color.Yellow.ToArgb());
        public static ushort color_Tan = PixelBuffer.ColorConverters._5Bit((uint)Color.Tan.ToArgb());
        public static ushort color_Gray = PixelBuffer.ColorConverters._5Bit((uint)Color.Gray.ToArgb());
        public static ushort color_DarkGray = PixelBuffer.ColorConverters._5Bit((uint)Color.FromArgb(50, 50, 50).ToArgb());
        public static ushort color_DarkGreen = PixelBuffer.ColorConverters._5Bit((uint)Color.DarkGreen.ToArgb());

        private static PixelBuffer.Sprite miniPics;

        private static bool mute;
        private static double volume;

        private static Dictionary<SoundID, SoundEffect> soundEffects;

        /// <summary>
        /// Volume Property
        /// </summary>
        public static double Volume
        {
            get { return Player.MasterVolume; }
            set
            {
                volume = value;
                if (volume < 0) volume = 0;
                if (volume > 2) volume = 2;
                Player.MasterVolume = volume;
            }

           

        }

        static double lastVolume;
        /// <summary>
        /// Mute Property
        /// </summary>
        public static bool Mute
        {
            get { return mute; }
            set
            {
                mute = value;
                if (mute)
                {
                    lastVolume = Player.MasterVolume;
                    Player.MasterVolume = 0;
                }
                else Player.MasterVolume = lastVolume;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Abstraction for drawing minipics
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void DrawMiniPic(PixelBuffer buffer, int picID, int x, int y)
        {
            if (GlobalState.EasterHeads & picID < GlobalState.numHeads) picID += 60;
            buffer.DrawSprite(miniPics, picID, x, y);         
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the soundbag
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void Initialize(Control ownerControl)
        {
            HiPerfTimer timer = new HiPerfTimer();
            timer.Start();
            miniPics = new PixelBuffer.Sprite("PixelWhimsy.bitmaps.pictures.png", 80, 80, PixelBuffer.ColorConverters._5Bit);
            miniPics.TransparentColor = 0;
            double timePics = timer.ElapsedSeconds; timer.Start();

            iconPics = new PixelBuffer.Sprite("PixelWhimsy.bitmaps.Icons.png", 40, 40, PixelBuffer.ColorConverters._5Bit);
            iconPics.TransparentColor = 0;
            double timeIcon = timer.ElapsedSeconds; timer.Start();

            Player = new SoundPlayer(ownerControl);
            Player.MasterVolume = Settings.Volume;
            if (GlobalState.RunningInPreview) MediaBag.Mute = true;

            Thread soundLoadThread = new Thread(new ThreadStart(SoundLoadWorker));
            soundLoadThread.Start();

            // wait for the first three sounds to load
            if (effectList.Count < 3) Thread.Sleep(200);
            double timeSOund = timer.ElapsedSeconds; timer.Start();

            List<string> allowedFonts = new List<string>(
                new string[]{
                    "arial",
                    "papyrus",
                    "verdana",
                    "harrington",
                    "arial narrow",
                    "georgia",
                    "arial black",
                    "comic sans ms",
                    "batavia",
                    "arial rounded mt bold",
                    "book antiqua",
                    "calibri",
                    "castellar",
                    "courier new",
                    "stencil",
                    "goudy stout",
                    "impact",
                    "lucida console",
                    "segoe ui",
                    "rockwell",
                    "times new roman",
                    "trendy"
                });

            InstalledFonts = new List<string>();
            InstalledFontCollection installedFontCollection = new InstalledFontCollection();
            FontFamily[] fontFamilies = installedFontCollection.Families;
            for (int j = 0; j < fontFamilies.Length; ++j)
            {
                if (allowedFonts.Contains(fontFamilies[j].Name.ToLower()) && fontFamilies[j].IsStyleAvailable(FontStyle.Regular))
                {
                    InstalledFonts.Add(fontFamilies[j].Name);
                    if (fontFamilies[j].Name == "Arial")
                    {
                        Animation.TextEntry.FontID = InstalledFonts.Count-1;
                    }
                }
            }

            double timeFont = timer.ElapsedSeconds;

            soundEffects = new Dictionary<SoundID, SoundEffect>();
            for(int i = 0; i < (int)SoundID.NumberOfSounds; i++)
            {
                SoundEffect newEffect = VarmintGlobals.Content.Load<SoundEffect>("Sounds/" + ((SoundID)i).ToString());
                soundEffects.Add((SoundID)i, newEffect);               
            }
            playingInstances = new List<SoundEffectInstance>();
        }

        private static void SoundLoadWorker()
        {

            for (int i = 0; i < (int)SoundID.NumberOfSounds; i++)
            {
                SoundID soundID = (SoundID)i;
                //effectList.Add(Player.GetSoundEffect("PixelWhimsy.Sounds." + soundID.ToString() + ".wav"));
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a Sound effect
        /// </summary>
        /// --------------------------------------------------------------------------
        internal static SoundPlayer.SoundInstance Play(SoundID soundID)
        {
            return Play(soundID, 1, 1, false);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a Sound effect
        /// </summary>
        /// --------------------------------------------------------------------------
        internal static SoundPlayer.SoundInstance Play(SoundID soundID, double frequency)
        {
            return Play(soundID, frequency, 1, false);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a Sound effect
        /// </summary>
        /// --------------------------------------------------------------------------
        internal static SoundPlayer.SoundInstance Play(SoundID soundID, double frequency, double volume)
        {
            return Play(soundID, frequency, volume, false);
        }

        static List<SoundEffectInstance> playingInstances; 
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a Sound effect
        /// </summary>
        /// --------------------------------------------------------------------------
        internal static SoundPlayer.SoundInstance Play(SoundID soundID, double frequency, double volume, bool looping)
        {
            // in case we haven't loaded all the sounds yet, just pick the last one.
            if ((int)soundID >= soundEffects.Count) soundID = (SoundID)(effectList.Count - 1);

            SoundEffectInstance instance = soundEffects[soundID].CreateInstance();

            float pitch = (float)Math.Log(frequency, 2);
            if (pitch < -1) pitch = -1;
            if (pitch > 1) pitch = 1;

            if (volume > 1) volume = 1;

            instance.IsLooped = looping;
            instance.Pitch = pitch;
            instance.Volume = (float)volume;
            instance.Play();

            LastSoundPlayed = soundID;


            // Remove handles to dead sounds.
            for (int i = 0; i < playingInstances.Count; )
            {
                if (playingInstances[i].State == SoundState.Stopped)
                {
                    playingInstances.RemoveAt(i);
                    continue;
                }
                else
                {
                    i++;
                }
            }


            playingInstances.Add(instance);

            SoundPlayer.SoundInstance newEffectInstance = new SoundPlayer.SoundInstance(instance);

            return newEffectInstance;
        }


        static Dictionary<int, DirectVarmint.PixelBuffer.DVFont> wingdingFontCache = new Dictionary<int, PixelBuffer.DVFont>();
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set the wingding font to the correct size
        /// </summary>
        /// --------------------------------------------------------------------------
        internal static void SetWingdingFont(int size)
        {
            if (size != WingdingSize)
            {
                WingdingSize = size;
                if (!wingdingFontCache.ContainsKey(size))
                {
                    wingdingFontCache.Add(size, new PixelBuffer.DVFont("WingDings", size, FontStyle.Regular, true));
                }
                font_Wingding = wingdingFontCache[size];
            }
        }
    }
}
