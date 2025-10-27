using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class TestRenderFeature : ScriptableRendererFeature
{
    public LayerMask layerMask;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    public Material outlineMaterial;
    public Material blurMaterial;
    public Material whiteMaterial;

    public int downSample = 1;
    public int blurPassesCount = 1;

    private RTHandle maskRT;
    private RTHandle tmp1;
    private RTHandle tmp2;

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(layerMask, outlineMaterial, blurMaterial, whiteMaterial, downSample, blurPassesCount);

        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    class CustomRenderPass : ScriptableRenderPass
    {
        RTHandle maskRT;
        RTHandle blured1;
        RTHandle blured2;
        RTHandle finalRT;
        Material whiteMaterial;
        Material blurMaterial;
        Material outlineMaterial;

        int downSample = 1;
        int blurPassesCount = 1;

        RenderTextureDescriptor cameraTextureDescriptor;

        private List<ShaderTagId> shaderTagIdList;
        FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;

        private class PassData
        {

        }

        public CustomRenderPass(LayerMask layerMask, Material outlineMaterial, Material blurMaterial, Material whiteMaterial, int downSample, int blurPassesCount)
        {
            shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward") };
            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            this.downSample = downSample;
            this.blurPassesCount = blurPassesCount;

            this.whiteMaterial = whiteMaterial;
            //this.outlineMaterial = outlineMaterial;
            //this.blurMaterial = blurMaterial;
            //renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }
    }
}
