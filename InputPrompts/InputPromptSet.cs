using Godot;
using Godot.Collections;

namespace ShroomJamGame.InputPrompts;

public abstract partial class InputPromptSet : Resource
{
    public abstract Texture2D? GetInputPrompt(InputEvent inputEvent);
}