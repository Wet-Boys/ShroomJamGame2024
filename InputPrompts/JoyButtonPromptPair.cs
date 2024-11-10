using Godot;

namespace ShroomJamGame.InputPrompts;

[GlobalClass]
public partial class JoyButtonPromptPair : BasePromptPair
{
    [Export]
    private JoyButton _button;
    
    public JoyButton Button => _button;
}