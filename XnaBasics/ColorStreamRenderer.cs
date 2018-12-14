//------------------------------------------------------------------------------
// <copyright file="ColorStreamRenderer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.XnaBasics
{
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// This class renders the current color stream frame.
    /// </summary>
    public class ColorStreamRenderer : Object2D
    {
        /// <summary>
        /// This child responsible for rendering the color stream's skeleton.
        /// </summary>
        private readonly SkeletonStreamRenderer skeletonStream;
        
        /// <summary>
        /// The last frame of color data.
        /// </summary>
        private byte[] colorData;
        public byte[] LastFrame { get { return colorData; } }

        /// <summary>
        /// The update byte for the color data
        /// </summary>
        private static bool updateByte = false;
        public static bool UpdateByte { get { return updateByte; } }

        /// <summary>
        /// Whether or not the kinect is currently available to render the color stream.
        /// </summary>
        public bool KinectAvailable
        {
            get
            {
                return null == this.Chooser.Sensor ||
                    false == this.Chooser.Sensor.IsRunning ||
                    KinectStatus.Connected != this.Chooser.Sensor.Status;
            }
        }

        /// <summary>
        /// The color frame as a texture.
        /// </summary>
        private Texture2D colorTexture;

        /// <summary>
        /// The back buffer where color frame is scaled as requested by the Size.
        /// </summary>
        private RenderTarget2D backBuffer;
        
        /// <summary>
        /// This Xna effect is used to swap the Red and Blue bytes of the color stream data.
        /// </summary>
        private Effect kinectColorVisualizer;

        /// <summary>
        /// Whether or not the back buffer needs updating.
        /// </summary>
        private bool needToRedrawBackBuffer = true;

        /// <summary>
        /// The location and size the render is to draw at
        /// </summary>
        public Rectangle drawRectangle = new Rectangle(15, 20, 320, 240);
        public static Rectangle ThreeTwentyDisplay = new Rectangle(15, 15, 320, 240);
        public static Rectangle TwoFortyDisplay = new Rectangle(15, 15, 240, 180);

        public bool DisplayingSkeleton = true;

        /// <summary>
        /// Initializes a new instance of the ColorStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        public ColorStreamRenderer(Game game)
            : base(game)
        {
            this.skeletonStream = new SkeletonStreamRenderer(game, this.SkeletonToColorMap);
        }

        /// <summary>
        /// Initializes the necessary children.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            this.Size = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
        }

        /// <summary>
        /// The update method where the new color frame is retrieved.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If the sensor is not found, not running, or not connected, stop now
            if (null == this.Chooser.Sensor ||
                false == this.Chooser.Sensor.IsRunning ||
                KinectStatus.Connected != this.Chooser.Sensor.Status)
            {
                return;
            }

            Texture2D frameTexture;
            using (var frame = this.Chooser.Sensor.ColorStream.OpenNextFrame(0))
            {
                // Sometimes we get a null frame back if no data is ready
                if (frame == null)
                {
                    return;
                }


                // Reallocate values if necessary
                if (this.colorData == null || this.colorData.Length != frame.PixelDataLength)
                {
                    this.colorData = new byte[frame.PixelDataLength];

                    this.colorTexture = new Texture2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color);

                    this.backBuffer = new RenderTarget2D(
                        this.Game.GraphicsDevice, 
                        frame.Width, 
                        frame.Height, 
                        false, 
                        SurfaceFormat.Color, 
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount, 
                        RenderTargetUsage.PreserveContents);            
                }

                frame.CopyPixelDataTo(this.colorData);
                updateByte = !updateByte;
                this.needToRedrawBackBuffer = true;

                frameTexture = new Texture2D(
                        this.Game.GraphicsDevice,
                        frame.Width,
                        frame.Height,
                        false,
                        SurfaceFormat.Color);
            }

            // Update the skeleton renderer
            this.skeletonStream.Update(gameTime);

            frameTexture.SetData<byte>(colorData);
            FrameBuffer.AddFrame(new Frame(frameTexture, SkeletonStreamRenderer.SkeletonData, 
                (float)gameTime.TotalGameTime.TotalSeconds));
        }

        /// <summary>
        /// This method renders the color and skeleton frame.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Draw(GameTime gameTime)
        {
            // If we don't have the effect load, load it
            if (null == this.kinectColorVisualizer)
            {
                this.LoadContent();
            }

            // If we don't have a target, don't try to render
            if (null == this.colorTexture)
            {
                return;
            }

            if (this.needToRedrawBackBuffer)
            {
                // Set the backbuffer and clear
                this.Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                this.Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                this.colorTexture.SetData<byte>(this.colorData);

                // Draw the color image
                this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, this.kinectColorVisualizer);
                this.SharedSpriteBatch.Draw(this.colorTexture, new Vector2(0, 0), Color.White);
                this.SharedSpriteBatch.End();

                // Draw the skeleton
                if (DisplayingSkeleton)
                    this.skeletonStream.Draw(gameTime);

                // Reset the render target and prepare to draw scaled image
                this.Game.GraphicsDevice.SetRenderTargets(null);

                // No need to re-render the back buffer until we get new data
                this.needToRedrawBackBuffer = false;
            }

            // Draw the scaled texture
            this.SharedSpriteBatch.Begin();
            this.SharedSpriteBatch.Draw(
                this.backBuffer,
                drawRectangle,
                null,
                Color.White);
            this.SharedSpriteBatch.End();
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// This method loads the Xna effect.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // This effect is necessary to remap the BGRX byte data we get
            // to the XNA color RGBA format.
            this.kinectColorVisualizer = Game.Content.Load<Effect>("KinectColorVisualizer");
        }

        /// <summary>
        /// This method is used to map the SkeletonPoint to the color frame.
        /// </summary>
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the color frame.</returns>
        private Vector2 SkeletonToColorMap(SkeletonPoint point)
        {
            if ((null != Chooser.Sensor) && (null != Chooser.Sensor.ColorStream))
            {
                // This is used to map a skeleton point to the color image location
                var colorPt = Chooser.Sensor.CoordinateMapper.MapSkeletonPointToColorPoint(point, Chooser.Sensor.ColorStream.Format);
                return new Vector2(colorPt.X, colorPt.Y);
            }

            return Vector2.Zero;
        }
    }
}
