using UnityEngine;
using System.Collections;

//TODO: Implement Rest of this.

public class Tile : MonoBehaviour {
	public bool isPassable;
	public string tileName;
	public int movementCost; //Set to 1 for impassible terrain.
	public Sprite baseSprite;
	public bool blocksLineOfFire;
	public Sprite highlightedSprite;
	public bool isGoal;
	public Color nonTargetingColour = Color.clear;
	public Color friendlyTargetingColour = Color.green;
	public Color hostileTargetingColour = Color.red;
	public Color movementTargetingColour = Color.yellow;
	private Targeting _targeting;
	public Targeting targeting {
		get { return _targeting; }
		set { _targeting = value;
			if (_targeting == Targeting.MovementTargeting || _targeting == Targeting.InactiveMovementTargeting) {
				gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color = movementTargetingColour;
			} else if (_targeting == Targeting.HostileTargeting || _targeting == Targeting.InactiveHostileTargeting) {
				gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color = hostileTargetingColour;
			} else if (_targeting == Targeting.FriendlyTargeting || _targeting == Targeting.InactiveFriendlyTargeting) {
				gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color = friendlyTargetingColour;
			}
			if (_targeting != Targeting.NoTargeting) {
				Debug.Log ("Read Point");
				Color c = gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color;
				c.a = 0.2f;
				gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color = c;
			}
			if (_targeting == Targeting.MovementTargeting) {
				gameObject.transform.Find ("ActiveHighlighting").GetComponent<SpriteRenderer> ().color = movementTargetingColour;
			} else if (_targeting == Targeting.HostileTargeting) {
				gameObject.transform.Find ("ActiveHighlighting").GetComponent<SpriteRenderer> ().color = hostileTargetingColour;
			} else if (_targeting == Targeting.FriendlyTargeting) {
				gameObject.transform.Find ("ActiveHighlighting").GetComponent<SpriteRenderer> ().color = friendlyTargetingColour;
			}
			if (_targeting == Targeting.NoTargeting) {
				gameObject.transform.Find ("ActiveHighlighting").GetComponent<SpriteRenderer> ().color = nonTargetingColour;
				gameObject.transform.Find ("InactiveHighlighting").GetComponent<SpriteRenderer> ().color = nonTargetingColour;
			}
		}
	}

//	private bool _isHighlighted = false;
//	public bool isHighlighted {
//		get { return _isHighlighted; }
//		set {
//			if (_isHighlighted == value) {
//				return;
//			}
//			_isHighlighted = value;
//			if (_isHighlighted) {
//				gameObject.GetComponent<SpriteRenderer>().sprite = highlightedSprite;
//			} else {
//				gameObject.GetComponent<SpriteRenderer>().sprite = baseSprite;
//			}
//		}
//	}

	public void OnMouseDown() {
		if (GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().gameState == BoardHandler.GameStates.MovementMode) {
			if (targeting == Targeting.MovementTargeting) {
				GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().MoveSelectedTo (this);
			} else {
				GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().Unselect ();
			}
		} else {
			if (targeting == Targeting.HostileTargeting || targeting == Targeting.FriendlyTargeting || targeting == Targeting.MovementTargeting) {
				GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<ActionHandler>().TriggerAbility(gameObject);
			}
			//GameObject.FindGameObjectWithTag ("MessageLog").GetComponent<MessageBox> ().WarningLog ("Select an action first.");
		}
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
