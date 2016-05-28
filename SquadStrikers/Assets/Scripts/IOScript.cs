using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Assertions;
//using UnityEditor;
using UnityEngine.UI;
using System.Linq;

public class IOScript : MonoBehaviour {

	[SerializeField] private static string versionNumber = "0.2.0";
	public int needToLoad; //Frames until load.
	public int needToSave;
	[SerializeField] private static string defaultLocation = "SquadStikers.default";

	[System.Serializable]
	private class saveClass
	{
		public PlayerTeamScript.PlayerTeamScriptSave players;
		public BoardHandler.BoardHandlerSave board;
		public string versionNumber;

		public saveClass (PlayerTeamScript playerTeam, BoardHandler boardHandler) {
			players = new PlayerTeamScript.PlayerTeamScriptSave(playerTeam);
			board = new BoardHandler.BoardHandlerSave(boardHandler);
			versionNumber = IOScript.versionNumber;
		}
	}

	[System.Serializable]
	private class DefaultSave
	{
		string saveName;
		PlayerSetupSave[] players;

		[System.Serializable]
		public struct PlayerSetupSave
		{
			public string unitName;
			public string characterClass;
			public string startingItem;
		}

		public DefaultSave() {
			saveName = GameObject.FindGameObjectWithTag ("SaveNamePanel").GetComponent<InputField> ().text;
			GameObject[] characterPanels = GameObject.FindGameObjectsWithTag("CharacterCreationPanel");
			players = new PlayerSetupSave[characterPanels.Length];
			for (int i = 0; i < characterPanels.Length; i++) {
				CharacterCreator cc = characterPanels[i].GetComponent<CharacterCreator> ();
				PlayerSetupSave currentPlayer = new PlayerSetupSave();
				Toggle activeToggle = cc.transform.Find ("ClassSelector").GetComponent<ToggleGroup> ().ActiveToggles ().First();
				currentPlayer.characterClass = activeToggle.gameObject.transform.Find ("Label").GetComponent<Text> ().text;
				activeToggle = cc.transform.Find ("StartingWeaponSelector").GetComponent<ToggleGroup> ().ActiveToggles ().First();
				currentPlayer.startingItem = activeToggle.gameObject.transform.Find ("Label").GetComponent<Text> ().text;
				currentPlayer.unitName = cc.transform.Find("NamePanel/InputField/Text").GetComponent<Text>().text;
				players[i] = currentPlayer;
			}

		}

		public void Unpack () {
			GameObject.FindGameObjectWithTag ("SaveNamePanel").GetComponent<InputField> ().text = saveName;
			GameObject[] characterPanels = GameObject.FindGameObjectsWithTag("CharacterCreationPanel");
			//Debug.Log ("___" + characterPanels.Length);
			for (int i = 0; i < characterPanels.Length; i++) {
				CharacterCreator cc = characterPanels[i].GetComponent<CharacterCreator> ();
				cc.transform.Find ("ClassSelector").GetComponent<ToggleGroup> ().SetAllTogglesOff ();
				cc.transform.Find ("StartingWeaponSelector").GetComponent<ToggleGroup> ().SetAllTogglesOff ();
				cc.transform.Find ("NamePanel/InputField").GetComponent<InputField> ().text = players [i].unitName;
				Debug.Log (players [i].unitName);
				foreach (Toggle t in cc.transform.Find ("ClassSelector").GetComponentsInChildren<Toggle> ()) {
					if (t.gameObject.transform.Find ("Label").GetComponent<Text> ().text == players [i].characterClass) {
						t.isOn = true;
					}
					//Debug.Log (t.gameObject.transform.Find ("Label").GetComponent<Text> ().text + "//" + players [i].characterClass);
				}
				foreach (Toggle t in cc.transform.Find ("StartingWeaponSelector").GetComponentsInChildren<Toggle> ()) {
					if (t.gameObject.transform.Find ("Label").GetComponent<Text> ().text == players [i].startingItem) {
						t.isOn = true;
					}
				}
			}
		}
	}

