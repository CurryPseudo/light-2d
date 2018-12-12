using UnityEngine;
using UnityEngine.Rendering;
namespace Util
{
    public static class CommandBufferExtensions {
        public static void DrawMesh(this CommandBuffer commandBuffer, Mesh mesh, Transform transform, Material material) {
            commandBuffer.DrawMesh(mesh, transform.localToWorldMatrix, material);
        }
    }
}