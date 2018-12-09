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
	public Mesh lightMesh = null;
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
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();
		Func<Vector2, Vector2, bool> vectorEqual = (v1, v2) => Mathf.Approximately(0, (v1 - v2).magnitude);
		System.Action<Edge, bool, bool> createEdgeShadow = (edge, first, last) => {
			Vector2 A = edge.A;
			Vector2 B = edge.B;
			Vector2 C = circleHitPoint.center;
			Func<Vector2, Vector2, Vector2> normal = (c, p) => {
				Vector2 dir = p - c;
				return new Vector2(-dir.y, dir.x).normalized;
			};
			Vector2 ABnormal = -normal(A, B);
			Vector2 CA = A - C;
			float dis = Vector2.Dot(CA, ABnormal);
			float scale = circleHitPoint.radius / dis;
			Vector2 CAO = normal(C, A) * volumeRadius + C;
			Vector2 CAI = -normal(C, A) * volumeRadius + C;
			Vector2 CBI = normal(C, B) * volumeRadius + C;
			Vector2 CBO = -normal(C, B) * volumeRadius + C;
			Func<Vector2, Vector2, Vector2> project = (c, v2) => (v2 - c) * scale + c;
			triangles.Add(vertices.Count + 0);
			triangles.Add(vertices.Count + 3);
			triangles.Add(vertices.Count + 2);
			triangles.Add(vertices.Count + 1);
			triangles.Add(vertices.Count + 5);
			triangles.Add(vertices.Count + 4);
			triangles.Add(vertices.Count + 0);
			triangles.Add(vertices.Count + 1);
			triangles.Add(vertices.Count + 4);
			triangles.Add(vertices.Count + 0);
			triangles.Add(vertices.Count + 4);
			triangles.Add(vertices.Count + 3);
			vertices.Add(WorldV2ToLocalV3(A));
			uvs.Add(new Vector2(0,0));
			vertices.Add(WorldV2ToLocalV3(B));
			uvs.Add(new Vector2(0,0));
			vertices.Add(WorldV2ToLocalV3(project(CAO, A)));
			if(first) {
				uvs.Add(new Vector2(1,1));
			}
			else {
				uvs.Add(new Vector2(0,1));
			}
			vertices.Add(WorldV2ToLocalV3(project(CAI, A)));
			uvs.Add(new Vector2(0,1));
			vertices.Add(WorldV2ToLocalV3(project(CBI, B)));
			uvs.Add(new Vector2(0,1));
			vertices.Add(WorldV2ToLocalV3(project(CBO, B)));
			if(last) {
				uvs.Add(new Vector2(1,1));
			}
			else {
				uvs.Add(new Vector2(0,1));
			}
		};
		foreach(var edgeGroup in circleHitPoint.ExtractEdge(circleHitPoint.RaycastPoints())) {
			foreach(var edge in edgeGroup) {
				createEdgeShadow(edge, true, true);
			}
			/*
			if(edgeGroup.Count == 1) {
				createEdgeShadow(edgeGroup[0], true, true);
			}
			else if(edgeGroup.Count > 0){
				createEdgeShadow(edgeGroup[0], true, false);
				for(int i = 1; i < edgeGroup.Count - 1; i++) {
				createEdgeShadow(edgeGroup[i], false, false);
				}
				createEdgeShadow(edgeGroup[edgeGroup.Count - 1], false, true);
			}
			*/
		}
		shadowMesh.SetVertices(vertices);
		shadowMesh.SetTriangles(triangles, 0);
		shadowMesh.SetUVs(0, uvs);
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
	private IEnumerable<HitInfo> BinaryFindEdgeAndReturnPoint(HitInfo info1, HitInfo info2) {
		if(rayCount < 3) rayCount = 3;
		if((HitSame(info1.hit2D, info2.hit2D) && NormalSame(info1.hit2D, info2.hit2D)) 
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
	public IEnumerable<List<Edge>> ExtractEdge(IEnumerable<HitInfo> infos) {
		HitInfo? previous = null;
		HitInfo? last = null;
		List<Edge> edgeGroup = new List<Edge>();
		foreach(var current in infos) {
			if(previous != null) {
				if(!current.hit2D) {
					edgeGroup.Add(new Edge(Position(previous.Value), Position(last.Value)));
					yield return edgeGroup;
					edgeGroup.Clear();
					previous = null;
					last = null;
				}
				else {
					if(HitSame(previous.Value.hit2D, current.hit2D) && NormalSame(previous.Value.hit2D, current.hit2D)) {
						last = current;
					}
					else {
						edgeGroup.Add(new Edge(Position(previous.Value), Position(last.Value)));
						previous = current;
						last = current;
					}
				}
			}
			else {
				if(current.hit2D) {
					previous = current;
					last = current;
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