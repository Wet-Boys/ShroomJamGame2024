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
    
    [Export]
    private PanelContainer? _controlsPanel;
    
    

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
        else if (tabIndex == 2)
        {
            ShowPanel(_controlsPanel);
        }
    }

    private void ShowPanel(PanelContainer? panel)
    {
        if (_audioPanel is null || _graphicsPanel is null || _controlsPanel is null || panel is null)
            return;

        _audioPanel.Visible = false;
        _graphicsPanel.Visible = false;
        _controlsPanel.Visible = false;
        
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