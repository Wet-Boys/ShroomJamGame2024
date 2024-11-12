using Godot;
using ShroomJamGame.NPC;
using System;
using System.Collections.Generic;

namespace ShroomJamGame.Interaction;

[GlobalClass]
public partial class PhysicsInteractable : RigidBody3D, IInteractableObject
{
    public bool isHeld = false;
    public NpcMovementController owner;
    public bool holdable = true;
    public string nonHoldableText = "";
    public Func<PhysicsInteractable, int> interactionFunction = null;
    public ProgressBarRunner progressBar;
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
        if (interactionFunction is not null)
        {
            interactionFunction(this);
        }
    }

    public string GetOnHoverText()
    {
        if (holdable)
        {
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to pick up";
        }
        return nonHoldableText;
    }

    public string GetHeldHoverText()
    {
        return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to drop\nPress `{Controls.Instance.PrimaryAction.GetOsHumanReadableKeyLabel()}` to throw";
    }
}