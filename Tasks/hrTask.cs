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
    public partial class hrTask : BaseTask
    {
        RigidBody3D cardBoard;
        PhysicsInteractable monitor;
        NpcMovementController hr;
        CharacterBody3D player;
        int tracker = 0;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Fix HR's computer";
            foreach (var item in PhysicsInteractable.physicsObjects)
            {
                if (item.Name.ToString().StartsWith("Screen2_015") && item.GetParent().Name.ToString().StartsWith("Cubicle3"))
                {
                    monitor = item;
                    break;
                }
            }
            foreach (var item in NpcMovementController.npcs)
            {
                if (item.characterBody3D.Name.ToString().StartsWith("HR"))
                {
                    hr = item;
                    break;
                }
            }
            cardBoard = monitor.GetNode<RigidBody3D>("../hr cardboard");
            player = TaskTracker.instance.player;
            hr.GoToPositionAndSayWords(hr.originalPosition, "");
            hr.permaWait = true;
            monitor.holdable = false;
            BroadCastHandler.instance.HighlightObject(monitor);
            return base.Setup(worldRoot);
        }
        public override void PerformTask()
        {
            if (tracker == 0 && player.GlobalPosition.DistanceTo(monitor.GlobalPosition) < 4 && !hr.AnimalesePlayer.isSpeaking)
            {
                tracker = 1;
                hr.onlyLookAtPlayer = true;
                hr.SayWords("Oh hey, glad you saw my ticket.\nYeah can you take a look at my computer?\nSome people are posting stuff about me online\nso I think I might have gotten hacked.");
                hr.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
            }
        }

        private void AnimalesePlayer_FinishedPlaying()
        {
            if (tracker == 1)
            {
                tracker = 2;
                taskName = "Uninstall \"hacks\"";
                monitor.nonHoldableText = "Uninstall Webphishing";
                monitor.interactionFunction = (physObject) =>
                {
                    tracker = 3;
                    var bar = BroadCastHandler.instance.CreateLoadingBarAtLocation(monitor, new Vector3(0, 0.012f, -0.007f), 30);
                    bar.sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Disabled;
                    bar.RotationDegrees = new Vector3(-90, 0, 0);
                    bar.Scale = new Vector3(0.378f, 0.378f, 0.378f);
                    bar.ProgressBarFinished += Bar_ProgressBarFinished;
                    monitor.nonHoldableText = "uninstalling...";
                    BroadCastHandler.instance.UnHighlightObject(monitor);
                    DropBoard();
                    monitor.interactionFunction = (physObject) =>
                    {
                        return 0;
                    };
                    return 0;
                };
            }
        }
        private async void DropBoard()
        {
            tracker = 4;
            await ToSignal(hr.GetTree().CreateTimer(10f), SceneTreeTimer.SignalName.Timeout);
            hr.GoToPositionAndSayWords(hr.characterBody3D.GlobalPosition,"uh");
            hr.permaWait = true;
            hr.onlyLookAtPlayer = false;
            await ToSignal(hr.GetTree().CreateTimer(.5f), SceneTreeTimer.SignalName.Timeout);
            cardBoard.Freeze = false;
            await ToSignal(hr.GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
        }

        private void Bar_ProgressBarFinished(Node3D bar)
        {
            tracker = 5;
            bar.QueueFree();
            monitor.nonHoldableText = "";
            monitor.holdable = true;
            EmitSignal(SignalName.TaskFinished, this);
            TaskTracker.instance.CreateTask(new MakeFoodTask());
        }
    }
}
