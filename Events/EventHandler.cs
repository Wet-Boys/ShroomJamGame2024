using Godot;
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
        public override void _Ready()
        {
            instance = this;
        }
    }
}
