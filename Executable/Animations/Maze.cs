using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    public abstract partial class Animation
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Maze animation
        /// </summary>
        /// --------------------------------------------------------------------------
        public class Maze : Animation
        {
            MazeBlock[,] mazeData;
            int mazeW, mazeH;
            int centeringX;
            int centeringY;
            int blockSize;

            public int BorderX { get { return centeringX + 2; } }
            public int BorderY { get { return centeringY + 2; } }
            public int MazeW { get { return mazeW; } }
            public int MazeH { get { return mazeH; } }
            public int BlockSize { get { return blockSize; } }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// --------------------------------------------------------------------------
            public Maze(DVWindow window, ushort color)
                : base(window)
            {
                blockSize = Rand(15) + 20;
                mazeW = window.MainBuffer.Width / blockSize;
                mazeH = window.MainBuffer.Height / blockSize;
                centeringX = (window.MainBuffer.Width % blockSize) / 2;
                centeringY = (window.MainBuffer.Height % blockSize) / 2;

                mazeData = new MazeBlock[mazeW, mazeH];

                for (int i = 0; i < mazeW; i++)
                {
                    for (int j = 0; j < mazeH; j++)
                    {
                        mazeData[i, j] = new MazeBlock(
                            GetAnimatedColor(color, i + j * mazeW), 
                            i * blockSize + centeringX, 
                            j * blockSize + centeringY, 
                            blockSize, 
                            blockSize);
                    }
                }

                DigMaze(Rand(mazeW), Rand(mazeH));
            }

            /// <summary>
            /// Track direction
            /// </summary>
            enum Direction
            {
                Up,
                Left,
                Right,
                Down
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Recursive method to dig the maze
            /// </summary>
            /// --------------------------------------------------------------------------
            bool DigMaze(int x, int y)
            {
                if (mazeData[x, y].touched) return false;
                mazeData[x, y].touched = true;

                List<Direction> choices = new List<Direction>();
                if (x > 0) choices.Add(Direction.Left);
                if (x < mazeW - 1) choices.Add(Direction.Right);
                if (y > 0 ) choices.Add(Direction.Up);
                if (y < mazeH - 1) choices.Add(Direction.Down);

                while (choices.Count > 0)
                {
                    mazeData[x, y].touched = true;
                    int choice = Rand(choices.Count);
                    switch (choices[choice])
                    {
                        case Direction.Up:
                            if (DigMaze(x, y - 1))
                            {
                                mazeData[x, y].wallTop = false;
                                mazeData[x, y - 1].wallBottom = false;
                            }
                            break;
                        case Direction.Down:
                            if (DigMaze(x, y + 1))
                            {
                                mazeData[x, y].wallBottom = false;
                                mazeData[x, y + 1].wallTop = false;
                            }
                            break;
                        case Direction.Left:
                            if (DigMaze(x - 1, y))
                            {
                                mazeData[x, y].wallLeft = false;
                                mazeData[x - 1, y].wallRight = false;
                            }
                            break;
                        case Direction.Right:
                            if (DigMaze(x + 1, y))
                            {
                                mazeData[x, y].wallRight = false;
                                mazeData[x + 1, y].wallLeft = false;
                            }
                            break;
                    }
                    choices.RemoveAt(choice);
                }

                return true;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Render
            /// </summary>
            /// --------------------------------------------------------------------------
            public override void Render()
            {
                if (IsDone) return;

                //int mx = (mousex - centeringX) / blockSize;
                //int my = (mousey - centeringY) / blockSize;
                //if (mx < 0) mx = 0;
                //if (my < 0) my = 0;
                //if (mx >= mazeW) mx = mazeW - 1;
                //if (my >= mazeH) my = mazeH - 1;
                //mazeData[mx, my].visible = true;

                for (int i = 0; i < mazeW; i++)
                {
                    for (int j = 0; j < mazeH; j++)
                    {
                        mazeData[i, j].Draw(dvWindow);
                    }
                }

                IsDone = true;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// One element of the maze
            /// </summary>
            /// --------------------------------------------------------------------------
            class MazeBlock
            {
                public bool wallTop = true;
                public bool wallLeft = true;
                public bool wallRight = true;
                public bool wallBottom = true;
                public bool visible = true;
                public bool touched = false;
                public bool done = false;
                public bool rendered = false;
                int x, y;
                int w, h;
                ushort color;

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Constructor
                /// </summary>
                /// --------------------------------------------------------------------------
                public MazeBlock(ushort color, int x, int y, int w, int h)
                {
                    this.x = x;
                    this.y = y;
                    this.w = w;
                    this.h = h;
                    this.color = color;
                }

                /// --------------------------------------------------------------------------
                /// <summary>
                /// Draw the block
                /// </summary>
                /// --------------------------------------------------------------------------
                public void Draw(DVWindow dvWindow)
                {
                    if (!visible) return;
                    if (wallTop) dvWindow.MainBuffer.DrawLine(color, x, y, x + w-1, y);
                    if (wallLeft) dvWindow.MainBuffer.DrawLine(color, x, y, x, y + h - 1);
                    if (wallRight) dvWindow.MainBuffer.DrawLine(color, x + w - 1, y, x + w - 1, y + h - 1);
                    if (wallBottom) dvWindow.MainBuffer.DrawLine(color, x, y + h - 1, x + w - 1, y + h - 1);
                    rendered = true;
                }
            }
        }
    }
}
