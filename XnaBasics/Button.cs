using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    class Button : DrawableGameComponent
    {
        private MouseState lms;
        private UserDisplay.StateManipDel action;
        private Rectangle rect;
        private SpriteBatch spritebatch;
        private String label;

        private SpriteFont font;

        private const int border = 3;

        public Button(Rectangle rect, String label, UserDisplay.StateManipDel action, SpriteBatch batch, Game game): base(game)
        {
            this.rect = rect;
            this.action = action;
            this.label = label;
            this.spritebatch = batch;

            LoadContent();
        }

        protected override void LoadContent()
        {
            font = Game.Content.Load<SpriteFont>("Segoe16");
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();

            if (ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released && rect.Contains(ms.X, ms.Y))
                action();

            base.Update(gameTime);
            lms = ms;
        }

        public override void Draw(GameTime gameTime)
        {
            spritebatch.Begin();
            spritebatch.Draw(XnaBasics.pixel, rect, Color.DarkRed);
            spritebatch.Draw(XnaBasics.pixel, new Rectangle(rect.X - border, rect.Y - border, 
                rect.Width + border*2, rect.Height + border*2), Color.Red);
            Vector2 labelSize = font.MeasureString(label);
            spritebatch.DrawString(font, label, new Vector2(rect.X + rect.Width/2 - labelSize.X/2, rect.Y + rect.Height/2 - labelSize.Y/2),
                Color.White);
            spritebatch.End();
            
            base.Draw(gameTime);
        }
    }
}
