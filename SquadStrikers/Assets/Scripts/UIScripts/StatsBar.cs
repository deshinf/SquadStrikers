using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StatsBar : MonoBehaviour {

	private string _backup;
	// Use this for initialization
	void Start () {
	
	}

	public void displayStats(Unit u) {
		gameObject.GetComponentInChildren<Text>().text = u.ToDisplayString();
	}

	public void displayStats(Item i) {
		gameObject.GetComponentInChildren<Text>().text = i.ToDisplayString();
	}

	//
//	public void displayStats(Enemy u) {
//		string lineBreak = System.Environment.NewLine;
//		string output = u.unitName + lineBreak;
//		output += "Enemy Unit" + lineBreak;
//		output += "Attack: " + u.attack.ToString() + lineBreak;
//		output += "Dodge: " + u.dodge.ToString() + lineBreak;
//		output += "Defense: " + u.minDefense.ToString() + "-" + u.maxDefense.ToString() + lineBreak;
//		output += "Damage: " + u.damage.ToString() + lineBreak;
//		output += "Move: " + u.move.ToString() + lineBreak;
//		output += "Hit Points: " + u.currentHP.ToString() + "/" + u.maxHP.ToString() + lineBreak;
//		output += "HP Regen: " + u.hPRegen.ToString() + lineBreak;
//		output += "Energy: " + u.currentEnergy.ToString() + "/" + u.maxEnergy.ToString() + lineBreak;
//		output += "Energy Regen: " + u.energyRegen.ToString() + lineBreak;
//		output += "Descirption: " + u.description;
//		gameObject.GetComponentInChildren<Text>().text = output;
//	}
//
//	public void displayStats(PCHandler u) {
//		string lineBreak = System.Environment.NewLine;
//		string output = u.unitName + lineBreak;
//		output += "Player Character" + lineBreak + "Class: " + u.characterClass.name + lineBreak;
//		if (u.attack == u.baseAttack) {
//			output += "Accuracy: " + u.attack.ToString () + lineBreak;
//		} else if (u.attack > u.baseAttack) {
//			output += "Accuracy: <color=green>" + u.attack.ToString () + "(" + u.baseAttack.ToString () + ")</color>" + lineBreak;
//		} else if (u.attack < u.baseAttack) {
//			output += "Accuracy: <color=red>" + u.attack.ToString () + "(" + u.baseAttack.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.dodge == u.baseDodge) {
//			output += "Dodge: " + u.dodge.ToString () + lineBreak;
//		} else if (u.attack > u.baseAttack) {
//			output += "Dodge: <color=green>" + u.dodge.ToString () + "(" + u.baseDodge.ToString () + ")</color>" + lineBreak;
//		} else if (u.attack < u.baseAttack) {
//			output += "Dodge: <color=red>" + u.dodge.ToString () + "(" + u.baseDodge.ToString () + ")</color>" + lineBreak;
//		}
//		output += "Armour: ";
//		if (u.minDefense == u.baseMinDefense) {
//			output +=  u.minDefense.ToString () + " - ";
//		} else if (u.minDefense > u.minDefense) {
//			output += "<color=green>" + u.minDefense.ToString () + "(" + u.baseMinDefense.ToString () + ")</color> - ";
//		} else if (u.attack < u.baseMinDefense) {
//			output += "<color=red>" + u.minDefense.ToString () + "(" + u.baseMinDefense.ToString () + ")</color> - ";
//		}
//		if (u.maxDefense == u.baseMaxDefense) {
//			output +=  u.maxDefense.ToString () + lineBreak;
//		} else if (u.maxDefense > u.maxDefense) {
//			output += "<color=green>" + u.maxDefense.ToString () + "(" + u.baseMaxDefense.ToString () + ")</color>" + lineBreak;
//		} else if (u.attack < u.baseMinDefense) {
//			output += "<color=red>" + u.maxDefense.ToString () + "(" + u.baseMaxDefense.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.damage == u.baseDamage) {
//			output += "Damage: " + u.damage.ToString () + lineBreak;
//		} else if (u.damage > u.baseDamage) {
//			output += "Damage: <color=green>" + u.damage.ToString () + "(" + u.baseDamage.ToString () + ")</color>" + lineBreak;
//		} else if (u.damage < u.baseDamage) {
//			output += "Damage: <color=red>" + u.damage.ToString () + "(" + u.baseDamage.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.move == u.baseMove) {
//			output += "Move: " + u.move.ToString () + lineBreak;
//		} else if (u.move > u.baseMove) {
//			output += "Move: <color=green>" + u.move.ToString () + "(" + u.baseMove.ToString () + ")</color>" + lineBreak;
//		} else if (u.move < u.baseMove) {
//			output += "Move: <color=red>" + u.move.ToString () + "(" + u.baseMove.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.currentHP == u.maxHP) {
//			output += "<color=green>Hit Points: " + u.currentHP.ToString() + "/</color>";
//		} else if (u.currentHP < u.maxHP / 3) {
//			output += "<color=red>Hit Points: " + u.currentHP.ToString() + "/</color>";
//		} else {
//			output += "<color=yellow>Hit Points: " + u.currentHP.ToString() + "/</color>";
//		}
//		if (u.maxHP == u.baseMaxHP) {
//			output +=  u.maxHP.ToString () + lineBreak;
//		} else if (u.maxHP > u.baseMaxHP) {
//			output += "<color=green>" + u.maxHP.ToString () + "(" + u.baseMaxHP.ToString () + ")</color>" + lineBreak;
//		} else if (u.maxHP < u.baseMaxHP) {
//			output += "<color=red>" + u.maxHP.ToString () + "(" + u.baseMaxHP.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.hPRegen == u.baseHPRegen) {
//			output += "HP Regen: " + u.hPRegen.ToString () + lineBreak;
//		} else if (u.hPRegen > u.baseHPRegen) {
//			output += "HP Regen: <color=green>" + u.hPRegen.ToString () + "(" + u.baseHPRegen.ToString () + ")</color>" + lineBreak;
//		} else if (u.hPRegen < u.baseHPRegen) {
//			output += "HP Regen: <color=red>" + u.hPRegen.ToString () + "(" + u.baseHPRegen.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.currentEnergy == u.maxEnergy) {
//			output += "<color=green>Energy: " + u.currentEnergy.ToString() + "/</color>";
//		} else if (u.currentEnergy < u.maxEnergy / 3) {
//			output += "<color=red>Energy: " + u.currentEnergy.ToString() + "/</color>";
//		} else {
//			output += "<color=yellow>Energy: " + u.currentEnergy.ToString() + "/</color>";
//		}
//		if (u.maxEnergy == u.baseMaxEnergy) {
//			output +=  u.maxEnergy.ToString () + lineBreak;
//		} else if (u.maxEnergy > u.baseMaxEnergy) {
//			output += "<color=green>" + u.maxEnergy.ToString () + "(" + u.baseMaxEnergy.ToString () + ")</color>" + lineBreak;
//		} else if (u.maxEnergy < u.baseMaxHP) {
//			output += "<color=red>" + u.maxEnergy.ToString () + "(" + u.baseMaxEnergy.ToString () + ")</color>" + lineBreak;
//		}
//		if (u.energyRegen == u.baseEnergyRegen) {
//			output += "Energy Regen: " + u.energyRegen.ToString () + lineBreak;
//		} else if (u.energyRegen > u.baseEnergyRegen) {
//			output += "Energy Regen: <color=green>" + u.energyRegen.ToString () + "(" + u.baseEnergyRegen.ToString () + ")</color>" + lineBreak;
//		} else if (u.energyRegen < u.baseEnergyRegen) {
//			output += "Energy Regen: <color=red>" + u.energyRegen.ToString () + "(" + u.baseEnergyRegen.ToString () + ")</color>" + lineBreak;
//		}
//		output += lineBreak + "Equipment:" + lineBreak;
//		foreach (Item i in u.inventory) {
//			output += i.itemName + lineBreak;
//		}
//		output += lineBreak + "Abilities:" + lineBreak;
//		foreach (PCHandler.Ability a in u.abilityList) {
//			output += PCHandler.getAbilityName(a) + lineBreak;
//		}
//		gameObject.GetComponentInChildren<Text>().text = output;
//	}


	//Temporarily posts a description of an action.
	public void displayUnitAndAction(PCHandler unit, PCHandler.Action action) {
		string output;
		string lineBreak = System.Environment.NewLine;
		output = unit.ToDisplayString () +lineBreak + lineBreak;
		if (action.attachedItem) {
			if (action.attachedItem is Weapon && !(action.actionName == "Pick Up Item")) {
				output += action.attachedItem.ToDisplayString ();
			} else {
				output += action.actionDescription + lineBreak + action.attachedItem.ToDisplayString ();
			}
		} else {
			output += System.Environment.NewLine + System.Environment.NewLine + action.actionName + ":" + lineBreak + action.actionDescription;
		}
		gameObject.GetComponentInChildren<Text>().text = output;
	}

	public void displayUnitActionAndTarget(PCHandler unit, PCHandler.Action action, GameObject target) {
		string output;
		string lineBreak = System.Environment.NewLine;
		output = unit.ToDisplayString () + lineBreak + lineBreak;
		if (action.attachedItem) {
			if (action.attachedItem is Weapon && !(action.actionName == "Pick Up Item")) {
				output += action.attachedItem.ToDisplayString ();
			} else {
				output += action.actionDescription + lineBreak + action.attachedItem.ToDisplayString ();
			}
		} else {
			output += System.Environment.NewLine + System.Environment.NewLine + action.actionName + ":" + lineBreak + action.actionDescription;
		}
		if (action.attachedItem && action.attachedItem is Weapon) {
			Weapon w = (Weapon)(action.attachedItem);
			output += lineBreak + lineBreak + "Target: " + (target.GetComponent<Unit> ()).unitName + lineBreak;
			int accuracy , damage;
//			if (w.itemClass == "Bow" && w.owner.hasAbility (PCHandler.Ability.BowMastery)) {
//				accuracy = w.attack + w.owner.attack + w.owner.bowMasteryBonusAttack;
//			}
//			else {
			accuracy = w.attack + w.owner.attack;
//			}
//			if (w.itemClass == "Bow" && w.owner.hasAbility (PCHandler.Ability.BowMastery)) {
//				damage = w.damage + w.owner.damage + w.owner.bowMasteryBonusDamage;
//			}
//			else {
			damage = w.damage + w.owner.damage;
//			}
			output += "Accuracy:" + accuracy + "-" + (target.GetComponent<Unit>()).dodge + "=" + (accuracy - (target.GetComponent<Unit>()).dodge) + "%" + lineBreak;
			output += "Damage:" + damage + "-(" + (target.GetComponent<Unit>()).minDefense + " to " + (target.GetComponent<Unit>()).maxDefense + ")=" +
			((damage - (target.GetComponent<Unit>()).maxDefense) > 0 ? (damage - (target.GetComponent<Unit>()).maxDefense) : 0) + " to " +
			((damage - (target.GetComponent<Unit>()).minDefense) > 0 ? (damage - (target.GetComponent<Unit>()).minDefense) : 0);
		}
		gameObject.GetComponentInChildren<Text>().text = output;
	}

	public void Clear() {
		gameObject.GetComponentInChildren<Text>().text = "";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
