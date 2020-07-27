using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Drawing = System.Drawing;
using Xna = Microsoft.Xna.Framework;

namespace DirectVarmint
{
    // System.Drawing and the XNA Framework both define Color and Rectangle
    // types. To avoid conflicts, we specify exactly which ones to use.
    using Color = System.Drawing.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;


    public static class VarmintGlobals
    {
        public static ContentManager Content;
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Manages access to XNA control
    /// </summary>
    /// --------------------------------------------------------------------------
    public class DirectVarmintControl : GraphicsDeviceControl
    {
        SpriteBatch spriteBatch;
        XNABitmap xnaBitmap;
        public int[] RgbBuffer
        {
            get
            {
                if (xnaBitmap == null) Init();
                return xnaBitmap.RgbBuffer;
            }
        }

        public bool HasMouseInside { get; set; }

        int graphicsWidth;
        int graphicsHeight;

        public int GraphicsWidth { get { return graphicsWidth; } }
        public int GraphicsHeight { get { return graphicsHeight; } }

        public DirectVarmintControl(int graphicsWidth, int graphicsHeight)
        {
            this.graphicsHeight = graphicsHeight;
            this.graphicsWidth = graphicsWidth;
            this.MouseLeave += new EventHandler(DirectVarmintControl_MouseLeave);
            this.MouseEnter += new EventHandler(DirectVarmintControl_MouseEnter);


        }

        void DirectVarmintControl_MouseEnter(object sender, EventArgs e)
        {
            HasMouseInside = true;
        }

        void DirectVarmintControl_MouseLeave(object sender, EventArgs e)
        {
            HasMouseInside = false;
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {                                                                                                  
        //        CreateParams cp = base.CreateParams; 
        //        cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT     
        //        return cp;   
        //    }
        //}
 
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Dispose(bool disposing) { }

        public override void UpdateBits()
        {
            xnaBitmap.UpdateBits();
        }

        protected override void Draw()
        {
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Red);
            spriteBatch.Begin();
            spriteBatch.Draw(xnaBitmap.Texture, new Microsoft.Xna.Framework.Rectangle(0, 0, Width, Height), Microsoft.Xna.Framework.Color.White);
            spriteBatch.End();
        }

        void Init()
        {
            xnaBitmap = new XNABitmap(GraphicsDevice, graphicsWidth, graphicsHeight);
            xnaBitmap.LoadContent();
        }
    }

    /// <summary>
    /// Custom control uses the XNA Framework GraphicsDevice to render onto
    /// a Windows Form. Derived classes can override the Initialize and Draw
    /// methods to add their own drawing code.
    /// </summary>
    abstract public class GraphicsDeviceControl : Control
    {
        #region Fields


        // However many GraphicsDeviceControl instances you have, they all share
        // the same underlying GraphicsDevice, managed by this helper service.
        GraphicsDeviceService graphicsDeviceService;


        #endregion

        #region Properties


        /// <summary>
        /// Gets a GraphicsDevice that can be used to draw onto this control.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDeviceService.GraphicsDevice; }
        }


        /// <summary>
        /// Gets an IServiceProvider containing our IGraphicsDeviceService.
        /// This can be used with components such as the ContentManager,
        /// which use this service to look up the GraphicsDevice.
        /// </summary>
        public ServiceContainer Services
        {
            get { return services; }
        }

        ServiceContainer services = new ServiceContainer();


        #endregion

        #region Initialization


        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void OnCreateControl()
        {
            // Don't initialize the graphics device if we are running in the designer.
            if (!DesignMode)
            {
                graphicsDeviceService = GraphicsDeviceService.AddRef(Handle,
                                                                     ClientSize.Width,
                                                                     ClientSize.Height);

                // Register the service, so components like ContentManager can find it.
                services.AddService<IGraphicsDeviceService>(graphicsDeviceService);

                if (VarmintGlobals.Content == null)
                {
                    VarmintGlobals.Content = new ContentManager(services, "Content");
                }

                // Give derived classes a chance to initialize themselves.
                Initialize();
            }

            base.OnCreateControl();
        }


        /// <summary>
        /// Disposes the control.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (graphicsDeviceService != null)
            {
                graphicsDeviceService.Release(disposing);
                graphicsDeviceService = null;
            }

            base.Dispose(disposing);
        }


        #endregion

        #region Paint


        /// <summary>
        /// Redraws the control in response to a WinForms paint message.
        /// </summary>
        int paintCounter = 0;
        protected override void OnPaint(PaintEventArgs e)
        {
            string beginDrawError = BeginDraw();

            if (string.IsNullOrEmpty(beginDrawError))
            {
                // Draw the control using the GraphicsDevice.
                Draw();
                EndDraw();
            }
            else
            {
                // If BeginDraw failed, show an error message using System.Drawing.
                PaintUsingSystemDrawing(e.Graphics, beginDrawError);
            }
        }


