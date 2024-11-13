using Godot;
using ShroomJamGame.Corruption;
using ShroomJamGame.Events;
using ShroomJamGame.Interaction;
using ShroomJamGame.Rendering.Effects;
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
        Godot.Collections.Array<Node3D> arrows = new Godot.Collections.Array<Node3D>();
        int tracker = 0;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Go fix it all";
            for (int i = 0; i < 10; i++)
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
                arrows.Add(((PackedScene)GD.Load("res://Assets/Prefabs/DestinationMarker.tscn")).Instantiate<Node3D>());
                arrows.Last().Scale *= .3f;
                arrows.Last().Position += new Vector3(0, .5f, 0);
                item.AddChild(arrows.Last());
                item.interactionFunction = (yote) =>
                {
                    arrows[interactables.IndexOf(item)].QueueFree();
                    arrows.RemoveAt(interactables.IndexOf(item));
                    interactables.Remove(item);
                    BroadCastHandler.instance.UnHighlightObject(item);
                    tracker++;
                    BroadCastHandler.instance.CreateLoadingBarAtLocation(item, new Vector3(0, 1, 0), 2).ProgressBarFinished += FixManyComputers_ProgressBarFinished;
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
        bool ending = false;
        private void FixManyComputers_ProgressBarFinished(Node3D bar)
        {
            taskName = $"Go fix it all, items left: {interactables.Count}";
            bar.QueueFree();
            switch (tracker)
            {
                default:
                    break;
            }
            if (interactables.Count == 0 && !ending)
            {
                ending = true;
                EmitSignal(SignalName.TaskFinished, this);
                TaskTracker.instance.personalAudio.Stream = (AudioStreamOggVorbis)GD.Load("res://Assets/Sfx/Vinny/manualBlast.ogg");
                TaskTracker.instance.personalAudio.Play();
                CrashGame();
            }
        }
        private async void CrashGame()
        {
            await ToSignal(TaskTracker.instance.GetTree().CreateTimer(6.4f), SceneTreeTimer.SignalName.Timeout);
            AudioCrash.Instance.StartAudioCrash();
            FrameCaptureEffect.CaptureNextFrame = true;
            DataMoshEffect.Instance.Enabled = true;
            DataMoshEffect.Instance.Blend = 1;
            await ToSignal(TaskTracker.instance.GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
            AudioCrash.Instance.StopAudioCrash();
            DataMoshEffect.Instance.Enabled = false;
            TaskTracker.instance.personalAudio.Stream = (AudioStreamOggVorbis)GD.Load("res://Assets/Sfx/glassShatter.ogg");
            TaskTracker.instance.personalAudio.Play();
            TaskTracker.instance.HUD.GetNode<Control>("CrashScreen").Visible = true;
        }
    }
}
