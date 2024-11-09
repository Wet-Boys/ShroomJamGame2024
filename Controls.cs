using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ShroomJamGame.Player;

namespace ShroomJamGame;

[GlobalClass]
public partial class Controls : Node
{
    private static Controls? _instance;
    
    public static Controls Instance
    {
        get
        {
            if (_instance is null)
                throw new NullReferenceException("Controls was accessed before it was initialized! This usually means it was accessed from an Editor script.");
            return _instance;
        }
    }
    
    private readonly List<ControlButton> _buttons = [];
    
    [Export]
    public float JoystickDeadzone = 0.1f;
    
    [ExportGroup("Events")]
    [Export]
    public StringName MoveForwardName = "player.move_forward";
    [Export]
    public StringName MoveBackwardName = "player.move_backward";
    [Export]
    public StringName MoveLeftName = "player.move_left";
    [Export]
    public StringName MoveRightName = "player.move_right";
    [Export]
    public StringName JumpName = "player.jump";
    [Export]
    public StringName CrouchName = "player.crouch";
    [Export]
    public StringName SprintName = "player.sprint";
    [Export]
    public StringName InteractName = "player.interact";
    [Export]
    public StringName PrimaryActionName = "player.primary_action";
    [Export]
    public StringName SecondaryActionName = "player.secondary_action";
    [Export]
    public StringName PauseName = "game.pause";

    private Vector2 _movement;

    public Vector2 Movement => _movement;

    public readonly ControlButton Jump = new();
    public readonly ControlButton Crouch = new();
    public readonly ControlButton Sprint = new();
    public readonly ControlButton Interact = new();
    public readonly ControlButton PrimaryAction = new();
    public readonly ControlButton SecondaryAction = new();
    public readonly ControlButton Pause = new(true);

    public override void _Ready()
    {
        _instance = this;

        Jump.Action = JumpName;
        Jump.SetControlsInstance(this);
        
        Crouch.Action = CrouchName;
        Crouch.SetControlsInstance(this);
        
        Sprint.Action = SprintName;
        Sprint.SetControlsInstance(this);
        
        Interact.Action = InteractName;
        Interact.SetControlsInstance(this);
        
        PrimaryAction.Action = PrimaryActionName;
        PrimaryAction.SetControlsInstance(this);
        
        SecondaryAction.Action = SecondaryActionName;
        SecondaryAction.SetControlsInstance(this);
        
        Pause.Action = PauseName;
        Pause.SetControlsInstance(this);
    }

    public override void _Input(InputEvent inputEvent)
    {
        HandleMovementAxisState(inputEvent);
        HandleButtonActions(inputEvent);
    }

    private void HandleMovementAxisState(InputEvent inputEvent)
    {
        var axis = Vector2.Zero;
        
        if (inputEvent is InputEventJoypadMotion joystickEvent)
        {
            if (joystickEvent.IsAction(MoveLeftName) || joystickEvent.IsAction(MoveRightName))
            {
                if (Mathf.Abs(joystickEvent.AxisValue) > JoystickDeadzone)
                {
                    axis.X = joystickEvent.AxisValue;
                }
                else
                {
                    axis.X = 0;
                }
            }
            else if (joystickEvent.IsAction(MoveForwardName) || joystickEvent.IsAction(MoveBackwardName))
            {
                if (Mathf.Abs(joystickEvent.AxisValue) > JoystickDeadzone)
                {
                    axis.Y = joystickEvent.AxisValue;
                }
                else
                {
                    axis.Y = 0;
                }
            }
        }
        else if (inputEvent is InputEventKey keyEvent)
        {
            if (keyEvent.IsAction(MoveLeftName))
            {
                axis.X += keyEvent.IsPressed() ? -1 : 1;
            }
            else if (keyEvent.IsAction(MoveRightName))
            {
                axis.X += keyEvent.IsPressed() ? 1 : -1;
            }
            else if (keyEvent.IsAction(MoveForwardName))
            {
                axis.Y += keyEvent.IsPressed() ? -1 : 1;
            }
            else if (keyEvent.IsAction(MoveBackwardName))
            {
                axis.Y += keyEvent.IsPressed() ? 1 : -1;
            }
        }
        
        _movement += axis;
        _movement.X = Mathf.Clamp(_movement.X, -1, 1);
        _movement.Y = Mathf.Clamp(_movement.Y, -1, 1);
    }

    private void HandleButtonActions(InputEvent inputEvent)
    {
        foreach (var controlButton in _buttons)
            controlButton.ProcessEvent(inputEvent);
    }

    public class ControlButton(bool unscaled = false)
    {
        public StringName Action = "";

        public void SetControlsInstance(Controls instance)
        {
            instance._buttons.Add(this);
        }

        ~ControlButton()
        {
            Instance._buttons.Remove(this);
        }

        public string GetOsHumanReadableKeyLabel()
        {
            var events = InputMap.Singleton.ActionGetEvents(Action);

            foreach (var inputEvent in events)
            {
                if (inputEvent is InputEventKey keyEvent)
                {
                    var keycode = keyEvent.Keycode;

                    if (keycode == Key.None)
                        keycode = keyEvent.PhysicalKeycode;

                    return OS.GetKeycodeString(keycode);
                }
                
                if (inputEvent is InputEventMouseButton mouseEvent)
                {
                    return mouseEvent.ButtonIndex.ToString();
                }
            }

            return "";
        }

        public bool IsHeld { get; private set; }

        public event Action? OnPress;
        public event Action? OnRelease;
        
        public void ProcessEvent(InputEvent inputEvent)
        {
            if (!inputEvent.IsAction(Action))
                return;

            if (GameState.Paused && !unscaled)
                return;

            if (inputEvent.IsPressed())
            {
                IsHeld = true;
                OnPress?.Invoke();
            }

            if (inputEvent.IsReleased())
            {
                IsHeld = false;
                OnRelease?.Invoke();
            }
        }

        public static implicit operator bool(ControlButton controlButton) => controlButton.IsHeld;
    }
}