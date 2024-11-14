using Godot;

namespace ShroomJamGame.UI;

[GlobalClass]
public partial class AudioSliderController : Node
{
    [Export]
    private string _audioBusName = string.Empty;

    [Export]
    private HSlider? _slider;

    private int _busIndex;

    public override void _Ready()
    {
        if (_slider is null)
            return;
        
        var audioServer = AudioServer.Singleton;
        for (int i = 0; i < audioServer.BusCount; i++)
        {
            var busName = audioServer.GetBusName(i);
            if (busName != _audioBusName)
                continue;
            
            _busIndex = i;
        }

        var currentDb = audioServer.GetBusVolumeDb(_busIndex);
        _slider.Value = Mathf.DbToLinear(currentDb);
    }

    private void OnSliderValueChanged(float value)
    {
        AudioServer.Singleton.SetBusVolumeDb(_busIndex, Mathf.LinearToDb(value));
    }
}