using Godot;

namespace ShroomJamGame.Player;

[Tool]
[GlobalClass]
public partial class PlayerMovementController : Node
{
    [Export]
    private float _moveSpeed = 100f;
    [Export]
    private float _maxSpeed = 1000f;
    [Export]
    private float _sprintMultiplier = 1.75f;
    [Export]
    private float _crouchMultiplier = 0.75f;
    [Export]
    private float _jumpForce = 900f;
    [Export]
    private double _coyoteTime = 0.25f;
    
    [Export]
    public float Gravity = 25f;
    [Export]
    private float _groundFriction = 10f;
    [Export]
    private float _airFriction = 9.5f;

    
    private float _characterHeight = 2f;
    [Export]
    public float CharacterHeight
    {
        get => _characterHeight;
        set
        {
           _characterHeight = value;
           UpdateCharacterSize();
        }
    }
    
    private float _crouchCharacterHeight = 1f;
    [Export]
    public float CrouchCharacterHeight
    {
        get => _crouchCharacterHeight;
        set
        {
            _crouchCharacterHeight = value;
            UpdateCharacterSize();
        }
    }
    
    
    [ExportGroup("Node References")]
    [Export]
    private CharacterBody3D? _playerBody;
    [Export]
    private Node3D? _playerHead;
    [Export]
    private CollisionShape3D? _playerCollisionShape;
    [Export]
    private StepHelper? _stepHelper;
    
    private bool _jumpBuffered;
    private double _coyoteJumpTimer;
    
