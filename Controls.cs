using System;
using System.Collections.Generic;
using Godot;

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
    private StringName _moveForward = "player.move_forward";
    [Export]
    private StringName _moveBackward = "player.move_backward";
    [Export]
    private StringName _moveLeft = "player.move_left";
    [Export]
    private StringName _moveRight = "player.move_right";
    [Export]
    private StringName _jump = "player.jump";
    [Export]
    private StringName _crouch = "player.crouch";
    [Export]
    private StringName _sprint = "player.sprint";
    [Export]
    private StringName _interact = "player.interact";
    [Export]
    private StringName _primaryAction = "player.primary_action";
    [Export]
    private StringName _secondaryAction = "player.secondary_action";
    [Export]
    private StringName _pause = "game.pause";

    private Vector2 _movement;

    public Vector2 Movement => _movement;

    public readonly ControlButton Jump = new();
    public readonly ControlButton Crouch = new();
    public readonly ControlButton Sprint = new();
    public readonly ControlButton Interact = new();
    public readonly ControlButton PrimaryAction = new();
    public readonly ControlButton SecondaryAction = new();
    public readonly ControlButton Pause = new();

    public override void _Ready()
    {
        _instance = this;

        Jump.Action = _jump;
        Jump.SetControlsInstance(this);
        
        Crouch.Action = _crouch;
        Crouch.SetControlsInstance(this);
        
        Sprint.Action = _sprint;
        Sprint.SetControlsInstance(this);
        
        Interact.Action = _interact;
        Interact.SetControlsInstance(this);
        
        PrimaryAction.Action = _primaryAction;
        PrimaryAction.SetControlsInstance(this);
        
        SecondaryAction.Action = _secondaryAction;
        SecondaryAction.SetControlsInstance(this);
        
        Pause.Action = _pause;
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
            if (joystickEvent.IsAction(_moveLeft) || joystickEvent.IsAction(_moveRight))
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
            else if (joystickEvent.IsAction(_moveForward) || joystickEvent.IsAction(_moveBackward))
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
            if (keyEvent.IsAction(_moveLeft))
            {
                axis.X += keyEvent.IsPressed() ? -1 : 1;
            }
            else if (keyEvent.IsAction(_moveRight))
            {
                axis.X += keyEvent.IsPressed() ? 1 : -1;
            }
            else if (keyEvent.IsAction(_moveForward))
            {
                axis.Y += keyEvent.IsPressed() ? -1 : 1;
            }
            else if (keyEvent.IsAction(_moveBackward))
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

    public class ControlButton
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

        public bool IsHeld { get; private set; }

        public event Action? OnPress;
        public event Action? OnRelease;
        
        public void ProcessEvent(InputEvent inputEvent)
        {
            if (!inputEvent.IsAction(Action))
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