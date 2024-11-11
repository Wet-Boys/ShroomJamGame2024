using Godot;
using ShroomJamGame.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShroomJamGame.Tasks
{
    [GlobalClass]
    public partial class TaskTracker : Node
    {
        public Godot.Collections.Array<BaseTask> Tasks = new Godot.Collections.Array<BaseTask>();
        public static TaskTracker instance;
        [Export]
        public CharacterBody3D player;
        double timer = 0;
        Node rootNode;

        public override void _Ready()
        {
            instance = this;
            rootNode = GetTree().Root.GetChild(0);
            BroadCastHandler.instance.CreateFixQuest += BroadCastHandler_CreateFixQuest;


            DebugCommand();
        }

        private async void DebugCommand()
        {
            await ToSignal(rootNode.GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
            BroadCastHandler.instance.CreateFixQuestBroadcast();
            Tasks.Add(new FixVendingMachineTask());
            Tasks.Last().Setup(rootNode).TaskFinished += TaskTracker_TaskFinished;
        }

        private void BroadCastHandler_CreateFixQuest()
        {
            Tasks.Add(new FixObjectTask());
            Tasks.Last().Setup(rootNode).TaskFinished += TaskTracker_TaskFinished;
        }

        public override void _Process(double delta)
        {
            timer += delta;
            if (timer > .25)
            {
                timer = 0;
                CheckAllTasks();
            }
        }
        public void CheckAllTasks()
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                Tasks[i].PerformTask();
            }
        }

        private void TaskTracker_TaskFinished(BaseTask task)
        {
            switch (task.taskName)
            {
                default:
                    Tasks.Remove(task);
                    task.TaskFinished -= TaskTracker_TaskFinished;
                    BroadCastHandler.instance.FinishQuestBroadcast(task.taskName);
                    if (Tasks.Count == 0)
                    {
                        BroadCastHandler.instance.CreateFixQuestBroadcast();
                    }
                    break;
            }
        }
    }
}
