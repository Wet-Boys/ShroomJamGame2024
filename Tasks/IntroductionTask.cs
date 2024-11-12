using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class IntroductionTask : BaseTask
    {
        Godot.Collections.Array<NpcMovementController> peopleToTalkTo = new Godot.Collections.Array<NpcMovementController>();
        public override BaseTask Setup(Node worldRoot)
        {
            foreach (var item in NpcMovementController.npcs)
            {
                peopleToTalkTo.Add(item);
                item.interactionComponent.isInteractable = true;
                item.interactionComponent.InteractedWith += InteractionComponent_InteractedWith;
                BroadCastHandler.instance.HighlightObject(item._visualController.skeleton);
            }
            taskName = "Greet everyone";
            return base.Setup(worldRoot);
        }

        private void InteractionComponent_InteractedWith(NpcMovementController itsme)
        {
            BroadCastHandler.instance.UnHighlightObject(itsme._visualController.skeleton);
            peopleToTalkTo.Remove(itsme);
        }

        public override void PerformTask()
        {
            if (peopleToTalkTo.Count == 0)
            {
                EmitSignal(SignalName.TaskFinished, this);
            }
        }
    }
}
