using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public CircleHitPoint circleHitPoint = new CircleHitPoint();
	public Color lightColor;
	public Material lightMaterial;
	public Material shadowMaterial;
	public Mesh lightMesh = null;
	public Mesh shadowMesh = null;
	public Vector2 Position {
		get {
			return transform.position;
		}
	}
	void UpdateLightMesh() {
		if(lightMesh == null) lightMesh = new Mesh();
		lightMesh.MarkDynamic();
		lightMesh.Clear();
		List<Vector3> vertices = new List<Vector3>{
			new Vector3(-1, -1, 0),
			new Vector3(1, -1, 0),
			new Vector3(-1, 1, 0),
			new Vector3(1, 1, 0),
		};
		for(int i = 0; i < vertices.Count; i++) {
			vertices[i] *= circleHitPoint.radius;
		}
		List<int> triangles = new List<int>{0, 2, 1, 2, 3, 1};
		lightMesh.SetVertices(vertices);
		lightMesh.SetTriangles(triangles, 0);
		lightMesh.RecalculateNormals();
	}
	void UpdateShadowMesh() {
		if(shadowMesh == null) shadowMesh = new Mesh();
		shadowMesh.MarkDynamic();
		shadowMesh.Clear();
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		CircleHitPoint.HitInfo? previous = null;
		foreach(var current in circleHitPoint.RaycastPoints()) {
			if(!current.hit2D) {
				previous = null;
			}
			else {
				if(previous != null) {
					// Consume previous is A, current is B
					if(previous.Value.hit2D.collider == current.hit2D.collider) {
						Vector2 A = circleHitPoint.Position(previous.Value);
						Vector2 B = circleHitPoint.Position(current);
						Vector2 C = circleHitPoint.center;
						Vector2 AB = B - A;
						Vector2 normal = new Vector2(AB.y, -AB.x).normalized;
						Vector2 CA = A - C;
						float dis = Vector2.Dot(CA, normal);
						float scale = circleHitPoint.radius / dis;
						Func<Vector2, Vector2> project = v2 => (v2 - C) * scale + C;
						triangles.Add(vertices.Count + 0);
						triangles.Add(vertices.Count + 3);
						triangles.Add(vertices.Count + 2);
						triangles.Add(vertices.Count + 0);
						triangles.Add(vertices.Count + 1);
						triangles.Add(vertices.Count + 3);
						vertices.Add(WorldV2ToLocalV3(A));
						vertices.Add(WorldV2ToLocalV3(B));
						vertices.Add(WorldV2ToLocalV3(project(A)));
						vertices.Add(WorldV2ToLocalV3(project(B)));
					}
				}
				previous = current;
			}
		}
		shadowMesh.SetVertices(vertices);
		shadowMesh.SetTriangles(triangles, 0);
		shadowMesh.RecalculateNormals();
	}
	Vector3 WorldV2ToLocalV3(Vector2 v2) {
		return transform.InverseTransformPoint(v2.x, v2.y, transform.position.z);
	}
	void Update() {
		circleHitPoint.center = Position;
		UpdateLightMesh();
		UpdateShadowMesh();

	}
	public void DrawLightMesh(ref CommandBuffer commandBuffer, int shadowMapId) {
		commandBuffer.SetGlobalVector("_LightPos", Position);
		commandBuffer.SetGlobalColor("_LightColor", lightColor);
		commandBuffer.SetGlobalFloat("_LightMaxDis", circleHitPoint.radius);
		commandBuffer.SetGlobalTexture("_ShadowMap", shadowMapId);
		LightPipe.DrawMesh(commandBuffer, lightMesh, transform, lightMaterial);
	}
	public void DrawShadowMesh(ref CommandBuffer commandBuffer) {
		LightPipe.DrawMesh(commandBuffer, shadowMesh, transform, shadowMaterial);
	}
}

[Serializable]
public class CircleHitPoint {
	public float radius;
	public LayerMask colliderLayer;
    public float binaryMaxDegree = 5;
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
	public float NormedHitRadius(HitInfo info) {
		return Mathf.Clamp01((Position(info) - center).magnitude / radius);
	}
	private IEnumerable<HitInfo> BinaryFindEdgeAndReturnPoint(HitInfo info1, HitInfo info2) {
		if(rayCount < 3) rayCount = 3;
		Func<RaycastHit2D, RaycastHit2D, bool> hitSame = (hit1, hit2) => {
			if(!hit1 && !hit2) {
				return true;
			}
			else if(hit1 ^ hit2) {
				return false;
			}
			else {
				return hit1.collider == hit2.collider;
			}
		};
		Func<RaycastHit2D, RaycastHit2D, bool> normalSame = (hit1, hit2) => {

			return (!hit1 && !hit2) || Mathf.Approximately((hit1.normal - hit2.normal).magnitude, 0);
		};
		if((hitSame(info1.hit2D, info2.hit2D) && normalSame(info1.hit2D, info2.hit2D)) 
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