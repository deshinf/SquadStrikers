using UnityEngine;
using System.Collections;

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
}
