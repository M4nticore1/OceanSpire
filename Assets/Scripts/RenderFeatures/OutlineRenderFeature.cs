using LineworkLite.Common.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    //OutlinePass m_ScriptablePass;

    [System.Serializable]
    public class RenderSettings
    {
        public Material material = null;
        public LayerMask layer = 0;
    }

    [SerializeField] private string _renderTextureName;
    [SerializeField] private RenderSettings _renderSettings;

    private RTHandle _renderTexture;
    private OutlinePass _renderPass;

    public override void Create()
    {
        _renderTexture = RTHandles.Alloc(_renderTextureName);
        _renderPass = new OutlinePass(_renderTexture, _renderSettings.layer, _renderSettings.material);
        _renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_renderPass);
    }

    class OutlinePass : ScriptableRenderPass
    {
        private RTHandle _rtHandle;
        private CommandBuffer _cmd;

        private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
        private FilteringSettings _filteringSettings;
        private RenderStateBlock _renderStateBlock;

        private Material _material;
        private LayerMask _layer = 0;

        private class PassData
        {

        }

        public OutlinePass(RTHandle rtHandle, int layerMask, Material overrideMaterial)
        {
            _rtHandle = rtHandle;

            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            _material = overrideMaterial;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(_rtHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("OutlinePass");

            // Устанавливаем камеру как render target
            CoreUtils.SetRenderTarget(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, ClearFlag.All, Color.black);

            // Создаём DrawingSettings для нужного слоя
            var sortingCriteria = SortingCriteria.CommonOpaque;
            var drawingSettings = RenderingUtils.CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, sortingCriteria);

            drawingSettings.overrideMaterial = _material;

            // Фильтруем по слою
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, 1 << _layer);

            // Рисуем объекты на экран
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            //var maskCmd = CommandBufferPool.Get();

            //var sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;

            //var drawingSettings = RenderingUtils.CreateDrawingSettings(RenderUtils.DefaultShaderTagIds, ref renderingData, sortingCriteria);
            //drawingSettings.overrideMaterial = _material;

            //context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);

            //// TODO: сюда добавляем рендеринг объектов или постэффект
            ////SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            ////DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
            ////drawingSettings.overrideMaterial = _material;

            ////CoreUtils.SetRenderTarget(maskCmd, _rtHandle, ClearFlag.All, Color.green);
            ////CoreUtils.DrawFullScreen(maskCmd, _material, _rtHandle);
            ////maskCmd.Blit(_rtHandle, renderingData.cameraData.renderer.cameraColorTargetHandle);

            //context.ExecuteCommandBuffer(maskCmd);
            //CommandBufferPool.Release(maskCmd);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // Use this scope to set the required inputs and outputs of the pass and to
                // setup the passData with the required properties needed at pass execution time.

                // Make use of frameData to access resources and camera data through the dedicated containers.
                // Eg:
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // Setup pass inputs and outputs through the builder interface.
                // Eg:
                // builder.UseTexture(sourceTexture);
                // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
                //builder.SetRenderFunc((PassData data, RasterGraphContext context) => Execute(context, data));
            }
        }
    }
}
