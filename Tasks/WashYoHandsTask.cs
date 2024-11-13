using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class WashYoHandsTask : BaseTask
    {
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "please wash your hands";
            foreach (var item in Interactable.interactables)
            {
                if (item is SinkInteractable toilet)
                {
                    toilet.isPartOfTask = true;
                    BroadCastHandler.instance.HighlightObject((Node3D)toilet.GetParent().GetParent());
                    toilet.task = this;
                    break;
                }
            }
            return base.Setup(worldRoot);
        }
        public void FinishTask()
        {
            EmitSignal(SignalName.TaskFinished, this);
            TaskTracker.instance.CreateTask(new CleanToiletTask());
        }
    }
}
