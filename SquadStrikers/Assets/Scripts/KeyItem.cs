using UnityEngine;
using System.Collections;

public class KeyItem : Item {
	
	//public string itemName;
	// Use this for initialization
	void Start () {
	
	}

	public override string ToDisplayString ()
	{
		return itemName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	[System.Serializable]
	public class KeyItemSave : ItemSave {
		public bool isOnFloor;

		public KeyItemSave (KeyItem k) {
			itemName = k.itemName;
			isOnFloor = k.isOnFloor;
		}

		public override GameObject ToGameObject () {
			GameObject prefab;
			if (!GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetItemByName (itemName, out prefab)) throw new System.Exception ("Item not in database");
			GameObject output = ((GameObject)(Instantiate (prefab, new Vector3 (0f, 0f, 0f), Quaternion.identity)));
			output.GetComponent<Item> ().isOnFloor = isOnFloor;
			if (!isOnFloor) {
				output.GetComponent<SpriteRenderer> ().enabled = false;
			}
			return output;
		}

	}
}