        /// <summary>
        /// Attempts to begin drawing the control. Returns an error message string
        /// if this was not possible, which can happen if the graphics device is
        /// lost, or if we are running inside the Form designer.
        /// </summary>
        string BeginDraw()
        {
            // If we have no graphics device, we must be running in the designer.
            if (graphicsDeviceService == null)
            {
                return Text + "\n\n" + GetType();
            }

            // Make sure the graphics device is big enough, and is not lost.
            string deviceResetError = HandleDeviceReset();

            if (!string.IsNullOrEmpty(deviceResetError))
            {
                return deviceResetError;
            }

            // Many GraphicsDeviceControl instances can be sharing the same
            // GraphicsDevice. The device backbuffer will be resized to fit the
            // largest of these controls. But what if we are currently drawing
            // a smaller control? To avoid unwanted stretching, we set the
            // viewport to only use the top left portion of the full backbuffer.
            Viewport viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;

            viewport.Width = ClientSize.Width;
            viewport.Height = ClientSize.Height;

            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;

            GraphicsDevice.Viewport = viewport;

            return null;
        }


        /// <summary>
        /// Ends drawing the control. This is called after derived classes
        /// have finished their Draw method, and is responsible for presenting
        /// the finished image onto the screen, using the appropriate WinForms
        /// control handle to make sure it shows up in the right place.
        /// </summary>
        void EndDraw()
        {
            try
            {
                Rectangle sourceRectangle = new Rectangle(0, 0, ClientSize.Width,
                                                                ClientSize.Height);

                GraphicsDevice.Present();
            }
            catch
            {
                // Present might throw if the device became lost while we were
                // drawing. The lost device will be handled by the next BeginDraw,
                // so we just swallow the exception.
            }
        }


        /// <summary>
        /// Helper used by BeginDraw. This checks the graphics device status,
        /// making sure it is big enough for drawing the current control, and
        /// that the device is not lost. Returns an error string if the device
        /// could not be reset.
        /// </summary>
        string HandleDeviceReset()
        {
            bool deviceNeedsReset = false;

            switch (GraphicsDevice.GraphicsDeviceStatus)
            {
                case GraphicsDeviceStatus.Lost:
                    // If the graphics device is lost, we cannot use it at all.
                    return "Graphics device lost";

                case GraphicsDeviceStatus.NotReset:
                    // If device is in the not-reset state, we should try to reset it.
                    deviceNeedsReset = true;
                    break;

                default:
                    // If the device state is ok, check whether it is big enough.
                    PresentationParameters pp = GraphicsDevice.PresentationParameters;

                    deviceNeedsReset = (ClientSize.Width > pp.BackBufferWidth) ||
                                       (ClientSize.Height > pp.BackBufferHeight);
                    break;
            }

            // Do we need to reset the device?
            if (deviceNeedsReset)
            {
                Debug.WriteLine("RESETING DEVICE ... ");
                try
                {
                    graphicsDeviceService.ResetDevice(ClientSize.Width,
                                                      ClientSize.Height);
                }
                catch (Exception e)
                {
                    return "Graphics device reset failed\n\n" + e;
                }
            }

            return null;
        }


