using Godot;
using ShroomJamGame.Events;
using ShroomJamGame.NPC;
using ShroomJamGame.Tasks;
using System.Linq;

namespace ShroomJamGame.Interaction;

[Tool]
[GlobalClass]
public partial class ToiletInteractable : Interactable
{
    public bool isPartOfTask = false;
    [Export]
    private Node3D? toiletNode;
    Node3D mesh;
    public CleanToiletTask task;

    public override void Interact()
    {
        if (isPartOfTask)
        {
            mesh = ((PackedScene)GD.Load("res://Corruption/head_2.tscn")).Instantiate<Node3D>();
            TaskTracker.instance.GetTree().Root.AddChild(mesh);
            mesh.GlobalPosition = toiletNode.GlobalPosition;
            mesh.LookAt(TaskTracker.instance.player.GlobalPosition);
            BroadCastHandler.instance.UnHighlightObject((Node3D)GetParent().GetParent());
            task.FinishTask();
            Test();
        }
        else
        {
            //TODO toilet flush sfx
        }
    }
    public async void Test()
    {
        TaskTracker.instance.currentDay = 1;
        await ToSignal(this.GetTree().CreateTimer(.1f), SceneTreeTimer.SignalName.Timeout);
        TaskTracker.instance.DayFinished();
        await ToSignal(this.GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);
        mesh.QueueFree();
        isPartOfTask = false;
    }
    public override void _Process(double delta)
    {
        if (isPartOfTask && IsInstanceValid(mesh))
        {
            mesh.LookAt(TaskTracker.instance.player.GetNode<Node3D>("Head").GlobalPosition);
            mesh.RotateObjectLocal(new Vector3(0, 1, 0), 160);
            mesh.GlobalPosition = mesh.GlobalPosition.Lerp(TaskTracker.instance.player.GetNode<Node3D>("Head").GlobalPosition, (float)delta * 5);
        }
    }

    public override string GetOnHoverText()
    {
        if (isPartOfTask)
        {
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to unclog the toilet";
        }
        return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to flush";
    }
}