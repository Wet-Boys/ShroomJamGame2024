using System;
using Godot;
using ShroomJamGame.Rendering.Utils;

namespace ShroomJamGame.Rendering.Effects;

[Tool]
[GlobalClass]
public partial class CreateScreenColorTextureEffect : BaseCompositorEffect
{
    public static readonly StringName Context = "CreateScreenColorTextureEffect";
    public static readonly StringName SceneColorName = "scene_color";
    
    private static CreateScreenColorTextureEffect? _instance;

    public static CreateScreenColorTextureEffect Instance
    {
        get
        {
            if (_instance is null)
                throw new InvalidOperationException($"{nameof(CreateScreenColorTextureEffect)} instance can't be null.");
            
            return _instance;
        }
    }
    
    private readonly RDAttachmentFormat _fragColorInAttachmentFormat = new()
    {
        Format = RenderingDevice.DataFormat.R16G16B16A16Sfloat,
        Samples = RenderingDevice.TextureSamples.Samples1,
        UsageFlags = (uint)(RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.InputAttachmentBit)
    };
    
    public readonly RDAttachmentFormat SceneColorAttachmentFormat = new()
    {
        Format = RenderingDevice.DataFormat.R16G16B16A16Sfloat,
        Samples = RenderingDevice.TextureSamples.Samples1,
        UsageFlags = (uint)(RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.CanCopyFromBit)
    };
    
    private readonly RDVertexAttribute _vertexAttribute = new()
    {
        Format = RenderingDevice.DataFormat.R32G32B32Sfloat,
        Location = 0,
        Stride = sizeof(float) * 3
    };
    
    private long _frameBufferFormat;
    private long _vertexBufferFormat;
    
    private RDShaderFile? _frameCaptureShaderFile;
    private Rid _frameCaptureShader;
    private Rid _frameCapturePipeline;
    private Rid _sampler;

    public CreateScreenColorTextureEffect()
    {
        EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;

        _texture = new Texture2Drd();

        _instance = this;
    }
    
    [Export]
    private RDShaderFile? FrameCaptureShaderFile
    {
        get => _frameCaptureShaderFile;
        set
        {
            _frameCaptureShaderFile = value;
            if (RenderDevice is not null)
            {
                Destruct();
                Construct();
            }
        }
    }
    
    private Texture2Drd _texture;
    
    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        base._RenderCallback(effectCallbackType, renderData);

        if (RenderDevice is null || _frameCaptureShaderFile is null)
            return;

        var renderSceneBuffers = renderData.GetRenderSceneBuffers();
        if (renderSceneBuffers is not RenderSceneBuffersRD sceneBuffers)
            return;

        var renderSceneData = renderData.GetRenderSceneData();
        if (renderSceneData is not RenderSceneDataRD sceneData)
            return;
        
        var renderSize = sceneBuffers.GetInternalSize();
        if (renderSize is { X: 0, Y: 0 })
            throw new InvalidOperationException("Render size is invalid!");

        if (sceneBuffers.HasTexture(Context, SceneColorName))
        {
            // Delete textures if render size changed
            var textureFormat = sceneBuffers.GetTextureFormat(Context, SceneColorName);
            if (textureFormat.Width != renderSize.X || textureFormat.Height != renderSize.Y)
                sceneBuffers.ClearContext(Context);
        }
        
        var viewCount = sceneBuffers.GetViewCount();

        if (!sceneBuffers.HasTexture(Context, SceneColorName))
        {
            // Create scene colors texture
            sceneBuffers.CreateTexture(
                Context,
                SceneColorName,
                SceneColorAttachmentFormat.Format,
                SceneColorAttachmentFormat.UsageFlags,
                SceneColorAttachmentFormat.Samples,
                renderSize,
                viewCount,
                1,
                true
            );
        }

        var clearColors = new Color[] { new(0, 0, 0, 0) };
        
        var vertices = new Vector3[]
        {
            new(-1, -1, 0),
            new(-1, 3, 0),
            new(3, -1, 0)
        };

        var indices = new uint[]
        {
            0, 1, 2
        };
        
        for (uint view = 0; view < viewCount; view++)
        {
            var (vertexBuffer, vertexArray) = RenderDevice.VertexArrayCreate(vertices, _vertexBufferFormat);
            var (indexBuffer, indexArray) = RenderDevice.IndexArrayCreate(indices);
            
            var targetSceneColors = sceneBuffers.GetTextureSlice(Context, SceneColorName, view, 0, 1, 1);
            var sourceSceneColors = sceneBuffers.GetColorLayer(view);

            var frameCaptureBuffer = RenderDevice.FramebufferCreate([targetSceneColors], _frameBufferFormat);
            if (!frameCaptureBuffer.IsValid)
                throw new InvalidOperationException("Frame capture buffer is invalid!");

            var fragColorTexUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 0,
            };
            fragColorTexUniform.AddId(_sampler);
            fragColorTexUniform.AddId(sourceSceneColors);