	public void SaveAsDefault () {
		
		if (!(Directory.Exists(Application.dataPath + "/.." + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/.." + "/Saves");
		}
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + defaultLocation)) {
			File.Delete (Application.dataPath + "/.." + "/Saves/" + defaultLocation);
		}
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.dataPath + "/.." + "/Saves/" + defaultLocation);
		bf.Serialize(file, new DefaultSave());
		file.Close();
	}

	public void SaveGame () {
		Debug.Log ("Saving");
//		GameObject.FindGameObjectWithTag("MessageBox").GetComponent<MessageBox>().Log ("Saving");
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (!(Directory.Exists(Application.dataPath + "/.." + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/.." + "/Saves");
		}
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".save")) {
			if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup")) {
				if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup2")) {
					if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup3")) {
						File.Delete (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup3");
					}
					File.Move(Application.dataPath + "/.." + "/Saves/" + saveName + ".backup2", Application.dataPath + "/.." + "/Saves/" + saveName + ".backup3");
				}
				File.Move(Application.dataPath + "/.." + "/Saves/" + saveName + ".backup", Application.dataPath + "/.." + "/Saves/" + saveName + ".backup2");
			}
			File.Move(Application.dataPath + "/.." + "/Saves/" + saveName + ".save", Application.dataPath + "/.." + "/Saves/" + saveName + ".backup");
		}
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.dataPath + "/.." + "/Saves/" + saveName + ".save");
		bf.Serialize(file, new saveClass(GameObject.FindGameObjectWithTag("PlayerTeam").GetComponent<PlayerTeamScript>(),GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>()));
		file.Close();
	}

	public List<string> ListOfSaves () {
		if (!(Directory.Exists(Application.dataPath + "/.." + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/.." + "/Saves");
		}
		string[] fullFiles = Directory.GetFiles(Application.dataPath + "/.." + "/Saves");
		List<string> output = new List<string> ();
		foreach (string filePath in fullFiles) {
			if (filePath.EndsWith(".save")) {
				char[] chars = filePath.ToCharArray ();
				int lastSlashPosition = -1;
				for (int i = 0; i < chars.Length; i++) {
					if (chars[i] == '/' || chars[i] == '\\') {
						lastSlashPosition = i;
					}
				}
				output.Add(filePath.Substring(lastSlashPosition + 1, filePath.Length - 5 - lastSlashPosition - 1));
			}
		}
		return output;
	}

	public void DeleteSave() {
		DeleteSave (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName);
	}

	public void DeleteSave (string saveName) {
		if (!(Directory.Exists(Application.dataPath + "/.." + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/.." + "/Saves");
		}

		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".save")) {
			File.Delete (Application.dataPath + "/.." + "/Saves/" + saveName + ".save");
		}
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup")) {
			File.Delete (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup");
		}
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup2")) {
			File.Delete (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup2");
		}
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup3")) {
			File.Delete (Application.dataPath + "/.." + "/Saves/" + saveName + ".backup3");
		}
	}

	public void LoadGame (string saveName) {
		//Debug.Log ("Loading");
		//		GameObject.FindGameObjectWithTag("MessageBox").GetComponent<MessageBox>().Log ("Loading");
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".save")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + "/.." + "/Saves/" + saveName + ".save", FileMode.Open);
			saveClass loaded = (saveClass)bf.Deserialize (file);
			file.Close ();
			if (versionNumber != loaded.versionNumber) {
				GameObject.FindGameObjectWithTag ("MessageBox").GetComponent<MessageBox> ().ErrorLog ("Save Game From Old Version. May not have loaded correctly.");
			}
			loaded.players.ToGameObject ();
			loaded.board.ToGameObject ();
		} else {
			throw new IOException ("Save File Does not exist.");
		}
	}

	public void LoadDefault () {
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + defaultLocation)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + "/.." + "/Saves/" + defaultLocation, FileMode.Open);
			DefaultSave loaded = (DefaultSave)bf.Deserialize (file);
			file.Close ();
			loaded.Unpack ();
		} else {
			throw new IOException ("Save File Does not exist.");
		}
	}

	public void setUpToLoadIfExists () {
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".save")) {
			needToLoad = 3;
		}
	}

	public void LoadIfExists () {
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (File.Exists (Application.dataPath + "/.." + "/Saves/" + saveName + ".save")) {
			LoadGame (saveName);
		}
	}

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
//		try
//		{
		Debug.Log("Loading Default");
			LoadDefault();
//		}
//		catch
//		{
//		}
	}
	
	// Update is called once per frame
	void Update () {
		if (needToLoad == 1) {
			LoadIfExists ();
			needToLoad = 0;
		} else if (needToLoad > 1) {
			needToLoad -= 1;
		}
		if (needToSave == 1) {
			SaveGame ();
			needToLoad = 0;
		} else if (needToSave > 1) {
			needToSave -= 1;
		}
	}
}