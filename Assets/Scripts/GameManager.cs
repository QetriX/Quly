namespace com.qetrix.apps.quly
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using UnityStandardAssets.CrossPlatformInput;
	using libs;

	public class GameManager : MonoBehaviour
	{
		public static GameManager instance;

		public GameObject busyIndicator;
		Text clock;
		Sun daytime;
		public static Scene scene;
		public string sceneName;
		bool isSaved = false;
		bool _joystick = false;
		GameObject quleUI;

		public static bool gameControls = true; /// Game controls are active (false = menu, modal dialog or anything but the game itself)
		public static bool worldClock = true; /// Showing world clock (false = race time, quest timer or anything but the world time itself)

		public List<Player> players = new List<Player>();
		Dictionary<string, libs.Material> _materials = new Dictionary<string, libs.Material>();
		Dictionary<string, Item> _items = new Dictionary<string, Item>();
		Dictionary<string, Recipe> _recipes = new Dictionary<string, Recipe>();

		void Awake()
		{
			instance = this;
			Application.targetFrameRate = 30;
			sceneName = "Scene001";

			string[] sfx = { "bounce", "crash", "pickup", "noenergy", "jump", "laser", "teleport", "boom", "modwinopen", "modwinclose" };
			Util.initSfx(sfx);
		}

		void Start()
		{
			/// UI setup
			busyIndicator = GameObject.Find("BusyIndicator");
			showLoading();

			/// Data setup
			loadMaterials();
			loadItems();
			loadRecipes();

			/// Environment setup
			clock = GameObject.Find("Clock").GetComponent<Text>();
			daytime = GameObject.Find("Sun").GetComponent<Sun>();

			/// Scene setup
			Type t = Type.GetType("com.qetrix.apps.quly." + sceneName);
			scene = (Scene) Activator.CreateInstance(t);
			MethodInfo method = t.GetMethod("init", BindingFlags.Instance | BindingFlags.Public);
			if (method != null) method.Invoke(scene, null);

			/// Player setup
			var cam = GameObject.Find(players[0].name() + "/QulyCam").GetComponent<QulyCam>();
			cam.player(players[1]);

			/// Qules setup
			foreach (Qule q in scene.qules) {
				Vector3 pos = q.transform().position();
				if (pos.y == -1) {
					pos = new Vector3(pos.x, Terrain.activeTerrain.SampleHeight(pos) + 2, pos.z);
					Debug.Log("New pos:" + pos + " for " + q.name());
				}
				QuleMB qq = (Instantiate(Resources.Load("Prefabs/Qule"), pos, q.transform().rotation()) as GameObject).GetComponent<QuleMB>();
				qq.q(q);
				qq.tag = (cam.playerFamily() == q.family() ? "Family" : "Neutral"); // TODO!!!!
				if (q.npc() != null) {
					qq.gameObject.AddComponent(Type.GetType("com.qetrix.apps.quly." + q.npc(), true, true));
					qq.gameObject.AddComponent<EnergyManager>().scriptCalled = q.npc();
					qq.tag = "Neutral";
					qq.GetComponent<Rigidbody>().sleepThreshold = 1f;
				}
				qq = null;
			}
			hideLoading();
		}

		public void setClock(string time)
		{
			clock.text = time;
		}

		void loadMaterials()
		{
			TextAsset txt = (TextAsset)Resources.Load("Data/materials", typeof(TextAsset));
			Util.parseData(txt.text, material);
			Debug.Log("Loaded " + _materials.Count + " materials");
		}

		bool material(Dictionary<string, string> data)
		{
			var mat = new libs.Material(data);
			_materials.Add(mat.name().Trim().ToLower(), mat);
			return true;
		}

		public libs.Material material(string name)
		{
			name = name.Trim().ToLower();
			if (!_materials.ContainsKey(name)) {
				Debug.Log("Material " + name + " not loaded (" + _materials.Count + " materials loaded)");
				return null;
			}
			return _materials[name];
		}

		void loadItems()
		{
			TextAsset txt = (TextAsset)Resources.Load("Data/items", typeof(TextAsset));
			Util.parseData(txt.text, item);
			Debug.Log("Loaded " + _items.Count + " items");
		}

		bool item(Dictionary<string, string> data)
		{
			var itm = new Item(data);
			_items.Add(itm.name().Trim().ToLower(), itm);
			return true;
		}

		public Item item(string name)
		{
			name = name.Trim().ToLower();
			if (!_items.ContainsKey(name)) {
				Debug.Log("Item " + name + " not loaded (" + _items.Count + " items loaded)");
				return null;
			}
			return _items[name];
		}

		void loadRecipes()
		{
			TextAsset txt = (TextAsset)Resources.Load("Data/recipes", typeof(TextAsset));
			string[] content = txt.text.Replace("\r", "").Split('\n');
			Debug.Log("Loaded " + content.Length + " recipes");
		}

		bool recipe(Dictionary<string, string> data)
		{
			var rcp = new Recipe(data);
			_recipes.Add(rcp.name().Trim().ToLower(), rcp);
			return true;
		}

		public Recipe recipe(string name)
		{
			name = name.Trim().ToLower();
			if (!_recipes.ContainsKey(name)) {
				Debug.Log("Recipe " + name + " not loaded (" + _recipes.Count + " recipes loaded)");
				return null;
			}
			return _recipes[name];
		}

		void Update()
		{
			if (worldClock) {
				float minutes = daytime.currentTimeOfDay * 1440;
				setClock(Mathf.Floor(minutes / 60f).ToString("00") + ":" + Mathf.Floor(minutes % 60f).ToString("00"));
				if (minutes % 60f < 0.5f) {
					if (!isSaved) {
						scene.save();
						isSaved = true;
					}
				} else isSaved = false;
			}

			if (gameControls && CrossPlatformInputManager.GetButtonDown("GameMenu")) {
				transform.FindChild("GameMenu").gameObject.SetActive(true);
				EventSystem.current.SetSelectedGameObject(GameObject.Find("ContinueButton"));
			}

			if (Input.GetKey (KeyCode.JoystickButton0)) _joystick = true;
		}

		/*public static AudioSource playClipAtPoint(AudioClip clip, Vector3 pos)
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
			Destroy(tempGO, clip.length);
			return aSource;
		}*/

		public enum materialCategory
		{
			gas,
			liquid,
			loose,
			solid,
			bulk
		}

		public enum itemCategory
		{
			item,
			herb,
			rock,
			meal,
			potion,
			weapon,
			container
		}

		public enum recipeCategory
		{
			crafting,
			herbalism,
			blacksmithing,
			alchemy
		}

		public bool joystick()
		{
			return _joystick;
		}

		public void showLoading()
		{
			busyIndicator.SetActive(true);
		}

		public void hideLoading()
		{
			busyIndicator.SetActive(false);
		}

		public void deadQule(Qule q)
		{
			var p = players[q.family()];
			scene.qules.Remove(q);
		}
	}
}
