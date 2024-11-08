using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.NPC
{
    [GlobalClass]
    public partial class NpcVisualController : Node3D
    {
        [Export]
        NpcOutfitController outfitController;
        [Export]
        AnimationTree animationTree;
        Tween tween;

        public void SetAnimationTreeState(float walkState)
        {
            if (tween is not null)
            {
                tween.Kill();
            }
            tween = CreateTween();
            tween.TweenProperty(animationTree, "parameters/movement_blend/blend_position", walkState, .25f);
        }
        public void RandomizeOutfit()
        {
            outfitController.RandomizeParts();
        }
    }
}
