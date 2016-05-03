using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ItemDatabase : MonoBehaviour {
	//This kludge was neccessary instead of using Dictionary due to needing visibility in the Editor.

	public GameObject[] items;

	//Returns true if item found, in which case output is the corresponding prefab
	public bool GetItemByName(string name, out GameObject output) {
		foreach (GameObject g in items) {
			if (g.GetComponent<Item> ().itemName == name) {
				output = g;
				return true;
			} 
		}
		output = null;
		return false;
	}

}
