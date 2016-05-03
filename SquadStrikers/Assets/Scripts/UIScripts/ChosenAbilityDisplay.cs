using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChosenAbilityDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void SetAbility(PCHandler.Ability a) {
		for (int i = 0; i < gameObject.transform.parent.GetComponent<GridPanelScript>().abilities.Length; i++) {
			if (gameObject.transform.parent.GetComponent<GridPanelScript>().abilities [i] == a) {
				gameObject.GetComponent<Image> ().sprite = gameObject.transform.parent.GetComponent<GridPanelScript>().AbilitySprites [i];
			}
			gameObject.GetComponent<Text> ().text = a.ToString() + System.Environment.NewLine + PCHandler.getAbilityDescription (a);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
