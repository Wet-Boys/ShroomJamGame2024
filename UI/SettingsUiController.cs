using Godot;

namespace ShroomJamGame.UI;

[GlobalClass]
public partial class SettingsUiController : Node
{
    [Export]
    private Control? _settingsUi;
    
    [Export]
    private Control? _pauseUi;
    
    [Export]
    private PanelContainer? _audioPanel;
    
    [Export]
    private PanelContainer? _graphicsPanel;

    private void OnTabChanged(int tabIndex)
    {
        if (tabIndex == 0)
        {
            ShowPanel(_audioPanel);
        }
        else if (tabIndex == 1)
        {
            ShowPanel(_graphicsPanel);
        }
    }

    private void ShowPanel(PanelContainer? panel)
    {
        if (_audioPanel is null || _graphicsPanel is null || panel is null)
            return;

        _audioPanel.Visible = false;
        _graphicsPanel.Visible = false;
        
        panel.Visible = true;
    }

    public void ShowSettingsPanel()
    {
        if (_settingsUi is null || _pauseUi is null)
            return;

        _pauseUi.Visible = false;
        _settingsUi.Visible = true;
    }

    public void CloseSettingsPanel()
    {
        if (_settingsUi is null || _pauseUi is null)
            return;
        
        _settingsUi.Visible = false;
        _pauseUi.Visible = true;
    }
}