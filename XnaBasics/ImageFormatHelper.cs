using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    class ImageFormatHelper
    {
        //The last frame processed
        private static byte[] lastbytearray = null;
        public static byte[] LastByteArray { get { return lastbytearray; } }
        //A value we flip each time the image is updated
        private static bool updateByte = false;
        public static bool UpdateByte { get { return updateByte; } }

        public static Texture2D ColorFrameToTexture(ColorImageFrame colorFrame, ref Texture2D output, GraphicsDevice device)
        {
            byte[] bgraImageData;
            if (colorFrame != null)
            {
                bgraImageData = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(bgraImageData);
                for (int i = 0; i < bgraImageData.Length; i += 4)
                    /*{
                        byte cup = bgraImageData[i];
                        bgraImageData[i] = bgraImageData[i + 2];
                        bgraImageData[i + 2] = cup;
                        bgraImageData[i + 3] = 255;
                    }*/
                    device.Textures[0] = null;
                output.SetData<byte>(bgraImageData);
                lastbytearray = bgraImageData;
                updateByte = !updateByte;
                return output;
            }
            else return null;
        }
    }
}
