using System;
using Godot;
using ShroomJamGame.Rendering.Utils;

namespace ShroomJamGame.Rendering.Effects;

[Tool]
[GlobalClass]
public partial class DataMoshEffect : BaseCompositorEffect
{
    public static readonly StringName Context = "DataMoshEffect";
    public static readonly StringName FragColorTexName = "composite";

    private static DataMoshEffect? _instance;

    public static DataMoshEffect Instance
    {
        get
        {
            if (_instance is null)
                throw new InvalidOperationException($"{nameof(DataMoshEffect)} instance can't be null.");
            
            return _instance;
        }
    }
    
    private readonly RDAttachmentFormat _fragColorAttachmentFormat = new()
    {
        Format = RenderingDevice.DataFormat.R16G16B16A16Sfloat,
        Samples = RenderingDevice.TextureSamples.Samples1,
        UsageFlags = (uint)(RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.InputAttachmentBit)
    };
    
    private readonly RDAttachmentFormat _frameBufferAttachmentFormat = new()
    {
        Format = RenderingDevice.DataFormat.R16G16B16A16Sfloat,
        Samples = RenderingDevice.TextureSamples.Samples1,
        UsageFlags = (uint)(RenderingDevice.TextureUsageBits.SamplingBit | RenderingDevice.TextureUsageBits.StorageBit | RenderingDevice.TextureUsageBits.ColorAttachmentBit | RenderingDevice.TextureUsageBits.InputAttachmentBit)
    };
    
    private readonly RDVertexAttribute _vertexAttribute = new()
    {
        Format = RenderingDevice.DataFormat.R32G32B32Sfloat,
        Location = 0,
        Stride = sizeof(float) * 3
    };
    
    private long _frameBufferFormat;
    private long _vertexBufferFormat;
    
    private RDShaderFile? _dataMoshShaderFile;
    private Rid _dataMoshPipeline;
    private Rid _dataMoshShader;
    private Rid _sampler;

    public DataMoshEffect()
    {
        EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
        NeedsMotionVectors = true;

        _instance = this;
    }
    
    [Export]
    private RDShaderFile? DataMoshShaderFile
    {
        get => _dataMoshShaderFile;
        set
        {
            _dataMoshShaderFile = value;
            if (RenderDevice is not null)
            {
                Destruct();
                Construct();
            }
        }
    }
    
    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        base._RenderCallback(effectCallbackType, renderData);

        if (RenderDevice is null || _dataMoshShaderFile is null)
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
        
        var xGroups = (uint)(renderSize.X - 1) / 8 + 1;
        var yGroups = (uint)(renderSize.Y - 1) / 8 + 1;
        const uint zGroups = 1;
        
        var copyTextureConstants = new float[] { renderSize.X, renderSize.Y, 0, 0 }.ToByteArray();

        if (!sceneBuffers.HasTexture(FrameCaptureEffect.Context, FrameCaptureEffect.SceneColorName))
            throw new InvalidOperationException($"{nameof(FrameCaptureEffect)} is required!");
        
        if (sceneBuffers.HasTexture(Context, FragColorTexName))
        {
            // Delete textures if render size changed
            var textureFormat = sceneBuffers.GetTextureFormat(Context, FragColorTexName);
            if (textureFormat.Width != renderSize.X || textureFormat.Height != renderSize.Y)
                sceneBuffers.ClearContext(Context);
        }
        
        var viewCount = sceneBuffers.GetViewCount();
        
        if (!sceneBuffers.HasTexture(Context, FragColorTexName))
        {
            sceneBuffers.CreateTexture(
                Context,
                FragColorTexName,
                _fragColorAttachmentFormat.Format,
                _fragColorAttachmentFormat.UsageFlags,
                _fragColorAttachmentFormat.Samples,
                renderSize,
                viewCount,
                1,
                true
            );
        }
        
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

            var fragColorIn = sceneBuffers.GetColorLayer(view);
            var capturedFrame = sceneBuffers.GetTextureSlice(FrameCaptureEffect.Context, FrameCaptureEffect.SceneColorName, view, 0, 1, 1);
            var velocityTexture = sceneBuffers.GetVelocityLayer(view);
            
            var fragColorUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 0
            };
            fragColorUniform.AddId(fragColorIn);
            
            var capturedFrameUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 1
            };
            capturedFrameUniform.AddId(capturedFrame);
            
            var velocityTexUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 2
            };
            velocityTexUniform.AddId(velocityTexture);
            
            var dstImageUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 3
            };
            dstImageUniform.AddId(fragColorIn);
            
            var dataMoshUniforms = UniformSetCacheRD.GetCache(_dataMoshShader, 0, [fragColorUniform, capturedFrameUniform, velocityTexUniform, dstImageUniform]);
            
            RenderDevice.DrawCommandBeginLabel("Data Moshing", new Color(1, 1, 1));
            var computeList = RenderDevice.ComputeListBegin();
            
            RenderDevice.ComputeListBindComputePipeline(computeList, _dataMoshPipeline);
            RenderDevice.ComputeListBindUniformSet(computeList, dataMoshUniforms, 0);
            RenderDevice.ComputeListSetPushConstant(computeList, copyTextureConstants);
            RenderDevice.ComputeListDispatch(computeList, xGroups, yGroups, zGroups);
            RenderDevice.ComputeListEnd();
            
            RenderDevice.DrawCommandEndLabel();
            
            RenderDevice.FreeRid(vertexArray);
            RenderDevice.FreeRid(vertexBuffer);
            RenderDevice.FreeRid(indexArray);
            RenderDevice.FreeRid(indexBuffer);
        }
    }

    protected override void ConstructEffect(RenderingDevice renderDevice)
    {
        _frameBufferFormat = renderDevice.FramebufferFormatCreate([_frameBufferAttachmentFormat]);
        _vertexBufferFormat = renderDevice.VertexFormatCreate([_vertexAttribute]);

        if (_dataMoshShaderFile is null)
            return;

        _dataMoshShader = renderDevice.ShaderCreateFromSpirV(_dataMoshShaderFile.GetSpirV());
        if (!_dataMoshShader.IsValid)
            throw new ArgumentException("Data Mosh Shader is invalid!");

        // var rasterState = new RDPipelineRasterizationState();
        //
        // var multiSampleState = new RDPipelineMultisampleState();
        //
        // var depthStencilState = new RDPipelineDepthStencilState();
        //
        // var blendState = new RDPipelineColorBlendState
        // {
        //     Attachments = [new RDPipelineColorBlendStateAttachment()]
        // };

        _dataMoshPipeline = renderDevice.ComputePipelineCreate(_dataMoshShader);

        if (!_dataMoshPipeline.IsValid)
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
        
        // if (_copyTextureShaderFile is null)
        //     return;
        //
        // _copyTextureShader = renderDevice.ShaderCreateFromSpirV(_copyTextureShaderFile.GetSpirV());
        // if (!_copyTextureShader.IsValid)
        //     throw new InvalidOperationException("Copy texture shader is invalid!");
        //
        // _copyTexturePipeline = renderDevice.ComputePipelineCreate(_copyTextureShader);
        // if (!_copyTexturePipeline.IsValid)
        //     throw new InvalidOperationException("Copy texture pipeline is invalid!");
    }
    
    protected override void DestructEffect(RenderingDevice renderDevice)
    {
        if (_dataMoshShader.IsValid)
        {
            renderDevice.FreeRid(_dataMoshShader);
            _dataMoshShader = default;
        }
        _dataMoshPipeline = default;
        
        if (_sampler.IsValid)
        {
            renderDevice.FreeRid(_sampler);
            _sampler = default;
        }
    }
}