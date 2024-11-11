using Godot;
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
    }
}
