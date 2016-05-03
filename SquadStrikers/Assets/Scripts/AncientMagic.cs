using UnityEngine;
using System.Collections;

public class AncientMagic : ActionItem {

	public int charges;
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
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
