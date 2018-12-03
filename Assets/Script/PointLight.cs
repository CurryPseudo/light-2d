using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public float radius;
	public int triangleCount;
	private MeshFilter meshFilter;
	private Mesh mesh;
	void Awake() {
		meshFilter = GetComponent<MeshFilter>();
	}
	void Update() {
		if(mesh == null) {
			mesh = new Mesh();
		}
		if(triangleCount < 3) {
			triangleCount = 3;
		}
		mesh.Clear();
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
		colors[0] = Color.white;
		for(int i = 0; i < triangleCount + 1; i++) {
			colors[i + 1] = Color.black;
		}
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;

		var meshRenderer = GetComponent<MeshRenderer>();
	}
}
