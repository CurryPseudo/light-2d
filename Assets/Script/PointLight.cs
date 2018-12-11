using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;

[ExecuteInEditMode]
public class PointLight : MonoBehaviour {
	public float volumeRadius = 0.3f;
	public CircleHitPoint circleHitPoint = new CircleHitPoint();
	public Color lightColor;
	public Material lightMaterial;
	public Material shadowMaterial;
	public Mesh shadowMesh = null;
	public Vector2 Position {
		get {
			return transform.position;
		}
	}
	void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(Position, volumeRadius);
		
	}
	Mesh GenLightMesh() {
		Mesh lightMesh = new Mesh();
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
		return lightMesh;
	}
	Mesh GenShadowMesh() {
		Mesh shadowMesh = new Mesh();
		shadowMesh.MarkDynamic();
		shadowMesh.Clear();
		List<Vector3> vertices = new List<Vector3>();
		List<Vector2> apos = new List<Vector2>();
		List<Vector2> bpos = new List<Vector2>();
		List<int> triangles = new List<int>();
		foreach(var edge in circleHitPoint.ExtractEdge()) {
			Vector2 A = edge.A;
			Vector2 B = edge.B;
			Vector2 C = circleHitPoint.center;
			Func<Vector2, Vector2, Vector2> normal = (c, p) => {
				Vector2 dir = p - c;
				return new Vector2(-dir.y, dir.x).normalized;
			};
			Vector2 ABnormal = -normal(A, B);
			Vector2 CAO = normal(C, A) * volumeRadius + C;
			Vector2 CBO = -normal(C, B) * volumeRadius + C;
			Func<Vector2, Vector2, Vector2, Vector2> project = (n, origin, point) => {
				float disToPoint = Vector2.Dot(origin - point, n);
				//float disToPoint = Vector2.Dot(point - origin, n);
				//float delta = circleHitPoint.radius - (disToC + disToPoint);
				disToPoint = Mathf.Abs(disToPoint);
				float delta = circleHitPoint.radius - disToPoint;
				delta = Mathf.Max(0, delta);
				float scale = (delta + disToPoint) / disToPoint;
				Debug.Assert(scale >= 1);
				return (point - origin) * scale + origin;
			};
			if(Vector2.Dot((B - A), normal(A, CAO)) >= 0) {
				Vector2 CBI = normal(C, B) * volumeRadius + C;
				triangles.Add(vertices.Count + 0);
				triangles.Add(vertices.Count + 2);
				triangles.Add(vertices.Count + 1);
				vertices.Add(WorldV2ToLocalV3(B));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project((B - C).normalized, CBI, B)));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project((B - C).normalized, CBO, B)));
				apos.Add(A);
				bpos.Add(B);
			}
			else if(Vector2.Dot((A - B), normal(CBO, B)) >= 0) {
				Vector2 CAI = -normal(C, A) * volumeRadius + C;
				triangles.Add(vertices.Count + 0);
				triangles.Add(vertices.Count + 2);
				triangles.Add(vertices.Count + 1);
				vertices.Add(WorldV2ToLocalV3(A));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project((A - C).normalized, CAO, A)));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project((A - C).normalized, CAI, A)));
				apos.Add(A);
				bpos.Add(B);
			}
			else {
				triangles.Add(vertices.Count + 0);
				triangles.Add(vertices.Count + 1);
				triangles.Add(vertices.Count + 3);
				triangles.Add(vertices.Count + 0);
				triangles.Add(vertices.Count + 3);
				triangles.Add(vertices.Count + 2);
				vertices.Add(WorldV2ToLocalV3(A));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(B));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project(ABnormal, CAO, A)));
				apos.Add(A);
				bpos.Add(B);
				vertices.Add(WorldV2ToLocalV3(project(ABnormal, CBO, B)));
				apos.Add(A);
				bpos.Add(B);
			}
		}
		shadowMesh.SetVertices(vertices);
		shadowMesh.SetTriangles(triangles, 0);
		shadowMesh.SetUVs(0, apos);
		shadowMesh.SetUVs(1, bpos);
		shadowMesh.RecalculateNormals();
		this.shadowMesh = shadowMesh;
		return shadowMesh;
	}
	Vector3 WorldV2ToLocalV3(Vector2 v2) {
		return transform.InverseTransformPoint(v2.x, v2.y, transform.position.z);
	}
	void Update() {
		circleHitPoint.center = Position;
	}
	public void DrawLightMesh(ref CommandBuffer commandBuffer, int shadowMapId) {
		Mesh lightMesh = GenLightMesh();
		commandBuffer.SetGlobalVector("_LightPos", Position);
		commandBuffer.SetGlobalColor("_LightColor", lightColor);
		commandBuffer.SetGlobalFloat("_LightMaxDis", circleHitPoint.radius);
		commandBuffer.SetGlobalTexture("_ShadowMap", shadowMapId);
		LightPipe.DrawMesh(commandBuffer, lightMesh, transform, lightMaterial);
	}
	public void DrawShadowMesh(ref CommandBuffer commandBuffer) {
		Mesh shadowMesh = GenShadowMesh();
		commandBuffer.SetGlobalVector("_LightPos", Position);
		commandBuffer.SetGlobalFloat("_LightVolumeRadius", volumeRadius);
		LightPipe.DrawMesh(commandBuffer, shadowMesh, transform, shadowMaterial);
	}
}

[Serializable]
public class CircleHitPoint {
	public float radius;
	public LayerMask colliderLayer;
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
	private bool HitSame(RaycastHit2D hit1, RaycastHit2D hit2) {
		if(!hit1 && !hit2) {
			return true;
		}
		else if(hit1 ^ hit2) {
			return false;
		}
		else {
			return hit1.collider == hit2.collider;
		}
	}
	private bool NormalSame(RaycastHit2D hit1, RaycastHit2D hit2) {
		return (!hit1 && !hit2) || Mathf.Approximately((hit1.normal - hit2.normal).magnitude, 0);
	}
	private Edge? GetValidEdge(PolygonCollider2D polygon, Vector2 previous, Vector2 current) {
		previous = polygon.transform.TransformPoint(previous + polygon.offset);
		current = polygon.transform.TransformPoint(current + polygon.offset);
		Vector2 dir = current - previous;
		Vector2 normal = new Vector2(-dir.y, dir.x).normalized;
		bool front = Vector2.Dot((current - center), normal) > 0;
		if(front) {
			return new Edge(current, previous);
		}
		else {
			return new Edge(previous, current);
		}
	}
	public IEnumerable<Edge> ExtractEdge() {
		Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
		foreach(var collider in colliders) {
			PolygonCollider2D polygon = collider.GetComponent<PolygonCollider2D>();
			if(polygon != null) {
				for(int i = 0; i < polygon.pathCount; i++) {
					Vector2? first = null;
					Vector2? last = null;
					foreach(var point in polygon.GetPath(i)) {
						if(first == null) {
							first = point;
						}
						if(last != null) {
							Edge? edge = GetValidEdge(polygon, last.Value, point);
							if(edge != null) {
								yield return edge.Value;
							}
						}
						last = point;
					}
					if(first != null && first.Value != last.Value) {
							Edge? edge = GetValidEdge(polygon, last.Value, first.Value);
							if(edge != null) {
								yield return edge.Value;
							}
					}
				}
			}
		}
		yield break;
	}
}
public struct Edge {
	public Vector2 A;
	public Vector2 B;

    public Edge(Vector2 a, Vector2 b)
    {
        A = a;
        B = b;
    }
}