using UnityEngine;
using System.Collections;
using Coords = BoardHandler.Coords;
using UnityEngine.Assertions;
using Action = PCHandler.Action;
using System.Collections.Generic;

public class ActionHandler : MonoBehaviour {

	// Use this for initialization
	BoardHandler boardHandler;
	GameObject defaultTarget;
	void Start () {
		boardHandler = gameObject.GetComponent<BoardHandler> ();
	}

	public Action currentAction { get; private set; }

	public bool SelectAction(Action action) {
		PCHandler actor = boardHandler.selectedUnit ();
		defaultTarget = null;
		//HashSet<Unit> targets;
		//Item i;
		string actionName = action.actionName;
		Assert.IsTrue (boardHandler.gameState == BoardHandler.GameStates.ActionMode);
		switch (actionName) { //Targeting
		case "Cancel":
		case "Do Nothing":
		case "Deadeye":
		case "Activate Ancient Magic":
		case"Pick Up Item":
		case "Drop Item":
		case "All Out Defense":
		case "Undo Movement":
		case "Exit Level":
			defaultTarget = boardHandler.Target (0, false, true, false, false, false, false, false, false, Targeting.FriendlyTargeting, Targeting.InactiveFriendlyTargeting, false);
			break;
		case "Sword":
			defaultTarget = boardHandler.Target(1,false,false,false,true,false,false,true,true,Targeting.HostileTargeting,Targeting.InactiveHostileTargeting,actor.hasAbility (PCHandler.Ability.SwordMastery));
			break;
		case "Axe":
			defaultTarget = boardHandler.Target(1,false,true,false,true,false,false,true,true,Targeting.HostileTargeting,Targeting.InactiveHostileTargeting,true);
			//InitiateAxeAttack ((Weapon) action.attachedItem);
			//endActionPhase();
			break;
		case "Spear":
			defaultTarget = boardHandler.Target (2, true, false, false, true,false, false,actor.hasAbility (PCHandler.Ability.SpearMastery), false, Targeting.HostileTargeting, Targeting.InactiveHostileTargeting, false);
			break;
		case "Mace":
			defaultTarget = boardHandler.Target (1, false, false, false, true,false,false, true, true, Targeting.HostileTargeting, Targeting.InactiveHostileTargeting,false);
			break;
		case "Bow":
			defaultTarget = boardHandler.Target(3,false,false,false,true,false,false,false,false, Targeting.HostileTargeting, Targeting.InactiveHostileTargeting,false);
			break;
		case "Mystic Blast":
		case "Greater Mystic Blast":
			if (actor.currentEnergy < actor.spellCost (action.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				break;
			}
			defaultTarget = boardHandler.Target (actor.spellRange (action.actionName), true, false, false, true,false,false,actor.hasAbility (PCHandler.Ability.ArcingMysticBlast), false, Targeting.HostileTargeting, Targeting.InactiveHostileTargeting,false);
			break;
		case "Explosion":
			if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost (action.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				break;
			}
			defaultTarget = boardHandler.Target (actor.spellRadius (action.actionName), false, true, true, true, true, false, true, false, Targeting.HostileTargeting, Targeting.InactiveHostileTargeting, false);
			break;
		case "Heal":
		case "Greater Heal":
		case "Full Restore":
			if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost (action.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				break;
			}
			defaultTarget = boardHandler.Target (1, true, false, true, false,false,false, false, false, Targeting.FriendlyTargeting, Targeting.InactiveFriendlyTargeting, false);
			break;
		case "Discharge Ancient Magic":
			if (actor.ancientMagic == null) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No ancient magic active.");
				break;
			} else {
				defaultTarget = boardHandler.Target (0, false, true, false, false, false, false, false, false, Targeting.FriendlyTargeting, Targeting.InactiveFriendlyTargeting, false);
				break;
			}
		case "Mass Healing":
			defaultTarget = boardHandler.Target (1, false, true, true, true, true, false, false, false, Targeting.FriendlyTargeting, Targeting.InactiveFriendlyTargeting, false);
			break;
		case "Exchange Places":
			defaultTarget = boardHandler.Target (1, true, false, true, true,false,false, false, false, Targeting.MovementTargeting, Targeting.InactiveMovementTargeting, false);
			break;
		default:
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Invalid Action: " + actionName + ".");
			break;
		}
		if (defaultTarget != null) {
			GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().UnselectAll ();
			currentAction = action;
			return true;
		} else {
			GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().SelectDoNothing ();
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Action cannot be preformed: " + actionName + ". Try another action.");
			return false;
		}
	}

//	private void InitiateCardinalAttackTargeting(int range, bool ignoresUnitBlocking) {
//		if (!boardHandler.CardinalAttackTargeting (range, ignoresUnitBlocking)) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//
//	private void InitiateNonpenetratingCardinalBuffTargeting(int range) {
//		if (!boardHandler.NonpenetratingCardinalBuffTargeting (range)) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//	private void InitiateNonpenetratingCardinalMixedTargeting(int range) {
//		if (!boardHandler.NonpenetratingCardinalMixedTargeting (range)) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//	private void InitiateSwordAttack() {
//		if (!boardHandler.SwordTargeting ()) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//	private void InitiateMaceAttack() {
//		if (!boardHandler.MaceTargeting ()) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//
//	private void InitiateBowAttack() {
//		if (!boardHandler.BowTargeting ()) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}
//
//	private void InitiateSpearAttack() {
//		if (!boardHandler.SpearTargeting ()) {
//			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found. Choose another action.");
//			PerformAction (new Action ("Cancel", "", null));
//		}
//	}

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

