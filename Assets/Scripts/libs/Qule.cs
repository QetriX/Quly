namespace com.qetrix.apps.quly.libs
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public class Qule : Item
	{
		[SerializeField]
		protected int _index; // Scene Unique Qule Index (SUQI)
		[SerializeField]
		protected int _family; // Qule Family (energy structure compatibility)
		[SerializeField]
		protected int _parent; // Qule parent's SUQI
		[SerializeField]
		protected int _fissions; // How many children Qule has
		[SerializeField]
		protected float _energy; // Current energy
		[SerializeField]
		protected S_Transform _transform;
		[SerializeField]
		protected string _npc;

		protected QuleState _state = QuleState.Single;
		protected Dictionary<string, int> _structure = new Dictionary<string, int>();
		protected List<string> _inventory = new List<string>();
		protected Dictionary<string, float> _skills = new Dictionary<string, float>();

		protected float _energyPower = 2f; // How strong and subsequently how long the beam is; derived from Material
		protected S_Color _color; // Qule color, derived from most used(?) Material
		protected float _defense; // Defense against attacks and falls (calculated from Material)
								  //protected float _weight;  // Total weight of the Qule (calculated from Material)

		protected int _energyMax; // Maximum energy (calculated from Material)
		protected int _energyMin; // Minimal viable energy (cache)
		protected float _energyRegenRate;  // Energy regeneration rate over time
		protected float _energyBeamLength; // Beam length (cache)
		protected S_Color _energyColor;    // Nice: DFB1FAFF

		// TODO: Buffs/debuffs: seconds active, buff specs - energyRegen delta, energyMax delta, energyPower delta + beamLength delta, energyColor delta, ... Buff can be as Material (shares the same properties)

		public Qule(int index, int family, S_Transform transform, Dictionary<string, int> structure)
		{
			_index = index;
			_family = family;
			_fissions = 0;
			_energy = 50;
			_name = "Qule " + index;
			_transform = transform;
			_structure = structure;

			init();
		}

		public void init()
		{
			_energyBeamLength = (float)Math.Sqrt(_energyPower);
			_energyMax = 100 * (int)Math.Round(Math.Pow(1.2, _fissions));
			_energyRegenRate = (float)Math.Round(Math.Pow(0.9, _fissions), 2);
			_energyMin = (int)Math.Round(_energyMax * 0.2f);


			foreach (KeyValuePair<string, int> mat in _structure) {
				Material m = GameManager.instance.material(mat.Key);
			}
			_color = new S_Color(UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), 255); // TODO: Derived from Material
			_energyColor = new S_Color(UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), UnityEngine.Random.Range(0, 255), 255); // TODO: Derived from Material??
			_defense = 0; // TODO: Derived from Material
			_weight = 1; // TODO: Derived from Material and Inventory
		}

		public string name()
		{
			return _name;
		}

		public float energy()
		{
			return _energy;
		}

		public int energyMax()
		{
			return _energyMax;
		}

		public int energyMin()
		{
			return _energyMin;
		}

		// Energy normalized (always from 0.0f to 1.0f)
		public float energyNormalized()
		{
			return _energy / _energyMax;
		}

		public float energyRegenRate()
		{
			return _energyRegenRate;
		}

		public float energyRegen()
		{
			if (_energy < _energyMax) {
				_energy += Time.deltaTime * energyRegenRate();
				if (_energy > _energyMax) _energy = _energyMax;
			} else if (_energy > _energyMax) {
				_energy -= Time.deltaTime;
				if (_energy < _energyMax) _energy = _energyMax;
			}
			return _energy;
		}

		public float energyChange(float delta)
		{
			_energy += delta;
			return _energy;
		}

		public float energyPower(bool isOverloaded) // force = turbo, sprint, nitro... Energy overload = jump
		{
			if (isOverloaded) return _energyPower * 3;
			return _energyPower;
		}

		public float energyPower()
		{
			return energyPower(false);
		}

		public float energyPower(float distance, bool isOverloaded)
		{
			if (distance < 1f) return energyPower(isOverloaded);
			return energyPower(isOverloaded) - (energyPower(isOverloaded) * (float)Math.Pow(distance / energyBeamLength(), energyBeamLength()));
		}

		public float energyPower(float distance)
		{
			return energyPower(distance, false);
		}

		public float energyBeamLength()
		{
			return _energyBeamLength;
		}

		public Color color()
		{
			return _color.toColor();
		}

		public Color energyColor()
		{
			return _energyColor.toColor();
		}

		public float damage(float dmg)
		{
			// TODO: deduct defense bonus, if any (derived from powerups or material specs)
			energyChange(_defense - dmg);
			return dmg;
		}

		protected void calcWeight()
		{
			float w = 0;
			foreach (KeyValuePair<string, int> st in _structure) {
				w += GameManager.instance.material(st.Key).weight();
			}
			foreach (string inv in _inventory) {
				w += GameManager.instance.item(inv).weight();
			}
			_weight = w;
		}

		public S_Transform transform()
		{
			return _transform;
		}

		public string npc()
		{
			return _npc;
		}

		public void npc(string value)
		{
			_npc = value;
		}

		public int family()
		{
			return _family;
		}

		public bool checkInventory(string itemName)
		{
			return false;
		}

		public QuleState state()
		{
			return _state;
		}

		public QuleState state(QuleState newState)
		{
			_state = newState;
			return _state;
		}

		public enum QuleState
		{
			Solid = 8,   // No inventory slot
			Single = 10, // One inventory slot
			Sixfold = 20 // Six inventory slots
		}

		public bool energySaver()
		{
			// TODO: EnergySaver is a skill for proximity, that if enemy is too far away, energy stops beaming and when is close enough, it will start again.
			return false;
		}
	}
}
