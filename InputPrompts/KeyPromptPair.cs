using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class KeyPromptPair : BasePromptPair
{
    [Export]
    private Key _key;
    
    public Key Key => _key;
}