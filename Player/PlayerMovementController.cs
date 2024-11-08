using Godot;
using ShroomJamGame.CharacterBody;

namespace ShroomJamGame.Player;

[Tool]
[GlobalClass]
public partial class PlayerMovementController : Node
{
    [Export]
    private CharacterMovementController? _characterMovementController;
    
    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;

        if (_characterMovementController is null)
            return;
        
        Controls.Instance.Jump.OnPress += _characterMovementController.BufferJump;
        Controls.Instance.Crouch.OnPress += _characterMovementController.StartCrouching;
        Controls.Instance.Crouch.OnRelease += _characterMovementController.StopCrouching;
    }

    public override void _Notification(int notification)
    {
        if (Engine.IsEditorHint())
            return;
        
        if (_characterMovementController is null)
            return;

        if (notification == NotificationPredelete)
        {
            Controls.Instance.Jump.OnPress -= _characterMovementController.BufferJump;
            Controls.Instance.Crouch.OnPress -= _characterMovementController.StartCrouching;
            Controls.Instance.Crouch.OnRelease -= _characterMovementController.StopCrouching;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;
        
        if (_characterMovementController is null)
            return;

        _characterMovementController.IsSprinting = Controls.Instance.Sprint;
        _characterMovementController.InputMovement = Controls.Instance.Movement;
    }
}