        /// <summary>
        /// If we do not have a valid graphics device (for instance if the device
        /// is lost, or if we are running inside the Form designer), we must use
        /// regular System.Drawing method to display a status message.
        /// </summary>
        protected virtual void PaintUsingSystemDrawing(Graphics graphics, string text)
        {
            graphics.Clear(Color.CornflowerBlue);

            using (Brush brush = new SolidBrush(Color.Black))
            {
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, Font, brush, ClientRectangle, format);
                }
            }
        }


        // Ignores WinForms paint-background messages to avoid flickering.
        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        #endregion

        protected abstract void Initialize();
        protected abstract void Draw();
        public abstract void UpdateBits();

    }

    /// <summary>
    /// Helper class responsible for creating and managing the GraphicsDevice.
    /// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
    /// so even though there can be many controls, there will only ever be a single
    /// underlying GraphicsDevice. This implements the standard IGraphicsDeviceService
    /// interface, which provides notification events for when the device is reset
    /// or disposed.
    /// </summary>
    class GraphicsDeviceService : IGraphicsDeviceService
    {
        #region Fields
        
        // Singleton device service instance.
        static GraphicsDeviceService singletonInstance;


        // Keep track of how many controls are sharing the singletonInstance.
        static int referenceCount;

        GraphicsDevice graphicsDevice;

        // Store the current device settings.
        PresentationParameters parameters;
        
        // IGraphicsDeviceService events.
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        #endregion


        /// <summary>
        /// Constructor is private, because this is a singleton class:
        /// client controls should use the public AddRef method instead.
        /// </summary>
        GraphicsDeviceService(IntPtr windowHandle, int width, int height)
        {
            parameters = new PresentationParameters();

            parameters.BackBufferWidth = Math.Max(width, 1);
            parameters.BackBufferHeight = Math.Max(height, 1);
            parameters.BackBufferFormat = SurfaceFormat.Color;
            parameters.DeviceWindowHandle = windowHandle;

            parameters.DepthStencilFormat = DepthFormat.Depth24;

            graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter,
                GraphicsProfile.HiDef, parameters);
        }


        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef(IntPtr windowHandle,
                                                   int width, int height)
        {
            return new GraphicsDeviceService(windowHandle, width, height);
            //// Increment the "how many controls sharing the device" reference count.
            //if (Interlocked.Increment(ref referenceCount) == 1)
            //{
            //    // If this is the first control to start using the
            //    // device, we must create the singleton instance.
            //    singletonInstance = new GraphicsDeviceService(windowHandle,
            //                                                  width, height);
            //}

            //return singletonInstance;
        }


        /// <summary>
        /// Releases a reference to the singleton instance.
        /// </summary>
        public void Release(bool disposing)
        {
            // Decrement the "how many controls sharing the device" reference count.
            if (Interlocked.Decrement(ref referenceCount) == 0)
            {
                // If this is the last control to finish using the
                // device, we should dispose the singleton instance.
                if (disposing)
                {
                    if (DeviceDisposing != null)
                        DeviceDisposing(this, EventArgs.Empty);

                    graphicsDevice.Dispose();
                }

                graphicsDevice = null;
            }
        }


        /// <summary>
        /// Resets the graphics device to whichever is bigger out of the specified
        /// resolution or its current size. This behavior means the device will
        /// demand-grow to the largest of all its GraphicsDeviceControl clients.
        /// </summary>
        public void ResetDevice(int width, int height)
        {
            if (DeviceResetting != null)
                DeviceResetting(this, EventArgs.Empty);

            parameters.BackBufferWidth = Math.Max(parameters.BackBufferWidth, width);
            parameters.BackBufferHeight = Math.Max(parameters.BackBufferHeight, height);

            // TODO:Actually reset the device
            Debug.WriteLine($"Unsuccessfuly tried to reset device to {width} by {height}");
            //graphicsDevice.Reset(parameters);

            if (DeviceReset != null)
                DeviceReset(this, EventArgs.Empty);
        }


        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }
    }

    /// <summary>
    /// Container class implements the IServiceProvider interface. This is used
    /// to pass shared services between different components, for instance the
    /// ContentManager uses it to locate the IGraphicsDeviceService implementation.
    /// </summary>
    public class ServiceContainer : IServiceProvider
    {
        Dictionary<Type, object> services = new Dictionary<Type, object>();


        /// <summary>
        /// Adds a new service to the collection.
        /// </summary>
        public void AddService<T>(T service)
        {
            services.Add(typeof(T), service);
        }


        /// <summary>
        /// Looks up the specified service.
        /// </summary>
        public object GetService(Type serviceType)
        {
            object service;

            services.TryGetValue(serviceType, out service);

            return service;
        }
    }

    public class XNABitmap
    {
        private Xna.Graphics.GraphicsDevice xnaDevice;

        // we'll be double-buffering the Xna surface
        private Xna.Graphics.Texture2D[] xnaBuffer = new Xna.Graphics.Texture2D[2];

        private int width = 256;
        private int height = 256;

        private int frontBufferIndex = 0;
        private int backBufferIndex = 1;

        public int[] RgbBuffer = null;
        int pitch;


        public XNABitmap(Xna.Graphics.GraphicsDevice xnaDevice, int width, int height)
        {
            this.xnaDevice = xnaDevice;
            this.width = width;
            this.height = height;
            Debug.WriteLine("Creating " + width + " x " + height);

        }

        /// <summary>
        /// Loads Xna resources, ie the Xna buffer texture. The gdiResources are created in the constructor.
        /// </summary>
        public void LoadContent()
        {
            // optionally kill resources left after device reset
            DisposeXnaResources();

            xnaBuffer[0] = new Xna.Graphics.Texture2D(xnaDevice, width, height, false, Xna.Graphics.SurfaceFormat.Color);
            xnaBuffer[1] = new Xna.Graphics.Texture2D(xnaDevice, width, height, false, Xna.Graphics.SurfaceFormat.Color);

            pitch = width;
            RgbBuffer = new int[pitch * height];
        }

        private void DisposeXnaResources()
        {
            if (xnaBuffer[0] != null)
            {
                xnaBuffer[0].Dispose();
                xnaBuffer[0] = null;
            }

            if (xnaBuffer[1] != null)
            {
                xnaBuffer[1].Dispose();
                xnaBuffer[1] = null;
            }
        }

        /// <summary>
        /// Disposes Xna resources, ie the Xna buyffer texture
        /// </summary>
        public void UnloadContent()
        {
            DisposeXnaResources();
        }

        /// <summary>
        /// Disposes all resources, both Xna and Gdi ones
        /// </summary>
        public void Dispose()
        {
            DisposeXnaResources();
        }


        public Xna.Graphics.Texture2D Texture
        {
            get
            {
                return xnaBuffer[frontBufferIndex];
            }
        }

        internal void UpdateBits()
        {
            xnaBuffer[backBufferIndex].SetData<int>(RgbBuffer);

            // Swap buffers
            backBufferIndex = (backBufferIndex + 1) % 2;
            frontBufferIndex = (frontBufferIndex + 1) % 2;
        }
    }
}