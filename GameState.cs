using ShroomJamGame.Player;

namespace ShroomJamGame;

public static class GameState
{
    public static bool Paused => PauseMenuController.GamePaused;
}