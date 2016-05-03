using UnityEngine;
using System.Collections;

//TODO: Implement Rest of this.

public class Tile : MonoBehaviour {
	public bool isPassable;
	public int movementCost; //Set to 1 for impassible terrain.
	public Sprite baseSprite;
	public bool blocksLineOfFire;
	public Sprite highlightedSprite;
	public bool isGoal;

	private bool _isHighlighted = false;
	public bool isHighlighted {
		get { return _isHighlighted; }
		set {
			if (_isHighlighted == value) {
				return;
			}
			_isHighlighted = value;
			if (_isHighlighted) {
				gameObject.GetComponent<SpriteRenderer>().sprite = highlightedSprite;
			} else {
				gameObject.GetComponent<SpriteRenderer>().sprite = baseSprite;
			}
		}
	}

	void OnMouseDown() {
		if (GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().gameState == BoardHandler.GameStates.MovementMode) {
			if (_isHighlighted) {
				GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().MoveSelectedTo (this);
			} else {
				GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().Unselect ();
			}
		} else {
			
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
