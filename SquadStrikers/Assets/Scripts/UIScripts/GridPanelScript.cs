using UnityEngine;
using System.Collections;
using Ability = PCHandler.Ability;
using AbilityGrid = PCHandler.AbilityGrid;
using AbilityTransition = PCHandler.AbilityTransition;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GridPanelScript : MonoBehaviour {

	public PCHandler playerCharacter;
	public GameObject SkillIconPrefab;
	public Ability[] abilities;
	public int playerIndex;
	[SerializeField] private bool _isActive = true;
	public bool isActive {
		get { return _isActive; }
		set {
			if (!value == _isActive) {
				_isActive = value;
				if (_isActive == false) {
					foreach (Transform g in gameObject.transform) {
						if (g.name != "PlayerTag") {
							g.gameObject.SetActive (false);
						}
					}
				} else {
					foreach (Transform g in gameObject.transform) {
						g.gameObject.SetActive (true);
					}

				}
			}
		}
	}


	bool stillToDraw = true;
	public float[] xCoords, yCoords;
	public float iconSize = 0.1f;//Relative to panel size.
	public Sprite[] AbilitySprites;
	public GameObject PointerGradientPrefab;
	public float pathWidth;
	private Ability _chosenAbility;
	public Ability chosenAbility { set {
			_chosenAbility = value;
			for (int i = 0; i < abilities.Length; i++) {
				if (abilities [i] == value) {
					gameObject.transform.Find ("PlayerTag/SkillIcon").gameObject.GetComponent<Image> ().sprite = AbilitySprites [i];
					gameObject.transform.Find ("RightHandDisplay/SkillIcon").gameObject.GetComponent<Image> ().sprite = AbilitySprites [i];
					gameObject.transform.Find ("RightHandDisplay/Text").gameObject.GetComponent<Text> ().text = PCHandler.getAbilityName(value) + ":" + System.Environment.NewLine + PCHandler.getAbilityDescription(value);
				}
			}
		}
		get { return _chosenAbility; }}

	public void Draw (PCHandler character) {
		playerCharacter = character;
		AbilityGrid grid = character.grid;
		foreach (AbilityTransition edge in grid.transitions) {
			if (grid.isAccessible (edge.initial)) {
				Vector2 start = new Vector2 (), finish = new Vector2 ();
				for (int i = 0; i < abilities.Length; i++) {
					if (abilities [i] == edge.initial) {
						start = new Vector2 (xCoords [i], yCoords [i]);
					}
					if (abilities [i] == edge.final) {
						finish = new Vector2 (xCoords [i], yCoords [i]);
					}
				}
				DrawPath (start, finish);
			}
		}
		for (int i = 0; i < abilities.Length; i++) {
			if (grid.isAccessible(abilities[i])) {
				GameObject skillButton = (GameObject) (GameObject.Instantiate (SkillIconPrefab, new Vector3 (0, 0, 0), Quaternion.identity));
				skillButton.transform.SetParent (gameObject.transform);
				skillButton.GetComponent<RectTransform> ().anchorMin = new Vector2 (xCoords [i] - iconSize/2, yCoords [i] - iconSize/2);
				skillButton.GetComponent<RectTransform> ().anchorMax = new Vector2 (xCoords [i] + iconSize/2, yCoords [i] + iconSize/2);
				skillButton.GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, 0f);
				skillButton.GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, 0f);
				skillButton.GetComponent<Image> ().sprite =AbilitySprites [i];
				skillButton.GetComponent<SkillButtonScript> ().ability =abilities [i];
				if (character.hasAbility (abilities [i])) {
					skillButton.GetComponent<SkillButtonScript> ().status = SkillButtonScript.Statuses.AlreadyKnown;
				} else if (grid.canBuyNext (character.abilityList, abilities[i])) {
					skillButton.GetComponent<SkillButtonScript> ().status = SkillButtonScript.Statuses.CanBuy;
				} else {
					skillButton.GetComponent<SkillButtonScript> ().status = SkillButtonScript.Statuses.CannotBuy;
				}
			}
		}
	}

	public void ChosenAbility(Ability a) {
		chosenAbility = a;
	}

	void DrawPath (Vector2 start, Vector2 finish) {
		float angle = Mathf.Atan2 ((finish - start).y, (finish - start).x) * 180f / Mathf.PI;
		float distance = (finish - start).magnitude - iconSize;
		Vector2 center = (finish + start) / 2;
		GameObject skillButton = (GameObject) (GameObject.Instantiate (PointerGradientPrefab, new Vector3 (0, 0, 0), Quaternion.identity));
		skillButton.transform.SetParent (gameObject.transform);
		skillButton.GetComponent<RectTransform> ().anchorMin = new Vector2 (center.x - distance/2, center.y - pathWidth/2);
		skillButton.GetComponent<RectTransform> ().anchorMax = new Vector2 (center.x + distance/2, center.y + pathWidth/2);
		skillButton.GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, 0f);
		skillButton.GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, 0f);
		skillButton.GetComponent<RectTransform> ().Rotate (new Vector3 (0f, 0f, angle));
	}

	// Use this for initialization
	void Start () {
		GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().getTeamMember (playerIndex);
		gameObject.transform.Find ("RightHandDisplay/FinalizeButton").gameObject.GetComponent<Button> ().onClick.AddListener(() => {FinalizeThis();});
	}
	
	// Update is called once per frame
	void Update () {
		if (stillToDraw) {
			Draw (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().getTeamMember (playerIndex));
			gameObject.transform.Find ("PlayerTag/Text").gameObject.GetComponent<Text> ().text = playerCharacter.unitName;
			stillToDraw = false;
			isActive = false;
		}
	}

	static void FinalizeThis() {
		foreach (GameObject grid in GameObject.FindGameObjectsWithTag("GridPanel")) {
			PCHandler player = grid.GetComponent<GridPanelScript> ().playerCharacter;
			Ability a = grid.GetComponent<GridPanelScript> ().chosenAbility;
			if (!player.hasAbility (a)) {
				player.learnAbility (a);
			}

		}
		SceneManager.LoadScene ("MainScene");
	}
}
