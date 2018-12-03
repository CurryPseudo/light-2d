using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


public class LightPipe : RenderPipeline {
	CommandBuffer commandBuffer;
	public override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
		base.Render(renderContext, cameras);
		if(commandBuffer == null) {
			commandBuffer = new CommandBuffer();
		}

		foreach(Camera camera in cameras) {
			renderContext.SetupCameraProperties(camera);
			commandBuffer.name = "Setup";
			commandBuffer.ClearRenderTarget(true, true, new Color(0,0,0,0));
			renderContext.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();

			var culled = new CullResults();
			CullResults.Cull(camera, renderContext, out culled);

			var settings = new DrawRendererSettings(camera, new ShaderPassName("Light"));
			var filter = new FilterRenderersSettings(true);
			renderContext.DrawRenderers(culled.visibleRenderers, ref settings, filter);
			renderContext.Submit();
		}

	}
	public override void Dispose()
        {
            base.Dispose();

            if (commandBuffer != null)
            {
                commandBuffer.Dispose();
                commandBuffer = null;
            }
        }
}
