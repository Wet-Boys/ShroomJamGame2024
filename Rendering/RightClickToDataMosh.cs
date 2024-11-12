using Godot;
using ShroomJamGame.Rendering.Effects;

namespace ShroomJamGame.Rendering;

[GlobalClass]
public partial class RightClickToDataMosh : Node
{
    public override void _Ready()
    {
        Controls.Instance.SecondaryAction.OnPress += DoDataMosh;
        Controls.Instance.SecondaryAction.OnRelease += StopDataMosh;
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
        {
            Controls.Instance.SecondaryAction.OnPress -= DoDataMosh;
            Controls.Instance.SecondaryAction.OnRelease -= StopDataMosh;
        }
    }

    private void DoDataMosh()
    {
        FrameCaptureEffect.CaptureNextFrame = true;
        DataMoshEffect.Instance.Enabled = true;
    }

    private void StopDataMosh()
    {
        DataMoshEffect.Instance.Enabled = false;
    }
}