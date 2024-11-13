using System;
using Godot;
using ShroomJamGame.Rendering.Utils;

namespace ShroomJamGame.Rendering.Effects;

[Tool]
[GlobalClass]
public partial class OutlineEffect : BaseCompositorEffect
{
    public static readonly StringName Context = "OutlineEffect";
    public static readonly StringName FragColorTexName = "outline_frag_color";
    
    private readonly RDAttachmentFormat _fragColorInAttachmentFormat = new()
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
    
    private RDShaderFile? _outlineShaderFile;
    private RDShaderFile? _copyTextureShaderFile;
    private Rid _outlineShader;
    private Rid _outlinePipeline;
    private Rid _sampler;
    private Rid _copyTextureShader;
    private Rid _copyTexturePipeline;
    
    public OutlineEffect()
    {
        EffectCallbackType = EffectCallbackTypeEnum.PostOpaque;
    }
    
    [Export]
    private RDShaderFile? OutlineShaderFile
    {
        get => _outlineShaderFile;
        set
        {
            _outlineShaderFile = value;
            if (RenderDevice is not null)
            {
                Destruct();
                Construct();
            }
        }
    }
    
    [Export]
    private RDShaderFile? CopyTextureShaderFile
    {
        get => _copyTextureShaderFile;
        set
        {
            _copyTextureShaderFile = value;
            if (RenderDevice is not null)
            {
                Destruct();
                Construct();
            }
        }
    }

    [Export]
    public int Scale = 1;
    
    [Export(PropertyHint.Range, "0, 5")]
    public float DepthThreshold = 1.5f;
    
    [Export(PropertyHint.Range, "0, 5")]
    public float NormalThreshold = 0.4f;
    
    [Export(PropertyHint.Range, "0, 5")]
    public float DepthNormalThreshold = 0.5f;
    
    [Export(PropertyHint.Range, "0, 20")]
    public float DepthNormalThresholdScale = 7f;
    
    [Export]
    public Color EdgeColor = Colors.White;

    [Export]
    public bool OnlyShowOutlines = false;

