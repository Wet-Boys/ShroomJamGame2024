using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.Interaction;
using ShroomJamGame.NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class PlugInComputerTask : BaseTask
    {
        public NpcMovementController npc;
        public override BaseTask Setup(Node worldRoot)
        {
            return base.Setup(worldRoot);
        }
        int counter = 1;
        PhysicsInteractable targetComputer;
        public void SetupForReal(NpcMovementController npc)
        {
            taskName = "Please Hold";
            this.npc = npc;
            npc.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
        }

        private void AnimalesePlayer_FinishedPlaying()
        {
            if (counter == 1)
            {
                foreach (var item in npc.ownedItems)
                {
                    if (item.Name.ToString().StartsWith("Gigabyte"))
                    {
                        BroadCastHandler.instance.HighlightObject(item);
                        BroadCastHandler.instance.StopNPCAndSpeak(npc, "Hey, while I've got you here\ncan you look at my computer?\nIt suddenly stopped working after I moved it\nand I'm not sure what's wrong.");
                        taskName = "Utilize your ears";
                        npc.targetNode = item;
                        targetComputer = item as PhysicsInteractable;
                        targetComputer.holdable = false;
                        break;
                    }
                }
            }
            else if (counter == 0)
            {
                npc.GoToPositionAndSayWords(targetComputer.GlobalPosition, "This is it. Do you mind taking a look?");
                npc.permaWait = true;
                taskName = "Follow Them";
            }
            else if (counter == -1)
            {
                taskName = "Fix the computer";
                targetComputer.nonHoldableText = $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to plug in power cord";
                targetComputer.interactionFunction = (physObject) =>
                {
                    physObject.progressBar = BroadCastHandler.instance.CreateLoadingBarAtLocation(physObject, new Vector3(0, 1, 0), 1);
                    physObject.progressBar.ProgressBarFinished += ProgressBar_ProgressBarFinished;
                    targetComputer.nonHoldableText = $"";
                    targetComputer.interactionFunction = (physObject) =>
                    {
                        return 0;
                    };
                    return 0;
                };
            }
            else if (counter == -2)
            {
                npc.permaWait = false;
                npc.waiting = false;
                TaskTracker.instance.CreateTask(new FixChairTask());
                npc.AnimalesePlayer.FinishedPlaying -= AnimalesePlayer_FinishedPlaying;
            }
            counter--;
        }

        private void ProgressBar_ProgressBarFinished(Node3D bar)
        {
            bar.QueueFree();
            npc.GoToPositionAndSayWords(npc.characterBody3D.GlobalPosition, "Oh it was that easy?\nYou IT people sure are good at your jobs.\nThat's why we keep you around haha..............");
            EmitSignal(SignalName.TaskFinished, this);
            BroadCastHandler.instance.UnHighlightObject(targetComputer);
            targetComputer.holdable = true;
        }
    }
}
