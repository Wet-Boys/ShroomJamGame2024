using Godot;
using Godot.Collections;

namespace ShroomJamGame.Animalese;

[GlobalClass]
public partial class AnimaleseSoundSet : Resource
{
    [Export]
    private Dictionary<string, AudioStream?> _sounds = new()
    {
        { "a", null },
        { "b", null },
        { "c", null },
        { "d", null },
        { "e", null },
        { "f", null },
        { "g", null },
        { "h", null },
        { "i", null },
        { "j", null },
        { "k", null },
        { "l", null },
        { "m", null },
        { "n", null },
        { "o", null },
        { "p", null },
        { "q", null },
        { "r", null },
        { "s", null },
        { "t", null },
        { "u", null },
        { "v", null },
        { "w", null },
        { "x", null },
        { "y", null },
        { "z", null },
        { "th", null },
        { "sh", null },
        { " ", null },
        { ".", null }
    };
    
    public bool ContainsSoundForChars(string chars)
    {
        if (!_sounds.TryGetValue(chars, out var sound))
            return false;
        
        return sound is not null;
    }

    public bool TryGetSoundForChars(string chars, out AudioStream? sound)
    {
        sound = null;
        if (!_sounds.TryGetValue(chars, out sound))
            return false;
        
        return sound is not null;
    }
}