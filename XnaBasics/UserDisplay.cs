using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    class UserDisplay : DrawableGameComponent
    {
        private Camera camera;
        private Key key;
        private Model model;

        private Vector3 modelTranslate = new Vector3(0, 0, 10);
        private Vector3 modelRotate = new Vector3(0, 0, 0);
        private float modelScale = 1;

        private Vector3 lastTranslate = new Vector3(0, 0, 10);
        private Vector3 lastRotate = new Vector3(0, 0, 0);
        private float lastScale = 1;

        private Vector3 desModelTranslate = new Vector3(0, 0, 10);
        private Vector3 desModelRotate = new Vector3(0, 0, 0);
        private float desModelScale = 1;

        private Cloister cloister;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private String errorMessage = "";
        private float errorTime = 0;

        public delegate void StateManipDel();
        public StateManipDel SummonKey, MoveLeft, MoveRight, MoveUp, MoveDown, TwistKey, TurnKey, OpenDoor, Revert;
        private StateManipDel lastDelCalled;

        public UserDisplay(XnaBasics game, SpriteBatch spriteBatch) : base(game) {
            this.spriteBatch = spriteBatch;
            SummonKey = spawnKey;
            MoveLeft = moveLeft;
            MoveRight = moveRight;
            MoveUp = moveUp;
            MoveDown = moveDown;
            TwistKey = twistKey;
            TurnKey = turnKey;
            OpenDoor = openDoor;
        }

        public override void Initialize()
        {
            camera = new Camera(Game, new Vector3(-375, 145, 400), Vector3.Forward, Vector3.Up);

            cloister = new Cloister(Game, camera, Vector3.Zero);

            base.Initialize();
        }

        public String getDelegateDescription(StateManipDel smd)
        {
            if (smd == SummonKey)
            {
                return "This summons the key in front of you, or resets it to its original position.";
            }
            else if (smd == MoveLeft)
            {
                return "This moves the key to the left.";
            }
            else if (smd == MoveRight)
            {
                return "This moves the key to the right.";
            }
            else if (smd == MoveUp)
            {
                return "This moves the key up.";
            }
            else if (smd == MoveDown)
            {
                return "This moves the key down.";
            }
            else if (smd == TurnKey)
            {
                return "This turns the key. If in a lock, this will open it.";
            }
            else if (smd == TwistKey)
            {
                return "This spins the key around, so you can face it towards a lock.";
            }
            else if (smd == OpenDoor)
            {
                return "This opens the door, once it has been unlocked.";
            }
            else return "";
        }

        protected override void LoadContent()
        {
            model = Game.Content.Load<Model>("cube");
            spriteFont = Game.Content.Load<SpriteFont>("Segoe16");
            key = new Key(Game, cloister.getSpawnPosition(), camera);
            key.scale = 2;
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            cloister.Update(gameTime);

            modelRotate.X = MathHelper.Lerp(modelRotate.X, desModelRotate.X, (float)gameTime.ElapsedGameTime.TotalSeconds*5);
            modelRotate.Y = MathHelper.Lerp(modelRotate.Y, desModelRotate.Y, (float)gameTime.ElapsedGameTime.TotalSeconds*5);
            modelTranslate.X = MathHelper.Lerp(modelTranslate.X, desModelTranslate.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
            modelTranslate.Y = MathHelper.Lerp(modelTranslate.Y, desModelTranslate.Y, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
            if (Math.Abs(modelRotate.X) >= MathHelper.TwoPi)
            {
                modelRotate.X %= MathHelper.TwoPi;
                desModelRotate.X %= MathHelper.TwoPi;
            }
            modelScale = MathHelper.Lerp(modelScale, desModelScale, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
            key.Update(gameTime);

            if (errorTime > 0)
            {
                errorTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (errorTime < 0) errorMessage = "";
            }
            
            base.Update(gameTime);
        }

        private void spawnKey()
        {
            key.Spawn(cloister.getSpawnPosition());
        }
        private void moveLeft()
        {
            key.Move(Vector3.Left * 20);
            lastDelCalled = MoveLeft;
        }
        private void moveRight()
        {
            key.Move(Vector3.Right * 20);
            lastDelCalled = MoveRight;
        }
        private void moveUp()
        {
            key.Move(Vector3.Up * 20);
            lastDelCalled = MoveUp;
        }
        private void moveDown()
        {
            key.Move(Vector3.Down * 20);
            lastDelCalled = MoveDown;
        }
        private void twistKey()
        {
            key.TwistKey(MathHelper.PiOver2);
            lastDelCalled = TwistKey;
        }
        private void turnKey()
        {
            key.TurnKey();
            if ((Math.Abs((key.yrotation%MathHelper.TwoPi) - MathHelper.PiOver2) < 0.1f) && cloister.unlockDoor(key.position.X, key.position.Y))
            {
                errorMessage = "Door unlocked!";
            }
            else
            {
                errorMessage = "Door was is still locked";
            }
            errorTime = 5;
            lastDelCalled = TurnKey;
        }
        private void openDoor()
        {
            if (cloister.openDoor())
            {
                key.Hide();
                key.TwistKey(-MathHelper.PiOver2);
                lastDelCalled = OpenDoor;
            }
            else
            {
                errorMessage = "Door is locked- cannot open!";
            }
            errorTime = 5;
        }

        private void rotateRight()
        {
            lastRotate = desModelRotate;
            desModelRotate.Y += MathHelper.Pi / 2;
            //lastDelCalled = RotateRight;
        }
        private void rotateDown()
        {
            lastRotate = desModelRotate;
            desModelRotate.X -= MathHelper.Pi / 2;
            //lastDelCalled = RotateDown;
        }
        private void rotateUp()
        {
            lastRotate = desModelRotate;
            desModelRotate.X += MathHelper.Pi / 2;
            //lastDelCalled = RotateUp;
        }
        private void translateSomewhere()
        {
            Random random = new Random();
            lastTranslate = desModelTranslate;
            desModelTranslate.X = random.Next(-5, 5);
            desModelTranslate.Y = random.Next(-5, 5);
            //lastDelCalled = TranslateSomewhere;
        }

        private void scaleRandom()
        {
            Random random = new Random();
            lastScale = desModelScale;
            desModelScale = (float)(random.NextDouble() * 1.5 + 0.5);
            //lastDelCalled = ScaleRandomly;
        }

        private void revert()
        {
            //if (lastDelCalled == ScaleRandomly) desModelScale = lastScale;
            //else if (lastDelCalled == TranslateSomewhere) desModelTranslate = lastTranslate;
            //else desModelRotate = lastRotate;
        }

        public override void Draw(GameTime gameTime)
        {
            if (model == null) this.LoadContent();

            /*foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = Matrix.CreateScale(modelScale) * Matrix.CreateRotationX(this.modelRotate.X) *
                        Matrix.CreateRotationY(this.modelRotate.Y) * Matrix.CreateTranslation(modelTranslate);
                }
                mesh.Draw();
            }*/
            cloister.Draw(gameTime);
            key.Draw(gameTime);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, "(" + camera.Position.X + "," + camera.Position.Y + "," + camera.Position.Z + ")",
                new Vector2(10, 600), Color.White);
            spriteBatch.DrawString(spriteFont, errorMessage, new Vector2(1920 / 2 - spriteFont.MeasureString(errorMessage).X / 2, 20),
                Color.Red);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
