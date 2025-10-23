using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

//public class OutlineRenderFeature : ScriptableRendererFeature
//{
//    public bool needsBlur = true;

//    private RTHandle renderedTexture;
//    private RTHandle bluredTexture;

//    public LayerMask layerMask = 0;
//    public Color fillColor = Color.red;
//    private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

//    [SerializeField] private string _renderTextureName;
//    [SerializeField] private RenderSettings _renderSettings;

//    RenderPass renderPass;
//    BlurPass blurPass;
//    OutlinePass outlinePass;

//    [System.Serializable]
//    public class BlurSettings
//    {
//        public Material BlurMaterial;
//        public int DownSample = 1;
//        public int PassesCount = 1;
//    }

//    [SerializeField] private string _bluredTextureName;
//    [SerializeField] private BlurSettings blurSettings;

//    [SerializeField] private Material _outlineMaterial;
//    private OutlinePass _outlinePass;

//    public override void Create()
//    {
//        renderPass = new RenderPass(renderedTexture, layerMask, fillColor)
//        {
//            renderPassEvent = renderPassEvent
//        };

//        blurPass = new BlurPass(needsBlur, blurSettings.BlurMaterial, 1, 1)
//        {
//            renderPassEvent = renderPassEvent
//        };

//        outlinePass = new OutlinePass(_outlineMaterial)
//        {
//            renderPassEvent = renderPassEvent
//        };

//        //renderedTexture = RTHandles.Alloc(Vector2.one, depthBufferBits: DepthBits.None, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, name: "_RenderTexture");
//        //bluredTexture = RTHandles.Alloc(Vector2.one, depthBufferBits: DepthBits.None, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, name: "_BlurTexture");
//    }

//    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//    {
//        renderPass.Setup(renderedTexture);
//        renderer.EnqueuePass(renderPass);

//        blurPass.Setup(bluredTexture);
//        renderer.EnqueuePass(blurPass);
//        renderer.EnqueuePass(outlinePass);
//    }

//    class RenderPass : ScriptableRenderPass
//    {
//        private CommandBuffer cmd;
//        private RenderTargetIdentifier cameraTarget;
//        private RTHandle destination;

//        private List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
//        private FilteringSettings filteringSettings;
//        private RenderStateBlock renderStateBlock;

//        private LayerMask layerMask;
//        private Color fillColor;
//        private Material whiteMaterial;
//        private Material blackMaterial;

//        public RenderPass(RTHandle destination, LayerMask layerMask, Color fillColor)
//        {
//            this.layerMask = layerMask;
//            this.fillColor = fillColor;

//            whiteMaterial = new Material(Shader.Find("Unlit/Color"));
//            whiteMaterial.color = Color.white;

//            blackMaterial = new Material(Shader.Find("Unlit/Color"));
//            blackMaterial.color = Color.black;

//            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
//            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
//        }

//        public void Setup(RTHandle destination)
//        {
//            this.destination = destination;
//        }

//        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//        {
//            ConfigureTarget(destination);
//            ConfigureClear(ClearFlag.All, Color.clear);
//        }

//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            Debug.Log("Executing Render");

//            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
//            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
//            drawingSettings.overrideMaterial = whiteMaterial;

//            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
//        }
//    }

//    class BlurPass : ScriptableRenderPass
//    {
//        private int _tmpBlurRTId1 = Shader.PropertyToID("_TempBlurTexture1");
//        private int _tmpBlurRTId2 = Shader.PropertyToID("_TempBlurTexture2");

//        private RenderTargetIdentifier _tmpBlurRT1;
//        private RenderTargetIdentifier _tmpBlurRT2;

//        private RenderTargetIdentifier _source;
//        private RTHandle destination;

//        private int _passesCount;
//        private int _downSample;
//        private Material _blurMaterial;

//        bool needsBlur;

//        public BlurPass(bool needsBlur, Material blurMaterial, int downSample, int passesCount)
//        {
//            this.needsBlur = needsBlur;
//            _blurMaterial = blurMaterial;
//            _downSample = downSample;
//            _passesCount = passesCount;
//        }

//        public void Setup(RTHandle destination)
//        {
//            this.destination = destination;
//        }

//        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//        {
//            int width = Mathf.Max(1, cameraTextureDescriptor.width >> _downSample);
//            int height = Mathf.Max(1, cameraTextureDescriptor.height >> _downSample);
//            RenderTextureDescriptor blurTextureDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0, 0);

//            _tmpBlurRT1 = new RenderTargetIdentifier(_tmpBlurRTId1);
//            _tmpBlurRT2 = new RenderTargetIdentifier(_tmpBlurRTId2);

//            cmd.GetTemporaryRT(_tmpBlurRTId1, blurTextureDesc, FilterMode.Bilinear);
//            cmd.GetTemporaryRT(_tmpBlurRTId2, blurTextureDesc, FilterMode.Bilinear);

//            cmd.GetTemporaryRT(0, blurTextureDesc, FilterMode.Bilinear);
//            ConfigureTarget(destination);
//        }

//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            if (needsBlur)
//            {
//                Debug.Log("Execute blur");

//                var cmd = CommandBufferPool.Get("BlurPass");

//                if (_passesCount > 0)
//                {
//                    cmd.Blit(_source, _tmpBlurRT1, _blurMaterial, 0);
//                    for (int i = 0; i < _passesCount - 1; i++)
//                    {
//                        cmd.Blit(_tmpBlurRT1, _tmpBlurRT2, _blurMaterial, 0);
//                        var t = _tmpBlurRT1;
//                        _tmpBlurRT1 = _tmpBlurRT2;
//                        _tmpBlurRT2 = t;
//                    }
//                    cmd.Blit(_tmpBlurRT1, destination);
//                }
//                else
//                    cmd.Blit(_source, destination);
//                context.ExecuteCommandBuffer(cmd);
//                CommandBufferPool.Release(cmd);
//            }
//        }
//    }

