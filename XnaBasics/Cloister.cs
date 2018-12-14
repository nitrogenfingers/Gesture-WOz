using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    class Cloister:DrawableGameComponent
    {
        private Model model;
        private Vector3 position;
        private Camera camera;
        private float camRotation = 0;
        private int doorToOpen = 0;
        private float rate = 0.5f;
        private List<Door> doorList = new List<Door>();
        KeyboardState lks;
        private List<Vector3> keyspawnpositions = new List<Vector3>();
        private List<Vector3> movepositions = new List<Vector3>();
        bool cameraMoving = false;

        private Dictionary<string, string> texID = new Dictionary<string, string>();

        public Cloister(Game game, Camera camera, Vector3 position)
            : base(game)
        {
            texID.Add("Straight_Hall", "Castle_Wall_Tiles_X");
            texID.Add("Cap_Wall", "Castle_Wall_Tiles_X");
            texID.Add("Window_Wall", "Castle_Wall_Tiles_X");
            texID.Add("4_Way_Vault", "Castle_Wall_Tiles_X");
            texID.Add("8_Way_Vault", "4_bay_diffuse");
            texID.Add("Room_Fireplace", "4_bay_diffuse");
            texID.Add("Hall_Arch", "Arch");
            texID.Add("Solid_Arch", "Arch");
            texID.Add("Capped_Arch", "Arch");
            texID.Add("Window__Arch", "Window_Arch");
            texID.Add("Window_Seat", "Stone_Tiles");
            texID.Add("Window_Dark", "Window_Dark");
            texID.Add("Stair_Hall", "ashlar-wall");
            texID.Add("Stairs", "ashlar-wall");
            texID.Add("Fireplace", "Fireplace_Diffuse");
            texID.Add("Ceiling__Joists", "old_wood_dark");
            texID.Add("Bracket", "Bracket_Diffuse");
            texID.Add("Plank_Ceiling", "Plank_floor");
            texID.Add("Left_Door", "2_Doors_Diffuse");
            texID.Add("Right_Door", "2_Doors_Diffuse");
            texID.Add("Ring_Pull", "Ring_Pull_Diffuse");
            texID.Add("Floor_Upstairs", "floor_pavers");
            texID.Add("Torch", "pitted_metal_dark256");

            this.position = position;
            this.camera = camera;
            model = game.Content.Load<Model>("models/Halls_TexTest");

            Console.Out.WriteLine(model.Meshes.Count);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    string name = mesh.Name;
                    int divider = name.LastIndexOf('_');
                    name = name.Substring(0, divider);

                    if (texID.ContainsKey(name)) effect.Texture = game.Content.Load<Texture2D>("textures/cloister/" + texID[name]);
                    else effect.Texture = game.Content.Load<Texture2D>("textures/cloister/DEFtexture");
                }
            }

            Door door;
            Random random = new Random();
            door = new Door(Game, camera, new Vector3(-374, 101.5f, 301.5f), MathHelper.Pi);
            door.scale = 10;
            door.setLockPosition(new Vector3((random.Next(2)-1) * 20, (random.Next(3)+1) * 20, 0));
            door.setLockRotation(MathHelper.Pi);
            doorList.Add(door);
            keyspawnpositions.Add(door.position - Vector3.Forward * 10 + Vector3.Up * 40);
            movepositions.Add(door.position - Vector3.Forward * 45 + Vector3.Up * 100);

            door = new Door(Game, camera, new Vector3(-374, 101.5f, 195), MathHelper.Pi);
            door.scale = 10;
            door.setLockPosition(new Vector3((random.Next(2) - 1) * 20, (random.Next(3) + 1) * 20, 0));
            door.setLockRotation(MathHelper.Pi);
            doorList.Add(door);
            keyspawnpositions.Add(door.position - Vector3.Forward * 10 + Vector3.Up * 40);
            movepositions.Add(door.position - Vector3.Forward * 45 + Vector3.Up * 100);

            door = new Door(Game, camera, new Vector3(-374, 101.5f, 88.5f), MathHelper.Pi);
            door.scale = 10;
            door.setLockPosition(new Vector3((random.Next(2) - 1) * 20, (random.Next(3) + 1) * 20, 0));
            door.setLockRotation(MathHelper.Pi);
            doorList.Add(door);
            keyspawnpositions.Add(door.position - Vector3.Forward * 10 + Vector3.Up * 40);
            movepositions.Add(door.position - Vector3.Forward * 45 + Vector3.Up * 100);

            door = new Door(Game, camera, new Vector3(-374, 101.5f, -120), MathHelper.Pi);
            door.scale = 10;
            door.setLockPosition(new Vector3((random.Next(2) - 1) * 20, (random.Next(3) + 1) * 20, 0));
            door.setLockRotation(MathHelper.Pi);
            doorList.Add(door);
            keyspawnpositions.Add(door.position - Vector3.Forward * 10 + Vector3.Up * 40);
            movepositions.Add(door.position - Vector3.Forward * 45 + Vector3.Up * 100);

            door = new Door(Game, camera, new Vector3(-374, 101.5f, -327), MathHelper.Pi);
            door.scale = 10;
            door.setLockPosition(new Vector3((random.Next(2) - 1) * 20, (random.Next(3) + 1) * 20, 0));
            door.setLockRotation(MathHelper.Pi);
            doorList.Add(door);
            keyspawnpositions.Add(door.position - Vector3.Forward * 10 + Vector3.Up * 40);
            movepositions.Add(door.position - Vector3.Forward * 45 + Vector3.Up * 100);
        }

        public Vector3 getSpawnPosition()
        {
            return keyspawnpositions[doorToOpen];
        }

        public Vector3 getLockPosition()
        {
            return doorList[doorToOpen].getLockPosition();
        }

        public bool unlockDoor(float x, float y)
        {
            return doorList[doorToOpen].tryUnlock(x, y);
        }

        public bool openDoor()
        {
            if (doorList[doorToOpen].Open())
            {
                doorToOpen++;
                //if (doorToOpen < doorList.Count) cameraMoving = true;
                return true;
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Up))
                camera.Position += camera.Target * rate;
            else if (ks.IsKeyDown(Keys.Down))
                camera.Position -= camera.Target * rate;
            /*if (ks.IsKeyDown (Keys.Left))
                camRotation -= rate/10f;
            else if (ks.IsKeyDown (Keys.Right))
                camRotation += rate/10f;*/
            if (ks.IsKeyDown(Keys.Q))
                camera.Position += new Vector3(0, rate, 0);
            else if (ks.IsKeyDown(Keys.A))
                camera.Position -= new Vector3(0, rate, 0);
            if (ks.IsKeyDown(Keys.OemMinus))
                rate += 0.05f;
            else if (ks.IsKeyDown(Keys.OemPlus))
                rate -= 0.05f;

            /*if (ks.IsKeyDown(Keys.O) && !lks.IsKeyDown(Keys.O))
            {
                doorList[doorToOpen++].Open();
            }*/

            if (cameraMoving)
            {
                Vector3 oldCamPos = camera.Position;
                bool atX = false, atY = false, atZ = false;
                if (Math.Abs(oldCamPos.X - movepositions[doorToOpen].X) < 0.5f)
                {
                    oldCamPos.X = movepositions[doorToOpen].X;
                    atX = true;
                }
                else oldCamPos.X += 0.5f * Math.Sign(movepositions[doorToOpen].X - oldCamPos.X);

                if (Math.Abs(oldCamPos.Y - movepositions[doorToOpen].Y) < 0.5f)
                {
                    oldCamPos.Y = movepositions[doorToOpen].Y;
                    atY = true;
                }
                else oldCamPos.Y += 0.5f * Math.Sign(movepositions[doorToOpen].Y - oldCamPos.Y);

                if (Math.Abs(oldCamPos.Z - movepositions[doorToOpen].Z) < 0.5f)
                {
                    oldCamPos.Z = movepositions[doorToOpen].Z;
                    atZ = true;
                }
                else oldCamPos.Z += 0.5f * Math.Sign(movepositions[doorToOpen].Z - oldCamPos.Z);

                if (atX && atY && atZ) cameraMoving = false;
            }

            setCamTarget();
            foreach (Door d in doorList) 
                d.Update(gameTime);
            base.Update(gameTime);

            lks = ks;
        }

        private void setCamTarget()
        {
            camera.Target = new Vector3(
                (float)Math.Sin(camRotation),
                0,
                -(float)Math.Cos(camRotation)
            );
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            RasterizerState ors = Game.GraphicsDevice.RasterizerState;
            RasterizerState crs = RasterizerState.CullNone;
            Game.GraphicsDevice.RasterizerState = crs;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(10f);
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

            foreach (Door d in doorList) d.Draw(gameTime);
            base.Draw(gameTime);
            Game.GraphicsDevice.RasterizerState = ors;
        }

        public void DrawFlatRender(GameTime gameTime, Color color)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = false;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(10f);

                    effect.DiffuseColor = color.ToVector3();
                    effect.AmbientLightColor = color.ToVector3();
                    effect.SpecularPower = 0;
                }
                mesh.Draw();
            }
        }
    }
}
