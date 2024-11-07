using Godot;

namespace ShroomJamGame.Animalese;

[GlobalClass]
public partial class AnimaleseDialogTest : Node
{
    [Export(PropertyHint.MultilineText)]
    public string Text = "";

    [Export]
    public AnimalesePlayer3D? AnimalesePlayer;

    [Export]
    public Label3D? TextLabel;

    public override void _Ready()
    {
        PlayNext();
    }

    private void OnCharacterPlayed(string chars)
    {
        if (TextLabel is null)
            return;

        TextLabel.Text += chars;
        
    }

    private void PlayNext()
    {
        if (AnimalesePlayer is null || TextLabel is null)
            return;

        if (string.IsNullOrEmpty(Text))
            return;

        TextLabel.Text = "";
        AnimalesePlayer.PlayText(Text);
    }
}