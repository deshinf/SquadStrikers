using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public partial class PCHandler : Unit {

	public AbilityGrid grid;
	public bool ascended;

	public struct AbilityTransition {
		public Ability initial;
		public Ability final;
		public AbilityTransition(Ability initialAbility, Ability finalAbility){
			initial = initialAbility;
			final = finalAbility;
		}
	}


	public class AbilityGrid {
		public List<AbilityTransition> transitions;
		public AbilityGrid(List<AbilityTransition> transitionsList) {
			transitions = transitionsList;
		}

		public int NumberOfAccessibleNodes() {
			HashSet<Ability> seen = new HashSet<Ability>();
			List<Ability> toInspect = new List<Ability>();
			seen.Add (Ability.None);
			toInspect.Add (Ability.None);
			while (toInspect.Count > 0) {
				foreach (AbilityTransition aT in transitions) {
					if (aT.initial == toInspect [0] && !seen.Contains(aT.final)) {
						seen.Add (aT.final);
						toInspect.Add (aT.final);
					}
				}
				toInspect.RemoveAt (0);
			}
		return seen.Count - 1;
		}

		public bool isAccessible(Ability a) {
			if (a == Ability.None) return true;
			HashSet<Ability> seen = new HashSet<Ability>();
			List<Ability> toInspect = new List<Ability>();
			seen.Add (Ability.None);
			toInspect.Add (Ability.None);
			while (toInspect.Count > 0) {
				foreach (AbilityTransition aT in transitions) {
					if (aT.initial == toInspect [0] && !seen.Contains(aT.final)) {
						if (aT.final == a) return true;
						seen.Add (aT.final);
						toInspect.Add (aT.final);
					}
				}
				toInspect.RemoveAt (0);
			}
			return false;
		}

		public bool canBuyNext(List<Ability> l, Ability a) {
			if (a == Ability.None) return false;
			if (l.Contains (a)) return false;
			foreach (AbilityTransition aT in transitions) {
				if (aT.final == a && l.Contains (aT.initial)) return true;
			}
			return false;
		}
	}

	private int  stoneDamageBonus;
	//Unlike Enemies, player character's stats are calculated from their class's base stats plus modifiers, rather than set directly.
	public override int attack {
		get {
			int output = characterClass.baseAttack;
			if (hasAbility (Ability.WeaponMaster)) {
				output += weaponMasterModifier;
			}
			if (hasAbility (Ability.CarefulStrike) && carefulStrikeActive) {
				output += carefulStrikeBonus;
			}
			if (hasAbility (Ability.CrowdBrawler)) {
				foreach (Unit u in BoardHandler.GetBoardHandler().GetOtherUnitsAround(BoardHandler.GetBoardHandler().FindUnit(this),1,false,true)) {
					if (u is Enemy) {
						output += crowdBrawlerAttackModifier;
					}
				}
			}
			if (hasAbility (Ability.FormationFighter)) {
				foreach (Unit u in BoardHandler.GetBoardHandler().GetOtherUnitsAround(BoardHandler.GetBoardHandler().FindUnit(this),1,false,true)) {
					if (u is PCHandler) {
						output += formationFighterAccuracyModifier;
					}
				}
			}
			if (hasAbility (Ability.Deadeye) && deadeyeActive) {
				output += deadeyeModifier;
			}
			if (hasAbility (Ability.PowerBlow) && powerBlowActive) {
				output -= powerBlowAccuracyPenalty;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); } }
	public override int dodge { get{ int output = characterClass.baseDodge;
			if (hasAbility (Ability.Dodger)) {
				output += dodgerModifier;
			}
			if (hasAbility (Ability.AllOutDefense) && allOutDefenseActive) {
				output = output * 2;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //attack-opponent's dodge is base chance to hit with attack.
	public override int minDefense { get{ return characterClass.baseMinDefense; }
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  }
	public override int maxDefense { get{
			int output = characterClass.baseMaxDefense;
			if (ancientMagic && ancientMagic.itemClass == "Stone") {
				output += ancientMagic.passiveExtent; 
			}
			if (hasAbility(Ability.Armourer)) {
				output += armourerDefenseModifier;
			}
			if (hasAbility (Ability.FormationFighter)) {
				foreach (Unit u in BoardHandler.GetBoardHandler().GetOtherUnitsAround(BoardHandler.GetBoardHandler().FindUnit(this),1,false,true)) {
					if (u is PCHandler) {
						output += formationFighterArmourModifier;
					}
				}
			}
			if (hasAbility (Ability.AllOutDefense) && allOutDefenseActive) {
				output = output * 2;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //A random number between these two is subtracted from all damage taken.
	public override int damage { get{
			int output = characterClass.baseDamage + stoneDamageBonus;
			if (hasAbility (Ability.StrongMan)) {
				output += strongManModifier;
			} if (hasAbility (Ability.StrenthReserves)) {
				output += strengthReserveBonus;
			}
			if (hasAbility (Ability.CrowdBrawler)) {
				foreach (Unit u in BoardHandler.GetBoardHandler().GetOtherUnitsAround(BoardHandler.GetBoardHandler().FindUnit(this),1,false,true)) {
					if (u is Enemy) {
						output += crowdBrawlerDamageModifier;
					}
				}
			}
			if (hasAbility (Ability.PowerBlow) && powerBlowActive) {
				output = output * 2;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //Base damage dealt with an attack
	public override int move { get{
			if (ancientMagic && ancientMagic.itemClass == "Mobility") {
				return characterClass.baseMoveSpeed + ancientMagic.passiveExtent; 
			} else {
				return characterClass.baseMoveSpeed;
			}
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //Base damage dealt with an attack
	public override int maxHP { get{
			int output = characterClass.baseMaxHP;
			if (hasAbility(Ability.ToughGuy1)) {
				if (hasAbility (Ability.ToughGuy2)) {
					if (hasAbility (Ability.ToughGuy3)) {
						output += toughGuy3CumulativeModifier;
					}
					output += toughGuy2CumulativeModifier;
				}
				output += toughGuyModifier;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  }
	public override int hPRegen { get{
			int output = characterClass.baseHPRegen;
			if (ancientMagic && ancientMagic.itemClass == "Health") {
				output += ancientMagic.passiveExtent;
			}
			if (hasAbility (Ability.FastHealer)) {
				output += fastHealerModifier;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //Gain this much each turn up to max.
	public override int maxEnergy { get{
			if (hasAbility (Ability.MagicalReserves1)) {
				if (hasAbility (Ability.MagicalReserves2)) {
					if (hasAbility (Ability.MagicalReserves3)) {
						return characterClass.baseMaxEnergy + magicalReserves1Modifier;
					}
					return characterClass.baseMaxEnergy + magicalReserves2Modifier;
				}
				return characterClass.baseMaxEnergy + magicalReserves1Modifier;
			}
			return characterClass.baseMaxEnergy;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  }
	public override int energyRegen {get{
			if (hasAbility (Ability.ManaCycling)) {
				return characterClass.baseDamage + manaCyclingModifier;
			} else {
				return characterClass.baseDamage;
			}
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  } //Gain this much each turn up to max.
	private bool _canMove = true;
	public int inventoryCapacity { get{
			int output = characterClass.inventoryCapacity;
			if (hasAbility (Ability.HeavyLifter)) {
				output += 2;
			}
			return output;
		}
		set { throw new Exception ("Should not edit player stats directly. Add variable to represent effect."); }  }
	public int remainingInventory { get { return inventoryCapacity - inventory.Count; } }

	public List<Item> startingInventory;
	public List<Item> inventory;
	public AncientMagic ancientMagic;
	public bool carefulStrikeActive = false;
	public bool strengthReserveActive = false;
	public bool allOutDefenseActive = false;
	public bool deadeyeActive = false;
	public bool powerBlowActive = false;
	public enum Ability { None,
		SwordMastery,AxeMastery,SpearMastery,MaceMastery,BowMastery,//5
		Isolationist, CarefulStrike,StrenthReserves,CrowdBrawler,FormationFighter,//10
		StrongMan, PowerBlow, HeavyLifter,WeaponMaster,Conservationist, Deadeye,//16
		Armourer,AllOutDefense,Dodger,ExchangePlaces,//20
		ToughGuy1,ToughGuy2,ToughGuy3,FastHealer,//24
		MysticBlast,GreaterMysticBlast, EfficientMysticBlast,ArcingMysticBlast,PenetratingMysticBlast,//29
		Explosion,//30
		Heal,GroupHeal,GreaterHealing,FullRestore,EfficientHealer,//35
		MagicalReserves1,MagicalReserves2,MagicalReserves3,ManaCycling,//39
	}

	public List<Ability> abilityList = new List<Ability>{Ability.None };

	public bool hasAbility(Ability a) {
		return abilityList.Contains (a);
	}

	public struct Action {
		public string actionName;
		public string actionDescription;
		public Item attachedItem; //note: This is often null.
		public Action(string name, string description, Item item) {
			actionName = name;
			actionDescription = description;
			attachedItem = item;
		}
	}

	public List<Action> actionList {
		get {
			List<Action> actions = new List<Action> {new Action ("Do Nothing", "This unit will end its turn.", null)};
			foreach (Item item in inventory) {
				if (item is ActionItem) {
					actions.Add (((ActionItem)item).CreateAction ());
				}
			}
			if (hasAbility (Ability.PowerBlow) && !powerBlowActive) {
				actions.Add(new Action("Power Blow",getAbilityDescription(Ability.PowerBlow), null));
			}
			if (hasAbility (Ability.Deadeye) && !deadeyeActive) {
				actions.Add(new Action("Deadeye",getAbilityDescription(Ability.Deadeye), null));
			}
			if (hasAbility (Ability.AllOutDefense)) {
				actions.Add(new Action(getAbilityName(Ability.AllOutDefense),getAbilityDescription(Ability.AllOutDefense), null));
			}
			if (hasAbility (Ability.ExchangePlaces)) {
				actions.Add(new Action(getAbilityName(Ability.ExchangePlaces),getAbilityDescription(Ability.ExchangePlaces), null));
			}
			if (hasAbility (Ability.MysticBlast)) {
				actions.Add(new Action("Mystic Blast","Sends a blast in a cardinal direction, dealing 25 damage to the first target it hits. Range 4. Can't miss. Costs 30 energy.", null));
			}
			if (hasAbility (Ability.GreaterMysticBlast)) {
				actions.Add(new Action("Greater Mystic Blast","Sends a blast in a cardinal direction, dealing 50 damage to the first target it hits. Range 4. Can't miss. Costs 90 energy.", null));
			}
			if (hasAbility (Ability.Explosion)) {
				actions.Add(new Action("Explosion","Causes a burst of energy, dealing 50 undodgable damage to everyone within 3 squares of the caster. Costs 100 energy", null));
			}
			if (hasAbility (Ability.Heal)) {
				actions.Add(new Action("Heal","Restores 20 HP to an adjacent ally. Costs 30 energy", null));
			}
			if (hasAbility (Ability.GreaterHealing)) {
				actions.Add(new Action("Heal","Restores 40 HP to an adjacent ally. Costs 80 energy", null));
			}
			if (hasAbility (Ability.FullRestore)) {
				actions.Add(new Action("Heal","Restores all HP to an adjacent ally. Costs 150 energy", null));
			}
			if (hasAbility (Ability.GroupHeal)) {
				actions.Add(new Action("Mass Healing","Restores 20 HP to all adjacent allies. Costs 100 energy", null));
			}
			BoardHandler boardHandler = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
			Item onSquare = boardHandler.getTileState (boardHandler.FindUnit (this)).item;
			if (onSquare) {
				Debug.Log ("Creating Pickup");
				actions.Add (new Action ("Pick up Item", "Pick up " + onSquare.itemName + ":" + System.Environment.NewLine + onSquare.description, onSquare));
			}
			Debug.Log ("NumberOfActions: " + actions.Count.ToString());
			return actions;
		}
	}

	public void loseItem(Item item) {
		inventory.Remove (item);
	}

	public void acquireItem(Item item) {
		inventory.Add (item);
		item.owner = this;
	}

	public Sprite baseSprite;
	public bool dead; //TODO: Do something about this.
	public Sprite fatiguedSprite; //Sprite used when unit has already moved.
	public bool canMove {
		get {
			return _canMove;
		} set {
			if (_canMove == value) {
				return;
			}
			_canMove = value;
			if (_canMove) {
				gameObject.GetComponent<SpriteRenderer>().sprite = baseSprite;
			} else {
				gameObject.GetComponent<SpriteRenderer>().sprite = fatiguedSprite;
			}
		}
	}

	//This is the class name that the character will be created with.
	public string initializationClass;

	private CharacterClass _characterClass;

	public void endTurn() {
		BoardHandler board = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		if (hasAbility(Ability.Isolationist) && board.GetOtherUnitsAround (board.FindUnit (this), 1, false, true).Count == 0) {
			heal (IsolationistHealRate);
		}
	}

	//Triggers start-of-turn rejuvenation.
	public void refresh() {
		if (!dead) {
			canMove = true;
			heal (hPRegen);
			gainEnergy (energyRegen);
			stoneDamageBonus = 0;
			carefulStrikeActive = false;
			allOutDefenseActive = false;
			deadeyeActive = false;
			powerBlowActive = false;
			if (ancientMagic) {
				ancientMagic.charges -= 1;
				if (ancientMagic.charges == 0) {
					AncientMagic a = ancientMagic;
					ancientMagic = null;
					loseItem (a);
					Destroy (a.gameObject);
					GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (unitName + "'s " + a.itemName + "has run dry.");
				}
			}
		}
	}

	public void dischargeAM() {
		switch (ancientMagic.itemName) {
		case "Ancient Magic of Stone":
			stoneDamageBonus = ancientMagic.dischargeExtent;
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Ancient Magic of Stone discharged, giving +" + ancientMagic.dischargeExtent + " to damage.");
			break;
		case "Ancient Magic of Health":
			heal (5 * ancientMagic.dischargeExtent);
			break;
		case "Ancient Magic of Mobility":
			randomTeleport (ancientMagic.dischargeExtent);
			break;
		}
		AncientMagic a = ancientMagic;
		ancientMagic = null;
		loseItem (a);
		Destroy (a.gameObject);
		GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log (unitName + "'s Ancient Magic " + a.itemName + "has run dry.");
	}

	public CharacterClass characterClass { get { return _characterClass; } private set { _characterClass = value; } }

	void randomTeleport(int range) {
		BoardHandler bH = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		List<BoardHandler.Coords> candidates = bH.getOpenTilesAround (bH.FindUnit (this), range);
		if (candidates.Count == 0) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Teleport failed due to no open squares in range.");
		} else {
			bH.MoveSelectedTo(candidates [UnityEngine.Random.Range (0, candidates.Count - 1)]);
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Teleport successful.");
		}
	}

	void Init (string cClass) {
		characterClass = CharacterClass.classList [cClass];
		abilityList = characterClass.startingAbilities;
		fullHeal();
		fullEnergy();
		isFriendly = true;
		grid = characterClass.grid.MakeConcrete ();
		Debug.Log ("Made Grid");
	}

	// Use this for initialization
	void Start () {
		gameObject.transform.Translate (0f, 0f, -0.1f);
		Init (initializationClass);
		foreach (Item i in startingInventory) {
			Item item = ((GameObject)(Instantiate (i.gameObject, new Vector3 (0f, 0f, 0f), Quaternion.identity))).GetComponent<Item> ();
			item.owner = this;
			item.transform.parent = transform;
			inventory.Add (item);
		}
	}

	public override void OnMouseDown()
	{
		Debug.Log ("Character " + this.unitName + " clicked on.");
		if (targeting == Targeting.MovementTargeting) {
			GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().KeepSelectedStill();
		} else if (targeting == Targeting.HostileTargeting || targeting == Targeting.FriendlyTargeting) {
			GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<ActionHandler>().TriggerTargetedAbility(gameObject);
		} else if (canMove) {
			GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().Select(this);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override void Die () {
		BoardHandler board = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		board.getTileState (board.FindUnit (this)).unit = null;
		dead = true;
		gameObject.transform.Translate(new Vector3(1000f,1000f,1000f));
		GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().checkWonLevelOrGameOver ();
	}

	public void Ascend () {
		BoardHandler board = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
		board.getTileState (board.FindUnit (this)).unit = null;
		ascended = true;
		gameObject.transform.Translate(new Vector3(1000f,1000f,1000f));
		GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().checkWonLevelOrGameOver ();
	}

	public void learnAbility(Ability a) {
		abilityList.Add (a);
		if (a == Ability.ToughGuy1) {
			currentHP += toughGuyModifier;
		}
		if (a == Ability.ToughGuy2) {
			currentHP += toughGuy2CumulativeModifier - toughGuyModifier;
		}
		if (a == Ability.ToughGuy3) {
			currentHP += toughGuy3CumulativeModifier - toughGuy2CumulativeModifier;
		}
		if (a == Ability.MagicalReserves1) {
			currentEnergy += magicalReserves1Modifier;
		}
		if (a == Ability.MagicalReserves2) {
			currentEnergy += magicalReserves2Modifier;
		}
		if (a == Ability.MagicalReserves3) {
			currentEnergy += magicalReserves3Modifier;
		}
	}

	//==========================================Ability Parameters Are Here==============================================

	public static string getAbilityDescription(Ability a) {
		switch (a) {
		case Ability.ArcingMysticBlast:
			return "Causes (Greater) Mystic Blast to arc, preventing other units from blocking line of fire.";
		case Ability.AxeMastery:
			return "Prevents axes from damaging your own team.";
		case Ability.EfficientHealer:
			return "Gives a -20% cost reduction on all healing spells.";
		case Ability.EfficientMysticBlast:
			return "Gives (Greater) Mystic Blast a -20% cost reduction.";
		case Ability.Explosion:
			return "Spell costing 100 mana. Deals 50 damage to everyone within 3 squares of you (including allies). Can't miss.";
		case Ability.FullRestore:
			return "Spell costing 150 mana. Fully heals a target.";
		case Ability.GreaterHealing:
			return "Spell costing 80 mana. Heals a target for 40 HP.";
		case Ability.GreaterMysticBlast:
			return "Autohit attack spell. Can only attack in cardinal directions. Costs 90 mana, does 50 damage. Range 4.";
		case Ability.GroupHeal:
			return "Spell costing 80 mana. Heals all adjacent targets (including enemies) for 20 HP.";
		case Ability.Heal:
			return "Spell costing 30 mana. Heals a target for 20 HP.";
		case Ability.MaceMastery:
			return "Doubles the bonus damage resulting from knockback.";
		case Ability.MagicalReserves1:
			return "Increases Max Mana by 25.";
		case Ability.MagicalReserves2:
			return "Increases Max Mana by 25. Stacks with Magical Reserves 1";
		case Ability.MagicalReserves3:
			return "Increases Max Mana by 25. Stacks with Magical Reserves 1 and 2";
		case Ability.ManaCycling:
			return "Causes (Greater) Mystic Blast to arc, preventing other units from blocking line of fire.";
		case Ability.MysticBlast:
			return "Autohit attack spell. Can only attack in cardinal directions. Costs 30 mana, does 25 damage. Range 4.";
		case Ability.None:
			return "";
		case Ability.PenetratingMysticBlast:
			return "(Greater) Mystic Blast now hits all Enemies (not allies) within range in the direction of fire.";
		case Ability.SpearMastery:
			return "You can now attack two people lined up with your spear.";
		case Ability.StrongMan:
			return "Increases damage with weapons by 5.";
		case Ability.SwordMastery:
			return "Allows you to attack diagonally with a sword.";
		case Ability.WeaponMaster:
			return "Increases accuracy with weapons by 10.";
		case Ability.BowMastery:
			return "Increases damage by 5 and accuracy by 20 for bows.";
		case Ability.Isolationist:
			return "Heal 10 HP when you end a turn with no-one next to you (including diagonals)";
		case Ability.CarefulStrike:
			return "+25 Accuracy when you didn't move this turn.";
		case Ability.StrenthReserves:
			return "+10 weapon damage if you did not attack with a weapon last turn.";
		case Ability.CrowdBrawler:
			return "+2 damage and accuracy for each adjacent enemy (including diagonals).";
		case Ability.FormationFighter:
			return "+5 accuracy and armour for each adjacent ally (includes diagonals)";
		case Ability.PowerBlow:
			return "Spend 50 energy and take -25 Accuracy to double your damage for all weapon attacks this turn. Does not use up your action but does not stack with itself.";
		case Ability.HeavyLifter:
			return "Increases carrying capacity by  2.";
		case Ability.Conservationist:
			return "Gives a 20% chance of not damaging your weapon on hit.";
		case Ability.Deadeye:
			return "Spend 30 energy to get +50 Accuracy to all attacks this turn. Does not use up your action but does not stack with itself.";
		case Ability.Armourer:
			return "Grants +20 armour (armour deducts a random number from 0 to your armour value each time you take damage).";
		case Ability.AllOutDefense:
			return "Spend your action to double your dodge and your armour.";
		case Ability.Dodger:
			return "Increases your dodge chance by 25%.";
		case Ability.ExchangePlaces:
			return "Exchange places with an adjacent enemy.";
		case Ability.ToughGuy1:
			return "Increases maximum HP by 25.";
		case Ability.ToughGuy2:
			return "Increases maximum HP by 25. Stacks with Tough Guy 1.";
		case Ability.ToughGuy3:
			return "Increases maximum HP by 25. Stacks with Tough Guy 1 and 2.";
		case Ability.FastHealer:
			return "Increases HP regeneration by 4.";
		default:
			return "No description written for this ability";
		}
	}
	public static string getAbilityName(Ability a) {
		switch (a) {
		case Ability.ArcingMysticBlast:
			return "Arcing Mystic Blast";
		case Ability.AxeMastery:
			return "Axe Mastery";
		case Ability.EfficientHealer:
			return "Efficient Healer.";
		case Ability.EfficientMysticBlast:
			return "Efficient Blaster";
		case Ability.Explosion:
			return "Explosion";
		case Ability.FullRestore:
			return "Full Restore";
		case Ability.GreaterHealing:
			return "Greater Healing";
		case Ability.GreaterMysticBlast:
			return "Greater Mystic Blast";
		case Ability.GroupHeal:
			return "Mass Healing";
		case Ability.Heal:
			return "Heal";
		case Ability.MaceMastery:
			return "Mace Mastery";
		case Ability.MagicalReserves1:
			return "Magical Reserves I";
		case Ability.MagicalReserves2:
			return "Magical Reserves II";
		case Ability.MagicalReserves3:
			return "Magical Reserves III";
		case Ability.ManaCycling:
			return "Mana Cycling";
		case Ability.MysticBlast:
			return "Mystic Blast";
		case Ability.None:
			return "";
		case Ability.PenetratingMysticBlast:
			return "Penetrating Mystic Blast";
		case Ability.SpearMastery:
			return "Spear Mastery";
		case Ability.StrongMan:
			return "Strong Man";
		case Ability.SwordMastery:
			return "Sword Mastery";
		case Ability.WeaponMaster:
			return "Weapon Master";
		case Ability.BowMastery:
			return "Archery Mastery";
		case Ability.Isolationist:
			return "Isolationist";
		case Ability.CarefulStrike:
			return "Careful Strike";
		case Ability.StrenthReserves:
			return "Strength Reserves";
		case Ability.CrowdBrawler:
			return "Crowd Brawler";
		case Ability.FormationFighter:
			return "Formation Fighter";
		case Ability.PowerBlow:
			return "Power Blow";
		case Ability.HeavyLifter:
			return "Heavy Lifter";
		case Ability.Conservationist:
			return "Conservationist";
		case Ability.Deadeye:
			return "Deadeye";
		case Ability.Armourer:
			return "Armourer";
		case Ability.AllOutDefense:
			return "All Out Defense";
		case Ability.Dodger:
			return "Dodger";
		case Ability.ExchangePlaces:
			return "Exchange Places";
		case Ability.ToughGuy1:
			return "Tough Guy 1";
		case Ability.ToughGuy2:
			return "Tough Guy 2";
		case Ability.ToughGuy3:
			return "Tough Guy 3";
		case Ability.FastHealer:
			return "Fast Healer";
		default:
			return "No description written for this ability";
		}
	}


	public int magicalReserves1Modifier = 25;
	public int magicalReserves2Modifier = 50;
	public int magicalReserves3Modifier = 75; //Added to mana if you have this level of trait. Cumulative.
	public int manaCyclingModifier = 5;
	public int bowMasteryBonusDamage = 5;
	public int bowMasteryBonusAttack = 10;
	public int IsolationistHealRate = 10;
	public int strengthReserveBonus = 10;
	public int carefulStrikeBonus = 25;
	public int crowdBrawlerAttackModifier = 2;
	public int formationFighterAccuracyModifier = 5;
	public int deadeyeModifier = 50;
	public int powerBlowAccuracyPenalty = 25;
	public int dodgerModifier = 25;
	public int armourerDefenseModifier = 20;
	public int toughGuyModifier = 25;	
	public int toughGuy2CumulativeModifier = 50;
	public int toughGuy3CumulativeModifier = 75;
	public int fastHealerModifier = 4;
	public int formationFighterArmourModifier = 5;
	public int crowdBrawlerDamageModifier = 2;
	public float conservationistProtectionProbability = 0.2f;

	public int knockbackDamage {
		get { return hasAbility (Ability.MaceMastery) ? 20 : 10;
		}
	}

	public int spellCost(string spellname) {
		switch (spellname) {
		case "Mystic Blast":
			return hasAbility(Ability.EfficientMysticBlast) ? 30 : 24;
		case "Greater Mystic Blast":
			return hasAbility(Ability.EfficientMysticBlast) ? 90 : 72;
		case "Explosion":
			return 100;

		case "Heal":
			return hasAbility(Ability.EfficientMysticBlast) ? 30 : 24;

		case "Mass Healing":
			return hasAbility(Ability.EfficientHealer) ? 100 : 80;

		case "Greater Heal":
			return hasAbility(Ability.EfficientHealer) ? 80 : 64;

		case "Full Restore":
			return hasAbility(Ability.EfficientHealer) ? 150 : 120;

		case "Power Blow":
			return 50;
		case "Deadeye":
			return 30;

		default:
			throw new Exception ("Invalid Spell Cost Request: " + spellname);
		}
	}

	public int spellRange(string spellname) {
		switch (spellname) {
		case "Mystic Blast":
			return 4;

		case "Greater Mystic Blast":
			return 4;

		default:
			return 1;

		}
	}

	public int spellPower(string spellname) {
		switch (spellname) {
		case "Mystic Blast":
			return 25;

		case "Greater Mystic Blast":
			return 50;

		case "Explosion":
			return 50;

		case "Heal":
			return 20;

		case "Mass Healing":
			return 20;

		case "Greater Heal":
			return 40;

		default:
			throw new Exception ("Invalid Spell Damage Request: " + spellname);
		}
	}

	public int spellRadius(string spellname) {
		switch (spellname) {
		case "Explosion":
			return 3;

		default:
			throw new Exception ("Invalid Spell Radius Request: " + spellname);
		}
	}

	public int weaponMasterModifier= 10; //Added to attack if you have Weapon Master.
	public int strongManModifier = 5; //Added to damage if you have Strong Man.	public enum Ability { None, SwordMastery,AxeMastery,SpearMastery,StrongMan,WeaponMaster,MysticBlast,Explosion,Heal,GroupHeal}


}
