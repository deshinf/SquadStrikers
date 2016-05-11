using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Unit {

	public static float attackWaitTime = 1f; //Time of pause after attack in seconds.

	// Use this for initialization
	void Start () {
		gameObject.transform.Translate (0f, 0f, -0.1f);
	}

	//Overriding all the properties from Unit to use Serialized Fields so that the Unity Editor can get at them.
	[SerializeField] int _attack;
	public override int attack { get { return _attack; } set { _attack = value; } }
	[SerializeField] int _dodge;
	public override int dodge { get { return _dodge; } set { _dodge = value; } }
	[SerializeField] int _minDefense;
	public override int minDefense { get { return _minDefense; } set { _minDefense = value; } }
	[SerializeField] int _maxDefense;
	public override int maxDefense { get { return _maxDefense; } set { _maxDefense = value; } }
	[SerializeField] int _damage;
	public override int damage { get { return _damage; } set { _damage = value; } }
	[SerializeField] int _move;
	public override int move { get { return _move; } set { _move = value; } }
	[SerializeField] int _maxHP;
	public override int maxHP { get { return _maxHP; } set { _maxHP = value; } }
	[SerializeField] int _currentHP;
	public override int currentHP { get { return _currentHP; } set { _currentHP = value; } }
	[SerializeField] int _hPRegen;
	public override int hPRegen { get { return _hPRegen; } set { _hPRegen = value; } }
	[SerializeField] int _maxEnergy;
	public override int maxEnergy { get { return _maxEnergy; } set { _maxEnergy = value; } }
	[SerializeField] int _currentEnergy;
	public override int currentEnergy { get { return _currentEnergy; } set { _currentEnergy = value; } }
	[SerializeField] int _energyRegen;
	public override int energyRegen { get { return _energyRegen; } set { _energyRegen = value; } }
	[SerializeField] int _attackRange; //Does nothing unless ranged AI.
	public int attackRange { get { return _attackRange; } set { _attackRange = value; } }

	public enum AIType { PatientAttacker, Inactive, Chaser, Sentinel, Magus, Flitterer, Guard, DormantChucker, Chucker};
	public AIType aIType;



	public string description;

	public void hostileTargeting() {

	}

	public override void Die () {
		BoardHandler board = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		board.getTileState (board.FindUnit (this)).unit = null;
		Object.Destroy (gameObject);
			
	}

	//AI Goes here. Returns true if it should pause afterwards.
	public bool Act() {
		//Debug.Log ("Act: " + unitName);
		BoardHandler board = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		BoardHandler.Coords amHere = board.FindUnit (this);
		PCHandler target;
		BoardHandler.Coords moveTo;
		bool output = false;
		switch (aIType) {
		case AIType.PatientAttacker:
			if (board.canMoveAndAttackPlayer (out target, out moveTo, amHere, move)) {
				board.MoveUnit (amHere, moveTo);
				Attack (target);
				return true;
			} else {
				return false;
			}
		case AIType.Sentinel:
			var unitsInRange = board.GetOtherUnitsAround (amHere, _attackRange, false);
			foreach (Unit u in unitsInRange) {
				if (u is PCHandler) {
					ArmourPiercingAttack ((PCHandler)u);
					output = true;
				}
			}
			return output;
		case AIType.Chaser:
			if (board.canMoveAndAttackPlayer (out target, out moveTo, amHere, move)) {
				board.MoveUnit (amHere, moveTo);
				Attack (target);
				return true;
			} else {
				var toGetTo = board.pursuitSquare (amHere, move);
				board.MoveUnit (amHere, toGetTo);
				return true;
			}
		case AIType.DormantChucker:
			if (board.canMoveAndSnipePlayerWithBow (out target, out moveTo, amHere, move)) {
				board.MoveUnit (amHere, moveTo);
				Attack (target);
				output = true;
				aIType = AIType.Chucker;
			}
			return output;
		case AIType.Chucker:
			if (board.canMoveAndSnipePlayerWithBow (out target, out moveTo, amHere, move)) {
				board.MoveUnit (amHere, moveTo);
				Attack (target);
			} else {
				var toGetTo = board.pursuitSquare (amHere, move);
				board.MoveUnit (amHere, toGetTo);
				return true;
			}
			return true;
		case AIType.Guard:
			//If there is a guardable point in range, they will move there. If not and there is a player, they will move to attack. Otherwise, they will wander randomly.
			//After their movement, they will attack with a spear if there is a player in range.
			output = true;
			if (board.findBestGuardPointWithin (out moveTo, amHere, move)) {
				if (amHere != moveTo) {
					board.MoveUnit (amHere, moveTo);
				} else {
					output = false;
				}
			} else if (board.canMoveAndAttackPlayerWithCardinalAttack (out target, out moveTo, amHere, move, 2,false)) {
				board.MoveUnit (amHere, moveTo);
			} else {
				board.BrownianMotion (amHere, move);
			}
			if (board.canAttackPlayerWithCardinalAttack (out target, amHere, 2, false)) {
				Attack (target);
				output = true;
			}
			return output;
		case AIType.Flitterer:
			//Brownian motion. If there is a player in range, there is a 50% chance they will attack it instead.
			if (Random.value < 0.5f) {
				board.BrownianMotion (amHere,move);
			} else {
				Debug.Log ("Case 2");
				if (board.canMoveAndAttackPlayer (out target, out moveTo, amHere, move)) {
					board.MoveUnit (amHere, moveTo);
					Attack (target);
				} else {
					board.BrownianMotion (amHere,move);
				}
			}
			return true;
		case AIType.Magus:
			//The magus moves to attack the player with Mystic Blast (Costing 30 Energy). If his HP drops below 30% or his Enegry below 70, he will instead teleport to a random square on the map. (costing 50 Energy).
			if (board.canMoveAndAttackPlayerWithCardinalAttack (out target, out moveTo, amHere, move, attackRange,false)) {
				if (currentEnergy > 69 && currentHP > (int)(maxHP * 0.3)) {
					board.MoveUnit (amHere, moveTo);
					AttackWithMysticBlast (target);
					currentEnergy -= 30;
				} else if (currentEnergy > 50) {
					List<BoardHandler.Coords> candidates = board.getOpenTilesAround (board.FindUnit (this), 100);
					if (candidates.Count == 0) {
						GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (unitName + " tried to teleport away but failed.");
					} else {
						board.MoveUnit (board.FindUnit (this), candidates [UnityEngine.Random.Range (0, candidates.Count - 1)]);
						currentEnergy -= 50;
						GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (unitName + "teleported away.");
					}
				} else {
					board.BrownianMotion (amHere, 1);
				}
			} else {
				board.BrownianMotion (amHere, 1);
			}
			return true;
		case AIType.Inactive:
			return false;
		default:
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponent<MessageBox> ().WarningLog ("Enemy AI not found: " + aIType.ToString () + " for enemy " + unitName + ". Doing nothing.");
			return true;
		}
	}



	public void Attack(PCHandler target) {
		string report;
		int toHitRoll = Random.Range (1, 100);
		int hitChance = attack - target.dodge;
		if (toHitRoll <= hitChance) {
			int armourRoll = Random.Range (target.minDefense, target.maxDefense);
			int targetStartingHealth = target.currentHP;
			int finalDamage = System.Math.Max (damage - armourRoll, 0);
			report = unitName + " attacks " + target.unitName + ". Hit chance was " + hitChance.ToString () + "% and " + unitName + " rolled " + toHitRoll.ToString () + "which hit dealing " + damage + "-("
			+ target.minDefense.ToString () + " to " + target.maxDefense.ToString () + ") = " + finalDamage.ToString () + System.Environment.NewLine;
			if (target.takeDamage (finalDamage)) {
				report += target.unitName + " died as a result.";
			} else {
				report += target.unitName + " went from " + targetStartingHealth.ToString () + "/" + target.maxHP.ToString () + "HP to " + target.currentHP.ToString () + "/" + target.maxHP.ToString () + "HP";
			}
		} else {
			report = unitName + " attacks " + target.unitName + ". Hit chance was " + hitChance.ToString () + "% and " + unitName + " rolled " + toHitRoll.ToString () + " which missed.";
		}
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().AlertLog (report);
	}

	public void ArmourPiercingAttack(PCHandler target) {
		string report;
		int toHitRoll = Random.Range (1, 100);
		int hitChance = attack - target.dodge;
		if (toHitRoll <= hitChance) {
			int targetStartingHealth = target.currentHP;
			report = unitName + " attacks " + target.unitName + ". Hit chance was " + hitChance.ToString () + "% and " + unitName + " rolled " + toHitRoll.ToString () + "which hit dealing " + damage + "(armour-piercing)" + System.Environment.NewLine;
			if (target.takeDamage (damage)) {
				report += target.unitName + " died as a result.";
			} else {
				report += target.unitName + " went from " + targetStartingHealth.ToString () + "/" + target.maxHP.ToString () + "HP to " + target.currentHP.ToString () + "/" + target.maxHP.ToString () + "HP";
			}
		} else {
			report = unitName + " attacks " + target.unitName + ". Hit chance was " + hitChance.ToString () + "% and " + unitName + " rolled " + toHitRoll.ToString () + " which missed.";
		}
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().AlertLog (report);
	}

	public void AttackWithMysticBlast(PCHandler target) {
		string report ="";
		int armourRoll = Random.Range (target.minDefense, target.maxDefense);
		int enemyStartingHealth = target.currentHP;
		int finalDamage = System.Math.Max (damage - armourRoll, 0);
		report = target.unitName + "has been attacked by " + unitName + " with the spell " + name + ". It autohit dealing " + damage.ToString () + "-("
			+ target.minDefense.ToString () + " to " + target.maxDefense.ToString () + ") = " + finalDamage.ToString () + " damage. ";
		if (target.takeDamage (damage)) {
			report += target.unitName + " died as a result.";
		} else {
			report += target.unitName + " went from " + enemyStartingHealth.ToString () + "/" + target.maxHP.ToString () + "HP to " + target.currentHP.ToString () + "/" + target.maxHP.ToString () + "HP";
		}
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().AlertLog (report);
	}

	public override string ToDisplayString () {
		string lineBreak = System.Environment.NewLine;
		string output = unitName + lineBreak;
		output += "Enemy Unit" + lineBreak;
		output += "Attack: " + attack.ToString() + lineBreak;
		output += "Dodge: " + dodge.ToString() + lineBreak;
		output += "Defense: " + minDefense.ToString() + "-" + maxDefense.ToString() + lineBreak;
		output += "Damage: " + damage.ToString() + lineBreak;
		output += "Move: " + move.ToString() + lineBreak;
		output += "Hit Points: " + currentHP.ToString() + "/" + maxHP.ToString() + lineBreak;
		output += "HP Regen: " + hPRegen.ToString() + lineBreak;
		output += "Energy: " + currentEnergy.ToString() + "/" + maxEnergy.ToString() + lineBreak;
		output += "Energy Regen: " + energyRegen.ToString() + lineBreak;
		output += "Descirption: " + description;
		return output;
	}

	public void Refresh () {
		heal (hPRegen);
		gainEnergy (energyRegen);
	}

}