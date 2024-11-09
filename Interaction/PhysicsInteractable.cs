using Godot;

namespace ShroomJamGame.Interaction;

[GlobalClass]
public partial class PhysicsInteractable : RigidBody3D, IInteractableObject
{
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