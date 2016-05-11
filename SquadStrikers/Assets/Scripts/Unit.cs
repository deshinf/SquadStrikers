using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public abstract class Unit : MonoBehaviour {
    //TODO: This class isn't fully implemented yet

	public virtual int attack { get; set; }
	public virtual int dodge { get; set; } //attack-opponent's dodge is base chance to hit with attack.
	public virtual int minDefense { get; set; }
	public virtual int maxDefense { get; set; } //A random number between these two is subtracted from all damage taken.
	public virtual int damage { get; set; } //Base damage dealt with an attack
	public virtual int move { get; set; } //Amount of squares that can be moved each turn.
	public virtual int maxHP { get; set; }
	public virtual int currentHP { get; set; } //Cannot exceed previous. Run out to die.
	public virtual int hPRegen { get; set; } //Gain this much each turn up to max.
	public virtual int maxEnergy { get; set; }
	public virtual int currentEnergy { get; set; } //Cannot exceed previous. Use for special abilities.
	public virtual int energyRegen { get; set; } //Gain this much each turn up to max.
	[SerializeField] private string _unitName;
	public virtual string unitName {get { return _unitName; } set { _unitName = value; } }

	public Color basicColour = Color.white;
	public Color friendlyTargetingColour = Color.green;
	public Color hostileTargetingColour = Color.red;
	public Color movementTargetingColour = Color.yellow;

	public bool allowsHostilesThrough() {
		return (this is Enemy);
	}


	private Targeting _targeting = Targeting.NoTargeting;
	public Targeting targeting {
		get { return _targeting; }
		set {
			if (_targeting == value) {
				return;
			}
			_targeting = value;
			switch (_targeting) {
			case Targeting.NoTargeting:
				gameObject.GetComponent<SpriteRenderer> ().color = basicColour;
				break;
			case Targeting.FriendlyTargeting:
				gameObject.GetComponent<SpriteRenderer> ().color = friendlyTargetingColour;
				break;
			case Targeting.HostileTargeting:
				gameObject.GetComponent<SpriteRenderer> ().color = hostileTargetingColour;
				break;
			case Targeting.MovementTargeting:
				gameObject.GetComponent<SpriteRenderer> ().color = movementTargetingColour;
				break;
			default:
				throw new System.Exception ("Invalid Targeting Type");
			}
		}
	}

	public bool isFriendly = false;
//	private int _xPosition;
//	public int xPostion
//	{
//		get
//		{
//			return _xPosition;
//		}
//		set
//		{
//			_xPosition = value;
//			gameObject.GetComponent<Rigidbody2D>().MovePosition(value * BoardHandler.tileSize, _yPosition * BoardHandler.tileSize);
//		}
//	}
//	private int _yPosition;
//	public int yPostion
//	{
//		get
//		{
//			return _yPosition;
//		}
//		set
//		{
//			_yPosition = value;
//			gameObject.GetComponent<Rigidbody2D>().MovePosition(_xPosition * BoardHandler.tileSize, value * BoardHandler.tileSize);
//		}
//	}

	public void fullHeal() {
		currentHP = maxHP;
	}

	//Returns whether or not the unit was killed by the attack
	public bool takeDamage(int damage) {
		if (damage >= currentHP) {
			currentHP = 0;
			Die ();
			return true;
		} else {
			currentHP -= damage;
			return false;
		}
	}

	//If more is used than there is, sets to 0, but this should be checked before use in most cases.
	public void useEnergy(int energy) {
		if (energy >= currentEnergy) {
			currentEnergy = 0;
		} else {
			currentEnergy -= damage;
		}
	}

	public abstract void Die ();

	public void fullEnergy() {
		currentEnergy = maxEnergy;
	}

	public void heal(int i) {
		if (currentHP + i > maxHP) {
			currentHP = maxHP;
		} else {
			currentHP += i;
		}
	}

	public void OnMouseOver() {
		if (Input.GetMouseButtonDown (1)) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar>().displayStats(this);
		}
	}

	public void gainEnergy(int i) {
		if (currentEnergy + i > maxEnergy) {
			currentEnergy = maxEnergy;
		} else {
			currentEnergy += i;
		}
	}

	public void updatePosition(){
		//Checks the game board to find where it should be and updates its position to match.
		BoardHandler.Coords c = GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().FindUnit (this);
		gameObject.transform.position=new Vector3(c.x * BoardHandler.tileSize, c.y * BoardHandler.tileSize,-0.1f);
	}

	// Use this for initialization
    void Start () {
	
	}

	public abstract string ToDisplayString ();

	public virtual void OnMouseDown()
	{
		if (targeting == Targeting.HostileTargeting || targeting == Targeting.FriendlyTargeting) {
			GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<ActionHandler>().TriggerTargetedAbility(gameObject);
		}
	}

	public void OnMouseEnter () {
		if (targeting == Targeting.HostileTargeting || targeting == Targeting.FriendlyTargeting) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayUnitActionAndTarget(GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit(),GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<ActionHandler>().currentAction,gameObject);
		}
	}

	public void OnMouseExit () {
		if (targeting == Targeting.HostileTargeting || targeting == Targeting.FriendlyTargeting) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayUnitAndAction(GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit(),GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<ActionHandler>().currentAction);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
