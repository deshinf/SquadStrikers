using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillButtonScript : MonoBehaviour {

	public PCHandler.Ability ability;
	public Color boughtColor;

	public enum Statuses { AlreadyKnown, CanBuy, CannotBuy };
	private Statuses _status;
	public Statuses status {
		get { return _status; }
		set {
			_status = value;
			if (_status == Statuses.CannotBuy) {
				gameObject.GetComponent<Button> ().interactable = false;
			} else if (_status == Statuses.AlreadyKnown) {
				gameObject.GetComponent<Button> ().interactable = false;
				gameObject.GetComponent<Button> ().image.color = boughtColor;
			} else {
				gameObject.GetComponent<Button> ().onClick.AddListener (() => {
					SelectThis ();
				});
			}
		}
	}

	void SelectThis () {
		GridPanelScript gPS = gameObject.transform.parent.gameObject.GetComponent<GridPanelScript> ();
		gPS.ChosenAbility (ability);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
