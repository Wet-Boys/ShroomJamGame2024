using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    [GlobalClass]
    public partial class BaseTask : Node
    {
        [Signal]
        public delegate void TaskFinishedEventHandler(BaseTask task);
        public string taskName = "";
        public virtual BaseTask Setup(Node worldRoot)
        {
            return this;
        }
        public virtual void PerformTask()
        {
        }
    }
}
