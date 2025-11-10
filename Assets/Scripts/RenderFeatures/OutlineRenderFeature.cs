using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    [Header("Render Settings")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineIntensity = 1.0f;

    [Header("Blur Settings")]
    [SerializeField] private int blurPassesCount = 1;

    private OutlinePass outlinePass;

    public override void Create()
    {
        outlinePass = new OutlinePass(layerMask, outlineColor, outlineIntensity, blurPassesCount);
        outlinePass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(outlinePass);
    }

    class OutlinePass : ScriptableRenderPass
    {
        private Color outlineColor = Color.white;
        private float outlineIntensity = 0;

        private int blurPassesCount = 0;

        private RTHandle maskRT;
        private RTHandle blured1;
        private RTHandle blured2;
        private RTHandle finalRT;

        private Material whiteMaterial;
        private Material blurMaterial;
        private Material outlineMaterial;

        private List<ShaderTagId> shaderTagIdList;
        private FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;

        public OutlinePass(LayerMask layerMask, Color outlineColor, float outlineIntensity, int blurPassesCount)
        {
            this.outlineColor = outlineColor;
            this.outlineIntensity = outlineIntensity;

            this.blurPassesCount = blurPassesCount;

            shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader)
                whiteMaterial = new Material(unlitShader);
            else
                Debug.LogWarning("Unlit shader was not found at the Universal Render Pipeline/Unlit path");

            Shader outlineShader = Shader.Find("Hidden/RenderFeatureOutline");
            if (outlineShader)
                outlineMaterial = new Material(outlineShader);
            else
                Debug.LogWarning("Outline shader was not found at the Hidden/RenderFeatureOutline path");

            Shader blurShader = Shader.Find("Hidden/SimpleBlur");
            if (blurShader)
                blurMaterial = new Material(blurShader);
            else
                Debug.LogWarning("Blur shader was not found at the Hidden/SimpleBlur path");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int width = cameraTextureDescriptor.width;
            int height = cameraTextureDescriptor.height;

            if (maskRT == null)
                maskRT = RTHandles.Alloc(width, height, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, dimension: TextureDimension.Tex2D, useDynamicScale: false, name: "_maskRT");
            if (blured1 == null)
                blured1 = RTHandles.Alloc(width, height, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, dimension: TextureDimension.Tex2D, useDynamicScale: false, name: "_tmp1");
            if (blured2 == null)
                blured2 = RTHandles.Alloc(width, height, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, dimension: TextureDimension.Tex2D, useDynamicScale: false, name: "_tmp2");
            if (finalRT == null)
                finalRT = RTHandles.Alloc(width, height, colorFormat: GraphicsFormat.R8G8B8A8_UNorm, dimension: TextureDimension.Tex2D, useDynamicScale: false, name: "_finalRT");

            if (maskRT != null)
                maskRT.rt.wrapMode = TextureWrapMode.Clamp;
            if (blured1 != null)
                blured1.rt.wrapMode = TextureWrapMode.Clamp;
            if (blured2 != null)
                blured2.rt.wrapMode = TextureWrapMode.Clamp;
            if (finalRT != null)
                finalRT.rt.wrapMode = TextureWrapMode.Clamp;

            ConfigureTarget(maskRT);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //if (maskRT == null || blured1 == null || blured2 == null || finalRT == null || !outlineMaterial || !blurMaterial) return;

            //CommandBuffer cmd = CommandBufferPool.Get("Outline");

            //var renderer = renderingData.cameraData.renderer;

            //// Render
            //SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            //DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
            //drawingSettings.overrideMaterial = whiteMaterial;

            //context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);

            //// Blur
            //cmd.Blit(maskRT.rt, blured1.rt);
            //blurMaterial.SetTexture("_MainTex", maskRT);
            //for (int i = 0; i < blurPassesCount; i++)
            //{
            //    cmd.Blit(blured1.rt, blured2.rt, blurMaterial, 0);
            //    var t = blured1;
            //    blured1 = blured2;
            //    blured2 = t;
            //}

            //// Outline
            //outlineMaterial.SetTexture("_MainTex", renderer.cameraColorTargetHandle.rt);
            //outlineMaterial.SetTexture("_MaskTex", maskRT.rt);
            //outlineMaterial.SetTexture("_BluredMaskTex", blured1.rt);
            //outlineMaterial.SetFloat("_Intensity", outlineIntensity);
            //outlineMaterial.SetColor("_Color", outlineColor);

            //cmd.Blit(renderer.cameraColorTargetHandle.rt, finalRT.rt, outlineMaterial, 0);
            //cmd.Blit(finalRT.rt, renderer.cameraColorTargetHandle.rt);

            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
        }
    }
}