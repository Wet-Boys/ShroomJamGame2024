using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using ShroomJamGame.Tasks;
using System.Linq;

namespace ShroomJamGame.Interaction;

[Tool]
[GlobalClass]
public partial class VendingMachineInteractable : Interactable
{
    public bool isInteractable = false;
    [Export]
    public Node3D? vendingMachineNode;
    private NpcMovementController npc;
    ProgressBarRunner progressBar;
    public bool done = false;
    Vector3 targetNpcPosition;

    public override void Interact()
    {
        if (isInteractable)
        {
            npc = NpcMovementController.npcs.PickRandom();
            targetNpcPosition = vendingMachineNode.GlobalPosition;
            npc.GoToPositionAndSayWords(targetNpcPosition, "Ah, vending machine broke again I see.\nThat's rough buddy..............\nYou know, funny thing I read on the internet a bit ago.\nApparently for every 1 person on earth, there are 10 vending machines.\nWho knew?");
            npc.ReachedDestination += Npc_ReachedDestination;
            npc.AnimalesePlayer.FinishedPlaying += AnimalesePlayer_FinishedPlaying;
            progressBar = BroadCastHandler.instance.CreateLoadingBarAtLocation(vendingMachineNode, new Vector3(-0.013f, 0.523f, 0.502f), 35);
            progressBar.sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Disabled;
            progressBar.ProgressBarFinished += ProgressBar_ProgressBarFinished;
            isInteractable = false;
            BroadCastHandler.instance.UnHighlightObject(vendingMachineNode);
        }
    }

    private void AnimalesePlayer_FinishedPlaying()
    {
        npc.ReachedDestination -= Npc_ReachedDestination;
        npc.permaWait = false;
        npc.onlyLookAtPlayer = false;
    }

    private void ProgressBar_ProgressBarFinished(Node3D bar)
    {
        progressBar.QueueFree();
        done = true;
        EventHandler.instance.StartSpawningVendingMachinesEvent(20);
    }

    private void Npc_ReachedDestination(bool needToSaySomething, string thing)
    {
        if (needToSaySomething && thing.StartsWith("Ah, vending machine"))
        {
            npc.onlyLookAtPlayer = true;
            npc.ReachedDestination -= Npc_ReachedDestination;
        }
    }

    public override string GetOnHoverText()
    {
        if (isInteractable)
        {
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to fix";
        }
        return "";
    }
}