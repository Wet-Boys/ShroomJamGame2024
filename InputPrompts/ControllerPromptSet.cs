using Godot;
using System.Linq;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class ControllerPromptSet : InputPromptSet
{
    [Export]
    private JoyButtonPromptPair[] _buttonPrompts = [];
    
    [Export]
    private JoyAxisPromptPair[] _axisPrompts = [];
    
    public override Texture2D? GetInputPrompt(InputEvent inputEvent)
    {
        if (inputEvent is InputEventJoypadButton buttonEvent)
        {
            var button = buttonEvent.ButtonIndex;
            var pair = _buttonPrompts.FirstOrDefault(pair => pair.Button == button);
            
            return pair?.PromptTexture;
        }

        if (inputEvent is InputEventJoypadMotion motionEvent)
        {
            var axis = motionEvent.Axis;
            var pair = _axisPrompts.FirstOrDefault(pair => pair.Axis == axis);
            
            return pair?.PromptTexture;
        }
        
        return null;
    }
}