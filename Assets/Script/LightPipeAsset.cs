using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu]
public class LightPipeAsset : RenderPipelineAsset {
    protected override IRenderPipeline InternalCreatePipeline()
    {
		return new LightPipe();
    }

}