    public override void _RenderCallback(int effectCallbackType, RenderData renderData)
    {
        base._RenderCallback(effectCallbackType, renderData);
        
        if (RenderDevice is null || _outlineShaderFile is null || _copyTextureShaderFile is null)
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
                _fragColorInAttachmentFormat.Format,
                _fragColorInAttachmentFormat.UsageFlags,
                _fragColorInAttachmentFormat.Samples,
                renderSize,
                viewCount,
                1,
                true
            );
        }
        
        var clearColors = new Color[] { new(0, 0, 0, 0) };
        
        var outlineShaderParams = new[]
        {
            Scale,
            DepthThreshold,
            NormalThreshold,
            DepthNormalThreshold,
            DepthNormalThresholdScale,
            OnlyShowOutlines ? 1 : 0,
            0,
            0
        }.ToByteArray();
        
        var outlineShaderParamsUniformBuffer = RenderDevice.UniformBufferCreate(outlineShaderParams, EdgeColor.ToByteArray());

        var seconds = Stopwatch.Elapsed.TotalSeconds;
        var pushConstants = new[]
        {
            (float)seconds / 20f,
            (float)seconds,
            (float)seconds * 2,
            (float)seconds * 3
        }.ToByteArray();

        for (uint view = 0; view < viewCount; view++)
        {
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
            
            var (vertexBuffer, vertexArray) = RenderDevice.VertexArrayCreate(vertices, _vertexBufferFormat);
            var (indexBuffer, indexArray) = RenderDevice.IndexArrayCreate(indices);
            
            var invProjMat = sceneData.GetViewProjection(view).Inverse();
            var invProjMatUniformBuffer = RenderDevice.UniformBufferCreate(invProjMat.ToByteArray());
            var invProjMatUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.UniformBuffer,
                Binding = 0
            };
            invProjMatUniform.AddId(invProjMatUniformBuffer);

            var outlineVertUniforms = UniformSetCacheRD.GetCache(_outlineShader, 0, [invProjMatUniform]);

            var colorTex = sceneBuffers.GetColorLayer(view);
            var depthTex = sceneBuffers.GetDepthLayer(view);
            var normalRoughnessTex = sceneBuffers.GetTextureSlice("forward_clustered", "normal_roughness", view, 0, 1, 1);
            
            var fragColorOut = sceneBuffers.GetTextureSlice(Context, FragColorTexName, view, 0, 1, 1);
            var framebuffer = RenderDevice.FramebufferCreate([fragColorOut], _frameBufferFormat);
            if (!framebuffer.IsValid)
                throw new InvalidOperationException("Outline frame buffer is invalid!");
            
            var colorTexUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 0
            };
            colorTexUniform.AddId(_sampler);
            colorTexUniform.AddId(colorTex);

            var depthTexUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 1
            };
            depthTexUniform.AddId(_sampler);
            depthTexUniform.AddId(depthTex);

            var normalRoughnessTexUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.SamplerWithTexture,
                Binding = 2
            };
            normalRoughnessTexUniform.AddId(_sampler);
            normalRoughnessTexUniform.AddId(normalRoughnessTex);

            var outlineFragParamsUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.UniformBuffer,
                Binding = 3
            };
            outlineFragParamsUniform.AddId(outlineShaderParamsUniformBuffer);

            var outlineFragUniforms = UniformSetCacheRD.GetCache(_outlineShader, 1, [colorTexUniform, depthTexUniform, normalRoughnessTexUniform, outlineFragParamsUniform]);
            
            RenderDevice.DrawCommandBeginLabel("Outline", new Color(1, 1, 1));
            var drawList = RenderDevice.DrawListBegin(
                framebuffer,
                RenderingDevice.InitialAction.Clear,
                RenderingDevice.FinalAction.Store,
                RenderingDevice.InitialAction.Discard,
                RenderingDevice.FinalAction.Discard,
                clearColors
            );
            
            RenderDevice.DrawListBindRenderPipeline(drawList, _outlinePipeline);
            RenderDevice.DrawListBindVertexArray(drawList, vertexArray);
            RenderDevice.DrawListBindIndexArray(drawList, indexArray);
            RenderDevice.DrawListBindUniformSet(drawList, outlineVertUniforms, 0);
            RenderDevice.DrawListBindUniformSet(drawList, outlineFragUniforms, 1);
            RenderDevice.DrawListSetPushConstant(drawList, pushConstants);
            RenderDevice.DrawListDraw(drawList, false, 1);
            RenderDevice.DrawListEnd();
            
            var srcImageUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 0
            };
            srcImageUniform.AddId(fragColorOut);

            var dstImageUniform = new RDUniform
            {
                UniformType = RenderingDevice.UniformType.Image,
                Binding = 1
            };
            dstImageUniform.AddId(colorTex);
            
            var copyTextureUniforms = UniformSetCacheRD.GetCache(_copyTextureShader, 0, [srcImageUniform, dstImageUniform]);

            var computeList = RenderDevice.ComputeListBegin();
            
            RenderDevice.ComputeListBindComputePipeline(computeList, _copyTexturePipeline);
            RenderDevice.ComputeListBindUniformSet(computeList, copyTextureUniforms, 0);
            RenderDevice.ComputeListSetPushConstant(computeList, copyTextureConstants);
            RenderDevice.ComputeListDispatch(computeList, xGroups, yGroups, zGroups);
            RenderDevice.ComputeListEnd();
            
            RenderDevice.DrawCommandEndLabel();
            
            RenderDevice.FreeRid(framebuffer);
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

        if (_outlineShaderFile is null)
            return;
        
        _outlineShader = renderDevice.ShaderCreateFromSpirV(_outlineShaderFile.GetSpirV());
        if (!_outlineShader.IsValid)
            throw new InvalidOperationException("Outline shader is invalid!");
        
        var rasterState = new RDPipelineRasterizationState();
        var multiSampleState = new RDPipelineMultisampleState();
        var depthStencilState = new RDPipelineDepthStencilState();

        var blendState = new RDPipelineColorBlendState
        {
            Attachments = [new RDPipelineColorBlendStateAttachment()]
        };

        _outlinePipeline = renderDevice.RenderPipelineCreate(
            _outlineShader,
            _frameBufferFormat,
            _vertexBufferFormat,
            RenderingDevice.RenderPrimitive.Triangles,
            rasterState,
            multiSampleState,
            depthStencilState,
            blendState
        );

        if (!_outlinePipeline.IsValid)
            throw new InvalidOperationException("Outline Pipeline is invalid!");
        
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
        
        if (_copyTextureShaderFile is null)
            return;
        
        _copyTextureShader = renderDevice.ShaderCreateFromSpirV(_copyTextureShaderFile.GetSpirV());
        if (!_copyTextureShader.IsValid)
            throw new InvalidOperationException("Copy texture shader is invalid!");
        
        _copyTexturePipeline = renderDevice.ComputePipelineCreate(_copyTextureShader);
        if (!_copyTexturePipeline.IsValid)
            throw new InvalidOperationException("Copy texture pipeline is invalid!");
    }

    protected override void DestructEffect(RenderingDevice renderDevice)
    {
        if (_outlineShader.IsValid)
        {
            renderDevice.FreeRid(_outlineShader);
            _outlineShader = default;
        }
        _outlinePipeline = default;
        
        if (_copyTextureShader.IsValid)
        {
            renderDevice.FreeRid(_copyTextureShader);
            _copyTextureShader = default;
        }
        _copyTexturePipeline = default;
    }
}