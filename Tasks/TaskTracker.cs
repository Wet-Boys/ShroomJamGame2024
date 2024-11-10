using Godot;
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
        [Export]
        private CharacterBody3D player;
        double timer = 0;
        public virtual void AddFetchQuest()
        {
            Tasks.Add(new PlaceItemAtPositionTask());
            Tasks[Tasks.Count - 1].Setup(player.GetTree().Root.GetChild(0)).TaskFinished += TaskTracker_TaskFinished;
        }

        private void TaskTracker_TaskFinished(BaseTask task)
        {
            Tasks.Remove(task);
            if (Tasks.Count == 0)
            {
                AddFetchQuest();
            }
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
    }
}
