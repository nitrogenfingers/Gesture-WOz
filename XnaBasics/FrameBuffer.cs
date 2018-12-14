using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    struct ValueJoint
    {
        public Vector3 position;
        public JointTrackingState jts;
        public ValueJoint(Vector3 position, JointTrackingState jts)
        {
            this.position = position;
            this.jts = jts;
        }
    }

    class Frame
    {
        public Texture2D colorFrame;
        public Dictionary<JointType, ValueJoint> skeletonDict;
        public float gameTimeSeconds;

        public Frame(Texture2D colorFrame, Skeleton[] skeletonFrame, float gameTimeSeconds)
        {
            this.colorFrame = colorFrame;
            this.skeletonDict = new Dictionary<JointType, ValueJoint>();

            Skeleton closeSkeleton = null;
            foreach (Skeleton skeleton in skeletonFrame)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if (closeSkeleton == null)
                        closeSkeleton = skeleton;
                    else if (closeSkeleton != null && ((skeleton.Position.X + skeleton.Position.Y + skeleton.Position.Z) < (closeSkeleton.Position.X +
                            closeSkeleton.Position.Y + closeSkeleton.Position.Z)))
                        closeSkeleton = skeleton;
                }
            }

            if (closeSkeleton != null && closeSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                foreach (Joint j in closeSkeleton.Joints)
                {
                    skeletonDict.Add(j.JointType, new ValueJoint(new Vector3(j.Position.X, j.Position.Y, j.Position.Z), j.TrackingState));
                }

            this.gameTimeSeconds = gameTimeSeconds;
        }

        public void Dispose()
        {
            colorFrame.Dispose();
        }
    }

    class FrameBuffer
    {
        private static List<Frame> buffer = new List<Frame>();
        //The number of seconds the buffer stores. A length of 5 would mean at any time the buffer contains the last
        //5 seconds the Kinect has recorded. (default 2)
        public static int bufferLength = 2;

        public static void AddFrame(Frame newFrame)
        {
            buffer.Add(newFrame);
            for (int i = 0; i < buffer.Count; i++)
            {
                Frame f = buffer[i];
                if (newFrame.gameTimeSeconds - f.gameTimeSeconds >= bufferLength)
                {
                    buffer[i].Dispose();
                    buffer.RemoveAt(i--);
                }
            }
        }

        public static List<Frame> copyBuffer()
        {
            List<Frame> newBuffer = new List<Frame>();
            foreach (Frame f in buffer){
                newBuffer.Add(f);
            }
            buffer.Clear();

            return newBuffer;
        }

        public static int getSize()
        {
            return buffer.Count;
        }
    }
}
