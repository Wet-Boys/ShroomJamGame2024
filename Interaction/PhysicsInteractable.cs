using Godot;
using ShroomJamGame.NPC;

namespace ShroomJamGame.Interaction;

[GlobalClass]
public partial class PhysicsInteractable : RigidBody3D, IInteractableObject
{
    public bool isHeld = false;
    public NpcMovementController owner;
    public static Godot.Collections.Array<PhysicsInteractable> physicsObjects = new Godot.Collections.Array<PhysicsInteractable>();
    public override void _Ready()
    {
        base._Ready();
        physicsObjects.Add(this);
    }
    public override void _ExitTree()
    {
        physicsObjects.Remove(this);
        base._ExitTree();
    }
    public void Interact()
    {
        
    }

    public string GetOnHoverText()
    {
        return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to pick up";
    }

    public string GetHeldHoverText()
    {
        return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to drop\nPress `{Controls.Instance.PrimaryAction.GetOsHumanReadableKeyLabel()}` to throw";
    }
}