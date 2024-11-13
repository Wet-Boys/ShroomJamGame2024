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
    public partial class CleanToiletTask : BaseTask
    {
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Unclog the toilet";
            foreach (var item in Interactable.interactables)
            {
                if (item is ToiletInteractable toilet)
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
        }
    }
}
