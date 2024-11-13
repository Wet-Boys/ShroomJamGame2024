using Godot;
using ShroomJamGame.Tasks;
using System;

public partial class ProgressBarRunner : Node3D
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	public Sprite3D sprite;
	[Export]
	SubViewport viewport;
    [Export]
    public ProgressBar progressBar;
    public double timeToComplete;
    [Signal]
    public delegate void ProgressBarFinishedEventHandler(Node3D bar);
	public CharacterBody3D player;
    public override void _Ready()
	{
		player = TaskTracker.instance.player;
		sprite.Texture = viewport.GetTexture();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (progressBar.Value >= progressBar.MaxValue)
		{
			EmitSignal(SignalName.ProgressBarFinished, this);
		}
		else
        {
            if (player.GlobalPosition.DistanceTo(GlobalPosition) > 3)
            {
				sprite.Modulate = new Color(1, 0, 0);
            }
			else
            {
                sprite.Modulate = new Color(1, 1, 1);
                progressBar.Value += delta * (100 / timeToComplete);
            }
        }
	}
}
