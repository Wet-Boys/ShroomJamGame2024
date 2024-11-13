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
    public partial class FixManyComputers : BaseTask//many monsters here this door many monsters this door many monsters this door many monsters this door many monsters this door many monsters this door many monsters this door many monsters this door
    {
        Godot.Collections.Array<PhysicsInteractable> interactables = new Godot.Collections.Array<PhysicsInteractable>();
        int tracker = 0;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Go fix it all";
            for (int i = 0; i < 20; i++)
            {
                PhysicsInteractable yeet;
                do
                {
                   yeet = PhysicsInteractable.physicsObjects.PickRandom();
                } while (interactables.Contains(yeet));
                interactables.Add(yeet);
            }
            foreach (var item in interactables)
            {
                item.holdable = false;
                BroadCastHandler.instance.HighlightObject(item);
                item.nonHoldableText = "Repair it";
                item.interactionFunction = (yote) =>
                {
                    interactables.Remove(item);
                    BroadCastHandler.instance.UnHighlightObject(item);
                    tracker++;
                    BroadCastHandler.instance.CreateLoadingBarAtLocation(item, new Vector3(0, 1, 0), 5).ProgressBarFinished += FixManyComputers_ProgressBarFinished;
                    item.nonHoldableText = "";
                    item.interactionFunction = (yote2) =>
                    {
                        return 0;
                    };
                    return 0;
                };
            }
            return base.Setup(worldRoot);
        }

        private void FixManyComputers_ProgressBarFinished(Node3D bar)
        {
            taskName = $"Go fix it all, items left: {interactables.Count}";
            bar.QueueFree();
            switch (tracker)
            {
                default:
                    break;
            }
            if (interactables.Count == 0)
            {
                EmitSignal(SignalName.TaskFinished, this);
            }
        }
    }
}
