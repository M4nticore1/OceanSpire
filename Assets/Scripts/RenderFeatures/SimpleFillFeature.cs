using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SimpleFillFeature : ScriptableRendererFeature
{
    class SimpleFillPass : ScriptableRenderPass
    {
        private Material _material;

        public SimpleFillPass(Material material)
        {
            _material = material;
            renderPassEvent = RenderPassEvent.AfterRendering; // после всего рендеринга
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SimpleFillPass");

            // Рисуем прямо на камеру
            CoreUtils.SetRenderTarget(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, ClearFlag.All, Color.black);

            // Рисуем FullScreen Quad с материалом
            CoreUtils.DrawFullScreen(cmd, _material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [SerializeField] private Material _material;

    private SimpleFillPass _pass;

    public override void Create()
    {
        // Если материала нет, создаём простейший Unlit/Color
        if (_material == null)
        {
            _material = new Material(Shader.Find("Unlit/Color"));
            _material.SetColor("_Color", Color.green);
        }

        _pass = new SimpleFillPass(_material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}