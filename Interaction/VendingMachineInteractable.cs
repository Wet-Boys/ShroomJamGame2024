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
    private Node3D? vendingMachineNode;
    private NpcMovementController npc;

    public override void Interact()
    {
        if (isInteractable)
        {
            npc = NpcMovementController.npcs.PickRandom();
            npc.GoToPositionAndSayWords(vendingMachineNode.GlobalPosition, "Ah, vending machine broke again I see.\nThat's rough buddy..............\nYou know, funny thing I read on the internet a bit ago.\n Apparently for every 1 person on earth, there are 10 vending machines.\nWho knew?");
            npc.ReachedDestination += Npc_ReachedDestination;
            var progressBar = BroadCastHandler.instance.CreateLoadingBarAtLocation(vendingMachineNode, new Vector3(-0.013f, 0.523f, 0.502f), 45);
            progressBar.sprite.Billboard = BaseMaterial3D.BillboardModeEnum.Disabled;
            isInteractable = false;
        }
    }

    private void Npc_ReachedDestination()
    {
        npc.LookAtPosition(TaskTracker.instance.player.GlobalPosition);
        if (npc.needToSay.First().StartsWith("Ah, vending machine"))
        {
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