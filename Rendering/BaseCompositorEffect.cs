using System.Diagnostics;
using System.Timers;
using Godot;

namespace ShroomJamGame.Rendering;

public abstract partial class BaseCompositorEffect : CompositorEffect, ISerializationListener
{
    protected RenderingDevice? RenderDevice { get; private set; }
    
    protected Stopwatch Stopwatch { get; private set; }
    // private readonly System.Timers.Timer _timer;
    
    public BaseCompositorEffect()
    {
        Stopwatch = new Stopwatch();
        RenderDevice ??= RenderingServer.GetRenderingDevice();
        Callable.From(Construct).CallDeferred();
        
        // _timer = new System.Timers.Timer();
        // _timer.Interval = 30 * 1000;
        // _timer.AutoReset = true;
        // _timer.Elapsed += ResetStopWatch;
    }

    protected void Construct()
    {
        if (RenderDevice is null)
            return;
        
        // Stopwatch.Restart();
        // _timer.Start();
        
        RenderingServer.CallOnRenderThread(Callable.From(() => ConstructEffect(RenderDevice)));
    }

    protected void Destruct()
    {
        if (RenderDevice is null)
            return;
        
        DestructEffect(RenderDevice);
        
        Stopwatch.Stop();
    }
    
    protected abstract void ConstructEffect(RenderingDevice renderDevice);
    protected abstract void DestructEffect(RenderingDevice renderDevice);
    
    public void OnBeforeSerialize()
    {
        Destruct();
    }

    public void OnAfterDeserialize()
    {
        Construct();
    }
    
    private void ResetStopWatch(object? sender, ElapsedEventArgs e)
    {
        Stopwatch.Restart();
    }
}