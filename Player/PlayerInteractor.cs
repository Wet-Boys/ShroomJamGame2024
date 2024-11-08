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
    
    private CollisionObject3D? _playerBodyCollision;

    [Export]
    public CollisionObject3D? PlayerBodyCollision
    {
        get => _playerBodyCollision;
        set
        {
            _playerBodyCollision = value;
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

    private Interactable? _currentTarget;

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
        _currentTarget = null;
        
        if (Engine.IsEditorHint())
            return;
        
        if (_interactorRay is null)
            return;

        var hit = _interactorRay.GetCollider();

        if (hit is not CollisionObject3D targetCollision)
            return;

        if (!targetCollision.HasMeta("InteractableChildIndex"))
            return;
        
        var interactableIndex = targetCollision.GetMeta("InteractableChildIndex");
        
        _currentTarget = targetCollision.GetChildOrNull<Interactable>(interactableIndex.AsInt32());
    }
    
    private void OnInteractPressed()
    {
        if (_currentTarget is null)
            return;
        
        _currentTarget.Interact();
    }

    private void EnsureExceptions()
    {
        _interactorRay?.ClearExceptions();
        
        _interactorRay?.AddException(_playerBodyCollision);
    }
}