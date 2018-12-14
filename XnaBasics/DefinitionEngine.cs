using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Samples.Kinect.XnaBasics
{
    public enum ActionType
    {
        MOVETO, DEFINE, PERFORMSPECIFIC, SOLVE
    }

    struct GestureAction
    {
        public int id;
        public UserDisplay.StateManipDel del;
        public string label;
        public string description;
    }

    /// <summary>
    /// This manages tasks as they are presented to the user.
    /// </summary>
    class DefinitionEngine
    {
        
    }
}
