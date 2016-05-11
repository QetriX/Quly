namespace com.qetrix.apps.quly.libs
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;

	public class Util
	{
		static Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();

		public static void loadSfx(string name)
		{
			sfx.Add(name, Resources.Load<AudioClip>("Sfx/" + name));
		}

		public static void loadSfx(string[] names)
		{
			//string[] names = { "bounce", "crash", "pickup", "noenergy", "jump", "laser", "checkpoint" };
			sfx = new Dictionary<string, AudioClip>();
			foreach (string s in names) sfx.Add(s, Resources.Load<AudioClip>("Sfx/" + s));
		}

		public static AudioSource playClipAtPoint(AudioClip clip, Vector3 pos)
		{
			return playClipAtPoint(clip, pos, true);
		}

		public static AudioSource playClipAtPoint(AudioClip clip, Vector3 pos, bool mySound)
		{
			GameObject tempGO = new GameObject("TempAudio_" + clip.name);
			tempGO.transform.position = pos;
			AudioSource aSource = tempGO.AddComponent<AudioSource>();
			if (!mySound) aSource.spatialBlend = 1;
			aSource.clip = clip;
			aSource.Play();
			MonoBehaviour.Destroy(tempGO, clip.length);
			return aSource;
		}

		public static AudioSource playClipAtPoint(string clip, Vector3 pos, bool mySound)
		{
			return playClipAtPoint(sfx[clip], pos, mySound);
		}

		public static AudioSource playClipAtPoint(string clip, Vector3 pos)
		{
			return playClipAtPoint(sfx[clip], pos, true);
		}

		public static void log(object obj)
		{
			Debug.Log(obj);
		}

		public static void desintegrate(GameObject go)
		{
			MeshFilter MF = go.GetComponent<MeshFilter>();
			MeshRenderer MR = go.GetComponent<MeshRenderer>();
			Mesh M = MF.mesh;
			Vector3[] verts = M.vertices;
			Vector3[] normals = M.normals;
			Vector2[] uvs = M.uv;

			int[] indices = M.GetTriangles(0);
			for (int i = 0; i < indices.Length; i += 3) {
				Vector3[] newVerts = new Vector3[3];
				Vector3[] newNormals = new Vector3[3];
				Vector2[] newUvs = new Vector2[3];
				for (int n = 0; n < 3; n++) {
					int index = indices[i + n];
					newVerts[n] = verts[index];
					newUvs[n] = uvs[index];
					newNormals[n] = normals[index];
				}
				Mesh mesh = new Mesh();
				mesh.vertices = newVerts;
				mesh.normals = newNormals;
				mesh.uv = newUvs;

				mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };

				GameObject triangle = new GameObject(go.name + " part " + (i / 3));
				triangle.transform.position = go.transform.position;
				triangle.transform.rotation = go.transform.rotation;
				triangle.AddComponent<MeshRenderer>().material = MR.materials[0];
				triangle.AddComponent<MeshFilter>().mesh = mesh;
				triangle.AddComponent<BoxCollider>();
				triangle.AddComponent<Rigidbody>();
				MonoBehaviour.Destroy(triangle, UnityEngine.Random.Range(0.0f, 3.0f));
			}
			MR.enabled = false;
			MonoBehaviour.Destroy(go);
		}
	}

	/// <summary>
	/// Serializable Color
	/// </summary>
	[System.Serializable]
	public class S_Color
	{
		public int r;
		public int g;
		public int b;
		public int a;

		public S_Color(int rr, int gg, int bb, int aa)
		{
			r = rr;
			g = gg;
			b = bb;
			a = aa;
		}

		public S_Color(Color c)
		{
			r = (int)(c.r * 255f);
			g = (int)(c.g * 255f);
			b = (int)(c.b * 255f);
			a = (int)(c.a * 255f);
		}

		public Color toColor()
		{
			return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
		}
	}

	/// <summary>
	/// Serializable Transform
	/// </summary>
	[System.Serializable]
	public class S_Transform
	{
		public string name = "";
		public string instance = "";
		public float px = 0;
		public float py = 0;
		public float pz = 0;
		public float rx = 0;
		public float ry = 0;
		public float rz = 0;
		public float sx = 1;
		public float sy = 1;
		public float sz = 1;

		public S_Transform(float _px, float _py, float _pz)
		{
			px = _px;
			py = _py;
			pz = _pz;
		}

		public S_Transform(float _px, float _py, float _pz, float _rx, float _ry, float _rz)
		{
			px = _px;
			py = _py;
			pz = _pz;
			rx = _rx;
			ry = _ry;
			rz = _rz;
		}

		public Vector3 position()
		{
			return new Vector3(px, py, pz);
		}

		public Quaternion rotation()
		{
			return Quaternion.Euler(rx, ry, rz);
		}

		public Vector3 scale()
		{
			return new Vector3(sx, sy, sz);
		}

		public GameObject gameObject()
		{
			GameObject go = new GameObject();
			go.transform.localPosition = position();
			go.transform.localRotation = rotation();
			go.transform.localScale = scale();
			return go;
		}
	}
}
