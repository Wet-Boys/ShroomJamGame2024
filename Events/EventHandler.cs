using Godot;
using ShroomJamGame.Interaction;
using ShroomJamGame.NPC;
using ShroomJamGame.Player;
using ShroomJamGame.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Events
{
    [GlobalClass]
    public partial class EventHandler : Node
    {
        public static EventHandler instance;
        RandomNumberGenerator randomGenerator;
        public override void _Ready()
        {
            instance = this;
            randomGenerator = new RandomNumberGenerator();
        }
        public void StopAllEvents()
        {
            StopSpawningVendingMachinesEvent();
        }


        
        public void StartSpawningVendingMachinesEvent(float duration)
        {
            spawnVendingMachines = true;
            TransformObjectIntoVendingMachine();
            SpawnNewVendingMachine();
            StopVendingMachinesAfterTime(duration);
        }
        public async void StopVendingMachinesAfterTime(float duration)
        {
            await ToSignal(this.GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
            spawnVendingMachines = false;
            BroadCastHandler.instance.FinishDay1();
            await ToSignal(this.GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
            StopSpawningVendingMachinesEvent();
        }
        public void StopSpawningVendingMachinesEvent()
        {
            spawnVendingMachines = false;
            foreach (var item in vendingMachines)
            {
                item.QueueFree();
            }
            foreach (var npc in NpcMovementController.npcs)
            {
                for (int i = 0; i < npc.ownedItems.Count; i++)
                {
                    PhysicsInteractable realItem = npc.ownedItems[i] as PhysicsInteractable;
                    realItem.Freeze = false;
                    realItem.GlobalPosition = npc.ownedItemsStartingPositions[i];
                    realItem.Rotation = npc.ownedItemsStartingRotations[i];
                }
            }
            vendingMachines.Clear();
        }
        bool spawnVendingMachines = false;
        Godot.Collections.Array<Node3D> vendingMachines = new Godot.Collections.Array<Node3D>();
        public async void TransformObjectIntoVendingMachine()
        {
            if (spawnVendingMachines)
            {
                await ToSignal(this.GetTree().CreateTimer(.2f), SceneTreeTimer.SignalName.Timeout);
                foreach (var objectToChange in PhysicsInteractable.physicsObjects)
                {
                    if (IsInstanceValid(objectToChange) && !objectToChange.Freeze)
                    {
                        Node3D newVendingMachine = ((PackedScene)GD.Load("res://Assets/Prefabs/WorldObjects/vending_machine.tscn")).Instantiate<RigidBody3D>();
                        newVendingMachine.GetNode<OmniLight3D>("OmniLight3D").QueueFree();
                        objectToChange.GetTree().Root.AddChild(newVendingMachine);
                        newVendingMachine.GlobalPosition = objectToChange.GlobalPosition;
                        newVendingMachine.RotationDegrees = new Vector3(randomGenerator.Randf(), randomGenerator.Randf(), randomGenerator.Randf()) * 360;
                        objectToChange.Freeze = true;
                        objectToChange.GlobalPosition = new Vector3(-9999, -9999, -9999);
                        vendingMachines.Add(newVendingMachine);
                        break;
                    }
                }
                TransformObjectIntoVendingMachine();
            }
        }
        public async void SpawnNewVendingMachine()
        {
            if (spawnVendingMachines)
            {
                await ToSignal(this.GetTree().CreateTimer(.2f), SceneTreeTimer.SignalName.Timeout);
                if (vendingMachines.Count != 0)
                {
                    Node3D newVendingMachine = ((PackedScene)GD.Load("res://Assets/Prefabs/WorldObjects/vending_machine.tscn")).Instantiate<RigidBody3D>();
                    newVendingMachine.GetNode<OmniLight3D>("OmniLight3D").QueueFree();
                    Node3D vendingMachine = vendingMachines.PickRandom();
                    vendingMachine.GetTree().Root.AddChild(newVendingMachine);
                    newVendingMachine.GlobalPosition = vendingMachine.GlobalPosition;
                    newVendingMachine.RotationDegrees = new Vector3(randomGenerator.Randf(), randomGenerator.Randf(), randomGenerator.Randf()) * 360;
                    vendingMachines.Add(newVendingMachine);
                }
                SpawnNewVendingMachine();
            }
        }

    }
}
