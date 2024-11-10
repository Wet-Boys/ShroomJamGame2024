using System.Linq;
using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class InputPromptControl : TextureRect
{
    private StringName? _action;

    [Export]
    public StringName? Action
    {
        get => _action;
        set
        {
            _action = value;
            UpdatePromptTexture();
        }
    }

    private InputPromptSet? _promptSet;

    public override void _Ready()
    {
        _promptSet = Controls.Instance.ActivePromptSet;
        
        Controls.Instance.ActivePromptSetChanged += OnActivePromptSetChanged;
        
        UpdatePromptTexture();
    }
    
    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
        {
            Controls.Instance.ActivePromptSetChanged -= OnActivePromptSetChanged;
        }
    }

    private void OnActivePromptSetChanged(InputPromptSet activePromptSet)
    {
        _promptSet = activePromptSet;
        UpdatePromptTexture();
    }

    private void UpdatePromptTexture()
    {
        if (_promptSet is null)
            return;
        
        var events = InputMap.Singleton.ActionGetEvents(Action);

        if (_promptSet is KbmPromptSet)
        {
            var kbmEvent = events.FirstOrDefault(@event => @event is InputEventKey or InputEventMouse);
            if (kbmEvent is null)
            {
                Texture = null;
                return;
            }
            
            Texture = _promptSet.GetInputPrompt(kbmEvent);
            return;
        }
        
        var inputEvent = events.FirstOrDefault(@event => @event is InputEventJoypadButton or InputEventJoypadMotion);
        if (inputEvent is null)
        {
            Texture = null;
            return;
        }
        
        Texture = _promptSet.GetInputPrompt(inputEvent);
    }
}