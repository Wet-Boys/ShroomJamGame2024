using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Interaction
{
    [Tool]
    [GlobalClass]
    public partial class BossInteractables : Interactable
    {
        public bool isActive = false;
        [Export]
        public bool sink;
        [Export]
        public bool urinal;
        [Export]
        public Node3D? objectNode;
        public string hoverText = "";
        public static Godot.Collections.Array<BossInteractables> bossItems = new Godot.Collections.Array<BossInteractables>();
        public Func<BossInteractables, int> interactionFunction = null;

        public override void _Ready()
        {
            bossItems.Add(this);
            if (sink)
            {
                objectNode.Visible = false;
            }
            base._Ready();
        }
        public override void Interact()
        {
            if (interactionFunction is not null)
            {
                interactionFunction(this);
            }
        }

        public override string GetOnHoverText()
        {
            if (isActive)
            {
                return $"{hoverText}";
            }
            return $"";
        }
    }
}
