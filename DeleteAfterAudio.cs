using Godot;
using System;

public partial class DeleteAfterAudio : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	AudioStreamPlayer3D player;
	public override void _Ready()
	{
        player.Finished += Player_Finished;
	}

    private void Player_Finished()
    {
		QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
