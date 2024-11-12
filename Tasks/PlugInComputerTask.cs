using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class PlugInComputerTask : BaseTask
    {
        public NpcMovementController npc;
        public override BaseTask Setup(Node worldRoot)
        {
            taskName = "Fix the computer";
            return base.Setup(worldRoot);
        }
        public void SetupForReal(NpcMovementController npc)
        {
            this.npc = npc;
            foreach (var item in npc.ownedItems)
            {
                if (item.Name.ToString().StartsWith("Gigabyte"))
                {
                    BroadCastHandler.instance.HighlightObject(item);
                    BroadCastHandler.instance.StopNPCAndSpeak(npc, "Hey, while I've got you here\ncan you look at my computer?\nIt suddenly stopped working after I moved it\nand I'm not sure what's wrong.");
                    npc.targetNode = item;
                    npc.waitAtNextDestination = true;
                }
            }
        }
    }
}
