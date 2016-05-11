using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class Weapon : ActionItem {

	public int attack;
	public int damage;
	public int maxCharges;
	public override string description { get { return base.description; } set { base.description = value; } }// + System.Environment.NewLine + "Attack: " + attack + System.Environment.NewLine + "Damage: " + damage + System.Environment.NewLine + "Charges: " + charges + "/" + maxCharges; } set { base.description = value; } }
	private int _charges;
	public int charges {
		get { return _charges; }
		private set { _charges = value; }
	}

	public void useCharges (int c) {
		if(owner.hasAbility(PCHandler.Ability.Conservationist)) {
			c = binomial(c,1- owner.conservationistProtectionProbability);
		}
		if (charges - c > 0) {
			charges -= c;
		} else {
			BreakItem ();
			charges = c;
		}
	}


	public int binomial (int n, float p) {
		int output = 0;
		for (int i = 0; i < n; i++) {
			if (Random.value < p) {
				output += 1;
			}
		}
		return output;
	}
	// Use this for initialization
	void Start () {
		_charges = maxCharges;
	}

	void BreakItem () {
		Assert.IsNotNull (owner);
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().AlertLog (owner.unitName + "'s " + itemName + " has broken.");
		owner.loseItem (this);
		Destroy (gameObject);
	}


	public override string ToDisplayString () {
		string output;
		string lineBreak = System.Environment.NewLine;
		if (owner) {
			output = itemName + "(" + itemClass + "):" + lineBreak + description + lineBreak;
			if (itemClass == "Bow" && owner.hasAbility (PCHandler.Ability.BowMastery)) {
				output += "Accuracy: " + attack + "+" + owner.attack + "+" + owner.bowMasteryBonusAttack + "=" + (attack + owner.attack + owner.bowMasteryBonusAttack) + lineBreak;
			}
			else {
				output += "Accuracy: " + attack + "+" + owner.attack + "=" + (attack + owner.attack) + lineBreak;
			}
			if (itemClass == "Bow" && owner.hasAbility (PCHandler.Ability.BowMastery)) {
				output += "Damage: " + damage + "+" + owner.damage + "+" + owner.bowMasteryBonusAttack + "=" + (damage + owner.damage + owner.bowMasteryBonusDamage) + lineBreak;
			}
			else {
				output += "Damage: " + damage + "+" + owner.damage + "=" + (damage + owner.damage) + lineBreak;
			}
			output += "Charges: " + charges + "/" + maxCharges;
		} else {
			output = itemName + "(" + itemClass + "):" + lineBreak + description + lineBreak;
			output += "Accuracy: " + attack;
			output += "Damage: " + damage;
			output += "Charges: " + charges + "/" + maxCharges;
		}
		return output;
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
