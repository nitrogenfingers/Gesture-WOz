//------------------------------------------------------------------------------
// <copyright file="XnaBasicsGame.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.XnaBasics
{
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// The main Xna game implementation.
    /// </summary>
    public class XnaBasics : Microsoft.Xna.Framework.Game
    {
        //The title of this trial
        public const string TrialLabel = "V01";

        /// <summary>
        /// This is used to adjust the window size.
        /// </summary>
        private const int Width = 1200;

        /// <summary>
        /// The graphics device manager provided by Xna.
        /// </summary>
        private readonly GraphicsDeviceManager graphics;
        
        /// <summary>
        /// This control selects a sensor, and displays a notice if one is
        /// not connected.
        /// </summary>
        private readonly KinectChooser chooser;

        /// <summary>
        /// This manages the rendering of the color stream.
        /// </summary>
        public readonly ColorStreamRenderer colorStream;

        /// <summary>
        /// The output display for the on-screen visualization the user believes they are manipulating.
        /// </summary>
        public readonly UserDisplayWindow displayWindow;
        UserDisplay udisplay;

        /// <summary>
        /// This is the location of the color stream when minimized.
        /// </summary>
        private readonly Vector2 colorSmallPosition;

        /// <summary>
        /// This is the viewport of the streams.
        /// </summary>
        private readonly Rectangle viewPortRectangle;

        /// <summary>
        /// This is the SpriteBatch used for rendering the header/footer.
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// This tracks the previous keyboard state.
        /// </summary>
        private KeyboardState previousKeyboard;

        /// <summary>
        /// This is the texture for the header.
        /// </summary>
        private Texture2D header;

        /// <summary>
        /// This is the font for the footer.
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// This is a single white pixel for blanket drawing.
        /// </summary>
        public static Texture2D pixel;

        private string[] labelList = { "SUMMON KEY", "MOVE LEFT", "MOVE RIGHT", "MOVE UP", "MOVE DOWN", "TWIST KEY", "TURN KEY", "OPEN DOOR"};
        private List<UserDisplay.StateManipDel> delList = new List<UserDisplay.StateManipDel>();

        /// <summary>
        /// Initializes a new instance of the XnaBasics class.
        /// </summary>
        public XnaBasics()
        {
            this.IsFixedTimeStep = false;
            this.IsMouseVisible = true;
            this.Window.Title = "Wizard Interface";

            // This sets the width to the desired width
            // It also forces a 4:3 ratio for height
            // Adds 110 for header/footer
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = Width;
            this.graphics.PreferredBackBufferHeight = 710;
            this.graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            this.graphics.SynchronizeWithVerticalRetrace = true;
            this.viewPortRectangle = new Rectangle(10, 80, Width - 20, ((Width - 2) / 4) * 3);

            Content.RootDirectory = "Content";

            // The Kinect sensor will use 640x480 for both streams
            // To make your app handle multiple Kinects and other scenarios,
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            this.chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            this.Services.AddService(typeof(KinectChooser), this.chooser);

            // Default size is the full viewport
            this.colorStream = new ColorStreamRenderer(this);

            displayWindow = new UserDisplayWindow(1920, 1050);
            displayWindow.Visible = true;

            // Store the values so we can animate them later
            this.colorSmallPosition = new Vector2(15, 85);

            this.Components.Add(this.chooser);

            this.previousKeyboard = Keyboard.GetState();

        }

        /// <summary>
        /// Loads the Xna related content.
        /// </summary>
        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.Services.AddService(typeof(SpriteBatch), this.spriteBatch);

            this.header = Content.Load<Texture2D>("Header");
            this.font = Content.Load<SpriteFont>("Segoe16");

            pixel = Content.Load<Texture2D>("pixel");

            udisplay = new UserDisplay(this, spriteBatch);

            delList.Add(udisplay.SummonKey);
            delList.Add(udisplay.MoveLeft);
            delList.Add(udisplay.MoveRight);
            delList.Add(udisplay.MoveUp);
            delList.Add(udisplay.MoveDown);
            delList.Add(udisplay.TwistKey);
            delList.Add(udisplay.TurnKey);
            delList.Add(udisplay.OpenDoor);
            udisplay.Initialize();

            base.LoadContent();
        }

        /// <summary>
        /// Initializes class and components
        /// </summary>
        protected override void Initialize()
        {
            this.Components.Add(this.colorStream);

            base.Initialize();
            System.Random random = new System.Random();
            this.Components.Add(new Button(new Rectangle(colorStream.drawRectangle.Left, colorStream.drawRectangle.Bottom + 20,
                colorStream.drawRectangle.Width, 60), "Undo", udisplay.Revert, spriteBatch, this));

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (i * 3 + j >= delList.Count) break;
                    /*if (delList.Count == 0) break;
                    UserDisplay.StateManipDel selDel = delList[ (int)(delList.Count * random.NextDouble())];
                    delList.Remove(selDel);*/
                    ScreenDisplay tsd = new ScreenDisplay
                        (new Vector2(colorStream.drawRectangle.Right + 10 + 240 * j + 5 * (j + 1),
                            200 * i + 5 * (i + 1)),
                        this,
                        spriteBatch,
                        labelList[i*3 + j],
                        /*selDel*/
                        delList[i*3 + j]);
                    this.Components.Add(tsd);
                }
        }

        /// <summary>
        /// This method updates the game state. Including monitoring
        /// keyboard state and the transitions.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If the spacebar has been pressed, toggle the focus
            KeyboardState newState = Keyboard.GetState();
            udisplay.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            foreach (DrawableGameComponent dgc in Components)
            {
                if (dgc is ScreenDisplay)
                {
                    ScreenDisplay sd = (ScreenDisplay)(dgc);
                    Console.Out.WriteLine("Writing " + sd.label);
                    sd.WriteDataToFile();
                }
            }
            
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// This method renders the current state.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.graphics.PreferredBackBufferWidth = Width + 100;
            this.graphics.PreferredBackBufferHeight = 810;
            GraphicsDevice.Clear(Color.Black);
            //this.spriteBatch.Begin();
            //this.spriteBatch.DrawString(this.font, "Display Window", new Vector2(0, 0), Color.White);
            //this.spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            udisplay.Draw(gameTime);
            this.colorStream.Draw(gameTime);

            //Moving back to the main canvas
            this.GraphicsDevice.Present(null, null, displayWindow.Canvas);

            this.graphics.PreferredBackBufferWidth = 1920;
            this.graphics.PreferredBackBufferHeight = 1050;

            // Clear the screen
            GraphicsDevice.Clear(Color.SlateGray);

            base.Draw(gameTime);

            this.spriteBatch.Begin();
            Vector2 strlen = this.font.MeasureString("LIVE FEED");
            this.spriteBatch.DrawString(this.font, "LIVE FEED", new Vector2(colorStream.drawRectangle.Center.X - strlen.X / 2,
                colorStream.drawRectangle.Bottom - strlen.Y), Color.Red);
            this.spriteBatch.DrawString(this.font, "Buffer: " + FrameBuffer.getSize(), new Vector2(20, 500), Color.White);
            this.spriteBatch.End();

        }

        /// <summary>
        /// This method ensures that we can render to the back buffer without
        /// losing the data we already had in our previous back buffer.  This
        /// is necessary for the SkeletonStreamRenderer.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="e">The event args.</param>
        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
