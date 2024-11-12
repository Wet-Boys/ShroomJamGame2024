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
        [Export]
        public Control HUD;
        Control TaskList;
        double timer = 0;
        Node rootNode;
        Godot.Collections.Dictionary<BaseTask, Control> tasksToControls = new Godot.Collections.Dictionary<BaseTask, Control>();
        public override void _Ready()
        {
            instance = this;
            rootNode = GetTree().Root.GetChild(0);
            BroadCastHandler.instance.CreateFixQuest += BroadCastHandler_CreateFixQuest;
            TaskList = HUD.GetNode<Control>("Task Container");

            DebugCommand();
        }

        public BaseTask CreateTask(BaseTask inputTask)
        {
            Tasks.Add(inputTask);
            inputTask.Setup(rootNode);
            inputTask.TaskFinished += TaskTracker_TaskFinished;
            var thing = ((PackedScene)GD.Load("res://Assets/Prefabs/TaskPopup.tscn")).Instantiate<Panel>();
            thing.GetNode<Label>("TaskName").Text = inputTask.taskName;
            TaskList.AddChild(thing);
            tasksToControls.Add(inputTask, thing);
            return inputTask;
        }

        private async void DebugCommand()
        {
            await ToSignal(rootNode.GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
            CreateTask(new IntroductionTask());
        }

        private void BroadCastHandler_CreateFixQuest()
        {
            CreateTask(new FixObjectTask());
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
                    tasksToControls[task].QueueFree();
                    tasksToControls.Remove(task);
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
