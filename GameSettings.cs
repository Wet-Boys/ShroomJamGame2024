using System;
using Godot;
using ShroomJamGame.InputPrompts;

namespace ShroomJamGame;

[GlobalClass]
public partial class GameSettings : Node
{
    private static GameSettings? _instance;
    
    public static GameSettings Instance
    {
        get
        {
            if (_instance is null)
                throw new NullReferenceException("GameSettings was accessed before it was initialized! This usually means it was accessed from an Editor script.");
            return _instance;
        }
    }

    public override void _Ready()
    {
        _instance = this;
    }
    
    [Export]
    public InputPromptSet KbmPromptSet;
    
    private ControllerPromptType _controllerPromptType;

    [Export]
    public ControllerPromptType ControllerPromptType
    {
        get => _controllerPromptType;
        set
        {
            _controllerPromptType = value;
            EmitSignal(SignalName.ControllerPromptTypeUpdated, Variant.From(ControllerPromptType));
        }
    }

    [Signal]
    public delegate void ControllerPromptTypeUpdatedEventHandler(ControllerPromptType controllerPromptType);

    [ExportGroup("Input Prompts")]
    [Export]
    private InputPromptSet _xbox;

    [Export]
    private InputPromptSet _dualsense;

    public InputPromptSet GetControllerPromptSet()
    {
        return ControllerPromptType switch
        {
            ControllerPromptType.XboxOne => _xbox,
            ControllerPromptType.DualSense => _dualsense,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}