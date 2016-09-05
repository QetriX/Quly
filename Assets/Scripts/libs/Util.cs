namespace com.qetrix.apps.quly.libs
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	public class Util
	{
		static Dictionary<string, AudioClip> sfx = new Dictionary<string, AudioClip>();

		public static void initSfx(string[] names)
		{
			//string[] names = { "bounce", "crash", "pickup", "noenergy", "jump", "laser", "checkpoint" };
			sfx = new Dictionary<string, AudioClip>();
			foreach (string name in names) if (!sfx.ContainsKey(name)) sfx.Add(name, Resources.Load<AudioClip>("Sfx/" + name)); else Debug.Log("Warning: sfx \"" + name + "\" is already loaded");
		}

		public static void addSfx(string name)
		{
			if (!sfx.ContainsKey(name)) sfx.Add(name, Resources.Load<AudioClip>("Sfx/" + name)); else Debug.Log("Warning: sfx \"" + name + "\" is already loaded");
		}

		public static void removeSfx(string name)
		{
			sfx.Remove(name);
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
			if (sfx.ContainsKey(clip)) return playClipAtPoint(sfx[clip], pos, mySound);
			Debug.Log("playClipAtPoint warn: No clip \"" + clip + "\" loaded");
			return null;
		}

		public static AudioSource playClipAtPoint(string clip, Vector3 pos)
		{
			if (sfx.ContainsKey(clip)) return playClipAtPoint(sfx[clip], pos, true);
			Debug.Log("playClipAtPoint warn: No clip \"" + clip + "\" loaded");
			return null;
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
				GameObject.Destroy(triangle, UnityEngine.Random.Range(0.0f, 3.0f));
			}
			MR.enabled = false;
			GameObject.Destroy(go);
		}

		public static string randomString(int length)
		{
			var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
			var stringChars = new char[length];

			for (int i = 0; i < stringChars.Length; i++) {
				stringChars[i] = chars[(int)Math.Floor(UnityEngine.Random.value * chars.Length)];
			}

			return new String(stringChars);
		}

		// Usage: parseData("{x:10,y:20}", MethodName);
		public static void parseData(string data, Func<Dictionary<string, string>, bool> func)
		{
			string[] d = data.Trim().Replace("\r", "").Replace("}", "\n}").Replace("{", "{\n").Replace("\n", "\n\n").Split('\n');
			var vals = new Dictionary<string, string>();

			for (int row = 0; row < d.Length; row++) {
				var line = d[row].Trim();
				if (line.Length == 0) continue;

				if (line.Substring(0, 1) == "}") {
					func.DynamicInvoke(vals);
					vals = new Dictionary<string, string>();
				} else {
					var keyval = line.Split(new char[] { ':' }, 2);
					string key = keyval[0].Trim().ToLower();
					if (keyval.Length == 2 && !vals.ContainsKey(key)) vals.Add(key, keyval[1].Trim());
				}
			}
		}

		// If num is between min and max, return num. If num is bigger, than max, return max. If num i smaller, than min, returns min.
		public static float between(float num, float min, float max)
		{
			return Mathf.Min(Mathf.Max(num, min), max);
		}

		public static Vector2 worldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
		{
			//Vector position (percentage from 0 to 1) considering camera size.
			//For example (0,0) is lower left, middle is (0.5,0.5)
			Vector2 temp = camera.WorldToViewportPoint(position);

			//Calculate position considering our percentage, using our canvas size
			//So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
			temp.x *= canvas.sizeDelta.x;
			temp.y *= canvas.sizeDelta.y;

			//The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
			//But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
			//We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
			//returned value will still be correct.

			temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
			temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

			return temp;
		}


		/// <summary>
		/// Fade gameObject in or out
		/// NOTE: Set the material shader to: Standard > Fade 
		/// Usage - Fade out: StartCoroutine(FadeTo(0.0f, 1.0f));
		/// Usage - Fade in: StartCoroutine(FadeTo(1.0f, 1.0f));
		/// </summary>
		/// <param name="t">Transform of gameObject to fade</param>
		/// <param name="targetAlpha">Target alpha opacity - from 0 (transparent) to 1 (opaque)</param>
		/// <param name="duration">Duration of the fade, in seconds</param>
		/// <param name="delayBefore">Delay before the fade begins, in seconds</param>
		/// <param name="destroyAfter">Destroy t.gameObject after the fade?</param>
		/// <returns></returns>
		public static IEnumerator fade(Transform t, float targetAlpha, float duration, float delayBefore, bool destroyAfter)
		{
			if (delayBefore > 0) yield return new WaitForSeconds(delayBefore);

			Renderer sr = t.GetComponent<Renderer>();
			float diffAlpha = (targetAlpha - sr.material.color.a);

			float counter = 0;
			while (counter < duration) {
				float alphaAmount = sr.material.color.a + (Time.deltaTime * diffAlpha) / duration;
				sr.material.color = new Color(sr.material.color.r, sr.material.color.g, sr.material.color.b, alphaAmount);
				counter += Time.deltaTime;
				yield return null;
			}
			sr.material.color = new Color(sr.material.color.r, sr.material.color.g, sr.material.color.b, targetAlpha);

			if (destroyAfter) GameObject.Destroy(t.gameObject);
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

		public S_Transform(float _px, float _pz)
		{
			px = _px;
			py = -1;
			pz = _pz;
		}

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
