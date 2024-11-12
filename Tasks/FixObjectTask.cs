using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShroomJamGame.NPC.NpcOutfitController;

namespace ShroomJamGame.Tasks
{
    public partial class FixObjectTask : BaseTask
    {
        //have any data for the task needed here, like new objects, target objects, stuff like that
        public Vector3 placementPosition;
        public PhysicsInteractable target;
        public Vector3 originalPosition;
        Godot.Collections.Array<MeshInstance3D> meshInstances = new Godot.Collections.Array<MeshInstance3D>();
        Node3D circle;

        //override this with setup for your task, worldRoot is for spawning stuff in, returns itself for tracking purposes
        public override BaseTask Setup(Node worldRoot)
        {
            target = PhysicsInteractable.physicsObjects.PickRandom();
            circle = ((PackedScene)GD.Load("res://Assets/Prefabs/WorldObjects/ClickbaitCircle.tscn")).Instantiate<Node3D>();
            target.AddChild(circle);
            circle.Position = Vector3.Zero;
            BroadCastHandler.instance.HighlightObject(target);
            originalPosition = target.GlobalPosition;
            placementPosition = new Vector3(-2.85f, 7.48f, -3.72f);
            taskName = "Take object to your work station";
            return base.Setup(worldRoot);
        }

        //Not required, all that is required is that at some point, you make this task emit the TaskFinished signal.
        //override this with completion info, in the example case, the ball just needs to be close to the target.
        public override void PerformTask()
        {
            if (IsInstanceValid(target) && target.GlobalPosition.DistanceTo(placementPosition) < .5f)
            {
                if (taskName == "Take object to your work station")
                {
                    taskName = "Take object back";
                    placementPosition = originalPosition;
                    circle.Free();
                    circle = ((PackedScene)GD.Load("res://Assets/Prefabs/WorldObjects/ClickbaitCheck.tscn")).Instantiate<Node3D>();
                    target.AddChild(circle);
                    circle.Position = Vector3.Zero;
                }
                else
                {
                    BroadCastHandler.instance.UnHighlightObject(target);
                    circle.QueueFree();
                    EmitSignal(SignalName.TaskFinished, this);
                }
            }
        }
    }
}
