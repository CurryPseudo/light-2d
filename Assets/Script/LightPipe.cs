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
		commandBuffer.ClearRenderTarget(true, true, new Color(0,0,0,0));
		renderContext.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		renderContext.Submit();
	}
}
