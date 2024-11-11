using Godot;
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
    public override void _Ready()
	{
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
            progressBar.Value += delta * (100 / timeToComplete);
        }
	}
}
