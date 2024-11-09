using Godot;
using ShroomJamGame.Player;

namespace ShroomJamGame.CharacterBody;

[Tool]
[GlobalClass]
public partial class CharacterMovementController : Node
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
    private float _jumpForce = 500f;
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

    private float _eyeOffsetFromTop = 0.25f;
    [Export]
    public float EyeOffsetFromTop
    {
        get => _eyeOffsetFromTop;
        set
        {
            _eyeOffsetFromTop = value;
            UpdateCharacterSize();
        }
    }
    
    [ExportGroup("Node References")]
    [Export]
    private CharacterBody3D? _characterBody;
    [Export]
    private Node3D? _characterHead;
    [Export]
    private CollisionShape3D? _characterCollisionShape;
    [Export]
    private StepHelper? _stepHelper;
    
    private bool _jumpBuffered;
    private double _coyoteJumpTimer;
    
    private Vector3 PlayerForward => _characterBody!.GlobalBasis.X.Cross(Vector3.Up);
    private Vector3 PlayerRight => _characterBody!.GlobalBasis.X;
    private float MoveSpeed => _moveSpeed * (IsCrouching ? _crouchMultiplier : IsSprinting ? _sprintMultiplier : 1);

    [Export]
    public bool IsCrouching;
    [Export]
    public bool IsSprinting;
    [Export]
    public Vector2 InputMovement;

    public override void _PhysicsProcess(double delta)
    {
        if (_characterHead is null || _characterBody is null || _stepHelper is null || Engine.IsEditorHint())
            return;

        var onFloor = _characterBody.IsOnFloor();
        // Reset coyote timer
        if (onFloor)
            _coyoteJumpTimer = _coyoteTime;
        
        // Apply friction
        var friction = onFloor ? _groundFriction : _airFriction;
        _characterBody.Velocity -= new Vector3(_characterBody.Velocity.X, 0, _characterBody.Velocity.Z) * friction * (float)delta;

        // Jump and cancel out existing vertical forces
        if (_jumpBuffered)
        {
            _characterBody.Velocity = new Vector3(_characterBody.Velocity.X, 0, _characterBody.Velocity.Z);
            
            _characterBody.Velocity += Vector3.Up * _jumpForce * (float)delta;
            _jumpBuffered = false;
            _coyoteJumpTimer = 0;
        }
        
        // Apply gravity
        if (!_characterBody.IsOnFloor())
        {
            _characterBody.Velocity += Vector3.Down * Gravity * (float)delta;

            if (_coyoteJumpTimer > 0)
                _coyoteJumpTimer -= delta;
        }
        
        // Add movement forces to velocity
        _characterBody.Velocity += PlayerForward * InputMovement.Y * MoveSpeed * (float)delta;
        _characterBody.Velocity += PlayerRight * InputMovement.X * MoveSpeed * (float)delta;

        // Check if we are colliding with a step
        RotateStepHelper();
        if (HandleStepUp(delta))
            return;
        
        // Execute physics of player body
        _characterBody.MoveAndSlide();
    }

    private void UpdateCharacterSize()
    {
        if (_characterCollisionShape is null || _characterHead is null || _stepHelper is null)
            return;

        if (!Engine.IsEditorHint())
            return;
        
        _characterCollisionShape.Shape = new CylinderShape3D
        {
            Height = _characterHeight,
        };

        _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _characterHeight };
        _characterHead.Position = new Vector3(0, (_characterHeight / 2f) - _eyeOffsetFromTop, 0);
    }
    
    public void StartCrouching()
    {
        IsCrouching = true;
        
        if (_characterCollisionShape is null || _characterHead is null || _characterBody is null || _stepHelper is null)
            return;
        
        _characterCollisionShape.Shape = new CylinderShape3D
        {
            Height = _crouchCharacterHeight,
        };
        
        var heightDiff = _characterHeight - _crouchCharacterHeight;
        _characterCollisionShape.Position = _characterCollisionShape.Position with { Y = -heightDiff / 2f };
        
        
        // BUG: modifying the min clearance causes the player to longer be able to enter doorways
        // _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _crouchCharacterHeight };
        _characterHead.Position = new Vector3(0, (_crouchCharacterHeight / 2f) - (heightDiff / 2f + _eyeOffsetFromTop) , 0);

        if (!_characterBody.IsOnFloor())
            _characterBody.GlobalPosition += new Vector3(0, heightDiff, 0);
    }

    public void StopCrouching()
    {
        IsCrouching = false;
        
        if (_characterCollisionShape is null || _characterHead is null || _characterBody is null || _stepHelper is null)
            return;
        
        _characterCollisionShape.Shape = new CylinderShape3D
        {
            Height = _characterHeight,
        };
        
        _characterCollisionShape.Position = _characterCollisionShape.Position with { Y = 0 };
        
        // _stepHelper.MinClearance = _stepHelper.MinClearance with { Y = _characterHeight };
        _characterHead.Position = new Vector3(0, (_characterHeight / 2f) - _eyeOffsetFromTop, 0);

        if (_characterBody.IsOnFloor())
            return;
        
        // Todo: Uncrouching handling in air
    }

    private void RotateCharacterOnMove()
    {
        if (_characterBody is null)
            return;
        
        
        if (Mathf.Abs(InputMovement.X) < 0.001f && Mathf.Abs(InputMovement.Y) < 0.001f)
            return;
        
        var dir = (PlayerForward * InputMovement.Y + PlayerRight * InputMovement.X).Normalized();
        
        _characterBody.LookAt(_characterBody.GlobalPosition - dir, Vector3.Up);
    }
    
    private void RotateStepHelper()
    {
        if (_characterBody is null || _stepHelper is null)
            return;
        
        var dir = new Vector3(_characterBody.Velocity.X, 0, _characterBody.Velocity.Z).Normalized();
        if (dir == Vector3.Zero)
            return;
        
        _stepHelper.LookAt(_stepHelper.GlobalPosition - dir, Vector3.Up);
    }
    
    private bool HandleStepUp(double delta)
    {
        if (_characterBody is null || _stepHelper is null)
            return false;
        
        // Do a test with the player above its next position and cast downwards.
        var velocity = _characterBody.Velocity * new Vector3(1, 0, 1) * (float)delta;
        var expectedTransform = _characterBody.GlobalTransform.Translated(velocity + new Vector3(0, _stepHelper.MinClearance.Y, 0));

        var result = new KinematicCollision3D();
        if (!_characterBody.TestMove(expectedTransform, new Vector3(0, -_stepHelper.MinClearance.Y, 0), result))
            return false;

        // var target = (CollisionObject3D)result.GetCollider();
        var height = (expectedTransform.Origin + result.GetTravel() - _characterBody.GlobalPosition).Y;

        if (height > _stepHelper.MaxStepHeight)
            return false;

        if (!_stepHelper.IsTouchingValidStep())
            return false;
        
        _characterBody.GlobalPosition = expectedTransform.Origin + result.GetTravel();

        return true;
    }
    
    public void BufferJump()
    {
        if (_characterBody is null)
            return;
        
        var canJump = _characterBody.IsOnFloor() || _coyoteJumpTimer > 0;
        if (!canJump)
            return;
        
        _jumpBuffered = true;
    }
}