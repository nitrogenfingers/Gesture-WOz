using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    class ScreenDisplay : DrawableGameComponent
    {
        //The width and height of the image. Constant.
        private const int DISPWIDTH = 240;
        private const int DISPHEIGHT = 180;

        //The location of the display on the screen
        private Rectangle screenRect;
        public Rectangle ScreenRect { get { return screenRect; } set { screenRect = value; } }
        //The "record" and "perform" buttons.
        private Rectangle recordRect;

        //Whether or not the display is currently recording frames
        private bool recording = false;
        public bool Recording { get { return recording; } set { recording = value; } }

        //The currently displaying frame (Assuming there's an image to display)
        private float cframe = 0;

        //The list of all recorded frames
        private List<Frame> frames = new List<Frame>();
        //The texture. We set the texture to the appropriate byte when necessary.
        private RenderTarget2D backBuffer;
        //Whether or not the texture needs to be updated
        private bool pushUpdate = false;
        //Whether or not the texture needs to be redrawn
        private bool needToRedrawBackBuffer = true;

        //The sprite batch
        private SpriteBatch spriteBatch;
        //The effect used to draw frames
        private Effect kinectColorVisualizer;
        //The last mouse state
        private MouseState lms;
        //The font used to write text
        private SpriteFont segoe16;
        //The name of the action
        public String label;
        //The action taken when the display is invoked
        private UserDisplay.StateManipDel invocation;
        //Whether or not this gesture has been flagged to be defined
        private bool todefine;
        //Whether or not this gesture has been defined yet
        public bool HasBeenDefined
        {
            get { return frames.Count > 0; }
        }

        public ScreenDisplay(Vector2 position, XnaBasics game, SpriteBatch spriteBatch, String label, UserDisplay.StateManipDel onInvoke) : base(game)
        {
            this.screenRect = new Rectangle((int)position.X, (int)position.Y, DISPWIDTH, DISPHEIGHT);
            this.recordRect = new Rectangle(screenRect.Left, screenRect.Bottom, screenRect.Width, 20);
            this.spriteBatch = spriteBatch;
            this.invocation = onInvoke;
            this.label = label;
        }

        public override void Initialize()
        {
            this.backBuffer = new RenderTarget2D(
                        this.Game.GraphicsDevice,
                        640, 480,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        this.Game.GraphicsDevice.PresentationParameters.MultiSampleCount,
                        RenderTargetUsage.PreserveContents); 

            base.Initialize();
        }

        /*
        protected void AddFrame(byte[] newFrame)
        {
            Texture2D nt = new Texture2D(Game.GraphicsDevice, 640, 480);
            nt.SetData<byte>(newFrame);
            frames.Add(nt);
            cframe = (short)(frames.Count - 1);
        }*/

        public void RequestDefinition()
        {
            todefine = true;
            ClearFrames();
        }

        protected void ClearFrames()
        {
            frames.Clear();
        }

        public void checkMouseClick(Point mousePos)
        {
            if (recordRect.Contains(mousePos))
            {
                frames = FrameBuffer.copyBuffer();
                cframe = 0;
                todefine = false;
                /*if (!recording) ClearFrames();
                recording = !recording;
                cframe = 0;
                pushUpdate = ColorStreamRenderer.UpdateByte;*/
            } 
            else if (screenRect.Contains(mousePos))
            {
                invocation();
            }
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            kinectColorVisualizer = Game.Content.Load<Effect>("KinectColorVisualizer");
            segoe16 = Game.Content.Load<SpriteFont>("Segoe16");
        }

        public override void Update(GameTime gameTime)
        {
            //Animation and recording
            /*if (recording)
            {
                if (frames.Count == 0 || pushUpdate != ColorStreamRenderer.UpdateByte)
                {
                    AddFrame(((XnaBasics)this.Game).colorStream.LastFrame);
                    pushUpdate = ColorStreamRenderer.UpdateByte;
                    needToRedrawBackBuffer = true;
                }
            }
            else*/
            {
                float lcf = cframe;
                //A balance of smoothness and speed. Performance is an issue here.
                cframe += (float)gameTime.ElapsedGameTime.TotalSeconds*16;
                cframe %= frames.Count;
                if (Math.Floor(lcf) != Math.Floor(cframe)) 
                    needToRedrawBackBuffer = true;
            }
            //Capturing and acting on mouse clicks
            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released)
                checkMouseClick(new Point(ms.X, ms.Y));
            lms = ms;

            base.Update(gameTime);
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

            //Draw the footer buttons: Redefine and Action
            spriteBatch.Begin();
            spriteBatch.Draw(XnaBasics.pixel, recordRect, Color.Red);
            Vector2 measure = segoe16.MeasureString("RECORD");
            spriteBatch.DrawString(segoe16, "RECORD", new Vector2(screenRect.Left + screenRect.Width / 2 - measure.X / 2,
                screenRect.Bottom + 10 - measure.Y / 2), recording ? Color.White : Color.Black);

            // If we don't have a target, don't try to render
            if (frames.Count == 0 || float.IsNaN(cframe))
            {
                spriteBatch.Draw(XnaBasics.pixel, screenRect, Color.Black);
                if (todefine) spriteBatch.DrawString(segoe16, "DEFINE", new Vector2(screenRect.X + screenRect.Width / 2 -
                    segoe16.MeasureString(label).X / 2, screenRect.Y + 5), Color.Green);
                spriteBatch.DrawString(segoe16, label, new Vector2(screenRect.X + screenRect.Width / 2 -
                    segoe16.MeasureString(label).X / 2, screenRect.Y + 5), Color.White);
                spriteBatch.Draw(XnaBasics.pixel, new Rectangle(screenRect.Left, screenRect.Bottom - 8, 
                    screenRect.Width, 8), Color.DarkGray);
                spriteBatch.End();
                return;
            }
            spriteBatch.End();

            if (this.needToRedrawBackBuffer)
            {
                if (this.backBuffer == null) this.Initialize();

                // Set the backbuffer and clear
                this.Game.GraphicsDevice.SetRenderTarget(this.backBuffer);
                this.Game.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                //this.colorTexture.SetData<byte>(frames[cframe]);

                // Draw the color image
                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, this.kinectColorVisualizer);
                spriteBatch.Draw(frames[(int)cframe].colorFrame, new Vector2(0, 0), Color.White);
                spriteBatch.End();

                // Draw the skeleton
                //this.skeletonStream.Draw(gameTime);

                // Reset the render target and prepare to draw scaled image
                //this.Game.GraphicsDevice.SetRenderTargets(null);

                // No need to re-render the back buffer until we get new data
                this.needToRedrawBackBuffer = false;
            }

            // Draw the scaled texture
            this.Game.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin();
            spriteBatch.Draw(
                this.backBuffer,
                screenRect,
                null,
                Color.White);

            /*spriteBatch.DrawString(segoe16, label, new Vector2(screenRect.X + screenRect.Width/2 - 
                segoe16.MeasureString(label).X/2, screenRect.Y + 5), Color.White);
            */
            if (todefine) spriteBatch.DrawString(segoe16, "DEFINE", new Vector2(screenRect.X + screenRect.Width / 2 -
                segoe16.MeasureString(label).X / 2, screenRect.Y + 5), Color.Green);
            spriteBatch.Draw(XnaBasics.pixel, new Rectangle(screenRect.Left, screenRect.Bottom - 8,
                screenRect.Width, 8), Color.DarkGray);
            spriteBatch.Draw(XnaBasics.pixel, new Rectangle(screenRect.Left, screenRect.Bottom - 8,
                (int)(screenRect.Width * ((float)(cframe) / (float)(frames.Count))), 8), Color.Green);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void WriteDataToFile()
        {
            StreamWriter writer = new StreamWriter(XnaBasics.TrialLabel + " " + this.label);
            Array jointnames = Enum.GetValues(typeof(JointType));
            String s = "";
            foreach (JointType typ in jointnames) s += typ.ToString() + ",";
            writer.WriteLine(s);
            s = "";

            foreach (Frame f in frames)
            {
                if (f.skeletonDict.Count == 0)
                {
                    for (int i = 0; i < jointnames.Length; i++)
                        s += ",";
                }
                else
                {
                    foreach (JointType typ in jointnames)
                    {
                        ValueJoint vj = f.skeletonDict[typ];
                        s += "(" + vj.position.X + "|" + vj.position.Y + "|" + vj.position.X + ")";
                        switch (vj.jts)
                        {
                            case JointTrackingState.Tracked: s += "T,"; break;
                            case JointTrackingState.Inferred: s += "I,"; break;
                            default : s += "N,"; break;
                        }
                    }
                }
                writer.WriteLine(s);
                s = "";
            }
            writer.Close();
        }
    }
}
