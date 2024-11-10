using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class JoyAxisPromptPair : BasePromptPair
{
    [Export]
    private JoyAxis _axis;
    
    public JoyAxis Axis => _axis;
}