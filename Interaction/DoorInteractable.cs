using Godot;

namespace ShroomJamGame.Interaction;

[Tool]
[GlobalClass]
public partial class DoorInteractable : Interactable
{
	private bool _isOpen;

	[Export]
	public bool IsOpen
	{
		get => _isOpen;
		set
		{
			_isOpen = value;
			UpdateDoorState();
		}
	}
	
	private Vector3 _openDegrees;

	[Export]
	public Vector3 OpenDegrees
	{
		get => _openDegrees;
		set
		{
			_openDegrees = value;
			UpdateDoorState();
		}
	}
	
	private Vector3 _closedDegrees;

	[Export]
	public Vector3 ClosedDegrees
	{
		get => _closedDegrees;
		set
		{
			_closedDegrees = value;
			UpdateDoorState();
		}
	}
	
	[Export]
	private Node3D? _doorNode;

	[Export]
	private AudioStreamPlayer3D? _soundEmitter;
	
	[Export]
	private AudioStream? _openSound;
	[Export]
	private AudioStream? _closeSound;
	
	public override void Interact()
	{
		IsOpen = !IsOpen;
	}

	public override string GetOnHoverText()
	{
		var nextDoorState = _isOpen ? "close" : "open";

		return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to {nextDoorState}";
	}

	private void UpdateDoorState()
	{
		if (_doorNode is null || _soundEmitter is null)
			return;
		
		var rotDegrees = _isOpen ? OpenDegrees : ClosedDegrees;
		
		_doorNode.RotationDegrees = rotDegrees;

		if (Engine.IsEditorHint())
			return;
		
		var sound = _isOpen ? _openSound : _closeSound;
		
		_soundEmitter.Stream = sound;
		_soundEmitter.Play();
	}
}