    private Vector3 PlayerForward => _playerBody!.GlobalBasis.X.Cross(Vector3.Up);
    private Vector3 PlayerRight => _playerBody!.GlobalBasis.X;
    private float MoveSpeed => _moveSpeed * (Controls.Instance.Crouch ? _crouchMultiplier : Controls.Instance.Sprint ? _sprintMultiplier : 1);
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        Controls.Instance.Jump.OnPress += BufferJump;
        Controls.Instance.Crouch.OnPress += OnStartCrouching;
        Controls.Instance.Crouch.OnRelease += OnStopCrouching;
    }

    public override void _Notification(int notification)
    {
        if (Engine.IsEditorHint())
            return;

        if (notification == NotificationPredelete)
        {
            Controls.Instance.Jump.OnPress -= BufferJump;
            Controls.Instance.Crouch.OnPress -= OnStartCrouching;
            Controls.Instance.Crouch.OnRelease -= OnStopCrouching;
        }
    }

    public override void _Process(double delta)
    {
        // RotatePlayerOnMove();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerHead is null || _playerBody is null || _stepHelper is null || Engine.IsEditorHint())
            return;

        var onFloor = _playerBody.IsOnFloor();
        // Reset coyote timer
        if (onFloor)
            _coyoteJumpTimer = _coyoteTime;
        
        // Apply friction
        var friction = onFloor ? _groundFriction : _airFriction;
        _playerBody.Velocity -= new Vector3(_playerBody.Velocity.X, 0, _playerBody.Velocity.Z) * friction * (float)delta;

        // Jump and cancel out existing vertical forces
        if (_jumpBuffered)
        {
            _playerBody.Velocity = new Vector3(_playerBody.Velocity.X, 0, _playerBody.Velocity.Z);
            
            _playerBody.Velocity += Vector3.Up * _jumpForce * (float)delta;
            _jumpBuffered = false;
            _coyoteJumpTimer = 0;
        }
        
        // Apply gravity
        if (!_playerBody.IsOnFloor())
        {
            _playerBody.Velocity += Vector3.Down * Gravity * (float)delta;

            if (_coyoteJumpTimer > 0)
                _coyoteJumpTimer -= delta;
        }
        
        // Add movement forces to velocity
        _playerBody.Velocity += PlayerForward * Controls.Instance.Movement.Y * MoveSpeed * (float)delta;
        _playerBody.Velocity += PlayerRight * Controls.Instance.Movement.X * MoveSpeed * (float)delta;

        // Check if we are colliding with a step
        RotateStepHelper();
        if (HandleStepUp(delta))
            return;
        
        // Execute physics of player body
        _playerBody.MoveAndSlide();
    }

    private void UpdateCharacterSize()
    {
        if (_playerCollisionShape is null || _playerHead is null || _stepHelper is null)
            return;

        if (!Engine.IsEditorHint())
            return;
        
        _playerCollisionShape.Shape = new BoxShape3D
        {
            Size = new Vector3(1f, _characterHeight, 1f)
        };

        _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _characterHeight };
        _playerHead.Position = new Vector3(0, _characterHeight / 2f, 0);
    }
    
    private void OnStartCrouching()
    {
        if (_playerCollisionShape is null || _playerHead is null || _playerBody is null || _stepHelper is null)
            return;
        
        _playerCollisionShape.Shape = new BoxShape3D
        {
            Size = new Vector3(1f, _crouchCharacterHeight, 1f)
        };
        var heightDiff = _characterHeight - _crouchCharacterHeight;
        _playerCollisionShape.Position = _playerCollisionShape.Position with { Y = -heightDiff / 2f };
        
        _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _crouchCharacterHeight };
        _playerHead.Position = new Vector3(0, (_crouchCharacterHeight / 2f) - (heightDiff / 2f) , 0);

        if (!_playerBody.IsOnFloor())
            _playerBody.GlobalPosition += new Vector3(0, heightDiff, 0);
    }

    private void OnStopCrouching()
    {
        if (_playerCollisionShape is null || _playerHead is null || _stepHelper is null)
            return;
        
        _playerCollisionShape.Shape = new BoxShape3D
        {
            Size = new Vector3(1f, _characterHeight, 1f)
        };
        _playerCollisionShape.Position = _playerCollisionShape.Position with { Y = 0 };
        
        _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _characterHeight };
        _playerHead.Position = new Vector3(0, _characterHeight / 2f, 0);
    }

    private void RotatePlayerOnMove()
    {
        if (_playerBody is null)
            return;

        var moveAxis = Controls.Instance.Movement;
        
        if (Mathf.Abs(moveAxis.X) < 0.001f && Mathf.Abs(moveAxis.Y) < 0.001f)
            return;
        
        var dir = (PlayerForward * moveAxis.Y + PlayerRight * moveAxis.X).Normalized();
        
        _playerBody.LookAt(_playerBody.GlobalPosition - dir, Vector3.Up);
    }
    
    private void RotateStepHelper()
    {
        if (_playerBody is null || _stepHelper is null)
            return;
        
        var dir = new Vector3(_playerBody.Velocity.X, 0, _playerBody.Velocity.Z).Normalized();
        if (dir == Vector3.Zero)
            return;
        
        _stepHelper.LookAt(_stepHelper.GlobalPosition - dir, Vector3.Up);
    }
    
    private bool HandleStepUp(double delta)
    {
        if (_playerBody is null || _stepHelper is null)
            return false;
        
        // Do a test with the player above its next position and cast downwards.
        var velocity = _playerBody.Velocity * new Vector3(1, 0, 1) * (float)delta;
        var expectedTransform = _playerBody.GlobalTransform.Translated(velocity + new Vector3(0, _stepHelper.MinClearance.Y, 0));

        var result = new KinematicCollision3D();
        if (!_playerBody.TestMove(expectedTransform, new Vector3(0, -_stepHelper.MinClearance.Y, 0), result))
            return false;

        // var target = (CollisionObject3D)result.GetCollider();
        var height = (expectedTransform.Origin + result.GetTravel() - _playerBody.GlobalPosition).Y;

        if (height > _stepHelper.MaxStepHeight)
            return false;

        if (!_stepHelper.IsTouchingValidStep())
            return false;
        
        _playerBody.GlobalPosition = expectedTransform.Origin + result.GetTravel();

        return true;
    }
    
    private void BufferJump()
    {
        if (_playerBody is null)
            return;
        
        var canJump = _playerBody.IsOnFloor() || _coyoteJumpTimer > 0;
        if (!canJump)
            return;
        
        _jumpBuffered = true;
    }
}