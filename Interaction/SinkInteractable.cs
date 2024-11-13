using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using ShroomJamGame.Tasks;
using System.Linq;

namespace ShroomJamGame.Interaction;

[Tool]
[GlobalClass]
public partial class SinkInteractable : Interactable
{
    public bool isPartOfTask = false;
    public WashYoHandsTask task;

    public override void Interact()
    {
        if (isPartOfTask)
        {
            isPartOfTask = false;
            task.FinishTask();
            BroadCastHandler.instance.UnHighlightObject((Node3D)GetParent().GetParent());
        }
    }

    public override string GetOnHoverText()
    {
        if (isPartOfTask)
        {
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to wash yo hands";
        }
        return $"Your hands aren't dirty";
    }
}