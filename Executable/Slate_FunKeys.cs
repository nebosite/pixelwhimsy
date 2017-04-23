using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DirectVarmint;

namespace PixelWhimsy
{
    public partial class Slate
    {
        public delegate void FunKeyFunc();
        public delegate void DelayedActionFunc();
        List<DelayedAction> delayedActions = new List<DelayedAction>();

        int mazeBorderX;
        int mazeBorderY;

        //[ ] Water Color
        //[ ] Bees and ants

        /// --------------------------------------------------------------------
        /// <summary>
        /// Show a cute message for unregistered users
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyUnregistered()
        {
            HaltEverything();
            MediaBag.Play((SoundID)Utilities.Rand((int)SoundID.NumberOfSounds));

            dvWindow.MainBuffer.Clear(Utilities.PickDarkRandomColor(dvWindow, true));
            MediaBag.DrawMiniPic(
                dvWindow.MainBuffer,
                Utilities.Rand(MediaBag.miniPicCount),
                dvWindow.Width - 90,
                dvWindow.Height - 90);

            string text =
                "Fun keys are enabled in\n" +
                "the registered version of\n" +
                "PixelWhimsy.  Please register\n" +
                "at www.pixelwhimsy.com.";

            DrawCenteredText(dvWindow.MainBuffer, text, MediaBag.color_White);
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Put some centered text on the screen
        /// </summary>
        /// --------------------------------------------------------------------
        private void DrawCenteredText(PixelBuffer buffer, string text, ushort color)
        {
            SizeF textSize = MediaBag.font_Text.Measure(text);
            int x = (int)((dvWindow.Width - textSize.Width) / 2);
            int y = (int)((dvWindow.Height - textSize.Height) / 2);
            buffer.Print(color, MediaBag.font_Text, x, y, text);
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Painting in the Rain
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyExploderAndBee()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            GlobalState.SetCurrentDrawingColor(Utilities.PickRandomColor(dvWindow, true));
            GlobalState.BrushSize = GlobalState.TargetBrushSize = 50;
            GlobalState.BrushType = BrushType.Bomb;
            GlobalState.BrushSound = MediaBag.Play(SoundID.Loop_Hiss, 1.5, .2, true);

            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), DelayedActionExploderBee));
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play with gravity rainbow
        /// </summary>
        /// --------------------------------------------------------------------
        void DelayedActionExploderBee()
        {
            if (GlobalState.BrushType == BrushType.Bomb) 
            {
                if (!AnimationExists(typeof(Animation.Bee)))
                {
                    int x = Utilities.Rand(dvWindow.Width);
                    int y = Utilities.Rand(dvWindow.Width);
                    Animation.Bee newBee = new Animation.Bee(dvWindow, x, y, (ushort)Utilities.Rand(0x10000));
                    newBee.MouseX = mouseX;
                    newBee.MouseY = mouseY;
                    AddAnimation(newBee);
                }

                AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), DelayedActionExploderBee));
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Playing around with bees and ants
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyBeesAndAnts()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            ushort color = (ushort)Utilities.Rand(0x8000);
            if (CtrlIsPressed())
            {
                color = GlobalState.CurrentDrawingColor;
            }

            for (int i = 0; i < beeColors.Length; i++)
            {
                int r = (color >> 10) & 0x1f;
                int g = (color >> 5) & 0x1f;
                int b = (color >> 0) & 0x1f;

                r += (Utilities.Rand(3) - 1) * Utilities.Rand(5);
                g += (Utilities.Rand(3) - 1) * Utilities.Rand(5);
                b += (Utilities.Rand(3) - 1) * Utilities.Rand(5);

                if (r > 0x1f) r = 0x1f;
                if (g > 0x1f) g = 0x1f;
                if (b > 0x1f) b = 0x1f;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;

                beeColors[i] = (ushort)((r << 10) | (g << 5) | b);
            }

            AddAnimation(new Animation.GroundCollapse(dvWindow));
            AddAnimation(new Animation.PixelDiffuser(dvWindow));
            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), DelayedActionAddSomeBeesAndAnts));
        }

        ushort[] beeColors = new ushort[20];

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play with gravity rainbow
        /// </summary>
        /// --------------------------------------------------------------------
        void DelayedActionAddSomeBeesAndAnts()
        {
            if (AnimationExists(typeof(Animation.GroundCollapse)))
            {
                for(int i = 0; i < 10; i++)
                {
                    int x = Utilities.Rand(dvWindow.Width);
                    int y = Utilities.Rand(dvWindow.Width);
                    Animation.Bee newBee = new Animation.Bee(dvWindow, x, y, beeColors[Utilities.Rand(beeColors.Length)]);
                    newBee.MouseX = mouseX;
                    newBee.MouseY = mouseY;
                    AddAnimation(newBee);
                }

                if(Utilities.Rand(100) < 70)
                {
                    int x = Utilities.Rand(dvWindow.Width);
                    int y = dvWindow.Height - Utilities.Rand(dvWindow.Height / 8);
                    AddAnimation(new Animation.AutoBrush(dvWindow, x, y, 1, Utilities.DRand(1.0) - .5, 0, 0));
                }

                AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), DelayedActionAddSomeBeesAndAnts));
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Painting in the Rain
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyWaterColor()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            AddAnimation(new Animation.Rain(dvWindow));
            dvWindow.MainBuffer.Clear(Utilities.PickRandomColor(dvWindow, false));
            GlobalState.SetCurrentDrawingColor(Utilities.PickRandomColor(dvWindow, true));
            GlobalState.BrushSize = GlobalState.TargetBrushSize = 8;
            GlobalState.BrushType = BrushType.Circle;
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Simple tile painting
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyTilePainter()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            GlobalState.PaintingStyle = PaintingStyle.Tile;
            GlobalState.BrushType = BrushType.PictureStamp;
            GlobalState.TargetBrushSize = Utilities.Rand(49) + 1;
        }


        /// --------------------------------------------------------------------
        /// <summary>
        /// Play around with ground falling
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyTextCoolness()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            ushort color1 = Utilities.PickDarkRandomColor(dvWindow, true);
            ushort color2 = Utilities.PickDarkRandomColor(dvWindow, false);
            int size = Utilities.Rand(40) + 20;

            AddAnimation(new Animation.CheckerBoard(dvWindow, color1, color2, size));

            DelayedActionFunc printText = delegate()
            {
                string text = "";
                for (int i = Utilities.Rand(4) + 1; i > 0; i--)
                {
                    text += MediaBag.madLib.GetString() + "  ";
                }
                text += "\n                    - " + MediaBag.madLib.GetName();

                int x = dvWindow.Width/10;
                int y = dvWindow.Height/10;

                Animation.TextEntry textEntry = new Animation.TextEntry(dvWindow, Utilities.PickRandomColor(dvWindow, false), x, y);
                textEntry.SetText(text);
                textEntry.WriteToMainBuffer();
            };

            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), printText));

        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play around with ground falling
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyFuzzyKaleidoscope()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            AddAnimation(new Animation.PixelDiffuser(dvWindow));
            GlobalState.PaintingStyle = PaintingStyle.Kaleidoscope;

            GenerateKeyPress(KeyAction.BrushSelectRandom);
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play around with ground falling
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyGroundFall()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            AddAnimation(new Animation.Fader(dvWindow, 0, true));
            AddAnimation(new Animation.GroundCollapse(dvWindow));

            ushort color = (ushort)Utilities.Rand(0x8000);
            if (CtrlIsPressed())
            {
                color = GlobalState.CurrentDrawingColor;
            }

            int x = Utilities.Rand(dvWindow.Width / 2) + dvWindow.Height / 4;
            int y = Utilities.Rand(dvWindow.Height / 2) + dvWindow.Height / 4;

            AddAnimation(new Animation.GravityRainbow(dvWindow, x, y, mouseX, mouseY, color));
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play around with flood filling
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyBlueChaser()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            AddAnimation(new Animation.ScreenDecay(dvWindow));
            GlobalState.BrushType = BrushType.Circle;
            GlobalState.SetCurrentDrawingColor(70);
            GlobalState.TargetBrushSize = Utilities.Rand(20) + 5;
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play around with flood filling
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyFloodFillPlay()
        {
            HaltEverything();
            // TODO:  Pick a sound for MediaBag.Play(SoundID.Slide07);

            AddAnimation(new Animation.Fader(dvWindow, 0, false));
            AddAnimation(new Animation.ColorDiffuser(dvWindow));

            ushort color = (ushort)Utilities.Rand(0x8000);
            if (CtrlIsPressed())
            {
                color = GlobalState.CurrentDrawingColor;
            }

            int x = Utilities.Rand(dvWindow.Width / 2) + dvWindow.Height / 4;
            int y = Utilities.Rand(dvWindow.Height / 2) + dvWindow.Height / 4;
            AddAnimation(new Animation.FloodFill(dvWindow, color, x, y));
            GlobalState.BrushType = BrushType.FloodFill;
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Do the maze
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyMazeGame()
        {
            HaltEverything();
            MediaBag.Play(SoundID.Slide07);

            ushort mazeColor = (ushort)Utilities.Rand(0x8000);
            if(CtrlIsPressed())
            {
                mazeColor = GlobalState.CurrentDrawingColor;
            }

            Animation.Maze maze = new Animation.Maze(dvWindow, mazeColor);
            mazeBorderX = maze.BorderX;
            mazeBorderY = maze.BorderY;
            AddAnimation(maze);
            int x = 1;
            if (Utilities.Rand(2) == 0) x = maze.MazeW - 2;
            x = x * maze.BlockSize + maze.BorderX;


            int y = 1;
            if (Utilities.Rand(2) == 0) y = maze.MazeH - 2;
            y = y * maze.BlockSize + maze.BorderY;

            ushort color = (ushort)Utilities.Rand(0x10000);
            AddAnimation(new Animation.KaCheese(dvWindow, color, x + 20, y + 15));
 

            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(.1), StartKaCheese));
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play with gravity rainbow
        /// </summary>
        /// --------------------------------------------------------------------
        void StartKaCheese()
        {
            if (mouseX > mazeBorderX && mouseX < dvWindow.Width - mazeBorderX &&
                mouseY > mazeBorderY && mouseY < dvWindow.Height - mazeBorderY &&
                dvWindow.MainBuffer.GetPixel(mouseX, mouseY) == 0)
            {
                GlobalState.BrushSize = 50;
                GlobalState.TargetBrushSize = 50;
                GlobalState.BrushType = BrushType.Bomb;
                GlobalState.BrushSound = MediaBag.Play(SoundID.Loop_Hiss, 1.5, .2, true);
            }
            else
            {
                AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(.1), StartKaCheese));
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Play with gravity rainbow
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyGravityPlay()
        {
            HaltEverything();
            MediaBag.Play(SoundID.Slide_Enginerev);

            ushort bgColor = (ushort)Utilities.Rand(0x8000);
            ushort fgColor = 0;
            if(CtrlIsPressed())
            {
                bgColor = GlobalState.CurrentDrawingColor;
                fgColor = GlobalState.PreviousDrawingColor;
                if(fgColor == bgColor) fgColor = 0;
            }

            AddAnimation(new Animation.Fader(dvWindow, bgColor, false));
            AddAnimation(new Animation.ColorDiffuser(dvWindow));

            DelayedActionFunc startGravity = delegate()
            {
                int x = Utilities.Rand(dvWindow.Width);
                int y = Utilities.Rand(dvWindow.Height);
                MediaBag.Play(SoundID.Slide04);

                ushort c = fgColor;
                if(c == 0) c = (ushort)Utilities.Rand(0x10000);
                AddAnimation(new Animation.GravityRainbow(dvWindow, x, y, mouseX, mouseY, c));
            };

            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(1), startGravity));
            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(2), startGravity));
            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(3), startGravity));
            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(4), startGravity));
            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(5), startGravity));
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Stop Everything, clear the screen
        /// </summary>
        /// --------------------------------------------------------------------
        void HaltEverything()
        {
            StopAnimations(null);
            StopDelayedActions();
            SetToDefaultState();
            dvWindow.MainBuffer.Clear(0);
            dvWindow.OverlayBuffer.Clear(0);
            keyPressData.Clear();
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Game of life play
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyLifePlayer()
        {
            HaltEverything();
            
            MediaBag.Play(SoundID.Cheer3, 1, .3);

            // Set up the teams
            ushort color1 = Utilities.PickRandomColor(dvWindow, false);
            ushort color2 = Utilities.PickRandomColor(dvWindow, false);
            while (color2 == color1) color2 = Utilities.PickRandomColor(dvWindow, false);


            PlaceLifePlayers(color1);
            FlipHorizontal();
            PlaceLifePlayers(color2);

            // show a scoreboard
            AddAnimation(new Animation.ColorCounter(dvWindow, 1, 1));

            // Start the life player
            DelayedActionFunc startLifeGame = delegate()
            {
                if (!AnimationExists(typeof(Animation.GameOfLife)))
                {
                    AddAnimation(new Animation.GameOfLife(dvWindow));
                }
            };

            AddDelayedAction(new DelayedAction(DateTime.Now.AddSeconds(2), startLifeGame)); 
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// This starts the actual game of life animation
        /// </summary>
        /// --------------------------------------------------------------------
        void FunKeyStartLifeGame()
        {
            if(!AnimationExists(typeof(Animation.GameOfLife)))
            {
                AddAnimation(new Animation.GameOfLife(dvWindow));
            }
        }

        /// --------------------------------------------------------------------
        /// <summary>
        /// Helper method to place a checkerboard patter of life players
        /// </summary>
        /// --------------------------------------------------------------------
        private void PlaceLifePlayers(ushort color1)
        {
            int numPatterns = LifePattern.GlobalPatterns.Count;
            int sizex = dvWindow.Width / 4;
            int sizey = dvWindow.Height / 2;
            for (int i = 0; i < 8; i += 2)
            {
                int row = i / 4;
                int column = i % 4;
                LifePattern pattern = LifePattern.GlobalPatterns[Utilities.Rand(numPatterns)];

                int rx = Utilities.Rand(sizex - pattern.width);
                int ry = Utilities.Rand(sizey - pattern.height);

                pattern.Draw(dvWindow.MainBuffer, color1, column * sizex + rx, row * sizey + ry);
            }
        }

    }
}
