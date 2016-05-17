using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Assertions;
using UnityEditor;

public class IOScript : MonoBehaviour {

	[SerializeField] private static string versionNumber = "0.1.4";
	private int needToLoad; //Frames until load.

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

	public void SaveGame () {
		Debug.Log ("Saving");
//		GameObject.FindGameObjectWithTag("MessageBox").GetComponent<MessageBox>().Log ("Saving");
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (!(Directory.Exists(Application.dataPath + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/Saves");
		}
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".save")) {
			if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup")) {
				if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup2")) {
					if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup3")) {
						FileUtil.DeleteFileOrDirectory (Application.dataPath + "/Saves/" + saveName + ".backup3");
					}
					FileUtil.MoveFileOrDirectory (Application.dataPath + "/Saves/" + saveName + ".backup2", Application.dataPath + "/Saves/" + saveName + ".backup3");
				}
				FileUtil.MoveFileOrDirectory (Application.dataPath + "/Saves/" + saveName + ".backup", Application.dataPath + "/Saves/" + saveName + ".backup2");
			}
			FileUtil.MoveFileOrDirectory (Application.dataPath + "/Saves/" + saveName + ".save", Application.dataPath + "/Saves/" + saveName + ".backup");
		}
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.dataPath + "/Saves/" + saveName + ".save");
		bf.Serialize(file, new saveClass(GameObject.FindGameObjectWithTag("PlayerTeam").GetComponent<PlayerTeamScript>(),GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>()));
		file.Close();
	}

	public List<string> ListOfSaves () {
		if (!(Directory.Exists(Application.dataPath + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/Saves");
		}
		string[] fullFiles = Directory.GetFiles(Application.dataPath + "/Saves");
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

	public void DeleteSave () {
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (!(Directory.Exists(Application.dataPath + "/Saves"))) {
			Directory.CreateDirectory (Application.dataPath + "/Saves");
		}

		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".save")) {
			File.Delete (Application.dataPath + "/Saves/" + saveName + ".save");
		}
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup")) {
			File.Delete (Application.dataPath + "/Saves/" + saveName + ".backup");
		}
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup2")) {
			File.Delete (Application.dataPath + "/Saves/" + saveName + ".backup2");
		}
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".backup3")) {
			File.Delete (Application.dataPath + "/Saves/" + saveName + ".backup3");
		}
	}

	public void LoadGame (string saveName) {
		Debug.Log ("Loading");
//		GameObject.FindGameObjectWithTag("MessageBox").GetComponent<MessageBox>().Log ("Loading");
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".save")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.dataPath + "/Saves/" + saveName + ".save", FileMode.Open);
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

	public void setUpToLoadIfExists () {
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".save")) {
			needToLoad = 3;
		}
	}

	public void LoadIfExists () {
		string saveName = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().saveName;
		if (File.Exists (Application.dataPath + "/Saves/" + saveName + ".save")) {
			LoadGame (saveName);
		}
	}

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (needToLoad == 1) {
			LoadIfExists ();
			needToLoad = 0;
		} else if (needToLoad > 1) {
			needToLoad -= 1;
		}
	}
}