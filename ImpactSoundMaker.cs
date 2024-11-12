using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame
{
    [GlobalClass]
    public partial class ImpactSoundMaker : Node3D
    {
        [Export]
        RigidBody3D body;
        [Export]
        private AudioStreamPlayer3D? soundEmitter;
        [Export]
        double cooldownTime = 1;
        double currentCooldown = 0;
        [Export]
        private AudioStream? impactSound;
        bool canPlay = false;
        public override void _Ready()
        {
            body.ContactMonitor = true;
            body.MaxContactsReported = 1;
            body.BodyEntered += Body_BodyEntered;
        }

        private void Body_BodyEntered(Node body)
        {
            if (canPlay)
            {
                soundEmitter.Stream = impactSound;
                soundEmitter.Play();
                canPlay = false;
            }
        }
        public override void _Process(double delta)
        {
            currentCooldown += delta;
            if (currentCooldown > cooldownTime)
            {
                currentCooldown = 0;
                canPlay = true;
            }
        }
    }
}
