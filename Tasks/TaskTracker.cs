using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.Interaction;
using ShroomJamGame.NPC;
using ShroomJamGame.Rendering.Effects;
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
        public Vector3 originalPlayerPos;
        [Export]
        public Control HUD;
        Control TaskList;
        double timer = 0;
        Node rootNode;
        Godot.Collections.Dictionary<BaseTask, Control> tasksToControls = new Godot.Collections.Dictionary<BaseTask, Control>();
        Godot.Collections.Dictionary<BaseTask, Label> taskNames = new Godot.Collections.Dictionary<BaseTask, Label>();
        public override void _Ready()
        {
            instance = this;
            originalPlayerPos = player.GlobalPosition;
            rootNode = GetTree().Root.GetChild(0);
            TaskList = HUD.GetNode<Control>("Task Container");

            DebugCommand();
            BroadCastHandler.instance.Day1Finished += DayFinished;
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
            taskNames.Add(inputTask, thing.GetNode<Label>("TaskName"));
            return inputTask;
        }

        private async void DebugCommand()
        {
            await ToSignal(rootNode.GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
            CreateTask(new FixChairTask());//RESET THIS TO WHATEVER STARTING POINT YOU WANT
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
            if (lowerBlend && DataMoshEffect.Instance.Blend > 0)
            {
                DataMoshEffect.Instance.Blend -= (float)delta * .2f;
            }
        }
        public void CheckAllTasks()
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                Tasks[i].PerformTask();
            }
            foreach (var item in taskNames.Keys)
            {
                taskNames[item].Text = item.taskName;
            }
        }
        private void CleanupTask(BaseTask task)
        {
            Tasks.Remove(task);
            task.TaskFinished -= TaskTracker_TaskFinished;
            tasksToControls[task].QueueFree();
            tasksToControls.Remove(task);
            taskNames.Remove(task);
            BroadCastHandler.instance.FinishQuestBroadcast(task.taskName);
        }
        private void TaskTracker_TaskFinished(BaseTask task)
        {
            CleanupTask(task);
        }

        public void DayFinished()
        {
            currentDay++;
            switch (currentDay)
            {
                case 1:
                    LoadNewScene(new Vector3(-0.174f, 7.8f, -2.107f));
                    break;
                case 2:
                    LoadNewScene(new Vector3(12.936f, 6.581f, 15.398f));
                    break;
                case 3:
                    LoadNewScene(new Vector3(-2.614f, 6.581f, 22.441f));
                    break;
                default:
                    break;
            }
        }
        bool lowerBlend = false;
        public int currentDay = 0;
        private async void LoadNewScene(Vector3 playerPosition)
        {
            FrameCaptureEffect.CaptureNextFrame = true;
            DataMoshEffect.Instance.Enabled = true;
            lowerBlend = false;
            DataMoshEffect.Instance.Blend = 1;
            await ToSignal(this.GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
            DataMoshEffect.Instance.EnableMoshingWithVelocity = false;
            player.GlobalPosition = new Vector3(playerPosition.X, player.GlobalPosition.Y, playerPosition.Z);
            foreach (var item in NpcMovementController.npcs)
            {
                item.characterBody3D.GlobalPosition = item.originalPosition;
                item.SayWords("");
            }

            await ToSignal(this.GetTree().CreateTimer(.1f), SceneTreeTimer.SignalName.Timeout);
            DataMoshEffect.Instance.EnableMoshingWithVelocity = true;
            await ToSignal(this.GetTree().CreateTimer(2f), SceneTreeTimer.SignalName.Timeout);
            lowerBlend = true;
            await ToSignal(this.GetTree().CreateTimer(5f), SceneTreeTimer.SignalName.Timeout);
            foreach (var npc in NpcMovementController.npcs)
            {
                for (int i = 0; i < npc.ownedItems.Count; i++)
                {
                    PhysicsInteractable realItem = npc.ownedItems[i] as PhysicsInteractable;
                    realItem.Freeze = false;
                    realItem.GlobalPosition = npc.ownedItemsStartingPositions[i];
                    realItem.Rotation = npc.ownedItemsStartingRotations[i];
                }
            }
            lowerBlend = false;
            DataMoshEffect.Instance.Enabled = false;
            switch (currentDay)
            {
                case 1:
                    TaskTracker.instance.CreateTask(new BossToiletTask());
                    break;
                case 2:
                    TaskTracker.instance.CreateTask(new hrTask());
                    break;
                case 3:
                    TaskTracker.instance.CreateTask(new FixManyComputers());
                    break;
                default:
                    break;
            }
        }
    }
}
