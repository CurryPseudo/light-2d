﻿using System.Collections;
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
			commandBuffer.ClearRenderTarget(true, true, Color.black);

			PointLight[] lights = MonoBehaviour.FindObjectsOfType<PointLight>();

			foreach(var light in lights) {
				commandBuffer.name = "Light";

				commandBuffer.SetRenderTarget(shadowMapId);
				commandBuffer.ClearRenderTarget(false, true, Color.black);
				// I dont know why, I need to clear render target double time to actually clear it on Mac OS.
				commandBuffer.ClearRenderTarget(false, true, Color.black);
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
