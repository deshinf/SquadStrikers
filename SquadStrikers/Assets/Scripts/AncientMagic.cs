using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class AncientMagic : ActionItem {

	public int charges;
	public int maxCharges;
	public bool isActive;
	public int passiveExtent;
	public int perChargeDischargeExtent, baseDischargeExtent;
	public int dischargeExtent {
		get { return perChargeDischargeExtent * charges + baseDischargeExtent; }
	}

	public override PCHandler.Action CreateAction () {
		if (isActive) {
			return new PCHandler.Action ("Discharge Ancient Magic", description, this);
		} else {
			return new PCHandler.Action ("Activate Ancient Magic", description, this);
		}
	}

	// Use this for initialization
	void Start () {
		charges = maxCharges;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public override string ToDisplayString ()
	{
		string lineBreak = System.Environment.NewLine;
		string output = itemName + "(" + itemClass + "):" + lineBreak + description + lineBreak;
		output += (isActive ? "Currently Inactive" : "<color=green> Active</color>") + lineBreak;
		output += "Charges: " + charges + "/" + maxCharges;
		return output;
	}

	//======================================================IO Here=============================================================//

	[System.Serializable]
	public class AncientMagicSave : ActionItemSave {
		public int charges;
		public bool isActive;
		//Owner takes possession of this instead.
//		public int _owner;
		public bool _isOnFloor;


		public AncientMagicSave(AncientMagic am) {
			itemName = am.itemName;
			charges = am.charges;
			isActive = am.isActive;
//			if (am.owner == null) {
//				_owner = -1;
//			} else {
//				_owner = -2;	
//				for (int i = 0; i < PlayerTeamScript.TEAM_SIZE; i++) {
//					if (am.owner == GameObject.FindGameObjectWithTag("PlayerTeam").GetComponent<PlayerTeamScript>().getTeamMember(i)) {
//						_owner = i;
//					}
//				}
//				Assert.IsFalse(_owner == -2);
//			}
			_isOnFloor = am.isOnFloor;
		}

		public override GameObject ToGameObject () {
			GameObject prefab;
			if (!GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetItemByName (itemName, out prefab)) throw new System.Exception ("Item not in database");
			GameObject output = ((GameObject)(Instantiate (prefab, new Vector3 (0f, 0f, 0f), Quaternion.identity)));
//			if (_owner == -1) {
//				output.GetComponent<Item> ().owner = null;
//			} else {
//				output.GetComponent<Item> ().owner = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().getTeamMember (_owner);
//			}
			output.GetComponent<Item> ().isOnFloor = _isOnFloor;
			if (!_isOnFloor) {
				output.GetComponent<SpriteRenderer> ().enabled = false;
			}
			output.GetComponent<AncientMagic> ().charges = charges;
			output.GetComponent<AncientMagic> ().isActive = isActive;
			return output;
		}
	}
}
