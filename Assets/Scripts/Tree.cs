namespace com.qetrix.apps.quly
{
	using System;
	using UnityEngine;
	using System.Collections;
	using com.qetrix.apps.quly.libs;

	// Attach this to empty GameObject with BoxCollider, placed on position of terrain tree.
	public class Tree : MonoBehaviour
	{
		public float _age = 0.5f;

		[SerializeField]
		float wood = 10;
		float growth = 1;
		bool dead = false;
		MeshRenderer mr;
		TreeInstance _treeInstance;

		void Start()
		{
			tag = "Material"; // TODO: Always?
			mr = GetComponent<MeshRenderer>(); /// If three has no Mesh Renderer, it is a Terrain Tree, which doesn't grow.
			if (mr != null) {
				//System.Random rnd = new System.Random();
				_age = (float)(UnityEngine.Random.value);
				growth = 0.01f; /*(float)(UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);*/
				transform.localRotation = Quaternion.Euler((float)UnityEngine.Random.value * 3.6f, (float)UnityEngine.Random.value * 360, (float)UnityEngine.Random.value * 3.6f);
			}
		}

		void Update()
		{
			if (dead || mr == null) return;

			if (_age < 1f && wood > 0) {
				_age += (Time.deltaTime / 10) * growth;
				wood += Time.deltaTime * growth;
				transform.localScale = new Vector3(_age, _age, _age);
			} else {
				timber();
				dead = true;
			}
		}

		public void timber()
		{
			tag = "Untagged";
			if (mr != null) {
				var rb = gameObject.AddComponent<Rigidbody>();
				rb.AddExplosionForce(100, transform.position + transform.forward + (transform.up * transform.localScale.y), 20);
				Destroy(gameObject, 6f);
			} else {
				GameManager.scene.delTree(_treeInstance);
				Destroy(gameObject);
			}
		}

		// Returns amount of wood harvested
		public EnergyResult action(GameObject sender, float energy)
		{
			wood -= energy;
			if (wood < 0) {
				energy -= Math.Abs(wood);
				timber();
				dead = true;
			}

			var er = new EnergyResult();
			er.items(new Item("wood", (int)Math.Floor(energy)));
			return er;
		}

		public Tree treeInstance(TreeInstance treeInstance)
		{
			this._treeInstance = treeInstance;
			return this;
		}

		public Tree age(float age)
		{
			this._age = age;
			return this;
		}
	}
}
