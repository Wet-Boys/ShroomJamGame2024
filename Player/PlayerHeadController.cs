using Godot;

namespace ShroomJamGame.Player;

[GlobalClass]
public partial class PlayerHeadController : Node
{
    [Export]
    private float _mouseSensitivity = 0.5f;
    [Export(PropertyHint.Range, "65, 90")]
    private float _maxXRotation = 85f;
    [Export(PropertyHint.Range, "-65, -90")]
    private float _minXRotation = -85f;
    
    [Export]
    public bool InvertY = false;
    [Export]
    public bool InvertX = false;
    
    [ExportGroup("Node References")]
    [Export]
    private CharacterBody3D? _playerBody;
    [Export]
    private Node3D? _playerHead;
    
    private float _xRotDegrees;
    private float _yRotDegrees;
    private Vector2 _mouseVelocity;
    
    public override void _Ready()
    {
        // TODO: move cursor capturing into it's own script
        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (_playerBody is null)
            GD.PushWarning("Player Body is null!");
        if (_playerHead is null)
            GD.PushWarning("Player Head is null!");

        if (_playerHead is not null)
        {
            _xRotDegrees = _playerHead.RotationDegrees.X;
            _yRotDegrees = _playerHead.RotationDegrees.Y;
        }

        Controls.Instance.Pause.OnPress += ToggleMouseLock;
    }
    
    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
            Controls.Instance.Pause.OnPress -= ToggleMouseLock;
    }
    
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseMotion mouseMotionEvent)
            return;

        _mouseVelocity += mouseMotionEvent.ScreenRelative;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        LookRotation(delta);
    }

    private void ToggleMouseLock()
    {
        Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
            ? Input.MouseModeEnum.Visible
            : Input.MouseModeEnum.Captured;
    }
    
    private void LookRotation(double delta)
    {
        if (_playerHead is null || _playerBody is null)
            return;
        
        _mouseVelocity /= 10f;
        
        var velX = Mathf.RadToDeg(-_mouseVelocity.X * _mouseSensitivity * (float)delta);
        var velY = Mathf.RadToDeg(_mouseVelocity.Y * _mouseSensitivity * (float)delta);
        
        _mouseVelocity = Vector2.Zero;
        
        _xRotDegrees -= velY * (InvertY ? -1 : 1);
        _xRotDegrees = Mathf.Clamp(_xRotDegrees, _minXRotation, _maxXRotation);

        _yRotDegrees += velX * (InvertX ? -1 : 1);
        
        _playerHead.RotationDegrees = new Vector3(_xRotDegrees, 0, 0);
        _playerBody.RotationDegrees = new Vector3(0, _yRotDegrees, 0);
    }
}