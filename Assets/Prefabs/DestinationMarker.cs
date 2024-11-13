using Godot;
using System;

public partial class DestinationMarker : Node3D
{
	float maxHeight = 1;
	float minHeight = 0;
	bool goingUp = true;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (goingUp)
		{
			Position += new Vector3(0, (float)delta,0 );
		}
		else
		{
            Position -= new Vector3(0, (float)delta, 0);
        }
		if (Position.Y > maxHeight)
		{
			goingUp = false;
		}
		else if (Position.Y < minHeight)
		{
			goingUp = true;
		}
		RotateY((float)delta * .33f);
	}
}
