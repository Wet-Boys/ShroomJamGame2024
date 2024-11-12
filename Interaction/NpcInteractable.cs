using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using ShroomJamGame.Tasks;
using System.Linq;

namespace ShroomJamGame.Interaction;

[GlobalClass]
public partial class NpcInteractable : Interactable
{
    public bool isInteractable = false;
    [Export]
    private NpcMovementController npcController;
    [Signal]
    public delegate void InteractedWithEventHandler(NpcMovementController itsme);

    Godot.Collections.Array<string> introductions = new Godot.Collections.Array<string>() { "Oh so you're the new hire, welcome!", "Skibidi toilet to you my fellow zoomer.", "Hey", "Hello Mario.", "Hello chat", ":Stare:" };
    public override void Interact()
    {
        if (isInteractable && !npcController.IsDoingStuff())
        {
            isInteractable = false;
            npcController.onlyLookAtPlayer = true;
            npcController.GoToPositionAndSayWords(npcController.characterBody3D.GlobalPosition, introductions.PickRandom());
            EmitSignal(SignalName.InteractedWith, npcController);
            npcController.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
        }
    }

    private void AnimalesePlayer_FinishedPlaying()
    {
        npcController.onlyLookAtPlayer = false;
    }

    public override string GetOnHoverText()
    {
        if (isInteractable && !npcController.IsDoingStuff())
        {
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to introduce yourself";
        }
        return "";
    }
}