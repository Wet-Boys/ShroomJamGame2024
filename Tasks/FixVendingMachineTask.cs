using Godot;
using ShroomJamGame.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    public partial class FixVendingMachineTask : BaseTask
    {
        VendingMachineInteractable vendingMachine;

        public override BaseTask Setup(Node worldRoot)
        {
            foreach (var item in Interactable.interactables)
            {
                if (item.GetParent().Name == "VendingMachineToFix")
                {
                    vendingMachine = (VendingMachineInteractable)item;
                    vendingMachine.isInteractable = true;
                    break;
                }
            }
            taskName = "Fix the vending machine";
            return base.Setup(worldRoot);
        }


        public override void PerformTask()
        {
            if (vendingMachine.done)
            {
                EmitSignal(SignalName.TaskFinished, this);
            }
        }
    }
}
