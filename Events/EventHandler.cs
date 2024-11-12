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


        
        public void StartSpawningVendingMachinesEvent()
        {
            spawnVendingMachines = true;
            TransformObjectIntoVendingMachine();
            SpawnNewVendingMachine();
        }
        public void StopSpawningVendingMachinesEvent()
        {
            spawnVendingMachines = false;
            vendingMachines.Clear();
        }
        bool spawnVendingMachines = false;
        Godot.Collections.Array<Node3D> vendingMachines = new Godot.Collections.Array<Node3D>();
        public async void TransformObjectIntoVendingMachine()
        {
            if (spawnVendingMachines && PhysicsInteractable.physicsObjects.Count != 0)
            {
                await ToSignal(this.GetTree().CreateTimer(.2f), SceneTreeTimer.SignalName.Timeout);
                PhysicsInteractable objectToChange = PhysicsInteractable.physicsObjects.PickRandom();
                Node3D newVendingMachine = ((PackedScene)GD.Load("res://Assets/Prefabs/WorldObjects/vending_machine.tscn")).Instantiate<RigidBody3D>();
                newVendingMachine.GetNode<OmniLight3D>("OmniLight3D").QueueFree();
                if (IsInstanceValid(objectToChange))
                {
                    objectToChange.GetTree().Root.AddChild(newVendingMachine);
                    newVendingMachine.GlobalPosition = objectToChange.GlobalPosition;
                    newVendingMachine.RotationDegrees = new Vector3(randomGenerator.Randf(), randomGenerator.Randf(), randomGenerator.Randf()) * 360;
                    if (IsInstanceValid(objectToChange.owner))
                    {
                        objectToChange.owner.ownedItems.Remove(objectToChange);
                    }
                    objectToChange.QueueFree();
                    vendingMachines.Add(newVendingMachine);
                }
                else
                {
                    newVendingMachine.QueueFree();
                }
                TransformObjectIntoVendingMachine();
            }
        }
        public async void SpawnNewVendingMachine()
        {
            if (spawnVendingMachines)
            {
                await ToSignal(this.GetTree().CreateTimer(.01f), SceneTreeTimer.SignalName.Timeout);
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
