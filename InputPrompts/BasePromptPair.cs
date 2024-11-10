using Godot;

namespace ShroomJamGame.InputPrompts;

public abstract partial class BasePromptPair : Resource
{
    [Export]
    private Texture2D? _promptTexture;

    public virtual Texture2D? PromptTexture => _promptTexture;
}