            var frameCaptureFragUniforms = UniformSetCacheRD.GetCache(_frameCaptureShader, 0, [fragColorTexUniform]);
            
            RenderDevice.DrawCommandBeginLabel("Frame Capture", new Color(1, 1, 1));
            var drawList = RenderDevice.DrawListBegin(
                frameCaptureBuffer,
                RenderingDevice.InitialAction.Clear,
                RenderingDevice.FinalAction.Store,
                RenderingDevice.InitialAction.Clear,
                RenderingDevice.FinalAction.Store,
                clearColors,
                0
            );
            
            RenderDevice.DrawListBindRenderPipeline(drawList, _frameCapturePipeline);
            RenderDevice.DrawListBindVertexArray(drawList, vertexArray);
            RenderDevice.DrawListBindIndexArray(drawList, indexArray);
            RenderDevice.DrawListBindUniformSet(drawList, frameCaptureFragUniforms, 0);
            RenderDevice.DrawListDraw(drawList, false, 1);
            RenderDevice.DrawListEnd();
            RenderDevice.DrawCommandEndLabel();
            
            RenderDevice.FreeRid(frameCaptureBuffer);
            RenderDevice.FreeRid(vertexArray);
            RenderDevice.FreeRid(vertexBuffer);
            RenderDevice.FreeRid(indexArray);
            RenderDevice.FreeRid(indexBuffer);
            
            _texture.SetTextureRdRid(targetSceneColors);
        }
        
        RenderingServer.GlobalShaderParameterSet("scene_colors", _texture);
    }
    
    protected override void ConstructEffect(RenderingDevice renderDevice)
    {
        _frameBufferFormat = renderDevice.FramebufferFormatCreate([SceneColorAttachmentFormat]);
        _vertexBufferFormat = renderDevice.VertexFormatCreate([_vertexAttribute]);

        if (_frameCaptureShaderFile is null)
            return;

        _frameCaptureShader = renderDevice.ShaderCreateFromSpirV(_frameCaptureShaderFile.GetSpirV());
        if (!_frameCaptureShader.IsValid)
            throw new ArgumentException("Frame Capture Shader is invalid!");

        var rasterState = new RDPipelineRasterizationState();
        
        var multiSampleState = new RDPipelineMultisampleState();

        var depthStencilState = new RDPipelineDepthStencilState();
        
        var blendState = new RDPipelineColorBlendState
        {
            Attachments = [new RDPipelineColorBlendStateAttachment()]
        };

        _frameCapturePipeline = renderDevice.RenderPipelineCreate(
            _frameCaptureShader,
            _frameBufferFormat,
            _vertexBufferFormat,
            RenderingDevice.RenderPrimitive.Triangles,
            rasterState,
            multiSampleState,
            depthStencilState,
            blendState
        );

        if (!_frameCapturePipeline.IsValid)
            throw new ArgumentException("RenderPipeline is invalid!");
        
        var samplerState = new RDSamplerState
        {
            MagFilter = RenderingDevice.SamplerFilter.Nearest,
            MinFilter = RenderingDevice.SamplerFilter.Nearest,
            MipFilter = RenderingDevice.SamplerFilter.Nearest,
            RepeatU = RenderingDevice.SamplerRepeatMode.Repeat,
            RepeatV = RenderingDevice.SamplerRepeatMode.Repeat,
            RepeatW = RenderingDevice.SamplerRepeatMode.Repeat,
            LodBias = 0,
            UseAnisotropy = false,
            AnisotropyMax = 0,
            EnableCompare = false,
            CompareOp = RenderingDevice.CompareOperator.Never,
            MinLod = 0,
            MaxLod = 0,
            BorderColor = RenderingDevice.SamplerBorderColor.FloatTransparentBlack,
            UnnormalizedUvw = false
        };
        
        _sampler = renderDevice.SamplerCreate(samplerState);
        if (!_sampler.IsValid)
            throw new InvalidOperationException("Sampler is invalid!");
    }
    
    protected override void DestructEffect(RenderingDevice renderDevice)
    {
        if (_frameCaptureShader.IsValid)
        {
            renderDevice.FreeRid(_frameCaptureShader);
            _frameCaptureShader = default;
        }
        
        _frameCapturePipeline = default;

        if (_sampler.IsValid)
        {
            renderDevice.FreeRid(_sampler);
            _sampler = default;
        }
    }
}