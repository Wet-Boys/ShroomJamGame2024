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
    public partial class FixChairTask : BaseTask
    {
        PhysicsInteractable targetChair;
        NpcMovementController taskNpc;
        Node3D destMarker;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "What was that noise in the kitchen?";
            Godot.Collections.Array<PhysicsInteractable> barStools = new Godot.Collections.Array<PhysicsInteractable>();
            foreach (var item in PhysicsInteractable.physicsObjects)
            {
                if (item.Name.ToString().StartsWith("Bar Stool"))
                {
                    barStools.Add(item);
                }
            }
            targetChair = barStools.PickRandom();
            BroadCastHandler.instance.HighlightObject(targetChair);
            float closestDistance = 100;
            NpcMovementController closestNPC = NpcMovementController.npcs.First();
            foreach (var item in NpcMovementController.npcs)
            {
                float dist = item.characterBody3D.GlobalPosition.DistanceTo(targetChair.GlobalPosition);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestNPC = item;
                }
            }
            taskNpc = closestNPC;
            taskNpc.permaWait = true;
            taskNpc.GoToPositionAndSayWords(targetChair.GlobalPosition, "");
            Node3D audio = ((PackedScene)GD.Load("res://one_shot_audio.tscn")).Instantiate<Node3D>();
            taskNpc.GetTree().Root.AddChild(audio);
            audio.GlobalPosition = new Vector3(-12.354f, 6.273f, -12.723f);
            audio.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
            return base.Setup(worldRoot);
        }

        private void FixChairTask_Finished()
        {
            throw new NotImplementedException();
        }

        bool npcSpoke = false;
        int tracker = 0;
        bool tracker2Done = false;
        bool flag = false;
        public override void PerformTask()
        {
            if (tracker == 0)
            {
                if (TaskTracker.instance.player.GlobalPosition.DistanceTo(targetChair.GlobalPosition) < 4 && !npcSpoke)
                {
                    npcSpoke = true;
                    taskNpc.onlyLookAtPlayer = true;
                    taskNpc.SayWords("Yeah... Sorry about that.\nMy gamer PC isn't what it used to be.\nCould you take it back to your office and fix it up?");
                    taskNpc.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
                }
            }
            if (tracker == 1)
            {
                if (!tracker2Done)
                {
                    tracker2Done = true;
                    destMarker = ((PackedScene)GD.Load("res://Assets/Prefabs/DestinationMarker.tscn")).Instantiate<Node3D>();
                    TaskTracker.instance.player.GetTree().Root.AddChild(destMarker);
                    destMarker.GlobalPosition = new Vector3(0, 6.581f, -1.3878f);
                    targetChair.offLimits = true;
                }
                if (IsInstanceValid(targetChair) && IsInstanceValid(destMarker) && targetChair.GlobalPosition.DistanceTo(destMarker.GlobalPosition) < 3 && !flag)
                {
                    flag = true;
                    taskName = "Swap out the graphics card";
                    targetChair.holdable = false;
                    targetChair.nonHoldableText = $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to swap out the GPU";
                    targetChair.interactionFunction = (physObject) =>
                    {
                        physObject.progressBar = BroadCastHandler.instance.CreateLoadingBarAtLocation(physObject, new Vector3(0, 1, 0), 30);
                        physObject.progressBar.ProgressBarFinished += ProgressBar_ProgressBarFinished;
                        taskNpc.GoToPositionAndSayWords(new Vector3(0, 6.581f, -1.3878f), "Hey not to pile too much on you, but when you finish\ncan you go fix the vending machine on the right?\nIt's been acting up lately.");
                        destMarker.QueueFree();
                        targetChair.nonHoldableText = $"";
                        targetChair.interactionFunction = (physObject) =>
                        {
                            return 0;
                        };
                        return 0;
                    };
                }
            }
        }

        private void ProgressBar_ProgressBarFinished(Node3D bar)
        {
            bar.QueueFree();
            EmitSignal(SignalName.TaskFinished, this);
            BroadCastHandler.instance.UnHighlightObject(targetChair);
            targetChair.holdable = true;
            targetChair.offLimits = false;
            TaskTracker.instance.CreateTask(new FixVendingMachineTask());
        }

        private void AnimalesePlayer_FinishedPlaying()
        {
            if (tracker == 0)
            {
                taskNpc.onlyLookAtPlayer = false;
                taskName = "Take the computer back to your office";
            }
            if (tracker == 1)
            {
                taskNpc.AnimalesePlayer.FinishedPlaying -= AnimalesePlayer_FinishedPlaying;
                taskNpc.permaWait = false;
                taskNpc.waiting = false;
            }
            tracker++;
        }
    }
}
