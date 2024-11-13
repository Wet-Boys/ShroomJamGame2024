using Godot;
using ShroomJamGame.Interaction;
using ShroomJamGame.NPC;
using ShroomJamGame.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Events
{
    [GlobalClass]
    public partial class BroadCastHandler : Node //godot doesn't have broadcastmessage like Unity does, cry about it.
        //this is just a global function class now lmao
    {
        public static BroadCastHandler instance;
        public override void _Ready()
        {
            instance = this;
            RandomizeNPCs();
        }

        //put new broadcast messages here
        [Signal]
        public delegate void FinishedQuestEventHandler(string taskName);
        public void FinishQuestBroadcast(string taskName)
        {
            EmitSignal(SignalName.FinishedQuest, taskName);
        }
        ///////////////////////////////////
        [Signal]
        public delegate void CreateFixQuestEventHandler();
        public void CreateFixQuestBroadcast()
        {
            EmitSignal(SignalName.CreateFixQuest);
        }

        public ProgressBarRunner CreateLoadingBarAtLocation(Node3D parent, Vector3 position, float timeToDoTask)
        {
            Node3D bar = ((PackedScene)GD.Load("res://Assets/Prefabs/ProgressBar/ProgressBar.tscn")).Instantiate<Node3D>();
            parent.AddChild(bar);
            bar.Position = position;
            (bar as ProgressBarRunner).timeToComplete = timeToDoTask;
            return (bar as ProgressBarRunner);
        }

        Godot.Collections.Dictionary<Node3D, Godot.Collections.Array<MeshInstance3D>> glowyObjects = new Godot.Collections.Dictionary<Node3D, Godot.Collections.Array<MeshInstance3D>>();
        public void HighlightObject(Node3D obj)
        {
            if (glowyObjects.ContainsKey(obj))
            {
                return;
            }
            glowyObjects.Add(obj, new Godot.Collections.Array<MeshInstance3D>());
            int childCount = obj.GetChildren().Count;
            for (int i = 0; i < childCount; i++)
            {
                if (obj.GetChildren()[i] is MeshInstance3D mesh)
                {
                    glowyObjects[obj].Add(mesh.Duplicate() as MeshInstance3D);
                    obj.AddChild(glowyObjects[obj].Last());
                    for (int x = 0; x < glowyObjects[obj].Last().Mesh.GetSurfaceCount(); x++)
                    {
                        glowyObjects[obj].Last().Mesh = (Mesh)glowyObjects[obj].Last().Mesh.Duplicate();
                        glowyObjects[obj].Last().Mesh.SurfaceSetMaterial(x, (Material)GD.Load("res://Assets/Shaders/GlowMaterial.tres"));
                    }
                }
            }
        }
        public void UnHighlightObject(Node3D obj)
        {
            foreach (var item in glowyObjects[obj])
            {
                item.QueueFree();
            }
            glowyObjects.Remove(obj);
        }


        [Signal]
        public delegate void Day1FinishedEventHandler();
        public void FinishDay1()
        {
            EmitSignal(SignalName.Day1Finished);
        }
        public void StopNPCAndSpeak(NpcMovementController npcController, string whatToSay)
        {
            npcController.STOP();
            npcController.onlyLookAtPlayer = true;
            npcController.GoToPositionAndSayWords(npcController.characterBody3D.GlobalPosition, whatToSay);
        }
        public void ResetPlayer()
        {
            TaskTracker.instance.player.GlobalPosition = TaskTracker.instance.originalPlayerPos;
        }

        bool randomizing = false;
        RandomNumberGenerator rando = new RandomNumberGenerator();
        public async void RandomizeNPCs()
        {
            await ToSignal(this.GetTree().CreateTimer(rando.RandfRange(60, 120)), SceneTreeTimer.SignalName.Timeout);
            randomizing = true;
            await ToSignal(this.GetTree().CreateTimer(6f), SceneTreeTimer.SignalName.Timeout);
            randomizing = false;
            RandomizeNPCs();
        }
        public override void _Process(double delta)
        {
            if (randomizing)
            {
                foreach (var npc in NpcMovementController.npcs)
                {
                    npc._visualController.RandomizeOutfit();
                }
            }
        }
    }
}
