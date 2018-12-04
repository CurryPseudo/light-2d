using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public CircleHitPoint circleHitPoint = new CircleHitPoint();
	public Color lightColor;
	public Material lightMaterial;
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
	public Vector2 Position {
		get {
			return transform.position;
		}
	}
	void Update() {
		light.Setup(this, "LightMesh", lightMaterial);
		light.mesh.Clear();
		List<Vector3> vertices = new List<Vector3>();
		vertices.Add(Vector3.zero);
		vertices.Add(Vector3.zero);
		List<int> triangles = new List<int>();
		List<Color> colors = new List<Color>();
		colors.Add(lightColor);
		colors.Add(Color.black);
		circleHitPoint.center = Position;
		CircleHitPoint.HitInfo? lastHitInfo = null;
		Func<CircleHitPoint.HitInfo, Vector3> hitToVertex = info =>	{
			Vector2 point = circleHitPoint.Position(info);
			Vector3 pointV3 = new Vector3(point.x, point.y, transform.position.z);
			return transform.InverseTransformPoint(pointV3);
		};
		CircleHitPoint.HitInfo firstHitInfo = new CircleHitPoint.HitInfo();
		foreach(var hitPointInfo in circleHitPoint.RaycastPoints()) {
			if(lastHitInfo != null) {
				if(hitPointInfo.hit2D && lastHitInfo.Value.hit2D) {
					triangles.Add(1);
				}
				else {
					triangles.Add(0);
				}
				triangles.Add(vertices.Count + 0);
				triangles.Add(vertices.Count - 1);
				vertices.Add(hitToVertex(hitPointInfo));
				colors.Add(Color.black);
			}
			else {
				firstHitInfo = hitPointInfo;
			}
			lastHitInfo = hitPointInfo;
		}
		if(firstHitInfo.hit2D && lastHitInfo.Value.hit2D) {
			triangles.Add(1);
		}
		else {
			triangles.Add(0);
		}
		triangles.Add(2);
		triangles.Add(vertices.Count - 1);
		light.mesh.SetVertices(vertices);
		light.mesh.SetTriangles(triangles, 0);
		light.mesh.SetColors(colors);
		light.mesh.RecalculateNormals();
	}
	Vector2 AngleToNormVec(float angle) {
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
	} 
}

[Serializable]
public class CircleHitPoint {
	public float radius;
	public LayerMask colliderLayer;
    public float binaryMaxDistance = 0.2f;
    public float binaryMaxDegree = 5;
    public float binaryMaxNormalDelta = 0.2f;
	public int rayCount;
	public Vector2 center;
	public struct HitInfo {
		public RaycastHit2D hit2D;
		public float angle;

        public HitInfo(RaycastHit2D hit2D, float angle)
        {
            this.hit2D = hit2D;
            this.angle = angle;
        }
		public Vector2 Position(Vector2 center, float radius) {
			if(hit2D) {
				return hit2D.point;
			}
			else {
				return center + CircleHitPoint.Degree2Dir(angle) * radius;
			}
		}
    }
	
	private static Vector2 Degree2Dir(float degree) {
		float rayRad = Mathf.Deg2Rad * degree;
		Vector2 dir = new Vector2(Mathf.Cos(rayRad), Mathf.Sin(rayRad));
		return dir;
	}
	private RaycastHit2D AngleRayCast(float angle) {
		var rayDir = Degree2Dir(angle);
		var hit = Physics2D.Raycast(center, rayDir, radius, colliderLayer);
		return hit;
	}
	public Vector2 Position(HitInfo info) {
		return info.Position(center, radius);
	}
	private IEnumerable<HitInfo> BinaryFindEdgeAndReturnPoint(HitInfo info1, HitInfo info2) {
		if(rayCount < 3) rayCount = 3;
		Func<RaycastHit2D, float> hitDis = hit => hit.collider == null ? radius : hit.distance;
		Func<RaycastHit2D, Vector2> hitNormal = hit => hit.collider == null ? Vector2.zero : hit.normal;
		if((Mathf.Abs(hitDis(info1.hit2D) - hitDis(info2.hit2D)) < binaryMaxDistance 
			&& (hitNormal(info1.hit2D) - hitNormal(info2.hit2D)).magnitude < binaryMaxNormalDelta) 
			|| info2.angle - info1.angle < binaryMaxDegree) {
			yield return new HitInfo(info2.hit2D, info2.angle);
			yield break;
		}
		var midDegree = (info1.angle + info2.angle) / 2;
		var midHit = AngleRayCast(midDegree);
		var midHitInfo = new HitInfo(midHit, midDegree);
		foreach(var hitInfo in BinaryFindEdgeAndReturnPoint(info1, midHitInfo)) {
			yield return hitInfo;
		}
		foreach(var hitInfo in BinaryFindEdgeAndReturnPoint(midHitInfo, info2)) {
			yield return hitInfo;
		}
	}
    public IEnumerable<HitInfo> RaycastPoints() {
        float deltaDegree = 360.0f / (float) rayCount;    
        float lastDegree = 0;
        RaycastHit2D lastHit = new RaycastHit2D();
        for(int i = 0; i < rayCount + 1; i++) {
            float rayDegree = deltaDegree * i;
            var hit = AngleRayCast(rayDegree);
            if(i > 0) {
				foreach(var hitInfo in BinaryFindEdgeAndReturnPoint(new HitInfo(lastHit, lastDegree), new HitInfo(hit, rayDegree))) {
					yield return hitInfo;
				}
            }
            else {
				yield return new HitInfo(hit, rayDegree);
            }
            lastHit = hit;
            lastDegree = rayDegree;
            

        }
    }
}