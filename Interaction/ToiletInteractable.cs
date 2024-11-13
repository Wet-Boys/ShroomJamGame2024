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
    [Export]
    private AudioStreamPlayer3D? _toiletPlayer;
    
    public bool isPartOfTask = false;
    [Export]
    private Node3D? toiletNode;
    Node3D mesh;
    public CleanToiletTask task;
    bool moveMesh = false;
    float speed = 4f;

    public override void Interact()
    {
        _toiletPlayer?.Play();
        
        if (isPartOfTask)
        {
            moveMesh = true;
            mesh.GlobalPosition = toiletNode.GlobalPosition;
            mesh.LookAt(TaskTracker.instance.player.GlobalPosition);
            BroadCastHandler.instance.UnHighlightObject((Node3D)GetParent().GetParent());
            task.FinishTask();
            isPartOfTask = false;
            TaskTracker.instance.currentDay = 1;
        }
    }
    public async void Test()
    {
        await ToSignal(this.GetTree().CreateTimer(2f), SceneTreeTimer.SignalName.Timeout);
        mesh.QueueFree();
    }
    public override void _Process(double delta)
    {
        if (IsInstanceValid(mesh) && moveMesh)
        {
            mesh.LookAt(TaskTracker.instance.player.GetNode<Node3D>("Head").GlobalPosition);
            mesh.RotateObjectLocal(new Vector3(0, 1, 0), 160);
            mesh.GlobalPosition = mesh.GlobalPosition.Lerp(TaskTracker.instance.player.GetNode<Node3D>("Head").GlobalPosition, (float)delta * speed);
            speed += (float)delta * 3;
            if (mesh.GlobalPosition.DistanceTo(TaskTracker.instance.player.GetNode<Node3D>("Head").GlobalPosition) < .1f)
            {
                TaskTracker.instance.DayFinished();
                mesh.Reparent(TaskTracker.instance.player.GetNode<Node3D>("Head"), true);
                moveMesh = false;
                Test();
            }
        }
    }

    public override string GetOnHoverText()
    {
        if (isPartOfTask)
        {
            if (!IsInstanceValid(mesh))
            {

                mesh = ((PackedScene)GD.Load("res://Corruption/head_2.tscn")).Instantiate<Node3D>();
                TaskTracker.instance.GetTree().Root.AddChild(mesh);
                mesh.GlobalPosition = new Vector3(-68, -68, -68);
            }
            return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to unclog the toilet";
        }
        return $"Press `{Controls.Instance.Interact.GetOsHumanReadableKeyLabel()}` to flush";
    }
}