using UnityEngine;
using System.Collections;

public abstract class ActionItem : Item {

	public string itemClass; //Determines the basic action this item does.
	public virtual PCHandler.Action CreateAction () {
		return new PCHandler.Action (itemClass, description, this);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
