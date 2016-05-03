using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SubstanceX : MonoBehaviour {

	private int _amount;
	public int amount {
		get { return _amount; }
		set {
			_amount = value;
			if (_amount < 4) {
				gameObject.GetComponent<TextMesh> ().text = new string ('X', _amount);
			} else if (_amount < 6) {
				gameObject.GetComponent<TextMesh> ().text = "XXX" + System.Environment.NewLine + new string ('X', _amount - 3);
			} else {
				gameObject.GetComponent<TextMesh> ().text = "X" + _amount.ToString();
			}
			}
	}
	public SubstanceX (int iAmount) {
		amount = iAmount;
	}

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<MeshRenderer> ().sortingLayerName = "Items";
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
