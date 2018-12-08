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
			PointLight[] lights = MonoBehaviour.FindObjectsOfType<PointLight>();

			commandBuffer.name = "Light";
			foreach(var light in lights) {
				light.DrawMesh(ref commandBuffer);
			}
			renderContext.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			renderContext.Submit();
		}

	}
	public static void DrawMesh(CommandBuffer commandBuffer, Mesh mesh, Transform transform, Material material) {
		commandBuffer.DrawMesh(mesh, transform.localToWorldMatrix, material);
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
