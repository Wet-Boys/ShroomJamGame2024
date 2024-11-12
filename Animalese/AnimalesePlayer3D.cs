using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ShroomJamGame.Animalese;

[GlobalClass]
public partial class AnimalesePlayer3D : AudioStreamPlayer3D
{
    [Export]
    public AnimaleseSoundSet? SoundSet;

    [Export]
    public float PitchMultiplierRange = 0.3f;

    [Export]
    public float InflectionShift = 0.4f;

    [Export]
    public float BasePitch = 3.5f;

    [Signal]
    public delegate void CharactersPlayedEventHandler(string chars);
    
    [Signal]
    public delegate void FinishedPlayingEventHandler();
    
    
    private readonly Queue<SoundSymbol> _remainingSounds = new();
    private readonly RandomNumberGenerator _rng = new();
    public bool isSpeaking = false;

    public void PlayText(string text)
    {
        ParseText(text);
        PlayNextSound();
    }

    private void PlayNextSound()
    {
        if (SoundSet is null)
            return;
        isSpeaking = _remainingSounds.Count > 0;
        if (!isSpeaking)
        {
            EmitSignal(SignalName.FinishedPlaying);
            return;
        }
        
        var nextSound = _remainingSounds.Dequeue();
        EmitSignal(SignalName.CharactersPlayed, nextSound.Chars);

        if (string.IsNullOrEmpty(nextSound.SoundChars))
        {
            PlayNextSound();
            return;
        }

        if (!SoundSet.TryGetSoundForChars(nextSound.SoundChars, out var sound))
            return;
        
        PitchScale = BasePitch + (PitchMultiplierRange * _rng.Randf()) + (nextSound.Inflective ? InflectionShift : 0);
        Stream = sound;
        Play();
    }

    private void ParseText(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            ParseWord(word);
            EnqueueSound(" ", " ");
        }
    }

    private void ParseWord(string word)
    {
        if (SoundSet is null)
            return;
        
        var lastCharInWord = word[^1];
        var isInflective = lastCharInWord == '?';
        var skipNextChar = false;

        for (var i = 0; i < word.Length; i++)
        {
            var character = word.Substring(i, 1);
            
            if (skipNextChar)
            {
                skipNextChar = false;
                continue;
            }

            if (i < word.Length -1)
            {
                var subChars = word.Substring(i, 2);
                if (SoundSet.ContainsSoundForChars(subChars.ToLower()))
                {
                    EnqueueSound(subChars.ToLower(), subChars, isInflective);
                    continue;
                }
            }

            if (SoundSet.ContainsSoundForChars(character.ToLower()))
            {
                EnqueueSound(character.ToLower(), character, isInflective);
                continue;
            }

            EnqueueSound("", character);
        }
    }

    private void EnqueueSound(string sound, string textChars, bool inflective = false)
    {
        _remainingSounds.Enqueue(new SoundSymbol
        {
            SoundChars = sound,
            Chars = textChars,
            Inflective = inflective
        });
    }
    public void StopSpeaking()
    {
        _remainingSounds.Clear();
    }

    private struct SoundSymbol
    {
        public string SoundChars;
        public string Chars;
        public bool Inflective;
    }
}