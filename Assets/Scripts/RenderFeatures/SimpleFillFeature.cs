using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    class OutlinePass : ScriptableRenderPass
    {
        private RTHandle source { get; set; }
        private RTHandle destination { get; set; }
        public Material outlineMaterial = null;
        RTHandle temporaryColorTexture;

        public void Setup(RTHandle source, RTHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public OutlinePass(Material outlineMaterial)
        {
            this.outlineMaterial = outlineMaterial;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            temporaryColorTexture = RTHandles.Alloc("_TemporaryColor", name: "_TemporaryColor");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("_OutlinePass");

            RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDescriptor.depthBufferBits = 0;

            Debug.Log("Executing outline renderer");

            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                cmd.GetTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name), opaqueDescriptor, FilterMode.Point);
                Blit(cmd, source, temporaryColorTexture, outlineMaterial, 0);
                Blit(cmd, temporaryColorTexture, source);
                // Blit(cmd, source, source);
            }
            else
            {
                Blit(cmd, source, destination, outlineMaterial, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (temporaryColorTexture != null)
            {
                RTHandles.Release(temporaryColorTexture);
                temporaryColorTexture = null;
            }
        }
    }

    [System.Serializable]
    public class OutlineSettings
    {
        public Material outlineMaterial = null;
    }

    public OutlineSettings settings = new OutlineSettings();
    OutlinePass outlinePass;
    RTHandle outlineTexture;

    public override void Create()
    {
        outlinePass = new OutlinePass(settings.outlineMaterial);
        outlinePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        outlineTexture = RTHandles.Alloc("_OutlineTexture", name: "_OutlineTexture");
        // outlineTexture.Init("_OutlineTexture");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null)
        {
            Debug.LogWarningFormat("Missing Outline Material");
            return;
        }
        renderer.EnqueuePass(outlinePass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        outlinePass.Setup(renderer.cameraColorTargetHandle, renderer.cameraColorTargetHandle);
    }
}