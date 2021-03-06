﻿using UnityEngine;
using System.Collections;

public abstract class Item : MonoBehaviour {

	public string itemName;
	public Sprite icon;

	[SerializeField] private PCHandler _owner;
	[SerializeField] private string _description; //Determines what appears on the side when item is used.
	public virtual string description {get {return _description;} set {_description = value; }}
	public PCHandler owner {
		get { return _owner; }
		set { _owner = value;
			if (_owner) {
				isOnFloor = false;
			}
		}
	}
	[SerializeField] private bool _isOnFloor;
	public bool isOnFloor {
		get { return _isOnFloor; }
		set {
			_isOnFloor = value;
			if (_isOnFloor) {
				gameObject.GetComponent<SpriteRenderer> ().enabled = true;
				owner = null;
			} else {
				gameObject.GetComponent<SpriteRenderer> ().enabled = false;
			}
		}
	}

	public int value; //Used for determining how many items to spawn on each level.
	public bool legendary; //Legendary items only spawn in special circumstances.
	public float naturalDepth; //The most common depth for this to spawn at.
	public float frequency; //This determines how likely the item is to drop.

	public void OnMouseDown() {
		BoardHandler bh = BoardHandler.GetBoardHandler ();
		bh.getTileState (bh.FindItem (this)).tile.OnMouseDown ();
	}


	public void OnMouseOver() {
		if (Input.GetMouseButtonDown (1) && isOnFloor) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar>().displayStats(this);
		}
	}

	public void updatePosition(){
		//Checks the game board to find where it should be and updates its position to match. Also sets it's isOnFloor to true.
		isOnFloor = true;
		BoardHandler.Coords c = GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().FindItem (this);
		gameObject.transform.position=new Vector3(c.x * BoardHandler.tileSize, c.y * BoardHandler.tileSize,0);
	}

	// Use this for initialization
	void Start () {
		
	}

	public abstract string ToDisplayString ();
	
	// Update is called once per frame
	void Update () {
	
	}


	//======================================================IO Here=============================================================//

	[System.Serializable]
	public abstract class ItemSave {
		public abstract GameObject ToGameObject ();
		public string itemName;

		public static ItemSave CreateFromItem(Item i) {
			if (i is Weapon) {
				return new Weapon.WeaponSave ((Weapon)i);
			} else if (i is AncientMagic) {
				return new AncientMagic.AncientMagicSave ((AncientMagic)i);
			} else if (i is KeyItem) {
				return new KeyItem.KeyItemSave ((KeyItem)i);
			} else {
				throw new UnityException ("Don't know how to load this item type:" + i.itemName);
			}
		}
	}
}
