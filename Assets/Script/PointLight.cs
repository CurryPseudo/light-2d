using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public float radius;
	public int triangleCount;
	public Color lightColor;
	public Material lightMaterial;
	public LayerMask colliderLayer;
	public Material shadowMaterial;
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
				mesh.MarkDynamic();
				filter.mesh = mesh;
			}
		}
    }
	public new MeshRenderInfo light = new MeshRenderInfo();
	public MeshRenderInfo shadow = new MeshRenderInfo();
	public int shadowRayCount;
	public Vector2 Position {
		get {
			return transform.position;
		}
	}
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
			Vector2 dir = AngleToNormVec(currentAngle) * radius;
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

		UpdateShadowMesh();
	}
	Vector2 AngleToNormVec(float angle) {
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
	} 
	void UpdateShadowMesh() {
		shadow.Setup(this, "ShadowMesh", shadowMaterial);
		shadow.mesh.Clear();
		Vector2? lastHit = null;
		Vector2 lastStart = Vector2.zero;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color> colors = new List<Color>();
		float stepAngle = Mathf.PI * 2 / shadowRayCount;
		float currentAngle = 0;
		Action<Vector2> addVertex = v2 => {
			Vector3 vertex = new Vector3(v2.x, v2.y, transform.position.z);
			vertex = transform.InverseTransformPoint(vertex);
			vertices.Add(vertex);
		};
		for(int i = 0; i < shadowRayCount; i++) {
			currentAngle += stepAngle;
			Vector2 dir = -AngleToNormVec(currentAngle) * radius;
			Vector2 start = Position - dir;
			var hit = Physics2D.Raycast(start, dir, dir.magnitude, colliderLayer);
			if(hit) {
				addVertex(hit.point);
				colors.Add(Color.black);
				addVertex(start);
				colors.Add(Color.black);
				if(lastHit != null) {
					int lastOrigin = vertices.Count - 4;
					triangles.Add(lastOrigin + 0);
					triangles.Add(lastOrigin + 3);
					triangles.Add(lastOrigin + 1);
					triangles.Add(lastOrigin + 0);
					triangles.Add(lastOrigin + 2);
					triangles.Add(lastOrigin + 3);
				}
				lastHit = hit.point;
				lastStart = start;
			}
			else {
				lastHit = null;
			}
		}
		if(vertices.Count > 2) {
			shadow.mesh.SetVertices(vertices);
			shadow.mesh.SetTriangles(triangles, 0);
			shadow.mesh.SetColors(colors);
			shadow.mesh.RecalculateNormals();
		}
	}
}