	//Note: Target can be null to select a default target or the actor for untargetted abilities.
	public void TriggerAbility(GameObject target) {
		if (target == null) {
			target = defaultTarget;
		}
		Assert.IsNotNull (target);
		PCHandler actor = boardHandler.selectedUnit();
		int initialHP;
		Item item;
		switch (currentAction.actionName) {
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
			initialHP = unitTarget.currentHP;
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

		case "Do Nothing":
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (actor.unitName + " does nothing.");
			endActionPhase();
			break;
		case "Deadeye":
			if (actor.currentEnergy < actor.spellCost(currentAction.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				boardHandler.gameState = BoardHandler.GameStates.TargetMode;
				PerformAction (new Action ("Cancel", "", null),null);
				break;
			}
			actor.deadeyeActive = true;
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
			PerformAction (new Action ("Cancel", "", null),null);
			break;
		case "Power Blow":
			if (actor.currentEnergy < actor.spellCost(currentAction.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				boardHandler.gameState = BoardHandler.GameStates.TargetMode;
				PerformAction (new Action ("Cancel", "", null),null);
				break;
			}
			actor.deadeyeActive = true;
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
			PerformAction (new Action ("Cancel", "", null),null);
			break;
		case "Explosion":
			if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost (currentAction.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				break;
			}
			HashSet<Unit> targets = boardHandler.GetOtherUnitsAround (boardHandler.selected, actor.spellRadius(currentAction.actionName));
			Debug.Log (targets.Count.ToString ());
			foreach (Unit t in targets) {
				TriggerAutohitSpellAttack (actor.spellPower(currentAction.actionName), t, "Explosion");
			}
			boardHandler.selectedUnit ().useEnergy (actor.spellCost(currentAction.actionName));
			endActionPhase();
			break;
		case "Discharge Ancient Magic":
			if (actor.ancientMagic == null) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No ancient magic active.");
				break;
			} else {
				actor.dischargeAM ();
				GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (actor);
				currentAction = new Action ("Cancel", "", null);
				TriggerAbility(null);
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
			PerformAction (new Action ("Cancel", "", null),null);
			endActionPhase();
			break;
		case "Pick up Item":
			if (boardHandler.selectedUnit ().remainingInventory > 0) {
				item = boardHandler.getTileState (boardHandler.selected).item;
				boardHandler.getTileState (boardHandler.selected).item = null;
				boardHandler.selectedUnit ().acquireItem (item);
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit ().unitName + " has picked up " + item.itemName);
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
			item = boardHandler.getTileState (boardHandler.selected).item;
			if (currentAction.attachedItem == actor.ancientMagic) {
				//Active Ancient Magics can only be discharged, not dropped.
				actor.dischargeAM ();
			} else {
				boardHandler.getTileState (boardHandler.selected).item = currentAction.attachedItem;
				boardHandler.selectedUnit ().loseItem (currentAction.attachedItem);
				currentAction.attachedItem.isOnFloor = true;
			}
			boardHandler.selectedUnit ().acquireItem (item);
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit ().unitName + " has picked up " + item.itemName);
			endActionPhase();
			break;
		case "Mass Healing":
			if (boardHandler.selectedUnit ().currentEnergy < actor.spellCost(currentAction.actionName)) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("Not enough energy.");
				boardHandler.gameState = BoardHandler.GameStates.TargetMode;
				PerformAction (new PCHandler.Action("Cancel","",null as Item), null);
				endActionPhase();
				break;
			}
			targets = boardHandler.GetOtherUnitsAround (boardHandler.selected, actor.spellRadius(currentAction.actionName));
			foreach (Unit t in targets) {
				initialHP = t.currentHP;
				t.heal (actor.spellPower(currentAction.actionName));
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (boardHandler.selectedUnit().unitName + " heals " + t.unitName + ". " + t.unitName + " goes from " + initialHP.ToString() + " to " + t.currentHP.ToString() + " out of " + t.maxHP.ToString());
			}		
			boardHandler.selectedUnit().useEnergy(actor.spellCost(currentAction.actionName));
			endActionPhase();
			break;
		case "All Out Defense":
			boardHandler.selectedUnit ().allOutDefenseActive = true;
			endActionPhase ();
			break;
		case "Undo Movement":
			boardHandler.UndoMove ();
			endActionPhase ();
			break;
		case "Exit Level":
			Assert.IsTrue (boardHandler.getTileState (boardHandler.selected).tile.isGoal);
			if (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth == 10) {
				UnityEngine.SceneManagement.SceneManager.LoadScene ("VictoryScreen");
				break;
			} else {
				((PCHandler)boardHandler.selectedUnit ()).Ascend ();
				boardHandler.gameState = BoardHandler.GameStates.MovementMode;	
				currentAction = new Action ("", "", null);
				break;
			}
		default:
			throw new System.Exception ("Invalid Ability");
		}
		boardHandler.canUndoMovement = false;
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

	//NOTE: THIS METHOD IS DANGEROUS. DO NOT USE IT UNLESS YOU KNOW WHAT YOU ARE DOING.
	//Immediately performs an action, bypassing the usual targeting and checks.
	public void PerformAction (Action action, GameObject target)
	{
		currentAction = action;
		TriggerAbility (target);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
