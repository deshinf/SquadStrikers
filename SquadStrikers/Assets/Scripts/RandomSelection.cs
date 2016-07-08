using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class RandomSelection {

	public static T Select<T>(Dictionary<T,float> possibilities) {
		float totalProbability = 0f;
		foreach (KeyValuePair<T,float> p in possibilities) {
			totalProbability += p.Value;
		}
		float selectionValue = Random.value * totalProbability;
		foreach (KeyValuePair<T,float> p in possibilities) {
			selectionValue -= p.Value;
			if (selectionValue < 0) {
				return p.Key;
			}
		}
		return Enumerable.Last<KeyValuePair<T,float>>(possibilities).Key;
	}

}
