using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Database : MonoBehaviour {
	//This kludge was neccessary instead of using Dictionary due to needing visibility in the Editor.

	public GameObject[] items;
	public GameObject[] enemies;
	public GameObject[] bosses;
	public GameObject[] optionalBosses;
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
	//Just returns the prefab or null if it isn't in the database.
	public GameObject GetItemByName(string name) {
		foreach (GameObject g in items) {
			if (g.GetComponent<Item> ().itemName == name) {
				return g;
			} 
		}
		return null;
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
	public GameObject GetBossByID (int ID) {
		return bosses [ID];
	}
	public GameObject GetOptionalBossByID (int ID) {
		return optionalBosses [ID];
	}
	public int GetBossID(ref float difficultyRemaining, int depth) {
		Dictionary<int,float> spawnRates = new Dictionary<int, float> {};
		for (int i = 0; i < bosses.Length; i++) {
			spawnRates.Add (i, bosses[i].GetComponent<Enemy>().bossFrequency * Mathf.Exp (-Mathf.Pow (depth - bosses[i].GetComponent<Enemy>().naturalDepth, 2f)));
		}
		return RandomSelection.Select<int> (spawnRates);
	}

	public int GetOptionalBossID(ref float difficultyRemaining, int depth) {
		Dictionary<int,float> spawnRates = new Dictionary<int,float> {};
		for (int i = 0; i < optionalBosses.Length; i++) {
			spawnRates.Add (i, optionalBosses[i].GetComponent<Enemy>().optionalBossFrequency * Mathf.Exp (-Mathf.Pow (depth - optionalBosses[i].GetComponent<Enemy>().naturalDepth, 2f)));
		}
		return RandomSelection.Select<int> (spawnRates);
	}

	public GameObject GetRandomItemAtDepth(int depth) {
		Dictionary<GameObject,float> dropRates = new Dictionary<GameObject,float> {};
		for (int i = 0; i < items.Length; i++) {
			if (!items [i].GetComponent<Item>().legendary) {
				if (depth > items [i].GetComponent<Item>().naturalDepth) {
					dropRates.Add (items [i], items [i].GetComponent<Item>().frequency * Mathf.Exp (-(depth - items [i].GetComponent<Item>().naturalDepth) / 3f));
				} else {
					dropRates.Add (items [i], items [i].GetComponent<Item>().frequency * Mathf.Exp (-Mathf.Pow (depth - items [i].GetComponent<Item>().naturalDepth, 2f) / 10f));
				}
			}
		}
		return RandomSelection.Select<GameObject> (dropRates);
	}

	public GameObject GetRandomEnemyAtDepth(int depth) {
		Dictionary<GameObject,float> spawnRates = new Dictionary<GameObject,float> {};
		for (int i = 0; i < enemies.Length; i++) {
			if (depth > enemies [i].GetComponent<Enemy>().naturalDepth) {
				spawnRates.Add (enemies [i], enemies [i].GetComponent<Enemy>().frequency * Mathf.Exp (-(depth - enemies [i].GetComponent<Enemy>().naturalDepth) / 3f));
			} else {
				spawnRates.Add (enemies [i], enemies [i].GetComponent<Enemy>().frequency * Mathf.Exp (-Mathf.Pow (depth - enemies [i].GetComponent<Enemy>().naturalDepth, 2f) / 10f));
			}
		}
		return RandomSelection.Select<GameObject> (spawnRates);
	}
}
