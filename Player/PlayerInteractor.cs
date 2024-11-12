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
    [Export]
    private RichTextLabel? _tooltipLabel;

    [Export]
    private float _maxPhysicsDistance = 2.5f;

    [Export]
    private float _throwStrength = 25f;

    private IInteractableObject? _currentTarget;
    private PhysicsInteractable? _heldObject;
    private CollisionShape3D? _heldObjectShape;
    [Export]
    private ShapeCast3D? _heldObjectCast;
    private Vector3 _lastObjectPosition;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        
        Controls.Instance.Interact.OnPress += OnInteractPressed;
        Controls.Instance.PrimaryAction.OnPress += OnPrimaryPressed;
        
        EnsureExceptions();
    }

    

    public override void _Notification(int notification)
    {
        if (Engine.IsEditorHint())
            return;

        if (notification == NotificationPredelete)
        {
            Controls.Instance.Interact.OnPress -= OnInteractPressed;
            Controls.Instance.PrimaryAction.OnPress -= OnPrimaryPressed;
        }
    }

    public override void _Process(double delta)
    {
        if (GameState.Paused || Engine.IsEditorHint())
            return;
        
        HandleInteractRayCast();
        UpdateTooltip();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GameState.Paused || Engine.IsEditorHint())
            return;
        
        UpdatePhysicsInteractablePosition(delta);
    }
    
    private void HandleInteractRayCast()
    {
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

    private void UpdatePhysicsInteractablePosition(double delta)
    {
        if (_playerHead is null || _playerBody is null || _interactorRay is null || _stepHelper is null)
            return;

        if (_heldObject is null || _heldObjectShape is null || _heldObjectCast is null)
            return;
        
        _heldObject.AddCollisionExceptionWith(_playerBody);
        _interactorRay.AddException(_heldObject);
        _stepHelper.AddException(_heldObject);

        var dir = _interactorRay.IsColliding()
            ? (_interactorRay.GetCollisionPoint() - _playerHead.GlobalPosition).Normalized()
            : -_playerHead!.GlobalBasis.Z;
        
        var dist = _maxPhysicsDistance;

        if (_heldObjectCast.IsColliding())
        {
            var minDist = float.MaxValue;
            
            for (int i = 0; i < _heldObjectCast.GetCollisionCount(); i++)
            {
                var point = _heldObjectCast.GetCollisionPoint(i);
                minDist = Mathf.Min(minDist, Mathf.Abs(point.DistanceTo(_playerHead.GlobalPosition)));
            }
            
            dist = Mathf.Min(dist, minDist);
        }
        
        var globalPos = _playerHead.GlobalPosition + (dir * dist);
        
        var velocity = globalPos - _lastObjectPosition;
        _lastObjectPosition = _heldObject.GlobalPosition;
        
        _heldObject.LinearVelocity = velocity + _playerBody.Velocity;
        _heldObject.GlobalPosition = globalPos;
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
        
        if (_currentTarget is PhysicsInteractable physicsObject && physicsObject.holdable)
        {
            _heldObject = physicsObject;
            _heldObject.isHeld = true;

            foreach (var child in _heldObject.GetChildren())
            {
                if (child is not CollisionShape3D shape)
                    continue;
                
                _heldObjectShape = shape;
                break;
            }

            _heldObjectCast = new ShapeCast3D
            {
                Shape = _heldObjectShape?.Shape,
                TargetPosition = _interactorRay.TargetPosition
            };
            
            _heldObjectCast.AddException(_playerBody);
            _heldObjectCast.AddException(_heldObject);
            
            AddChild(_heldObjectCast);
            
            return;
        }
        
        _currentTarget?.Interact();
    }
    
    private void OnPrimaryPressed()
    {
        if (_playerHead is null || _playerBody is null || _interactorRay is null || _stepHelper is null)
            return;

        if (_heldObject is null || _heldObjectShape is null || _heldObjectCast is null)
            return;
        
        var dir = _interactorRay.IsColliding()
            ? (_interactorRay.GetCollisionPoint() - _playerHead.GlobalPosition).Normalized()
            : -_playerHead!.GlobalBasis.Z;
        
        _heldObject.ApplyCentralImpulse(dir * _throwStrength);
        DropHeldObject();
    }

    private void DropHeldObject()
    {
        _heldObject?.RemoveCollisionExceptionWith(_playerBody);
        _interactorRay?.RemoveException(_heldObject);
        _stepHelper?.RemoveException(_heldObject);

        if (_heldObjectCast is not null)
        {
            _heldObjectCast.QueueFree();
            _heldObjectCast = null;
        }

        _heldObjectShape = null;
        _heldObject.isHeld = false;
        _heldObject = null;
    }

    private void UpdateTooltip()
    {
        if (_tooltipLabel is null)
            return;
        
        if (_currentTarget is null && _heldObject is null)
        {
            _tooltipLabel.Text = "";
            return;
        }

        if (_currentTarget is not null)
        {
            _tooltipLabel.Text = _currentTarget.GetOnHoverText();
            return;
        }

        if (_heldObject is not null)
        {
            _tooltipLabel.Text = _heldObject.GetHeldHoverText();
        }
    }

    private void EnsureExceptions()
    {
        _interactorRay?.ClearExceptions();
        
        _interactorRay?.AddException(_playerBody);
    }
}