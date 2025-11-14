using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomPostProcessFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle m_SourceHandle; // 원본
        
        // 'int' ID와 'RTHandle'을 둘 다 관리해야 합니다.
        private int m_TempTextureID = Shader.PropertyToID("_TempPostProcessRT"); 
        private RTHandle m_TempTextureHandle; // 임시 텍스처 '핸들'

        public CustomRenderPass(Material material)
        {
            this.material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            
            // RTHandle을 미리 할당합니다.
            m_TempTextureHandle = RTHandles.Alloc(m_TempTextureID);
        }

        public void Setup(RTHandle source)
        {
            this.m_SourceHandle = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("CustomPostProcess");
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0; 

            // 1. 임시 텍스처의 '메모리'를 할당받습니다.
            cmd.GetTemporaryRT(m_TempTextureID, opaqueDesc, FilterMode.Point);

            // 2. Blit (원본 -> 임시)
            // m_SourceHandle (RTHandle) -> m_TempTextureHandle (RTHandle)
            // 이제 이 Blit은 최신 오버로드를 사용합니다.
            Blit(cmd, m_SourceHandle, m_TempTextureHandle, material); 
            
            // 3. Blit (임시 -> 원본)
            // m_TempTextureHandle (RTHandle) -> m_SourceHandle (RTHandle)
            Blit(cmd, m_TempTextureHandle, m_SourceHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            
            // 4. 사용한 '메모리'를 해제합니다.
            cmd.ReleaseTemporaryRT(m_TempTextureID);
        }

        // (추가됨) 핸들 자체를 해제합니다.
        public void Dispose()
        {
            m_TempTextureHandle?.Release();
        }

    } // CustomRenderPass 끝

    // --- 피처 설정 ---
    [System.Serializable]
    public class Settings { public Material material = null; }
    public Settings settings = new Settings(); 
    private CustomRenderPass scriptablePass; 

    public override void Create()
    {
        if (settings.material != null)
        {
            scriptablePass = new CustomRenderPass(settings.material);
        }
    }

    // (추가됨) 피처가 파괴될 때 Pass의 리소스를 해제합니다.
    protected override void Dispose(bool disposing)
    {
        scriptablePass?.Dispose();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (scriptablePass != null && settings.material != null)
        {
            scriptablePass.Setup(renderer.cameraColorTargetHandle); 
            renderer.EnqueuePass(scriptablePass); 
        }
    }
}