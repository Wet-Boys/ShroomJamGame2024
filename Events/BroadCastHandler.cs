using Godot;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Events
{
    [GlobalClass]
    public partial class BroadCastHandler : Node //godot doesn't have broadcastmessage like Unity does, cry about it.
    {
        public static BroadCastHandler instance;
        public override void _Ready()
        {
            instance = this;
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
    }
}
