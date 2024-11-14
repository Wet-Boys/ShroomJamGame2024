using System;
using Godot;
using ShroomJamGame.Rendering.Utils;

namespace ShroomJamGame.Rendering.Effects;

[Tool]
[GlobalClass]
public partial class PosterizationEffect : BaseCompositorEffect
{
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
    
    private static PosterizationEffect? _instance;

    public static PosterizationEffect Instance
    {
        get
        {
            if (_instance is null)
                throw new InvalidOperationException($"{nameof(PosterizationEffect)} instance can't be null.");
            
            return _instance;
        }
    }
    
    private long _frameBufferFormat;
    private long _vertexBufferFormat;
    
    private RDShaderFile? _posterizationShaderFile;
    private Rid _posterizationPipeline;
    private Rid _posterizationShader;
    private Rid _sampler;
    
    public PosterizationEffect()
    {
        _instance = this;
        
        EffectCallbackType = EffectCallbackTypeEnum.PostTransparent;
    }
    
    [Export]
    private RDShaderFile? PosterizationShader
    {
        get => _posterizationShaderFile;
        set
        {
            _posterizationShaderFile = value;
            if (RenderDevice is not null)
            {
                Destruct();
                Construct();
            }
        }
    }

    [Export]
    public float Levels = 10;
    
    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        base._RenderCallback(effectCallbackType, renderData);

        if (RenderDevice is null || _posterizationShaderFile is null)
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
        
        var posterizationConstants = new float[]
        {
            renderSize.X,
            renderSize.Y,
            Levels,
            0
        }.ToByteArray();
        
        var viewCount = sceneBuffers.GetViewCount();
        
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
            
            var fragColorUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 0
            };
            fragColorUniform.AddId(fragColorIn);
            
            var posterizationUniforms = UniformSetCacheRD.GetCache(_posterizationShader, 0, [fragColorUniform]);
            
            RenderDevice.DrawCommandBeginLabel("Posterization", new Color(1, 1, 1));
            var computeList = RenderDevice.ComputeListBegin();
            
            RenderDevice.ComputeListBindComputePipeline(computeList, _posterizationPipeline);
            RenderDevice.ComputeListBindUniformSet(computeList, posterizationUniforms, 0);
            RenderDevice.ComputeListSetPushConstant(computeList, posterizationConstants);
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

        if (_posterizationShaderFile is null)
            return;

        _posterizationShader = renderDevice.ShaderCreateFromSpirV(_posterizationShaderFile.GetSpirV());
        if (!_posterizationShader.IsValid)
            throw new ArgumentException("Posterization Shader is invalid!");

        _posterizationPipeline = renderDevice.ComputePipelineCreate(_posterizationShader);

        if (!_posterizationPipeline.IsValid)
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
        if (_posterizationShader.IsValid)
        {
            renderDevice.FreeRid(_posterizationShader);
            _posterizationShader = default;
        }
        _posterizationPipeline = default;
        
        if (_sampler.IsValid)
        {
            renderDevice.FreeRid(_sampler);
            _sampler = default;
        }
    }
}