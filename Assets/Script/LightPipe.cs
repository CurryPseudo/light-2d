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

		var shadowMapId = Shader.PropertyToID("_ShadowMap");
		var lightMapId = Shader.PropertyToID("_LightMap");
		foreach(Camera camera in cameras) {

			renderContext.SetupCameraProperties(camera);
			commandBuffer.name = "Setup";

			var shadowMapDesc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.RFloat);
			commandBuffer.GetTemporaryRT(shadowMapId, shadowMapDesc);

			var lightMapDesc = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight, RenderTextureFormat.ARGB32);
			commandBuffer.GetTemporaryRT(lightMapId, lightMapDesc);

			commandBuffer.SetRenderTarget(lightMapId);
			commandBuffer.ClearRenderTarget(true, true, new Color(0,0,0,0));

			PointLight[] lights = MonoBehaviour.FindObjectsOfType<PointLight>();

			foreach(var light in lights) {
				commandBuffer.name = "Light";

				commandBuffer.SetRenderTarget(shadowMapId);
				commandBuffer.ClearRenderTarget(false, true, Color.white);
				light.DrawShadowMesh(ref commandBuffer);

				commandBuffer.SetRenderTarget(lightMapId);
				light.DrawLightMesh(ref commandBuffer, shadowMapId);

			}

			commandBuffer.Blit(lightMapId, BuiltinRenderTextureType.CameraTarget);

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
