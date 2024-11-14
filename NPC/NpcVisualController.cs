using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShroomJamGame.NPC.NpcOutfitController;

namespace ShroomJamGame.NPC
{
    [GlobalClass]
    public partial class NpcVisualController : Node3D
    {
        [Export]
        NpcOutfitController outfitController;
        [Export]
        AnimationTree animationTree;
        [Export]
        public Skeleton3D skeleton;
        Godot.Collections.Array<string> oneShots = new Godot.Collections.Array<string>() { "1", "2", "3", "4" };
        bool sinking = false;
        float sinkingAmount = 0;
        public override void _Process(double delta)
        {
            if (sinking)
            {
                sinkingAmount += (float)delta;
                animationTree.Set("parameters/InGround/blend_amount", $"{sinkingAmount}");
            }
        }
        public void PlayRandomOneShot()
        {
            animationTree.Set($"parameters/{oneShots.PickRandom()}/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
        public void Sink()
        {
            sinking = true;
        }
        public void Interact()
        {
            animationTree.Set("parameters/OneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
        public void SetAnimationTreeState(float velocity, bool holding)
        {
            animationTree.Set("parameters/Transition/transition_request", velocity < .01f ? "Idle" : "Walk");
            animationTree.Set("parameters/HoldWalk/blend_amount", holding ? "1" : "0");
            animationTree.Set("parameters/currentWalkingSpeed/scale", velocity * 1.5f);
        }
        public void RandomizeOutfit()
        {
            outfitController.RandomizeParts();
        }
        public void SetEyes(Eyes eyeID)
        {
            outfitController.SetEyes(eyeID);
        }
        public void SetMouth(Mouths mouthID)
        {
            outfitController.SetMouth(mouthID);
        }
        public void SetFeet(Feets feetID)
        {
            outfitController.SetFeet(feetID);
        }
        public void SetHands(Hands handID)
        {
            outfitController.SetHands(handID);
        }
        public void SetTorsos(Torsos torsoID)
        {
            outfitController.SetTorsos(torsoID);
        }
        public void SetHead(Heads headID)
        {
            outfitController.SetHead(headID);
        }
    }
}
