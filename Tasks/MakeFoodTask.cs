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
    public partial class MakeFoodTask : BaseTask
    {
        DoorInteractable microwaveDoor;
        Godot.Collections.Array<PhysicsInteractable> foods = new Godot.Collections.Array<PhysicsInteractable>();
        Godot.Collections.Array<Vector3> positions = new Godot.Collections.Array<Vector3>() {new Vector3(-6.574f, 6.819f, -4.75f) , new Vector3(-6.751f, 6.819f, -4.634f), new Vector3(-6.751f, 7.085f, -4.634f), new Vector3(-6.751f, 8.076f, -4.774f), new Vector3(-7.485f, 7.414f, -4.622f), new Vector3(-7.628f, 7.223f, -4.622f), new Vector3(-8.03f, 6.86f, -4.8f), new Vector3(-8.03f, 6.86f, -5.011f) };
        Node3D legs;
        public override BaseTask Setup(Node worldRoot)
        {
            foreach (var item in PhysicsInteractable.physicsObjects)
            {
                if (item.Name.ToString().StartsWith("Mouse"))
                {
                    foods.Add(item);
                    item.offLimits = true;
                    BroadCastHandler.instance.HighlightObject(item);
                }
            }
            for (int i = 0; i < positions.Count; i++)
            {
                foods[i].GlobalPosition = positions[i];
            }
            taskName = "You're hungry, make food";
            foreach (var item in Interactable.interactables)
            {
                Node3D node = item.GetNode<Node3D>("../../..");
                if (node is not null)
                {
                    if (node.Name.ToString().StartsWith("Microwave"))
                    {
                        microwaveDoor = item as DoorInteractable;

                        legs = item.GetNode<Node3D>("../../../legs");
                        microwaveDoor.interactionFunction = (door) =>
                        {
                            if (!door.IsOpen)
                            {
                                foreach (var foood in foods)
                                {
                                    if (foood.GlobalPosition.DistanceTo(legs.GlobalPosition) < 1)
                                    {
                                        EmitSignal(SignalName.TaskFinished, this);
                                        TaskTracker.instance.currentDay = 2;
                                        TaskTracker.instance.DayFinished();
                                        foreach (var foooood in foods)
                                        {
                                            foooood.offLimits = false;
                                            BroadCastHandler.instance.UnHighlightObject(foooood);
                                        }
                                        door.interactionFunction = (door) =>
                                        {
                                            return 0;
                                        };
                                        break;
                                    }
                                }
                            }
                            return 0;
                        };
                        break;
                    }
                }
            }
            return base.Setup(worldRoot);
        }
    }
}
