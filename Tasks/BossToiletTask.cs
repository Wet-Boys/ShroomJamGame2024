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
    public partial class BossToiletTask : BaseTask
    {
        BossInteractables toilet;
        BossInteractables sink;
        BossInteractables urinal;
        NpcMovementController boss;
        CharacterBody3D player;
        int tracker = 0;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Fix your boss's smart toilet";
            foreach (var item in BossInteractables.bossItems)
            {
                if (item.sink)
                {
                    sink = item;
                }
                else if (item.urinal)
                {
                    urinal = item;
                }
                else
                {
                    toilet = item;
                }
            }
            sink.objectNode.Visible = true;
            urinal.objectNode.Visible = false;
            BroadCastHandler.instance.HighlightObject(toilet.objectNode);
            foreach (var item in NpcMovementController.npcs)
            {
                if (item.characterBody3D.Name.ToString().StartsWith("Boss"))
                {
                    boss = item;
                    break;
                }
            }
            boss.GoToPositionAndSayWords(new Vector3(-5.521f, 7.076f, 21.559f), "Ugh, it's broken again. I guess it just can't handle my girth...");
            boss.permaWait = true;
            boss.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
            player = TaskTracker.instance.player;
            return base.Setup(worldRoot);
        }

        private void AnimalesePlayer_FinishedPlaying()
        {
            if (tracker == 1)
            {
                tracker = 2;
                boss.onlyLookAtPlayer = false;
                boss.GoToPositionAndSayWords(new Vector3(-12.354f, 7.076f, 21.559f), "");
                toilet.isActive = true;
                toilet.hoverText = $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to unclog toilet";
                toilet.interactionFunction = (sigmaBalls) =>
                {
                    toilet.hoverText = "cleaning...";
                    tracker = 3;
                    BroadCastHandler.instance.CreateLoadingBarAtLocation(toilet.objectNode, new Vector3(0, 1, 0), 7.5f).ProgressBarFinished += BossToiletTask_ProgressBarFinished;
                    boss.SayWords("Pretty impressive isn't it?");
                    toilet.interactionFunction = (ligma) => { return 0; };
                    return 0;
                };
            }
            if (tracker == 6)
            {
                taskName = "...";
                tracker = 7;
                boss.onlyLookAtPlayer = false;
                boss.GoToPositionAndSayWords(new Vector3(-5.521f, 7.076f, 21.559f), "Yeah I don't know what you are on about.\nThanks for fixing my toilet though!");
            }
            else if (tracker == 7)
            {
                tracker = 8;
                boss.AnimalesePlayer.FinishedPlaying -= AnimalesePlayer_FinishedPlaying;
                EmitSignal(SignalName.TaskFinished, this);
                TaskTracker.instance.CreateTask(new WashYoHandsTask());
                boss.permaWait = false;
                boss.waiting = false;
            }
        }

        private void BossToiletTask_ProgressBarFinished(Node3D bar)
        {
            bar.QueueFree();
            tracker = 4;
            taskName = "Wash your hands in the sink";
            toilet.hoverText = "Freshly cleaned pipes";
            BroadCastHandler.instance.UnHighlightObject(toilet.objectNode);
            BroadCastHandler.instance.HighlightObject(sink.objectNode);
            sink.interactionFunction = (sink) =>
            {
                tracker = 5;
                taskName = "Report to your boss";
                sink.hoverText = "The sink seems to be malfunctioning, you should ask your boss";
                sink.interactionFunction = (ligma) => { return 0; };
                BroadCastHandler.instance.UnHighlightObject(sink.objectNode);
                return 0;
            };
            sink.hoverText = $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to wash your hands";
            sink.isActive = true;
        }

        public override void PerformTask()
        {
            if (tracker == 0 && player.GlobalPosition.DistanceTo(toilet.objectNode.GlobalPosition) < 4 && !boss.AnimalesePlayer.isSpeaking)
            {
                tracker = 1;
                boss.onlyLookAtPlayer = true;
                boss.SayWords("Hey newbie, I bet college didn't teach ya how to fix a toilet.\nHonestly they don't teach you kids any useful information anymore.\nWell anyway good luck, let me know when you are done.");
            }
            if (tracker == 5 && player.GlobalPosition.DistanceTo(boss.characterBody3D.GlobalPosition) < 3)
            {
                taskName = "Uh oh chat";
                tracker = 6;
                boss.onlyLookAtPlayer = true;
                boss.SayWords("Ah that was quicker than I expected.\n...\nMy sink...?\nWhat are you talking about?");
                sink.objectNode.Visible = false;
                urinal.objectNode.Visible = true;
            }
        }
    }
}
