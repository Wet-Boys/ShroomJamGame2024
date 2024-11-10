using Godot;
using ShroomJamGame.UI;

namespace ShroomJamGame.Player;

[GlobalClass]
public partial class PauseMenuController : Node
{
    public static bool GamePaused { get; private set; }

    [Export]
    private Control? _pauseMenu;
    
    [Export]
    private Control? _settingsMenu;
    
    [Export]
    private SettingsUiController? _settingsUiController;

    public override void _Ready()
    {
        Controls.Instance.Pause.OnPress += TogglePause;
    }
    
    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
            Controls.Instance.Pause.OnPress -= TogglePause;
    }

    public void TogglePause()
    {
        if (_settingsMenu is null || _settingsUiController is null)
            return;
        
        if (GamePaused)
        {
            if (_settingsMenu.Visible)
            {
                _settingsUiController.CloseSettingsPanel();
            }
            else
            {
                Resume();
            }
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        if (_pauseMenu is null)
            return;
        
        GamePaused = true;
        Engine.SetTimeScale(0.0);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        
        _pauseMenu.Visible = true;
        _pauseMenu.MouseFilter = Control.MouseFilterEnum.Stop;
    }

    private void Resume()
    {
        if (_pauseMenu is null)
            return;
        
        GamePaused = false;
        Engine.SetTimeScale(1.0);
        Input.MouseMode = Input.MouseModeEnum.Captured;
        
        _pauseMenu.Visible = false;
        _pauseMenu.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public void ExitToDesktop() => GetTree().Quit();
}