//    class OutlinePass : ScriptableRenderPass
//    {
//        private string _profilerTag = "Outline";
//        private Material _material;

//        public OutlinePass(Material material)
//        {
//            _material = material;
//        }

//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            //var cmd = CommandBufferPool.Get(_profilerTag);

//            //using (new ProfilingSample(cmd, _profilerTag))
//            //{
//            //    var mesh = RenderingUtils.fullscreenMesh;
//            //    cmd.DrawMesh(mesh, Matrix4x4.identity, _material, 0, 0);
//            //}

//            //context.ExecuteCommandBuffer(cmd);
//            //CommandBufferPool.Release(cmd);
//        }
//    }
//}

//public class OutlineRenderFeature : ScriptableRendererFeature
//{
//    private RTHandle renderedTexture;

//    public LayerMask layerMask = 0;
//    public Color fillColor = Color.red;
//    private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

//    RenderPass renderPass;

//    public override void Create()
//    {
//        renderPass = new RenderPass(renderedTexture, layerMask, fillColor)
//        {
//            renderPassEvent = renderPassEvent
//        };
//    }

//    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//    {
//        renderPass.Setup(renderedTexture);
//        renderer.EnqueuePass(renderPass);
//    }

//    class RenderPass : ScriptableRenderPass
//    {
//        private CommandBuffer cmd;
//        private RenderTargetIdentifier cameraTarget;
//        private RTHandle destination;

//        private List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
//        private FilteringSettings filteringSettings;
//        private RenderStateBlock renderStateBlock;

//        private LayerMask layerMask;
//        private Color fillColor;
//        private Material whiteMaterial;
//        private Material blackMaterial;

//        public RenderPass(RTHandle destination, LayerMask layerMask, Color fillColor)
//        {
//            this.layerMask = layerMask;
//            this.fillColor = fillColor;

//            whiteMaterial = new Material(Shader.Find("Unlit/Color"));
//            whiteMaterial.color = Color.white;

//            blackMaterial = new Material(Shader.Find("Unlit/Color"));
//            blackMaterial.color = Color.black;

//            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
//            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
//        }

//        public void Setup(RTHandle destination)
//        {
//            this.destination = destination;
//        }

//        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//        {
//            ConfigureTarget(destination);
//            ConfigureClear(ClearFlag.All, Color.clear);
//        }

//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            Debug.Log("Executing Render");

//            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
//            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
//            drawingSettings.overrideMaterial = whiteMaterial;

//            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
//        }
//    }
//}

public class OutlineRenderFeature : ScriptableRendererFeature
{
    public LayerMask layerMask;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    public Material outlineMaterial;
    private RTHandle destination;
    private RTHandle maskRT;
    private RTHandle tmp1;
    private RTHandle tmp2;
    private MaskPass maskPasm;
    //private OutlinePass outlinePass;

    public override void Create()
    {
        maskPasm = new MaskPass(layerMask, outlineMaterial, destination);
        maskPasm.Setup(maskRT, tmp1, tmp2);
        maskPasm.renderPassEvent = renderPassEvent;

        //outlinePass = new OutlinePass(layerMask);
        //outlinePass.Setup(maskRT, tmp1, tmp2);
        //outlinePass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(maskPasm);
        //renderer.EnqueuePass(outlinePass);
    }

    class MaskPass : ScriptableRenderPass
    {
        RTHandle destination;
        RTHandle maskRT;
        RTHandle tmp1;
        RTHandle tmp2;
        Material whiteMaterial;
        Material blurMaterial;
        Material outlineMaterial;
        private List<ShaderTagId> shaderTagIdList;
        FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;

        public MaskPass(LayerMask layerMask, Material outlineMaterial, RTHandle destination)
        {
            this.destination = destination;

            shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            whiteMaterial = new Material(Shader.Find("Unlit/Color"));
            whiteMaterial.color = Color.white;

            blurMaterial = new Material(Shader.Find("Hidden/BlurSimple"));
            blurMaterial.color = Color.white;

            this.outlineMaterial = outlineMaterial;
            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public void Setup(RTHandle maskRT, RTHandle tmp1, RTHandle tmp2)
        {
            this.maskRT = maskRT;
            this.tmp1 = tmp1;
            this.tmp2 = tmp2;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (destination == null)
                destination = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_SRGB, name: "_destination");
            if (maskRT == null)
                maskRT = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_SRGB, name: "_maskRT");
            if (tmp1 == null)
                tmp1 = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_SRGB, name: "_tmp1");
            if (tmp2 == null)
                tmp2 = RTHandles.Alloc(Vector2.one, colorFormat: GraphicsFormat.B8G8R8A8_SRGB, name: "_tmp2");

            ConfigureTarget(tmp1);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Outline");

            // Render
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = whiteMaterial;

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);

            // Blur
            Blitter.BlitCameraTexture(cmd, maskRT, tmp1, blurMaterial, 0);
            for (int i = 0; i < 2; i++)
            {
                cmd.Blit(tmp1, tmp2, blurMaterial, 0);
                var t = tmp1;
                tmp1 = tmp2;
                tmp2 = t;
            }
            Blitter.BlitCameraTexture(cmd, tmp2, maskRT);

            //Draw
            Blitter.BlitCameraTexture(cmd, tmp1, renderingData.cameraData.renderer.cameraColorTargetHandle);

            // Outline
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, outlineMaterial, 0, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            //if (tmp1 != null)
                //tmp1?.Release();
        }
    }
}