using Godot;

namespace ShroomJamGame.Interaction;

[Tool]
[GlobalClass]
public abstract partial class Interactable : Node, IInteractableObject
{
    private CollisionObject3D? _collisionObject;

    [Export]
    public CollisionObject3D? CollisionShape
    {
        get => _collisionObject;
        set
        {
            _collisionObject?.RemoveMeta("InteractableChildIndex");
            
            _collisionObject = value;

            if (_collisionObject is null)
                return;

            var idx = _collisionObject.GetChildCount();
            
            if (GetParentOrNull<CollisionObject3D>() == _collisionObject)
            {
                for (int i = 0; i < _collisionObject.GetChildCount(); i++)
                {
                    var child = _collisionObject.GetChildOrNull<Interactable>(i);
                    if (child == this)
                    {
                        idx = i;
                        break;
                    }
                }
            }
            
            _collisionObject.SetMeta("InteractableChildIndex", idx);
        }
    }
    

    public override void _Ready()
    {
        if (_collisionObject is null)
        {
            var parent = GetParentOrNull<CollisionObject3D>();
            if (parent is null)
                return;

            CollisionShape = parent;
        }
    }

    public abstract void Interact();

    public abstract string GetOnHoverText();
}