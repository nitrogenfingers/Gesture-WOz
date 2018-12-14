using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    enum KeyBehaviour
    {
        HIDING, APPEARING, DISAPPEARING, RESPAWNING, MOVING, TURNINGIN, TURNINGOUT, NONE
    }

    class Key : GEntity
    {
        //Debuggy stuff
        RasterizerState wireframe;
        Model spheremodel;
        BoundingSphere sphere;

        KeyBehaviour currentBehaviour = KeyBehaviour.HIDING;
        Vector3 desiredPosition;
        Vector3 desiredRotation = Vector3.Zero;

        public Key(Game game, Vector3 position, Camera camera) :
            base(game, position, camera, "models/tavern/key", "textures/brass_dark")
        {
            this.sphere = model.Meshes[0].BoundingSphere;
            spheremodel = game.Content.Load<Model>("models/fireflies");

            Texture2D baseTexture = game.Content.Load<Texture2D>("textures/brass_dark");
            foreach (ModelMesh mesh in model.Meshes) {
                foreach (BasicEffect effect in mesh.Effects) {
                    effect.TextureEnabled = true;
                    effect.Texture = baseTexture;
                }
            }

            wireframe = new RasterizerState();
            wireframe.FillMode = FillMode.WireFrame;
            desiredPosition = position;
            //Keeps the key upright
            desiredRotation.X = -MathHelper.PiOver2;
            desiredRotation.Y = 0;
        }

        public void Spawn(Vector3 newposition)
        {
            if (currentBehaviour == KeyBehaviour.NONE || currentBehaviour == KeyBehaviour.HIDING)
            {
                currentBehaviour = KeyBehaviour.RESPAWNING;
                desiredPosition = newposition;
            }
        }

        public String Push(Vector3 lockPosition)
        {
            if (!(desiredRotation.Y < MathHelper.PiOver2 + 0.1f && desiredRotation.Y > MathHelper.PiOver2 - 0.1f)) 
                return "Key is not rotated properly!";
            if (Math.Abs((position.X - lockPosition.X) + (position.Y - lockPosition.Y)) < 5)
            {
                desiredPosition.Z -= 10;
                return "";
            }
            else return "Key is not next to the lock!";
        }

        public void Move(Vector3 direction)
        {
            if (currentBehaviour == KeyBehaviour.NONE || currentBehaviour == KeyBehaviour.MOVING)
            {
                desiredPosition += direction;
            }
        }

        public void Hide()
        {
            currentBehaviour = KeyBehaviour.DISAPPEARING;
            desiredRotation.X = -MathHelper.PiOver2;
        }

        public void TurnKey()
        {
            desiredRotation.X = 0;
            currentBehaviour = KeyBehaviour.TURNINGIN;
        }

        public void TwistKey(float direction)
        {
            desiredRotation.Y += direction;
        }

        public void Disappear(){
            currentBehaviour = KeyBehaviour.DISAPPEARING;
        }

        public bool Intersects(Ray ray){
            return ray.Intersects(sphere) != null;
        }
        public bool Intersects(BoundingSphere oSphere){
            return sphere.Intersects(oSphere);
        }
        public bool Contains(Vector3 point)
        {
            return sphere.Contains(point) != ContainmentType.Disjoint;
        }

        public override void Update(GameTime gameTime)
        {
            switch (currentBehaviour)
            {
                case KeyBehaviour.HIDING:
                    this.transparency = 0;
                    break;
                case KeyBehaviour.RESPAWNING:
                    if (this.transparency == 0)
                    {
                        position = desiredPosition;
                        xrotation = desiredRotation.X;
                        yrotation = desiredRotation.Y;
                        zrotation = desiredRotation.Z;
                        currentBehaviour = KeyBehaviour.APPEARING;
                    }
                    else this.transparency = MathHelper.Clamp(this.transparency - (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 1);
                    break;
                case KeyBehaviour.APPEARING:
                    if (this.transparency == 1)
                        currentBehaviour = KeyBehaviour.NONE;
                    else this.transparency = MathHelper.Clamp(this.transparency + (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 1);
                    break;
                case KeyBehaviour.DISAPPEARING:
                    if (this.transparency == 0)
                        currentBehaviour = KeyBehaviour.HIDING;
                    else this.transparency = MathHelper.Clamp(this.transparency - (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 1);
                    break;
                case KeyBehaviour.TURNINGIN:
                    xrotation = MathHelper.Lerp(xrotation, desiredRotation.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    if (Math.Abs(xrotation - desiredRotation.X) < 0.01f)
                    {
                        desiredRotation.X = -MathHelper.PiOver2;
                        currentBehaviour = KeyBehaviour.NONE;
                    }
                    break;
                default:
                    position.X = MathHelper.Lerp(position.X, desiredPosition.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    position.Y = MathHelper.Lerp(position.Y, desiredPosition.Y, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    position.Z = MathHelper.Lerp(position.Z, desiredPosition.Z, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    xrotation = MathHelper.Lerp(xrotation, desiredRotation.X, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    yrotation = MathHelper.Lerp(yrotation, desiredRotation.Y, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    zrotation = MathHelper.Lerp(zrotation, desiredRotation.Z, (float)gameTime.ElapsedGameTime.TotalSeconds * 5);
                    break;
            }
            /*sphere.Center = position;
            if (selected && yrotation > -MathHelper.PiOver2)
            {
                yrotation -= (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;
            }*/
        }

        public override void Draw(GameTime gameTime)
        {
            
            #region Debuggy Stuff
            /*
            RasterizerState temp = game.GraphicsDevice.RasterizerState;
            game.GraphicsDevice.RasterizerState = wireframe;

            Matrix[] baseTransform = new Matrix[spheremodel.Bones.Count];
            spheremodel.CopyAbsoluteBoneTransformsTo(baseTransform);

            foreach (ModelMesh mesh in spheremodel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Center);
                }
                mesh.Draw();
            }

            game.GraphicsDevice.RasterizerState = temp;
             */
            #endregion

            base.Draw(gameTime);
        }

        public override void DrawFlatRender(GameTime gameTime, Color color)
        {
            if (selected)
            {
                Matrix[] baseTransform = new Matrix[spheremodel.Bones.Count];
                spheremodel.CopyAbsoluteBoneTransformsTo(baseTransform);

                foreach (ModelMesh mesh in spheremodel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = camera.GetViewMatrix();
                        effect.Projection = camera.GetProjectionMatrix();
                        effect.World = Matrix.CreateScale(sphere.Radius) * Matrix.CreateTranslation(sphere.Center);

                        effect.DiffuseColor = Color.White.ToVector3();
                        effect.AmbientLightColor = Color.White.ToVector3();
                    }
                    mesh.Draw();
                }
            }
            else
            {
                base.DrawFlatRender(gameTime, color);
            }
        }
    }
}
