using System.Linq;
using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class KbmPromptSet : InputPromptSet
{
    [Export]
    private KeyPromptPair[] _keyPrompts = [];
    
    [Export]
    private KeyRangePromptPair[] _keyRangePrompts = [];
    
    public override Texture2D? GetInputPrompt(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey keyEvent)
        {
            var key = keyEvent.Keycode;
            if (key is Key.None)
                key = keyEvent.PhysicalKeycode;
            
            var keyPrompt = _keyPrompts.FirstOrDefault(pair => pair.Key == key);
            if (keyPrompt is null)
            {
                var keyRange = _keyRangePrompts.FirstOrDefault(pair => pair.KeyInRange(key));
                if (keyRange is not null)
                    return keyRange.GetKeyPrompt(key);
            }

            return keyPrompt?.PromptTexture;
        }
        
        return null;
    }
}