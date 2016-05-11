namespace com.qetrix.apps.quly.libs
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class Material
	{
		string _name;
		Color _color;
		float _weight = 0.01f; // How (+/-) and how much one unit of Material is weighing
		float _defense = 0.0f; // How (+/-) and how much the Material changes Qule's defense
		float _power = 0.0f;   // How (+/-) and how much the Material changes Qule's energy power
		float _energy = 0.0f;  // How (+/-) and how much the Material changes Qule's total energy
		float _regen = 0.0f;   // How (+/-) and how much the Material changes Qule's energy regeneration

		public static Dictionary<string, int> load(string name)
		{
			Dictionary<string, int> mat = new Dictionary<string, int>();
			mat.Add(name, 100);
			return mat;
		}

		public float weight()
		{
			return _weight;
		}
	}
}
