using Godot;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class ExampleTask : BaseTask
    {
        //have any data for the task needed here, like new objects, target objects, stuff like that
        public Vector3 placementPostion;
        public Node3D physicsBall;

        //override this with setup for your task, worldRoot is for spawning stuff in, returns itself for tracking purposes
        public override BaseTask Setup(Node worldRoot)
        {
            physicsBall = ((PackedScene)GD.Load("res://Various Debug/physicsBall.tscn")).Instantiate<PhysicsInteractable>();
            physicsBall.Name = "sexhaver";
            worldRoot.AddChild(physicsBall);
            physicsBall.GlobalPosition = new Vector3(-7.324f, 10.148f, 13.948f);
            placementPostion = new Vector3(-16.102f, 2.145f, -4.401f);
            taskName = "Test Task";
            return base.Setup(worldRoot);
        }

        //Not required, all that is required is that at some point, you make this task emit the TaskFinished signal.
        //override this with completion info, in the example case, the ball just needs to be close to the target.
        public override void PerformTask()
        {
            if (IsInstanceValid(physicsBall) && physicsBall.GlobalPosition.DistanceTo(placementPostion) < .5f)
            {
                physicsBall.QueueFree();
                EmitSignal(SignalName.TaskFinished, this);
            }
        }
    }
}
