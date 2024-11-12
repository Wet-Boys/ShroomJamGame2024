using Godot;

namespace ShroomJamGame.Sound;

[GlobalClass]
public partial class ImpactSoundMaker : Node3D
{
    [Export]
    private RigidBody3D? _body;
    [Export]
    private AudioStreamPlayer3D? _soundEmitter;
    [Export]
    private double _cooldownTime = 1;
    [Export]
    private AudioStream? _impactSound;

    private bool _canPlay;
    private double _currentCooldown;

    public override void _Ready()
    {
        if (_body is null || _soundEmitter is null)
            return;
        
        _soundEmitter.Stream = _impactSound;
        
        _body.ContactMonitor = true;
        _body.MaxContactsReported = 1;
        _body.BodyEntered += Body_BodyEntered;
        _soundEmitter.Finished += SoundEmitter_Finished;
        _canPlay = true;
    }

    private void SoundEmitter_Finished()
    {
        _canPlay = true;
    }

    private void Body_BodyEntered(Node body)
    {
        if (_soundEmitter is null)
            return;

        // if (!_canPlay)
        //     return;
            
        // _soundEmitter.Stream = _impactSound;
        _soundEmitter.Play();
        // _canPlay = false;
    }
}