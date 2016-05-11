namespace com.qetrix.apps.quly.libs
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	[Serializable]
	public class Item
	{
		protected string _name;
		protected float _weight = 1;
		// List<Item> _rewards;

		protected int _materialCapacity = 100; // How much material can the item contain (max)
		protected int _materialCotnent = 0; // How much material the item contains
		protected string _materialType; // What Material the item accepts or contains

		public float weight()
		{
			return _weight;
		}
	}
}
