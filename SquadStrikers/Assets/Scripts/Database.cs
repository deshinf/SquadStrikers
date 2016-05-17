using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Database : MonoBehaviour {
	//This kludge was neccessary instead of using Dictionary due to needing visibility in the Editor.

	public GameObject[] items;
	public GameObject[] enemies;
	public GameObject[] tiles;

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
	//Returns true if item found, in which case output is the corresponding prefab
	public bool GetEnemyByName(string name, out GameObject output) {
		foreach (GameObject g in enemies) {
			if (g.GetComponent<Enemy> ().unitName == name) {
				output = g;
				return true;
			} 
		}
		output = null;
		return false;
	}
	//Returns true if item found, in which case output is the corresponding prefab
	public bool GetTileByName(string name, out GameObject output) {
		foreach (GameObject g in tiles) {
			if (g.GetComponent<Tile> ().tileName == name) {
				output = g;
				return true;
			}
		}
		output = null;
		return false;
	}
	public bool GetAnythingByName(string name, out GameObject output) {
		if (GetItemByName(name, out output)) {
			return true;
		} else if (GetEnemyByName(name, out output)) {
			return true;
		} else if (GetTileByName(name, out output)) {
			return true;
		} else {
			return false;
		}
	}
}
