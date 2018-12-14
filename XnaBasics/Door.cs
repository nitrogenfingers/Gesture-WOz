using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    public class Door : Entity, IFlatDrawable
    {
        public enum Opening { NONE, OFORWARD, OBACK, CLOSING }

        public float initRotation = 0;
        private Opening opening;
        Model left, right;
        GEntity doorLock;
        bool locked = true;

        public Door(Game game, Camera camera, Vector3 position, float initRotation)
            :base(game, position, camera, null)
        {
            left = game.Content.Load<Model>("models/leftdoor");
            right = game.Content.Load<Model>("models/rightdoor");
            doorLock = new GEntity(game, position, camera, "models/tavern/lock", "textures/brass_dark");
            doorLock.scale = 5;

            foreach (ModelMesh mesh in left.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Texture = game.Content.Load<Texture2D>("textures/cloister/2_Doors_Diffuse");
                }
            }
            foreach (ModelMesh mesh in right.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Texture = game.Content.Load<Texture2D>("textures/cloister/2_Doors_Diffuse");
                }
            }

            this.initRotation = initRotation;
        }

        public void setLockRotation(float rotation)
        {
            doorLock.yrotation = rotation;
        }

        public void setLockPosition(Vector3 lockrel)
        {
            doorLock.position = position + lockrel;
        }

        public Vector3 getLockPosition()
        {
            return doorLock.position;
        }

        public bool Open()
        {
            if (locked) return false;
            opening = Opening.OBACK;
            doorLock.transparency = 0;
            return true;
        }

        public void Close()
        {
            opening = Opening.CLOSING;
        }

        public bool tryUnlock(float x, float y)
        {
            if (Math.Abs((x - doorLock.position.X) + (y - doorLock.position.Y)) < 5)
                locked = false;
            return !locked;
        }

        public override void Update(GameTime gameTime)
        {
            switch (opening)
            {
                case Opening.OFORWARD:
                    if (yrotation < (MathHelper.Pi * 3f) / 8f)
                        yrotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else
                        opening = Opening.NONE;
                    break;
                case Opening.OBACK:
                    if (yrotation > -(MathHelper.Pi * 3f) / 8f)
                        yrotation -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else
                        opening = Opening.NONE;
                    break;
                case Opening.CLOSING:
                    if (yrotation != 0)
                        yrotation += -Math.Sign(yrotation) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Vector3 offset = new Vector3(2.427f, 0, 0);

            Matrix[] bones = new Matrix[left.Bones.Count];
            left.CopyAbsoluteBoneTransformsTo(bones);

            foreach (ModelMesh mesh in left.Meshes)
            {
                Matrix world = bones[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.Pi + yrotation);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = world * Matrix.CreateTranslation(offset*scale) * Matrix.CreateRotationY(initRotation) * Matrix.CreateTranslation(position);

                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.DiffuseColor = Color.Gray.ToVector3();
                    effect.SpecularPower = 0;
                }
                mesh.Draw();
            }

            bones = new Matrix[right.Bones.Count];
            right.CopyAbsoluteBoneTransformsTo(bones);

            foreach (ModelMesh mesh in right.Meshes)
            {
                Matrix world = bones[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.Pi + -yrotation);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = true;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = world * Matrix.CreateTranslation(-offset*scale) * Matrix.CreateRotationY(initRotation) * Matrix.CreateTranslation(position);

                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.DiffuseColor = Color.Gray.ToVector3();
                    effect.SpecularPower = 0;
                }
                mesh.Draw();
            }

            if (doorLock.transparency != 0) doorLock.Draw(gameTime);
        }

        public override void DrawFlatRender(GameTime gameTime, Color color)
        {
            Vector3 offset = new Vector3(2.427f, 0, 0);

            Matrix[] bones = new Matrix[left.Bones.Count];
            left.CopyAbsoluteBoneTransformsTo(bones);

            foreach (ModelMesh mesh in left.Meshes)
            {
                Matrix world = bones[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.Pi + yrotation);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = false;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = world * Matrix.CreateTranslation(offset * scale) *
                        Matrix.CreateRotationY(initRotation) * Matrix.CreateTranslation(position);

                    effect.AmbientLightColor = color.ToVector3();
                    effect.DiffuseColor = color.ToVector3();
                    effect.SpecularPower = 0;
                    effect.DirectionalLight0.Enabled = false;
                    effect.DirectionalLight1.Enabled = false;
                    effect.DirectionalLight2.Enabled = false;
                }
                mesh.Draw();
            }

            bones = new Matrix[right.Bones.Count];
            right.CopyAbsoluteBoneTransformsTo(bones);

            foreach (ModelMesh mesh in right.Meshes)
            {
                Matrix world = bones[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.Pi - yrotation);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.TextureEnabled = false;
                    effect.View = camera.GetViewMatrix();
                    effect.Projection = camera.GetProjectionMatrix();
                    effect.World = world * Matrix.CreateTranslation(-offset * scale) * Matrix.CreateRotationY(initRotation) * Matrix.CreateTranslation(position);

                    effect.AmbientLightColor = color.ToVector3();
                    effect.DiffuseColor = color.ToVector3();
                    effect.SpecularPower = 0;
                    effect.DirectionalLight0.Enabled = false;
                    effect.DirectionalLight1.Enabled = false;
                    effect.DirectionalLight2.Enabled = false;
                }
                mesh.Draw();
            }
        }
    }
}
