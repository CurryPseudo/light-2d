using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public float radius;
	public int triangleCount;
	public Color lightColor;
	public Material lightMaterial;
	[System.Serializable]
	public struct MeshRenderInfo {
		public MeshFilter filter;
		public Mesh mesh;

		public void Setup(PointLight pointLight, string name, Material material) {
			if(filter == null) {
				GameObject go = new GameObject(name);
				go.transform.SetParent(pointLight.transform, false);
				filter = go.AddComponent<MeshFilter>();
				var renderer = go.AddComponent<MeshRenderer>();
				renderer.material = material;
				mesh = new Mesh();
				filter.mesh = mesh;
			}
		}
    }
	public MeshRenderInfo light = new MeshRenderInfo();
	void Update() {
		light.Setup(this, "LightMesh", lightMaterial);
		if(triangleCount < 3) {
			triangleCount = 3;
		}
		light.mesh.Clear();
		Vector3[] vertices = new Vector3[triangleCount + 2];
		vertices[0] = Vector3.zero;
		float stepAngle = Mathf.PI * 2 / triangleCount;
		float currentAngle = 0;
		for(int i = 0; i < triangleCount + 1; i++) {
			currentAngle += stepAngle;
			Vector2 dir = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
			dir *= radius;
			Vector3 dirV3 = new Vector3(dir.x, dir.y, 0);
			Vector3 vertex = dirV3;
			vertices[i + 1] = vertex;
		}
		int[] triangles = new int[3 * triangleCount];
		for(int i = 0; i < triangleCount; i++) {
			triangles[3 * i] = 0;
			triangles[3 * i + 1] = i + 2;
			triangles[3 * i + 2] = i + 1;
		}

		Color[] colors = new Color[triangleCount + 2];
		colors[0] = lightColor;
		for(int i = 0; i < triangleCount + 1; i++) {
			colors[i + 1] = Color.black;
		}
		light.mesh.vertices = vertices;
		light.mesh.triangles = triangles;
		light.mesh.colors = colors;
		light.mesh.RecalculateNormals();
		light.filter.mesh = light.mesh;

	}
}
