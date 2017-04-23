using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Drawing;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// Base class for animations
    /// </summary>
    /// --------------------------------------------------------------------------
    public abstract partial class Animation : Utilities
    {
        bool iamDone = false;
        DateTime start = DateTime.Now;
        int mousex;
        int mousey;

        internal DVWindow dvWindow;
        public abstract void Render();

        public virtual bool IsDone{ get{return iamDone;} set{iamDone = value;}}

        public virtual int MouseX { get { return mousex; } set { mousex = value; } }
        public virtual int MouseY { get { return mousey; } set { mousey = value; } }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        public Animation(DVWindow drawingWindow)
        {
            this.dvWindow = drawingWindow;
        }


    }
}
