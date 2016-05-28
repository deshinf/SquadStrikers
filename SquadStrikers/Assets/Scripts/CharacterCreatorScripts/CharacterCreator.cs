using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;

public class CharacterCreator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public GameObject characterToCreate;
	public int index;

	public GameObject CreateCharacter() {
		characterToCreate.GetComponent<PCHandler> ().initializationClass = ClassSelection ();
		characterToCreate.GetComponent<PCHandler>().startingInventory = new List<Item>{ (Item) (StartingWeaponSelection ()) };
		characterToCreate.GetComponent<PCHandler> ().unitName = GivenName ();
		return characterToCreate;
	}

	//Just returns the class name.
	public string ClassSelection() {
		Toggle activeToggle = gameObject.transform.Find ("ClassSelector").GetComponent<ToggleGroup> ().ActiveToggles ().First();
		return activeToggle.gameObject.transform.Find ("Label").GetComponent<Text> ().text;
	}


	public Weapon StartingWeaponSelection() {
		var activeToggle = gameObject.transform.Find ("StartingWeaponSelector").GetComponent<ToggleGroup> ().ActiveToggles ().First();
		Assert.IsNotNull (activeToggle);
		string weaponName = activeToggle.gameObject.transform.Find ("Label").GetComponent<Text> ().text;
		GameObject output;
		if (!GameObject.FindGameObjectWithTag("PlayerTeam").GetComponent<Database>().GetItemByName(weaponName, out output)) Assert.IsTrue (false);
		return output.GetComponent<Weapon>();
	}

	public string GivenName() {
		string result = gameObject.transform.Find("NamePanel/InputField/Text").GetComponent<Text>().text;
		//Debug.Log (result);
		return result;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
