using Godot;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class PlaceItemAtPositionTask : BaseTask
    {
        public Vector3 placementPostion;
        public Node3D targetNode;
        public override BaseTask Setup(Node worldRoot)
        {
            targetNode = ((PackedScene)GD.Load("res://Various Debug/physicsBall.tscn")).Instantiate<PhysicsInteractable>();
            targetNode.Name = "sexhaver";
            worldRoot.AddChild(targetNode);
            targetNode.GlobalPosition = new Vector3(-7.324f, 10.148f, 13.948f);
            placementPostion = new Vector3(-16.102f, 2.145f, -4.401f);
            taskName = "Test Task";
            return base.Setup(worldRoot);
        }
        public override void PerformTask()
        {
            if (IsInstanceValid(targetNode) && targetNode.GlobalPosition.DistanceTo(placementPostion) < .5f)
            {
                targetNode.QueueFree();
                EmitSignal(SignalName.TaskFinished, this);
            }
        }
    }
}
