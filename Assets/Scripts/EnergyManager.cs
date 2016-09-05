namespace com.qetrix.apps.quly
{
	using libs;
	using System;
	using System.Collections;
	using System.Reflection;
	using UnityEngine;

	public class EnergyManager : MonoBehaviour
	{
		public string scriptCalled;
		Component comp;

		void Start()
		{
			if (scriptCalled != "") {
				Type t = Type.GetType("com.qetrix.apps.quly." + scriptCalled, true, true);
				if (t == null) Debug.LogError("Type " + scriptCalled + " is null!");
				comp = this.gameObject.GetComponent(t);
				if (comp == null) {
					comp = gameObject.AddComponent(t);
					if (comp == null) Debug.LogError("Unable to add component \"" + scriptCalled + "\" to \"" + gameObject.name + "\"."); else Debug.Log("Adding missing component \"" + scriptCalled + "\" to \"" + gameObject.name + "\".");
				}
			} else Debug.LogWarning("Empty EnergyManager.scriptCalled on \"" + gameObject.name + "\"!");
		}

		public EnergyResult onEnergy(GameObject sender, float energy)
		{
			if (comp != null) {
				Type thisType = comp.GetType();
				MethodInfo action = thisType.GetMethod("action");
				if (action != null) {
					return action.Invoke(comp, new object[] { sender, energy }) as EnergyResult;
				} else Debug.LogError("Script " + scriptCalled + " has no method action(GameObject sender, float energySent)!");
			} else if (scriptCalled == "") Debug.LogError("EnMgr script not defined! Check EnergyManager Component on " + sender.name);
			else Debug.LogError("EnMgr script \"" + scriptCalled + "\" on \"" + gameObject.name + "\" not found!");
			return new EnergyResult();
		}
	}
}
