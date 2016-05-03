using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour {

	private string _backup;
	// Use this for initialization
	void Start () {
	
	}

	//
	public void displayStats(Unit u) {
		string lineBreak = System.Environment.NewLine;
		string output = u.unitName + lineBreak;
		if (u is PCHandler) {
			output += "Player Character" + lineBreak + "Class: " + ((PCHandler)u).characterClass.name + lineBreak;
		} else if (u.isFriendly) {
			output += "Friendly Unit" + lineBreak;
		} else {
			output += "Enemy Unit" + lineBreak;
		}
		output += "Attack: " + u.attack.ToString() + lineBreak;
		output += "Dodge: " + u.dodge.ToString() + lineBreak;
		output += "Defense: " + u.minDefense.ToString() + "-" + u.maxDefense.ToString() + lineBreak;
		output += "Damage: " + u.damage.ToString() + lineBreak;
		output += "Move: " + u.move.ToString() + lineBreak;
		output += "Hit Points: " + u.currentHP.ToString() + "/" + u.maxHP.ToString() + lineBreak;
		output += "HP Regen: " + u.hPRegen.ToString() + lineBreak;
		output += "Energy: " + u.currentEnergy.ToString() + "/" + u.maxEnergy.ToString() + lineBreak;
		output += "Energy Regen: " + u.energyRegen.ToString() + lineBreak;
		if (u is Enemy) {
			output += "Descirption: " + ((Enemy)u).description;
		} else if (u is PCHandler) {
			output += lineBreak + "Equipment:" + lineBreak;
			foreach (Item i in ((PCHandler) u).inventory) {
				output += i.itemName + lineBreak;
			}
			output += lineBreak + "Abilities:" + lineBreak;
			foreach (PCHandler.Ability a in ((PCHandler) u).abilityList) {
				output += PCHandler.getAbilityName(a) + lineBreak;
			}
		}
		gameObject.GetComponentInChildren<Text>().text = output;
	}


	//Temporarily posts a description of an action.
	public void ActionDescription(PCHandler.Action action) {
		_backup = gameObject.GetComponentInChildren<Text> ().text;
		if (action.actionName == "Pick Up Item") {
			gameObject.GetComponentInChildren<Text> ().text += action.actionDescription;
		} else if (action.attachedItem) {
			gameObject.GetComponentInChildren<Text> ().text += System.Environment.NewLine + System.Environment.NewLine + action.attachedItem.itemName + "(" + ((ActionItem) action.attachedItem).itemClass + "):" + System.Environment.NewLine + action.attachedItem.description;
		} else {
			gameObject.GetComponentInChildren<Text> ().text += System.Environment.NewLine + System.Environment.NewLine + action.actionName + ":" + System.Environment.NewLine + action.actionDescription;
		}
	}

	//Temporarily posts a description of an action.
	public void Revert() {
		gameObject.GetComponentInChildren<Text> ().text = _backup;
	}

	public void Clear() {
		gameObject.GetComponentInChildren<Text>().text = "";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
