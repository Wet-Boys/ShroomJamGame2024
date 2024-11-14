using Godot;
using ShroomJamGame.Rendering.Effects;

namespace ShroomJamGame.UI;

[GlobalClass]
public partial class GraphicsSettingsController : Node
{
    [Export]
    private OptionButton? _resolutionButton;
    
    [Export]
    private CheckButton? _fullscreenButton;
    
    [Export]
    private CheckButton? _postProcessingButton;

    public override void _Ready()
    {
        if (_fullscreenButton is null || _postProcessingButton is null)
            return;

        var window = GetWindow();
        
        _fullscreenButton.ButtonPressed = window.Mode is Window.ModeEnum.Fullscreen;
        // _resolutionButton.Disabled = _fullscreenButton.ToggleMode;

        _postProcessingButton.ButtonPressed = PosterizationEffect.Instance.Enabled;
    }

    private void OnFullscreenToggled(bool fullscreen)
    {
        // if (_resolutionButton is null)
        //     return;
        //
        // _resolutionButton.ToggleMode = fullscreen;
        
        var window = GetWindow();
        window.Mode = fullscreen ? Window.ModeEnum.Fullscreen : Window.ModeEnum.Windowed;
    }

    private void OnPostProcessingToggled(bool postProcessing)
    {
        PosterizationEffect.Instance.Enabled = postProcessing;
    }
}