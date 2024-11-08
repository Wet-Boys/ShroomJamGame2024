using Godot;
using ShroomJamGame.Interaction;

namespace ShroomJamGame.Player;

[Tool]
[GlobalClass]
public partial class PlayerInteractor : Node3D
{
    private float _maxInteractDistance = 4f;

    [Export]
    public float MaxInteractDistance
    {
        get => _maxInteractDistance;
        set
        {
            _maxInteractDistance = value;
            if (_interactorRay is null)
                return;
            
            _interactorRay.TargetPosition = new Vector3(0, 0, -_maxInteractDistance);
        }
    }
    
    private CharacterBody3D? _playerBody;

    [Export]
    public CharacterBody3D? PlayerBody
    {
        get => _playerBody;
        set
        {
            _playerBody = value;
            EnsureExceptions();
        }
    }
    
    private RayCast3D? _interactorRay;

    [Export]
    public RayCast3D? InteractorRay
    {
        get => _interactorRay;
        set
        {
            _interactorRay = value;
            EnsureExceptions();
        }
    }

    [Export]
    private Node3D? _playerHead;
    [Export]
    private StepHelper? _stepHelper;

    private IInteractableObject? _currentTarget;
    private PhysicsInteractable? _heldObject;
    private Vector3 _lastObjectPosition;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        Controls.Instance.Interact.OnPress += OnInteractPressed;
        
        EnsureExceptions();
    }
    
    public override void _Notification(int notification)
    {
        if (Engine.IsEditorHint())
            return;

        if (notification == NotificationPredelete)
        {
            Controls.Instance.Interact.OnPress -= OnInteractPressed;
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        _currentTarget = null;
        
        var hit = _interactorRay?.GetCollider();
        if (hit is not CollisionObject3D targetCollision)
            return;

        if (targetCollision is PhysicsInteractable physicsInteractable)
        {
            _currentTarget = physicsInteractable;
            return;
        }

        if (!targetCollision.HasMeta("InteractableChildIndex"))
            return;
        
        var interactableIndex = targetCollision.GetMeta("InteractableChildIndex");
        
        _currentTarget = targetCollision.GetChildOrNull<Interactable>(interactableIndex.AsInt32());
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;
        
        UpdatePhysicsInteractablePosition(delta);
    }

    private void UpdatePhysicsInteractablePosition(double delta)
    {
        if (_playerHead is null || _playerBody is null || _interactorRay is null || _stepHelper is null)
            return;

        if (_heldObject is null)
            return;
        
        _heldObject.AddCollisionExceptionWith(_playerBody);
        _interactorRay.AddException(_heldObject);
        _stepHelper.AddException(_heldObject);

        var globalPos = _interactorRay.IsColliding()
            ? _interactorRay.GetCollisionPoint()
            : _playerHead.GlobalPosition + (-_playerHead!.GlobalBasis.Z * Mathf.Abs(_interactorRay.TargetPosition.DistanceTo(Vector3.Zero)));
        
        var velocity = globalPos - _lastObjectPosition;
        _lastObjectPosition = _heldObject.GlobalPosition;

        _heldObject.GlobalPosition = globalPos;
        _heldObject.LinearVelocity = velocity + _playerBody.Velocity;
    }
    
    private void OnInteractPressed()
    {
        if (_playerHead is null || _interactorRay is null)
            return;

        if (_heldObject is not null)
        {
            DropHeldObject();
            return;
        }
        
        if (_currentTarget is PhysicsInteractable physicsObject)
        {
            _heldObject = physicsObject;
            return;
        }
        
        _currentTarget?.Interact();
    }

    private void DropHeldObject()
    {
        _heldObject?.RemoveCollisionExceptionWith(_playerBody);
        _interactorRay?.RemoveException(_heldObject);
        _stepHelper?.RemoveException(_heldObject);
        
        _heldObject = null;
    }
    

    private void EnsureExceptions()
    {
        _interactorRay?.ClearExceptions();
        
        _interactorRay?.AddException(_playerBody);
    }
}