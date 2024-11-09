using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class KeyRangePromptPair : BasePromptPair
{
    [Export]
    private Key _startKey;
    
    [Export]
    private Key _endKey;

    [Export]
    private Texture2D[] _keyPrompts = [];

    public bool KeyInRange(Key key) => key >= _startKey && key <= _endKey;

    public Texture2D? GetKeyPrompt(Key key)
    {
        var index = key - _startKey;

        if (index >= _keyPrompts.Length)
            return null;
        
        return _keyPrompts[index];
    }
}