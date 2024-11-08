using Godot;
using ShroomJamGame.CharacterBody;

namespace ShroomJamGame.Player;

public partial class PlayerMovementController : CharacterMovementController
{
    protected override bool IsCrouching => Controls.Instance.Crouch;
    protected override bool IsSprinting => Controls.Instance.Sprint;
    protected override Vector2 GetMovementInput() => Controls.Instance.Movement;
    
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
}