using System;
using Godot;

namespace ShroomJamGame.Corruption;

[GlobalClass]
public partial class AudioCrash : Node
{
    private static AudioCrash? _instance;

    public static AudioCrash Instance
    {
        get
        {
            if (_instance is null)
                throw new InvalidOperationException($"{nameof(AudioCrash)} instance can't be null.");
            
            return _instance;
        }
    }
    
    [Export]
    private string _crashAudioBus = string.Empty;
    
    [Export]
    private AudioStreamPlayer? _streamPlayer;
    
    private AudioEffectRecord? _recordEffect;
    private Timer? _crashTimer;
    
    public override void _Ready()
    {
        _instance = this;
        
        _recordEffect = GetRecordEffect();
        
        _crashTimer = new Timer();
        AddChild(_crashTimer);
        
        _crashTimer.WaitTime = 0.05f;
        _crashTimer.OneShot = true;
        _crashTimer.Timeout += PlayCrashAudio;
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
        {
            if (_crashTimer is null)
                return;

            _crashTimer.Timeout -= PlayCrashAudio;
        }
    }

    public void StartAudioCrash()
    {
        if (_recordEffect is null || _crashTimer is null)
            return;
        
        _recordEffect.SetRecordingActive(true);
        _crashTimer.Start();
    }

    public void StopAudioCrash()
    {
        if (_streamPlayer is null)
            return;
        
        _streamPlayer.Stop();
        SetMuteAudioBuses(false);
    }

    private void PlayCrashAudio()
    {
        if (_recordEffect is null || _streamPlayer is null)
            return;
        
        var sample = _recordEffect.GetRecording();
        sample.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
        sample.LoopBegin = 0;
        sample.LoopEnd = (int)(sample.GetLength() * sample.MixRate);
        
        _recordEffect.SetRecordingActive(false);
        
        SetMuteAudioBuses(true);

        _streamPlayer.Stream = sample;
        _streamPlayer.Play();
    }

    private void SetMuteAudioBuses(bool mute)
    {
        var audioServer = AudioServer.Singleton;
        for (int i = 0; i < audioServer.BusCount; i++)
        {
            var busName = audioServer.GetBusName(i);
            if (busName == "Master" || busName == _crashAudioBus)
                continue;
            
            audioServer.SetBusMute(i, mute);
        }
    }

    private static AudioEffectRecord GetRecordEffect()
    {
        var audioServer = AudioServer.Singleton;
        for (int i = 0; i < audioServer.BusCount; i++)
        {
            var busName = audioServer.GetBusName(i);
            if (busName != "Master")
                continue;

            for (int j = 0; j < audioServer.GetBusEffectCount(i); j++)
            {
                var effect = audioServer.GetBusEffect(i, j);

                if (effect is not AudioEffectRecord effectRecord)
                    continue;
                
                return effectRecord;
            }
        }
        
        throw new InvalidOperationException("No Audio Effect Record Found!");
    }
}