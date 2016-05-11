using UnityEngine;
using System.Collections;
using Coords = BoardHandler.Coords;
using UnityEngine.Assertions;
using Action = PCHandler.Action;
using System.Collections.Generic;

public class ActionHandler : MonoBehaviour {

	private BoardHandler boardHandler;
	// Use this for initialization
	void Start () {
		boardHandler = gameObject.GetComponent<BoardHandler> ();
	}

	public Action currentAction { get; private set; }

	public void PerformAction(Action action) {
		PCHandler actor = boardHandler.selectedUnit ();
		currentAction = action;
		HashSet<Unit> targets;
		Item i;
		string actionName = action.actionName;
		if (actionName == "Cancel") {
			boardHandler.gameState = BoardHandler.GameStates.TargetMode;
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
			boardHandler.gameState = BoardHandler.GameStates.ActionMode;
		} else {
			Assert.IsTrue (boardHandler.gameState == BoardHandler.GameStates.ActionMode);
			switch (actionName) {
			case "Do Nothing":
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (actor.unitName + " does nothing.");
				endActionPhase();
				break;
//			case "Punch":
//				currentAction = "Punch";
//				InitiateStandardMeleeAttack ();
//				break;
			case "Sword":
				InitiateSwordAttack ();
				break;
			case "Axe":
				InitiateAxeAttack ((Weapon) action.attachedItem);
				endActionPhase();
				break;
			case "Spear":
				InitiateSpearAttack ();
				break;
			case "Mace":
				InitiateMaceAttack ();
				break;
			case "Bow":
				InitiateBowAttack ();
				break;
			case "Deadeye":
				if (actor.currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					break;
				}
				actor.deadeyeActive = true;
				GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
				PerformAction (new Action ("Cancel", "", null));
				break;
			case "Power Blow":
				if (actor.currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					break;
				}
				actor.deadeyeActive = true;
				GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
				PerformAction (new Action ("Cancel", "", null));
				break;
			case "Mystic Blast":
			case "Greater Mystic Blast":
				if (actor.currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					break;
				}
				InitiateCardinalAttackTargeting (actor.spellRange(currentAction.actionName),actor.hasAbility(PCHandler.Ability.ArcingMysticBlast));
				break;
			case "Explosion":
				if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					break;
				}
				targets = boardHandler.GetOtherUnitsAround (boardHandler.selected, actor.spellRadius(currentAction.actionName));
				Debug.Log (targets.Count.ToString ());
				foreach (Unit target in targets) {
					TriggerAutohitSpellAttack (actor.spellPower(currentAction.actionName), target, "Explosion");
				}
				boardHandler.selectedUnit ().useEnergy (actor.spellCost(currentAction.actionName));
				endActionPhase();
				break;
			case "Heal":
			case "Greater Heal":
			case "Full Restore":
				if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					break;
				}
				InitiateNonpenetratingCardinalBuffTargeting (1);
				break;
			case "Discharge Ancient Magic":
				actor.dischargeAM ();
				if (((ActionItem)currentAction.attachedItem).itemClass == "Stone") {
					GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
					PerformAction (new Action ("Cancel", "", null));
					break;
				} else {
					endActionPhase();
					break;
				}
			case "Activate Ancient Magic":
				if (actor.ancientMagic) {
					actor.dischargeAM ();
				} 
				actor.ancientMagic = ((AncientMagic) currentAction.attachedItem);
				((AncientMagic)currentAction.attachedItem).isActive = true;
				GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (currentAction.attachedItem.itemName + " activated.");
				PerformAction (new Action ("Cancel", "", null));
				break;
			case "Pick up Item":
				if (boardHandler.selectedUnit ().remainingInventory > 0) {
					i = boardHandler.getTileState (boardHandler.selected).item;
					boardHandler.getTileState (boardHandler.selected).item = null;
					boardHandler.selectedUnit ().acquireItem (i);
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit ().unitName + " has picked up " + i.itemName);
					endActionPhase();
					currentAction = new Action ("", "", null);
					break;
				} else {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Not enough space in " + boardHandler.selectedUnit ().unitName + "'s inventory. Drop something first.");
					Assert.IsTrue (boardHandler.gameState == BoardHandler.GameStates.ActionMode);
					GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats(actor);
					GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().DropItemSelector ();
					currentAction = new Action ("", "", null);
					break;
				}
			case "Drop Item":
				i = boardHandler.getTileState (boardHandler.selected).item;
				if (currentAction.attachedItem == actor.ancientMagic) {
					//Active Ancient Magics can only be discharged, not dropped.
					actor.dischargeAM ();
				} else {
					boardHandler.getTileState (boardHandler.selected).item = action.attachedItem;
					boardHandler.selectedUnit ().loseItem (action.attachedItem);
					action.attachedItem.isOnFloor = true;
				}
				boardHandler.selectedUnit ().acquireItem (i);
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit ().unitName + " has picked up " + i.itemName);
				endActionPhase();
				break;
			case "Mass Healing":
				if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost(currentAction.actionName)) {
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
					boardHandler.gameState = BoardHandler.GameStates.TargetMode;
					PerformAction (new Action ("Cancel", "", null));
					endActionPhase();
					break;
				}
				targets = boardHandler.GetOtherUnitsAround (boardHandler.selected, actor.spellRadius(currentAction.actionName));
				foreach (Unit target in targets) {
					int initialHP = target.currentHP;
					target.heal (actor.spellPower(currentAction.actionName));
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit().unitName + " heals " + target.unitName + ". " + target.unitName + " goes from " + initialHP.ToString() + " to " + target.currentHP.ToString() + " out of " + target.maxHP.ToString());
				}		
				boardHandler.selectedUnit().useEnergy(actor.spellCost(currentAction.actionName));
				endActionPhase();
				break;
			case "Exchange Places":
				InitiateNonpenetratingCardinalMixedTargeting (1);
				break;
			case "All Out Defense":
				boardHandler.selectedUnit ().allOutDefenseActive = true;
				endActionPhase ();
				break;
			default:
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Invalid Action: " + actionName + "by " + actor.unitName + ". Doing nothing.");
				endActionPhase();
				break;
			}
		}
	}

	private void InitiateCardinalAttackTargeting(int range, bool ignoresUnitBlocking) {
		if (!boardHandler.CardinalAttackTargeting (range, ignoresUnitBlocking)) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}

	private void InitiateNonpenetratingCardinalBuffTargeting(int range) {
		if (!boardHandler.NonpenetratingCardinalBuffTargeting (range)) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}
	private void InitiateNonpenetratingCardinalMixedTargeting(int range) {
		if (!boardHandler.NonpenetratingCardinalMixedTargeting (range)) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}
	private void InitiateSwordAttack() {
		if (!boardHandler.SwordTargeting ()) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}
	private void InitiateMaceAttack() {
		if (!boardHandler.MaceTargeting ()) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}

	private void InitiateBowAttack() {
		if (!boardHandler.BowTargeting ()) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}

	private void InitiateSpearAttack() {
		if (!boardHandler.SpearTargeting ()) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
			PerformAction (new Action ("Cancel", "", null));
		}
	}

	private void InitiateAxeAttack(Weapon weapon) {
		int hitCount = 0;
		Coords s = boardHandler.selected;
		foreach (Coords c in new[] {s + Coords.UP, s + Coords.DOWN, s + Coords.LEFT, s + Coords.RIGHT,
			s + Coords.UP + Coords.LEFT, s + Coords.UP + Coords.RIGHT, s + Coords.DOWN + Coords.LEFT,
			s+Coords.DOWN + Coords.RIGHT}) {
			if (boardHandler.inBounds(c) && boardHandler.getTileState(c).unit) {
				if (!boardHandler.selectedUnit ().hasAbility (PCHandler.Ability.AxeMastery) || boardHandler.getTileState (c).unit is Enemy) {
					if (TriggerAttack (weapon, boardHandler.getTileState (c).unit, false)) {
						hitCount++;
					}
				}
			}
		}
		weapon.useCharges (hitCount);
	}

	//Returns a Coord with a 1 in each direction that the original didn't have a zero.
	public Coords getDirection (Coords c) {
		int x, y;
		if (c.x > 0) {
			x = 1;
		} else if (c.x < 0) {
			x = -1;
		} else {
			x=0;
		}
		if (c.y > 0) {
			y = 1;
		} else if (c.y < 0) {
			y = -1;
		} else {
			y=0;
		}
		return new Coords (x, y);
	}

	//Note: This only gets called for targeted abilities and is what is called after a target is selected.
	public void TriggerTargetedAbility(GameObject target) {
		PCHandler actor = boardHandler.selectedUnit();
		switch (currentAction.actionName) {
//		case "Punch":
//			TriggerAttack ("Punch", 0, 0, target.GetComponent<Enemy>());
//			currentAction = "";
//			endActionPhase();
//			break;
		case "Bow":
		case "Sword":
			TriggerAttack ((Weapon)currentAction.attachedItem, target.GetComponent<Enemy> (), true);
			endActionPhase();
			break;
		case "Mace":
			if (TriggerAttack ((Weapon)currentAction.attachedItem, target.GetComponent<Enemy> (), true)) {
				Knockback (target.GetComponent<Enemy> ());
			}
			endActionPhase();
			break;
		case "Spear":
			if (boardHandler.selectedUnit().hasAbility(PCHandler.Ability.SpearMastery)) {
				int hitCount = 0;
				Coords direction = getDirection (boardHandler.FindUnit (target.GetComponent<Unit> ()) - boardHandler.selected);
				if (boardHandler.getTileState (boardHandler.selected + direction).unit && boardHandler.getTileState (boardHandler.selected + direction).unit is Enemy) { 
					if (TriggerAttack ((Weapon)currentAction.attachedItem,boardHandler.getTileState (boardHandler.selected + direction).unit , false)) {
						hitCount += 1;
					}
				}
				if (boardHandler.getTileState (boardHandler.selected + direction + direction).unit && boardHandler.getTileState (boardHandler.selected + direction + direction).unit is Enemy) { 
					if (TriggerAttack ((Weapon)currentAction.attachedItem,boardHandler.getTileState (boardHandler.selected + direction + direction).unit , false)) {
						hitCount += 1;
					}
				}
				((Weapon)currentAction.attachedItem).useCharges (hitCount);
			} else {
				TriggerAttack ((Weapon)currentAction.attachedItem, target.GetComponent<Enemy> (), true);
			}
			endActionPhase();
			break;
		case "Mystic Blast":
		case "Greater Mystic Blast":
			if (actor.hasAbility(PCHandler.Ability.PenetratingMysticBlast)) {
				Coords direction = getDirection (boardHandler.FindUnit (target.GetComponent<Unit> ()) - boardHandler.selected);
				Coords currentTarget = boardHandler.selected;
				for (int i = 1; i < actor.spellRange (currentAction.actionName); i++) {
					currentTarget += direction;
					if (boardHandler.getTileState (currentTarget).unit && boardHandler.getTileState (currentTarget).unit is Enemy) {
						TriggerAutohitSpellAttack (actor.spellPower (currentAction.actionName), boardHandler.getTileState (currentTarget).unit, currentAction.actionName);
					} else if (boardHandler.getTileState (currentTarget).tile.blocksLineOfFire) {
						break;
					}
				}
			} else {
				TriggerAutohitSpellAttack (actor.spellPower (currentAction.actionName), target.GetComponent<Enemy> (), currentAction.actionName);
			}
				boardHandler.selectedUnit ().useEnergy (actor.spellCost (currentAction.actionName));
				endActionPhase();
				break;
		case "Heal":
		case "Greater Heal":
		case "Full Restore":
			Unit unitTarget = target.GetComponent<Unit> ();
			int initialHP = unitTarget.currentHP;
			if (currentAction.actionName == "Full Restore") {
				unitTarget.fullHeal ();
			} else {
				unitTarget.heal (actor.spellPower (currentAction.actionName));
			}
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit ().unitName + " heals " + unitTarget.unitName + ". " + unitTarget.unitName + " goes from " + initialHP.ToString () + " to " + unitTarget.currentHP.ToString () + " out of " + unitTarget.maxHP.ToString ());
			boardHandler.selectedUnit ().useEnergy (actor.spellCost (currentAction.actionName));
			endActionPhase();
			break;
		case "Exchange Places":
			boardHandler.swapPlaces (boardHandler.selected, boardHandler.FindUnit (target.GetComponent<Unit> ()));
			endActionPhase ();
			break;
		default:
			throw new System.Exception ("Invalid Ability");
		}
	}


	public void TriggerAutohitSpellAttack (int damage, Unit target, string name) {
		PCHandler attacker = (PCHandler)boardHandler.selectedUnit ();
		string report ="";
		int armourRoll = Random.Range (target.minDefense, target.maxDefense);
		int enemyStartingHealth = target.currentHP;
		int finalDamage = System.Math.Max (damage - armourRoll, 0);
		report = attacker.unitName + " attacks " + target.unitName + " with the spell " + name + ". It autohit dealing " + damage.ToString () + "-("
			+ target.minDefense.ToString () + " to " + target.maxDefense.ToString () + ") = " + finalDamage.ToString () + " damage. ";
			if (target.takeDamage (finalDamage)) {
				report += target.unitName + " died as a result.";
			} else {
				report += target.unitName + " went from " + enemyStartingHealth.ToString () + "/" + target.maxHP.ToString () + "HP to " + target.currentHP.ToString () + "/" + target.maxHP.ToString () + "HP";
			}
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (report);
	}

	//Returns true if the attack hits.
	public bool TriggerAttack(Weapon weapon, Unit target, bool useCharges) {
		PCHandler attacker = (PCHandler)boardHandler.selectedUnit ();
		bool hit = false;
		int baseDamage = attacker.damage + weapon.damage;
		int hitChance = attacker.attack + weapon.attack - target.dodge;
		if (weapon.itemClass == "Bow" && attacker.hasAbility(PCHandler.Ability.BowMastery)) {
			baseDamage += attacker.bowMasteryBonusDamage;
			hitChance += attacker.bowMasteryBonusAttack;
		}
		if (attacker.hasAbility(PCHandler.Ability.PowerBlow) && attacker.powerBlowActive) {
			baseDamage = baseDamage * 2;
		}
		string report ="";
		int toHitRoll = Random.Range (1, 100);
		if (toHitRoll <= hitChance) {
			hit = true;
			int armourRoll = Random.Range (target.minDefense, target.maxDefense);
			int enemyStartingHealth = target.currentHP;
			int damage = System.Math.Max (baseDamage - armourRoll, 0);
			report = attacker.unitName + " attacks " + target.unitName + " with the weapon " + weapon.itemName + ". Hit chance was " + hitChance.ToString () + "% and " + attacker.unitName + " rolled " + toHitRoll.ToString () + " which hit dealing " + baseDamage.ToString () + "-("
			                + target.minDefense.ToString () + " to " + target.maxDefense.ToString () + ") = " + damage.ToString () + System.Environment.NewLine;
			if (useCharges) {
				weapon.useCharges (1);
			}
			if (target.takeDamage (damage)) {
				report += target.unitName + " died as a result.";
			} else {
				report += target.unitName + " went from " + enemyStartingHealth.ToString () + "/" + target.maxHP.ToString () + "HP to " + target.currentHP.ToString () + "/" + target.maxHP.ToString () + "HP";
			}
		} else {
			report = attacker.unitName + " attacks " + target.unitName + " with the weapon " + weapon.itemName + ". Hit chance was " + hitChance.ToString () + "% and " + attacker.unitName + " rolled " + toHitRoll.ToString () + " which missed.";
		}
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (report);
		return hit;
	}

	void Knockback (Enemy e) {
		try {
		Debug.Log ("Knocking back");
		Coords c = boardHandler.FindUnit(e);
		Coords direction = getDirection (c - boardHandler.selected);
		if (!boardHandler.getTileState (c + direction).unit && boardHandler.getTileState (c + direction).tile.isPassable) {
			boardHandler.MoveUnit (c, c + direction); 
		} else if (!boardHandler.getTileState (c + direction).unit) {
			KnockbackDamage (e);
		} else {
			KnockbackDamage (e);
			KnockbackDamage (boardHandler.getTileState (c + direction).unit);
		}
		}
		catch (System.Exception exep) {
			if (exep.Message != "Unit not found") {
				throw exep;
			}
		}
	}

	void KnockbackDamage(Unit e) {
		int damage = ((PCHandler)boardHandler.selectedUnit ()).knockbackDamage;
		if (e.takeDamage (damage)) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (e.unitName + " took " + damage.ToString() + " Knockback Damage and died.");
		} else {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (e.unitName + " took " + damage.ToString() + " Knockback Damage and ended up at " + e.currentHP.ToString () + "/" + e.maxHP.ToString () + " HP");
		}
	}

	void endActionPhase() {
		if (boardHandler.selectedUnit().hasAbility(PCHandler.Ability.CarefulStrike)) {
			List<string> typesOfWeapons =  new List<string>{"Axe","Mace","Sword","Bow","Spear"};
			boardHandler.selectedUnit().strengthReserveActive = ( !typesOfWeapons.Contains(currentAction.actionName));
		}
			boardHandler.gameState = BoardHandler.GameStates.MovementMode;	
			currentAction = new Action ("", "", null);

	}

	// Update is called once per frame
	void Update () {
	
	}
}
