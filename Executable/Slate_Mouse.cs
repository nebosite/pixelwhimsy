using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using DirectVarmint;
using System.Diagnostics;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// Mouse code for the drawing slate
    /// </summary>
    /// --------------------------------------------------------------------------
    public partial class Slate
    {
        List<Rectangle> mouseClearRectangles = new List<Rectangle>();
        Color mouseEngagementColor;
        int mouseX, mouseY, absoluteMouseX, absoluteMouseY;
        int mouseMoveCounter = 0;
        int lastMouseX, lastMouseY;
        bool leftMouse = false, rightMouse = false, middleMouse = false;
        SoundPlayer.SoundInstance drawSoundInstance = null;
        int drawSoundFrame = 0;
        double mouseVelocityX = 0;
        double mouseVelocityY = 0;
        DateTime lastActivityTime = DateTime.Now.AddDays(-100);

        uint paintingFrame = 0;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void Slate_MouseEnter(object sender, EventArgs e)
        {
            CursorHide();
        }

        bool cursorVisible = true;
        object mouseCursorLockHandle = new object();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Hide the mouse cursor in a balanced way
        /// </summary>
        /// --------------------------------------------------------------------------
        void CursorHide()
        {
            lock (mouseCursorLockHandle)
            {
                if (cursorVisible && !stressMode)
                {
                    cursorVisible = false;
                    Cursor.Hide();
                }
            }

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Show the mouse cursor in a balanced way
        /// </summary>
        /// --------------------------------------------------------------------------
        void CursorShow()
        {
            lock (mouseCursorLockHandle)
            {
                if (!cursorVisible && !stressMode)
                {
                    cursorVisible = true;
                    Cursor.Show();
                }
            }
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Convert window mouse coordinates to buffer mouse coordinates
        /// </summary>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void SetRealMousePosition(int x, int y)
        {
            absoluteMouseX = x;
            absoluteMouseY = y;
            mouseX = ((x - panelGame.Left) * dvWindow.Width) / panelGame.Width;// (e.X * dvWindow.OverlayBuffer.Width) / this.Width;
            mouseY = ((y - panelGame.Top) * dvWindow.Height) / panelGame.Height;// (e.Y * dvWindow.OverlayBuffer.Height) / this.Height;

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        public void Slate_MouseMove(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;

            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            int x = mouseArgs.X;
            int y = mouseArgs.Y;

            if (x > panelGame.Left && x < panelGame.Right &&
                y > panelGame.Top && y < panelGame.Bottom)
            {
                CursorHide();
            }
            else
            {
                CursorShow();
            }


            lock (eventsToDoLockHandle)
            {
                mouseMoveEventsToDo.Add(e);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// This is to be called from inside of the render loop
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Safe_Slate_MouseMove(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;
            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            lastMouseX = mouseX;
            lastMouseY = mouseY;
            SetRealMousePosition(mouseArgs.X, mouseArgs.Y);

            if (GlobalState.RunningAsScreenSaver)
            {
                if (!Settings.Registered || !Settings.PlayableScreensaver)
                {
                    GlobalState.EndApplication = true;
                    return;
                }
            }

            
            HandleMouseMove(mouseArgs.Delta, mouseX, mouseY);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle changes in mouse Movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void HandleMouseMove(int mouseWheelDelta, int moveToX, int moveToY)
        {
            mouseX = moveToX;
            mouseY = moveToY;

            SetMouseMoveCounter();
            if (mouseWheelDelta != 0 && this.isPrimaryWindow)
            {   
                GlobalState.BrushSize += mouseWheelDelta;
                GlobalState.TargetBrushSize = GlobalState.BrushSize;
                MediaBag.Play(SoundID.Dot17, 1.0 / (GlobalState.BrushSize / 50.0 + 0.5), .1);

                RenderBrushToolbar();
            }

            mouseVelocityX = ((mouseVelocityX * 9) + (mouseX - lastMouseX)) / 10.0;
            mouseVelocityY = ((mouseVelocityY * 9) + (mouseY - lastMouseY)) / 10.0;

            double newMouseX = mouseX;
            double newMouseY = mouseY;

            double deltaX = newMouseX - lastMouseX;
            double deltaY = newMouseY - lastMouseY;
            double delta = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (delta == 0)
            {
                return;
            }
            double deltaSkip = 1.0;

            // Space out the painting if we are tile drawing Pictures, because they
            // are expensive to draw
            int picMultiplier = 1;

            switch (GlobalState.PaintingStyle)
            {
                case PaintingStyle.Kaleidoscope: picMultiplier = 8; break;
                case PaintingStyle.Tile: picMultiplier = 16; break;
                default: picMultiplier = 1; break;
            }

            deltaSkip = (picMultiplier * delta) / 50.0;
            if (deltaSkip < 1.0) deltaSkip = 1.0;               

            if (drawSoundInstance != null)
            {
                drawSoundFrame += 2;
                drawSoundInstance.RelativeFrequency =
                    drawSoundInstance.RelativeFrequency * .75 +
                    (.35 + delta / 80.0) * .25;
            }
            deltaX /= delta;
            deltaY /= delta;

            for (double t = 0; t <= delta; t+= deltaSkip)
            {
                mouseX = (int)(lastMouseX + deltaX * t);
                mouseY = (int)(lastMouseY + deltaY * t);
                switch (screenMode)
                {
                    case ScreenMode.Normal: HandleMouse_Normal(); break;
                    default: break;
                }
            }


            lock (animationQueueLockHandle)
            {
                foreach (Animation animation in animationQueue)
                {
                    animation.MouseX = mouseX;
                    animation.MouseY = mouseY;
                }
            }

            if (screenMode == ScreenMode.LoadScreen || screenMode == ScreenMode.SaveScreen)
            {
                SetCurrentScreenSlot(mouseX / (dvWindow.MainBuffer.Width / 4) +
                    (mouseY / (dvWindow.MainBuffer.Height / 4)) * 4);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Turn on the mouse move counter
        /// </summary>
        /// --------------------------------------------------------------------------
        private void SetMouseMoveCounter()
        {
            mouseMoveCounter = (int)(DVTools.ActualFramesPerSecond * .5);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        public void Slate_MouseDown(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;
            lock (eventsToDoLockHandle)
            {
                mouseDownEventsToDo.Add(e);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// This is to be called from inside of the render loop
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Safe_Slate_MouseDown(object sender, EventArgs e)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            SetRealMousePosition(mouseArgs.X, mouseArgs.Y);

            if (GlobalState.RunningAsScreenSaver)
            {
                if (!Settings.Registered || !Settings.PlayableScreensaver)
                {
                    GlobalState.EndApplication = true;
                    return;
                }
            }

            HandleMouseDown(mouseArgs.Button);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle mouse down activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void HandleMouseDown(MouseButtons buttons)
        {
            if (dvWindowBrushes.HasMouseInside || dvWindowAnimations.HasMouseInside || dvWindowModulators.HasMouseInside) return;

            Animation.ColorCounter counter = (Animation.ColorCounter)GetFirstAnimation(typeof(Animation.ColorCounter));
            if (counter != null)
            {
                if (counter.MouseClicked(mouseX, mouseY)) return;
            }


            switch (buttons)
            {
                case MouseButtons.Left:

                    if (GlobalState.BrushType == BrushType.LifePattern)
                    {
                        skipMouseDraws = 8;
                    }
                    if (screenMode == ScreenMode.TextEntry) HandleMouseClick_TextEntry();

                    if (GlobalState.BrushType == BrushType.Dragging)
                    {
                        ClearWorkingPoints();
                        SetWorkingPoint();
                    }
                    else if (GlobalState.BrushType == BrushType.FloodFill)
                    {
                        AddAnimation(new Animation.FloodFill(dvWindow, GlobalState.CurrentDrawingColor, mouseX, mouseY));
                    }
                    else if (drawSoundInstance == null)
                    {
                        drawSoundInstance = MediaBag.Play(SoundID.Loop_Draw, .4, 0, true);
                    }
                    leftMouse = true;
                    mouseFrame = 0;
                    break;
                case MouseButtons.Right:
                    {
                        rightMouse = true;
                        ClearWorkingPoints();
                    } 
                    break;
                case MouseButtons.Middle: middleMouse = true; break;
                default: break;
            }

            MediaBag.Play(SoundID.Click02, 1.5, .05);
            ScreenCommit();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        public void Slate_MouseUp(object sender, EventArgs e)
        {
            lastActivityTime = DateTime.Now;
            lock (eventsToDoLockHandle)
            {
                mouseUpEventsToDo.Add(e);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// This is to be called from inside of the render loop
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Safe_Slate_MouseUp(object sender, EventArgs e)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)e;
            SetRealMousePosition(mouseArgs.X, mouseArgs.Y);

            if (GlobalState.RunningAsScreenSaver)
            {
                if (!Settings.Registered || !Settings.PlayableScreensaver)
                {
                    GlobalState.EndApplication = true;
                    return;
                }
            }

            HandleMouseUp(mouseArgs.Button);
        }

        bool needToStopInstructions = false;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle mouse up activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void HandleMouseUp(MouseButtons buttons)
        {
            needToStopInstructions = true;

            if (dvWindowBrushes.HasMouseInside)
            {
                panelBrushes_Click(null, null);

            }
            else if (dvWindowAnimations.HasMouseInside)
            {
                panelAnimations_Click(null, null);

            }
            else if (dvWindowModulators.HasMouseInside)
            {
                panelModulators_Click(null, null);

            }
            else if (insideExitButton)
            {
                buttonExit_Click(null, null);
            }

            //if (insideToolbar)
            //{
            //    if (absoluteMouseX > panelBrushes.Left && absoluteMouseX < panelBrushes.Right &&
            //        absoluteMouseY > panelBrushes.Top && absoluteMouseY < panelBrushes.Bottom)
            //    {
            //    }
            //    else if (absoluteMouseX > panelModulators.Left && absoluteMouseX < panelModulators.Right &&
            //        absoluteMouseY > panelModulators.Top && absoluteMouseY < panelModulators.Bottom)
            //    {
            //    }
            //    else
            //    {
            //    }
            //}
            //else if (insideExitButton)
            //{
            //    buttonExit_Click(null, null);
            //}

            switch (buttons)
            {
                case MouseButtons.Left:
                    if (GlobalState.BrushType == BrushType.Dragging && !insideToolbar)
                    {
                        dvWindow.OverlayBuffer.Clear(0);
                        SetWorkingPoint();
                    }
                    else if (drawSoundInstance != null)
                    {
                        drawSoundInstance.Finished = true;
                        drawSoundInstance = null;
                    }
                    leftMouse = false;
                    break;
                case MouseButtons.Right: rightMouse = false; break;
                case MouseButtons.Middle: middleMouse = false; break;
                default: break;
            }

            if (needToStopInstructions && AnimationExists(typeof(Animation.Instructions)))
            {
                StopAnimations(typeof(Animation.Instructions));
                return;
            }


        }
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle painting and drawing
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleMouse_Normal()
        {
            int size = GlobalState.BrushSize;
            if (colorPicker) HandleMouseOnColorPicker(mouseX, mouseY);

            if (rightMouse)
            {
                if (mouseX >= 0 && mouseX < dvWindow.MainBuffer.Width &&
                    mouseY >= 0 && mouseY < dvWindow.MainBuffer.Height)
                {
                    GlobalState.SetCurrentDrawingColor(dvWindow.MainBuffer.GetPixel(mouseX, mouseY));
                }
            }

            if (middleMouse) { }

            if (GlobalState.BrushType == BrushType.Bomb)
            {
                if (dvWindow.MainBuffer.GetPixel(mouseX, mouseY) > 0)
                {
                    GlobalState.BrushType = GlobalState.LastBrushType;
                    if (GlobalState.BrushType == BrushType.Bomb)
                    {
                        GlobalState.BrushType = BrushType.Circle;
                    }
                    MediaBag.Play(SoundID.Slide_VoiceSpitKaboom, 1, (GlobalState.BrushSize + 2) / 50.0 * 2);
                    AddAnimation(new Animation.Kaboom(dvWindow, GlobalState.CurrentDrawingColor, mouseX, mouseY, GlobalState.BrushSize));
                }
            }

            if (!leftMouse) return;

            paintingFrame++;

            if (colorPicker)
            {
                if (mouseX >= cpLeft && mouseX <= cpRight && mouseY >= cpTop && mouseY <= cpBottom)
                {
                    GlobalState.SetCurrentDrawingColor(dvWindow.MainBuffer.GetPixel(mouseX, mouseY));
                    leftMouse = false;
                }
                ToggleColorPicker(cpLeft, cpTop);
                return;
            }

            ushort color = GlobalState.CurrentDrawingColor;
            Utilities.AnimateColor(ref color, paintingFrame);

            if (GlobalState.BrushType == BrushType.Shader)
            {
                int frameMod = size / 2 + 1;
                if (mouseFrame % frameMod != 0)
                {
                    mouseFrame++;
                    return;
                }
            }

            switch (GlobalState.PaintingStyle)
            {
                case PaintingStyle.Tile:
                    {
                        int xsize = dvWindow.Width / 4;
                        int ysize = dvWindow.Height / 4;
                        int localx = mouseX % xsize;
                        int localy = mouseY % ysize;
                        for (int i = -1; i < 5; i++)
                        {
                            for (int j = -1; j < 5; j++)
                            {
                                PaintOne(i * xsize + localx, j * ysize + localy, size, color);
                            }
                        }
                    }
                    break;
                case PaintingStyle.Kaleidoscope:
                    int centerx = dvWindow.Width / 2;
                    int centery = dvWindow.Height / 2;
                    int dx = mouseX - centerx;
                    int dy = mouseY - centery;
                    double theta = Utilities.GetAngle(dx, dy, 1, 0);
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    for (int i = 0; i < 4; i++)
                    {
                        double theta2 = theta + i * (Math.PI / 2);
                        int x = (int)(centerx + distance * Math.Cos(theta2));
                        int y = (int)(centery + distance * Math.Sin(theta2));
                        PaintOne(x, y, size, color);

                        theta2 = (Math.PI - theta) + i * (Math.PI / 2);
                        x = (int)(centerx + distance * Math.Cos(theta2));
                        y = (int)(centery + distance * Math.Sin(theta2));
                        PaintOne(x, y, size, color);
                    }

                    break;
                default:
                    PaintOne(mouseX, mouseY, size, color);
                    break;
            }

        }
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handle painting and drawing
        /// </summary>
        /// --------------------------------------------------------------------------
        private void HandleMouseClick_TextEntry()
        {
            paintingFrame++;
            textAnimator.WriteToMainBuffer();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Paint one frame's worth 
        /// </summary>
        /// --------------------------------------------------------------------------
        private void PaintOne(int px, int py, int size, ushort color)
        {

            switch (GlobalState.BrushType)
            {
                case BrushType.Circle:
                    dvWindow.MainBuffer.DrawFilledCircle(
                        color,
                        px, py,
                        size);
                    break;
                case BrushType.SprayPaint:
                    SprayPaint(px, py, size, size + 5, color);
                    break;
                case BrushType.Windmill:
                    DrawWindmillStroke(px, py, size, color, dvWindow.MainBuffer);
                    break;
                case BrushType.PictureStamp:
                    int pictureIndex = GlobalState.BrushSize % MediaBag.miniPicCount;
                    MediaBag.DrawMiniPic(dvWindow.MainBuffer, pictureIndex, px - 40, py - 40);
                    break;
                case BrushType.LifePattern:
                    LifePattern currentPattern = LifePattern.GlobalPatterns[GlobalState.BrushSize % LifePattern.GlobalPatterns.Count];
                    int x = px - currentPattern.width / 2;
                    int y = py - currentPattern.height / 2;
                    currentPattern.Draw(dvWindow.MainBuffer, color, x, y);
                    break;
                case BrushType.LineTarget:
                    if (targetX < 0 || targetX > dvWindow.Width || targetY < 0 || targetY > dvWindow.Height)
                    {
                        targetX = mouseX;
                        targetY = mouseY;
                    }
                    dvWindow.MainBuffer.DrawLine(color, targetX, targetY, px, py);
                    dvWindow.MainBuffer.DrawLine(color, targetX, targetY, px - 1, py);
                    dvWindow.MainBuffer.DrawLine(color, targetX, targetY, px + 1, py);
                    dvWindow.MainBuffer.DrawLine(color, targetX, targetY, px, py - 1);
                    dvWindow.MainBuffer.DrawLine(color, targetX, targetY, px, py + 1);
                    break;
                case BrushType.Shader:
                    Shade(px, py, size);
                    break;
                default: break;
            }
            mouseFrame++;
        }

        private void DrawWindmillStroke(int px, int py, int size, ushort color, PixelBuffer buffer)
        {
            double theta = frame / (2.0 * (15 - modulator));
            double offset = 0.015;
            int x1 = (int)(px + size * Math.Cos(theta + offset));
            int x2 = (int)(px + size * Math.Cos(theta + Math.PI - offset));
            int x3 = (int)(px + size * Math.Cos(theta - offset));
            int x4 = (int)(px + size * Math.Cos(theta + Math.PI + offset));
            int y1 = (int)(py + size * Math.Sin(theta + offset));
            int y2 = (int)(py + size * Math.Sin(theta + Math.PI - offset));
            int y3 = (int)(py + size * Math.Sin(theta - offset));
            int y4 = (int)(py + size * Math.Sin(theta + Math.PI + offset));
            buffer.DrawLine(color, x1, y1, x2, y2);
            buffer.DrawLine(color, x1, y1, x4, y4);
            buffer.DrawLine(color, x1, y1, x3, y3);
            buffer.DrawLine(color, x2, y2, x4, y4);
            buffer.DrawLine(color, x3, y3, x4, y4);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// shade a part of the screen
        /// </summary>
        /// --------------------------------------------------------------------------
        private void Shade(int px, int py, int size)
        {
            ushort[] data = dvWindow.MainBuffer.RawBuffer;
            int pitch = dvWindow.MainBuffer.BufferPitch;
            int width = dvWindow.Width;
            int height = dvWindow.Height;
            int minx = px - size;
            int miny = py - size;
            int maxx = px + size;
            int maxy = py + size;
            if (minx >= width || miny >= height || maxx < 0 || maxy < 0) return;
            if (minx < 0) minx = 0;
            if (miny < 0) miny = 0;
            if (maxx > width - 1) maxx = width - 1;
            if (maxy > height - 1) maxy = height - 1;

            int r2 = size * size;
            int[] radialProbability = new int[r2];
            for (int i = 0; i < r2; i++)
            {
                double theta = (double)i / r2;
                radialProbability[i] = (int)(theta * 1000);
            }
            
            for (int y = miny; y <= maxy; y++)
            {
                for (int x = minx; x <= maxx; x++)
                {
                    int d = (px-x) * (px-x) + (py-y) * (py-y);
                    if (d >= r2) continue;
                    if (Utilities.Rand(1000) < radialProbability[d]) continue;
                    int spot = (int)(y * pitch + x);
                    uint color = data[spot];
                    if ((color & 0x8000) == 0)
                    {
                        uint red = (color >> 10) & 0x1f;
                        uint green = (color >> 5) & 0x1f;
                        uint blue = (color >> 0) & 0x1f;

                        if (red > 0) red--;
                        if (green > 0) green--;
                        if (blue > 0) blue--;

                        data[spot] = (ushort)(blue + (green << 5) + (red << 10));
                    }
                    else
                    {
                        data[spot] = Utilities.Flatten((ushort)color);
                    }
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Erase the last mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void ClearLastMouseCursor()
        {
            foreach (Rectangle rect in mouseClearRectangles)
            {
                dvWindow.OverlayBuffer.DrawFilledRectangle(
                    Color.Black,
                    rect.X,
                    rect.Y,
                    rect.X + rect.Width,
                    rect.Y + rect.Height);
            }

            mouseClearRectangles.Clear();
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a circle brush mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Circle()
        {
            int r = GlobalState.BrushSize;
            dvWindow.OverlayBuffer.DrawCircle(mouseEngagementColor, mouseX, mouseY, r);

            if (GlobalState.BrushType == BrushType.Circle)
            {
                for (int i = r; i > 0; i--)
                {
                    if (((i + frame / 3) % 5) == 0)
                    {
                        dvWindow.OverlayBuffer.DrawCircle(GlobalState.CurrentDrawingColor, mouseX, mouseY, i);
                    }
                }
            }
            else if (GlobalState.BrushType == BrushType.Shader)
            {
                for (int i = 0; i < r * 4; i++)
                {
                    double theta = Utilities.DRand(Math.PI * 2);
                    int x = (int)(mouseX + r * Math.Cos(theta));
                    int y = (int)(mouseY + r * Math.Sin(theta));
                    dvWindow.OverlayBuffer.DrawPixel((ushort)1, x, y);
                }
            }

            mouseClearRectangles.Add(new Rectangle(mouseX - r, mouseY - r, r * 2 + 1, r * 2 + 1));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a spraypaint brush mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_SprayPaint()
        {
            ushort color = GlobalState.CurrentDrawingColor;
            int radius = GlobalState.BrushSize;
            dvWindow.OverlayBuffer.DrawCircle(mouseEngagementColor, mouseX, mouseY, radius);

            Random localRand = new Random(1);
            for (int i = 0; i < radius * 3 + 10; i++)
            {
                Utilities.AnimateColor(ref color, (uint)(i));

                double theta = Utilities.DRand(2) * Math.PI;
                int r = localRand.Next(radius * 2) + localRand.Next(radius * 2);
                r = (r / 2) - radius;

                int x = (int)(mouseX + r * Math.Cos(theta));
                int y = (int)(mouseY + r * Math.Sin(theta));

                dvWindow.OverlayBuffer.DrawPixel(color, x, y);
            }

            mouseClearRectangles.Add(new Rectangle(mouseX - radius, mouseY - radius, radius * 2 + 1, radius * 2 + 1));
        }
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a spraypaint brush mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Bomb()
        {
            ushort color = GlobalState.CurrentDrawingColor;

            int radius = 2 + GlobalState.BrushSize / 6;

            for (int i = 0; i < (10 + 10 * GlobalState.BrushSize / 4); i++)
            {
                Utilities.AnimateColor(ref color, (uint)(i));

                double theta = Utilities.DRand(2) * Math.PI;
                int r = Utilities.Rand(radius * 2) + Utilities.Rand(radius * 2);
                r = (r / 2) - radius;

                int x = (int)(mouseX + r * Math.Cos(theta));
                int y = (int)(mouseY + r * Math.Sin(theta));

                if(Utilities.Rand(4) == 0)  dvWindow.OverlayBuffer.DrawPixel(Color.White, x, y);
                else dvWindow.OverlayBuffer.DrawPixel(color, x, y);
            }

            mouseClearRectangles.Add(new Rectangle(mouseX - radius, mouseY - radius, radius * 2 + 1, radius * 2 + 1));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw an animated windmill mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Windmill()
        {
            
            DrawWindmillStroke(mouseX, mouseY, GlobalState.BrushSize, PixelBuffer.ColorConverters._5Bit((uint)mouseEngagementColor.ToArgb()), dvWindow.OverlayBuffer);
            mouseClearRectangles.Add(new Rectangle(mouseX - GlobalState.BrushSize, mouseY - GlobalState.BrushSize, GlobalState.BrushSize * 2 + 1, GlobalState.BrushSize * 2 + 1));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw an animated square mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Square()
        {
            int r = GlobalState.BrushSize;
            dvWindow.OverlayBuffer.DrawRectangle(mouseEngagementColor, mouseX - r, mouseY - r, mouseX - r + r * 2, mouseY - r + r * 2);

            for (int i = r; i > 0; i--)
            {
                if (((i + frame / 3) % 5) == 0)
                {
                    dvWindow.OverlayBuffer.DrawRectangle(GlobalState.CurrentDrawingColor, mouseX - i, mouseY - i, mouseX - i + i * 2, mouseY - i + i * 2);
                }
            }

            mouseClearRectangles.Add(new Rectangle(mouseX - r, mouseY - r, r * 2 + 1, r * 2 + 1));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw an life pattern cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_LifePattern()
        {
            LifePattern currentPattern = LifePattern.GlobalPatterns[GlobalState.BrushSize % LifePattern.GlobalPatterns.Count];

            int x = mouseX - currentPattern.width / 2;
            int y = mouseY - currentPattern.height / 2;


            dvWindow.OverlayBuffer.DrawRectangle(Color.Gray, x-1, y-1, x+currentPattern.width, y+currentPattern.height);
            currentPattern.Draw(dvWindow.OverlayBuffer, GlobalState.CurrentDrawingColor, x, y);

            mouseClearRectangles.Add(new Rectangle(x - 1, y - 1, currentPattern.width+2, currentPattern.height+2));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw an life pattern cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_PictureStamp()
        {
            int pictureIndex = GlobalState.BrushSize % MediaBag.miniPicCount;
            MediaBag.DrawMiniPic(dvWindow.OverlayBuffer, pictureIndex, mouseX - 40, mouseY - 40);

            mouseClearRectangles.Add(new Rectangle(mouseX - 40, mouseY - 40, 80, 80));
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a regular pointer at the mouse location
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Pointer()
        {
            DrawMouseCursor_Pointer(mouseX, mouseY);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a regular pointer mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Pointer(int x, int y)
        {
            int cursorSize = 6;
            dvWindow.OverlayBuffer.DrawFilledRectangle(Color.FromArgb(1), x - cursorSize, y - 1, x + cursorSize, y + 1);
            dvWindow.OverlayBuffer.DrawFilledRectangle(Color.FromArgb(1), x - 1, y - cursorSize, x + 1, y + cursorSize);
            dvWindow.OverlayBuffer.DrawLine(Color.White, x - (cursorSize - 1), y, x + (cursorSize - 1), y);
            dvWindow.OverlayBuffer.DrawLine(Color.White, x, y - (cursorSize - 1), x, y + (cursorSize - 1));
            dvWindow.OverlayBuffer.DrawFilledRectangle(Color.Black, x - 1, y - 1, x + 1, y + 1);

            mouseClearRectangles.Add(new Rectangle(x - cursorSize, y - cursorSize, cursorSize * 2 + 1, cursorSize * 2 + 1));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a regular pointer mouse cursor
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_PaintBucket()
        {
            MediaBag.DrawMiniPic(dvWindow.OverlayBuffer, 51, mouseX - 3, mouseY - 21);

            mouseClearRectangles.Add(new Rectangle(mouseX - 3, mouseY - 21, 30, 30));
        }

        int lastDragX = 0, lastDragY = 0;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a rectangle and square to indicate dragging result
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor_Dragging()
        {
            if (leftMouse && workingPoints.Count > 0)
            {
                DrawDragShapes(lastDragX, lastDragY, workingPoints[0].X, workingPoints[0].Y, 0);
                DrawDragShapes(mouseX, mouseY, workingPoints[0].X, workingPoints[0].Y, MediaBag.color_White);
                lastDragX = mouseX;
                lastDragY = mouseY;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Draw a rectangle and square to indicate dragging result
        /// </summary>
        /// --------------------------------------------------------------------------
        private void DrawDragShapes(int lastDragX, int lastDragY, int wx, int wy, ushort color)
        {
            int dx = lastDragX - wx;
            int dy = lastDragY - wy;
            int r = (int)Math.Sqrt(dx * dx + dy * dy);

            dvWindow.OverlayBuffer.DrawCircle(color, wx, wy, r);
            dvWindow.OverlayBuffer.DrawRectangle(color, wx, wy, lastDragX, lastDragY);
            dvWindow.OverlayBuffer.DrawLine(color, wx, wy, lastDragX, lastDragY);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Handles drawing the mouse cursor
        /// </summary>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        private void DrawMouseCursor()
        {
            ClearLastMouseCursor();

            if (GlobalState.BrushSound != null) 
            {
                GlobalState.BrushSound.Volume = (GlobalState.BrushSize / 250.0 + .03);
                if (Utilities.Rand(5) == 0) GlobalState.BrushSound.Volume *= 3;
            }

            mouseEngagementColor = (mouseMoveCounter == 0) ? Color.Black : Color.White;

            if (colorPicker)
            {
                DrawMouseCursor_Pointer();
            }
            else if (mouseMotion || GlobalState.BrushType == BrushType.Bomb)
            {
                switch (GlobalState.BrushType)
                {
                    case BrushType.Circle: DrawMouseCursor_Circle(); break;
                    case BrushType.Shader: DrawMouseCursor_Circle(); break;
                    case BrushType.Windmill: DrawMouseCursor_Windmill(); break;
                    case BrushType.SprayPaint: DrawMouseCursor_SprayPaint(); break;
                    case BrushType.LifePattern: DrawMouseCursor_LifePattern(); break;
                    case BrushType.PictureStamp: DrawMouseCursor_PictureStamp(); break;
                    case BrushType.LineTarget:
                        DrawMouseCursor_Pointer();
                        DrawMouseCursor_Pointer(targetX, targetY);
                        break;
                    case BrushType.Bomb: DrawMouseCursor_Bomb(); break;
                    case BrushType.Dragging: DrawMouseCursor_Dragging(); DrawMouseCursor_Pointer(); break;
                    case BrushType.FloodFill: DrawMouseCursor_PaintBucket(); break;
                    default: DrawMouseCursor_Pointer(); break;
                }
            }

        }
    }

}
