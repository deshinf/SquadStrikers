using UnityEngine;
using System.Collections;
using Ability = PCHandler.Ability;

//Note: This is not a MonoBehavior and should not be instantiated at runtime.
//There are a list of constants and those are the only instantiations that should exist (hence the private constructor).
//Changing classes should be done by changing the stats passed to the constructors for these
//constants. CLASS_LIST is a List of all classes.
using System.Collections.Generic;
using System.Linq;

public class CharacterClass{

	public int baseAttack;
	public int baseDodge; //attack-opponent's dodge is base chance to hit with attack.
	public int baseMinDefense;
	public int baseMaxDefense; //A random number between these two is subtracted from all damage taken.
	public int baseDamage; //Base damage dealt with an attack
	public int baseMaxHP; //Run out to die.
	public int baseHPRegen; //Gain this much each turn up to max.
	public int baseMaxEnergy;//Use for special abilities.
	public int baseEnergyRegen; //Gain this much each turn up to max.
	public int baseMoveSpeed;
	public string name;
	public string description;
	public static int maxAbilityGridInitializationAttempts = 100;
	public static int minSizeOfAbilityGrid = 10;//TODO: Bump this higher when more abilities exist.
	public ClassAbilityGrid grid;
	public List<Ability> startingAbilities;
	public int inventoryCapacity;

	public struct ClassAbilityTransitionPossibility {
		public Ability initial;
		public Ability final;
		public float probability;
		public ClassAbilityTransitionPossibility(Ability initialAbility, Ability finalAbility, float probabilityOfTransition){
			initial = initialAbility;
			final = finalAbility;
			probability = probabilityOfTransition;
		}
		public PCHandler.AbilityTransition MakeConcrete () {
			return new PCHandler.AbilityTransition (initial, final);
		}
	}

	public class ClassAbilityGrid {
		List<ClassAbilityTransitionPossibility> transitionProbabilities;

		public ClassAbilityGrid(List<ClassAbilityTransitionPossibility> transitions) {
			transitionProbabilities = transitions;
		}
		public PCHandler.AbilityGrid MakeConcrete() {
			int attempts = 0;
			List<PCHandler.AbilityTransition> transitions = new List<PCHandler.AbilityTransition> ();
			while (attempts < maxAbilityGridInitializationAttempts) {
				attempts++;
				transitions = new List<PCHandler.AbilityTransition> ();
				foreach (ClassAbilityTransitionPossibility trans in transitionProbabilities) {
					if (Random.value <= trans.probability) {
						transitions.Add (trans.MakeConcrete ());
						//Debug.Log (transitions.Count);
					}
				}
				PCHandler.AbilityGrid output = new PCHandler.AbilityGrid (transitions);
				Debug.Log (output.NumberOfAccessibleNodes ());
				if (output.NumberOfAccessibleNodes() >= minSizeOfAbilityGrid) {
					return new PCHandler.AbilityGrid(transitions);
				}
			}
			throw new System.Exception ("Ran out of attempts to Initialize Class Grid");
		}
	}

	public static readonly ClassAbilityGrid CompleteClassGrid = new ClassAbilityGrid (//For testing purposes only
		new List<ClassAbilityTransitionPossibility> ()
		{
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.GreaterMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.EfficientMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ArcingMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ManaCycling,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.Explosion,1f),
			new ClassAbilityTransitionPossibility(Ability.ArcingMysticBlast,Ability.PenetratingMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.EfficientMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.ArcingMysticBlast,1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Heal, 1f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GreaterHealing, 1f),
			//new ClassAbilityTransitionPossibility(Ability.Heal,Ability.ManaCycling, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GroupHeal, 1f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.EfficientHealer, 1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterHealing,Ability.FullRestore, 1f),
			new ClassAbilityTransitionPossibility(Ability.FullRestore,Ability.EfficientHealer, 1f),

			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.FastHealer, 1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.MagicalReserves1, 1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.MagicalReserves2, 1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.MagicalReserves3, 1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.ManaCycling, 1f),
			new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterMysticBlast,1f),
			//new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterHealing,1f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterHealing,1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.GreaterMysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.FullRestore,1f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.Explosion,1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.SwordMastery, 1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.SpearMastery, 1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.BowMastery, 1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.AxeMastery, 1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MaceMastery,1f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrenthReserves,1f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.FormationFighter,1f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.CarefulStrike,1f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.CrowdBrawler,1f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.Isolationist,1f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrongMan,1f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.StrongMan,1f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.StrongMan,1f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.StrongMan,1f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.StrongMan,1f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.WeaponMaster,1f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.WeaponMaster,1f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.WeaponMaster,1f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.WeaponMaster,1f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.WeaponMaster,1f),

			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.HeavyLifter,1f),
			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.PowerBlow,1f),

			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Deadeye,1f),
			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Conservationist,1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.ToughGuy1,1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.ToughGuy2,1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy2,Ability.ToughGuy3,1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.FastHealer,1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Armourer,1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.Dodger,1f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.HeavyLifter,1f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.AllOutDefense,1f),
			new ClassAbilityTransitionPossibility(Ability.Dodger,Ability.ExchangePlaces,1f),


		}
	);

	public static readonly ClassAbilityGrid KnightAbilities = new ClassAbilityGrid (//For testing purposes only
		new List<ClassAbilityTransitionPossibility> ()
		{
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MysticBlast,0.25f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.GreaterMysticBlast,0.05f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.EfficientMysticBlast,0.1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ArcingMysticBlast,0.1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ManaCycling,0.15f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.Explosion,0f),
			new ClassAbilityTransitionPossibility(Ability.ArcingMysticBlast,Ability.PenetratingMysticBlast,0.05f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.EfficientMysticBlast,0.1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.ArcingMysticBlast,0.1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Heal, 0.25f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GreaterHealing, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GroupHeal, 0.1f),
			//new ClassAbilityTransitionPossibility(Ability.Heal,Ability.ManaCycling, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.EfficientHealer, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.GreaterHealing,Ability.FullRestore, 0f),
			new ClassAbilityTransitionPossibility(Ability.FullRestore,Ability.EfficientHealer, 0.3f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.MagicalReserves1, 0.25f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.MagicalReserves2, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.MagicalReserves3, 0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.ManaCycling, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterMysticBlast,0.05f),
			//new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterHealing,0.05f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterMysticBlast,0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterHealing,0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.GreaterMysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.FullRestore,0.5f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.Explosion,0.5f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.SwordMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.SpearMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.BowMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.AxeMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MaceMastery, 0.5f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrenthReserves,0.35f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.FormationFighter,0.5f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.CarefulStrike,0.35f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.CrowdBrawler,0.35f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.Isolationist,0.5f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrongMan,0.5f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.StrongMan,0.4f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.StrongMan,0.4f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.StrongMan,0.6f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.StrongMan,0.6f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.WeaponMaster,0.5f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.WeaponMaster,0.6f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.WeaponMaster,0.6f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.WeaponMaster,0.4f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.WeaponMaster,0.4f),

			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.HeavyLifter,0.25f),
			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.PowerBlow,0.25f),

			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Deadeye,0.25f),
			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Conservationist,0.25f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.ToughGuy1,1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.ToughGuy2,0.5f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy2,Ability.ToughGuy3,0.25f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.FastHealer,0.3f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Armourer,1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.Dodger,0.2f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.HeavyLifter,0.3f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.AllOutDefense,1f),
			new ClassAbilityTransitionPossibility(Ability.Dodger,Ability.ExchangePlaces,0.2f),

		}
	);



	public static readonly ClassAbilityGrid MageAbilities = new ClassAbilityGrid (//For testing purposes only
		new List<ClassAbilityTransitionPossibility> ()
		{

			new ClassAbilityTransitionPossibility(Ability.None,Ability.MysticBlast,1f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.GreaterMysticBlast,0.7f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.EfficientMysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ArcingMysticBlast,0.6f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ManaCycling,0.3f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.Explosion,0.3f),
			new ClassAbilityTransitionPossibility(Ability.ArcingMysticBlast,Ability.PenetratingMysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.EfficientMysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.ArcingMysticBlast,0.4f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Heal, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GreaterHealing, 0.2f),
			//new ClassAbilityTransitionPossibility(Ability.Heal,Ability.ManaCycling, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GroupHeal, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.EfficientHealer, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterHealing,Ability.FullRestore, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.FullRestore,Ability.EfficientHealer, 0.2f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.MagicalReserves1, 1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.MagicalReserves2, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.MagicalReserves3, 0.25f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.ManaCycling, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterMysticBlast,0.25f),
			//new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterHealing,0.1f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterMysticBlast,0.25f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterHealing,0.1f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.GreaterMysticBlast,0.25f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.FullRestore,0.2f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.Explosion,0.2f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.SwordMastery, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.SpearMastery, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.BowMastery, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.AxeMastery, 0.1f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MaceMastery, 0.1f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrenthReserves,0.3f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.FormationFighter,0.3f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.CarefulStrike,0.4f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.CrowdBrawler,0.1f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.Isolationist,0.3f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrongMan,0.2f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.StrongMan,0.1f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.StrongMan,0.1f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.StrongMan,0.3f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.StrongMan,0.3f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.WeaponMaster,0.2f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.WeaponMaster,0.3f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.WeaponMaster,0.3f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.WeaponMaster,0.2f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.WeaponMaster,0.2f),

			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.HeavyLifter,0.3f),
			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.PowerBlow,0.2f),

			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Deadeye,0.5f),
			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Conservationist,0.5f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.ToughGuy1,0.5f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.ToughGuy2,0.25f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy2,Ability.ToughGuy3,0.1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.FastHealer,0.3f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Armourer,0.2f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.Dodger,0.5f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.HeavyLifter,0.2f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.AllOutDefense,0.3f),
			new ClassAbilityTransitionPossibility(Ability.Dodger,Ability.ExchangePlaces,0.5f),

		}
	);

	public static readonly ClassAbilityGrid SwashbucklerAbilities = new ClassAbilityGrid (//For testing purposes only
		new List<ClassAbilityTransitionPossibility> ()
		{
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.GreaterMysticBlast,0.25f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.EfficientMysticBlast,0.2f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ArcingMysticBlast,0.3f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.ManaCycling,0.15f),
			new ClassAbilityTransitionPossibility(Ability.MysticBlast,Ability.Explosion,0.1f),
			new ClassAbilityTransitionPossibility(Ability.ArcingMysticBlast,Ability.PenetratingMysticBlast,0.1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.EfficientMysticBlast,0.1f),
			new ClassAbilityTransitionPossibility(Ability.GreaterMysticBlast,Ability.ArcingMysticBlast,0.1f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Heal, 0.25f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GreaterHealing, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.GroupHeal, 0.1f),
			//new ClassAbilityTransitionPossibility(Ability.Heal,Ability.ManaCycling, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.Heal,Ability.EfficientHealer, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.GreaterHealing,Ability.FullRestore, 0f),
			new ClassAbilityTransitionPossibility(Ability.FullRestore,Ability.EfficientHealer, 0.3f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.MagicalReserves1, 0.25f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.MagicalReserves2, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.MagicalReserves3, 0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves1,Ability.ManaCycling, 0.15f),
			new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterMysticBlast,0.05f),
			//new ClassAbilityTransitionPossibility(Ability.ManaCycling,Ability.GreaterHealing,0.05f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterMysticBlast,0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves2,Ability.GreaterHealing,0.05f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.GreaterMysticBlast,0.5f),
			new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.FullRestore,0.5f),
			//new ClassAbilityTransitionPossibility(Ability.MagicalReserves3,Ability.Explosion,0.5f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.SwordMastery, 0.3f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.SpearMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.BowMastery, 0.5f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.AxeMastery, 0.2f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.MaceMastery, 0.2f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrenthReserves,0.2f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.FormationFighter,0.2f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.CarefulStrike,0.2f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.CrowdBrawler,0.6f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.Isolationist,0.6f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.StrongMan,0.3f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.StrongMan,0.2f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.StrongMan,0.2f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.StrongMan,0.4f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.StrongMan,0.4f),

			new ClassAbilityTransitionPossibility(Ability.SwordMastery,Ability.WeaponMaster,0.5f),
			new ClassAbilityTransitionPossibility(Ability.SpearMastery,Ability.WeaponMaster,0.6f),
			new ClassAbilityTransitionPossibility(Ability.BowMastery,Ability.WeaponMaster,0.6f),
			new ClassAbilityTransitionPossibility(Ability.AxeMastery,Ability.WeaponMaster,0.4f),
			new ClassAbilityTransitionPossibility(Ability.MaceMastery,Ability.WeaponMaster,0.4f),

			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.HeavyLifter,0.15f),
			new ClassAbilityTransitionPossibility(Ability.StrongMan,Ability.PowerBlow,0.15f),

			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Deadeye,0.35f),
			new ClassAbilityTransitionPossibility(Ability.WeaponMaster,Ability.Conservationist,0.25f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.ToughGuy1,0.5f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.ToughGuy2,0.25f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy2,Ability.ToughGuy3,0.1f),
			new ClassAbilityTransitionPossibility(Ability.ToughGuy1,Ability.FastHealer,0.3f),

			new ClassAbilityTransitionPossibility(Ability.None,Ability.Armourer,0.2f),
			new ClassAbilityTransitionPossibility(Ability.None,Ability.Dodger,1f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.HeavyLifter,0.2f),
			new ClassAbilityTransitionPossibility(Ability.Armourer,Ability.AllOutDefense,0.4f),
			new ClassAbilityTransitionPossibility(Ability.Dodger,Ability.ExchangePlaces,1f),

		}
	);

	public static readonly CharacterClass Knight = new CharacterClass (80, 0, 0, 10, 25, 125, 5, 75, 3, 5, "Knight",
		"A strong and powerful melee fighter. The knight's defensive " +
		"capabilities are greater than his/her offensive, though both are" +
		"quite strong. However, the knight has low mobility and magical" +
		"capabilities.", KnightAbilities, new List<Ability>{Ability.None},4);
	public static readonly CharacterClass Mage = new CharacterClass (70, 0, 0, 0, 10, 100, 5, 150, 10, 5, "Mage",
		"While not the strongest physically (though don't discount the utility of a mage with a strong weapon), " +
		"the mage specializes in using powerful abilities that consume energy (which the mage has the most of any " +
		"class of). While less frequently usable than weaponry, these abilities can quicly turn the tide.", MageAbilities, new List<Ability>{Ability.None,Ability.MysticBlast},3);
	public static readonly CharacterClass Swashbuckler = new CharacterClass (90, 30, 0, 0, 20, 80, 5, 100, 5, 6, "Swashbuckler",
		"The swashbuckler is a highly mobile class, capable of " +
		"moving around the battlefield with ease, attacking with " +
		"great accuracy and dodging opponent's attacks. The " +
		"swashbuckler also hits nearly as hard as the knight and " +
		"has better magical capabilities. However, the swashbuckler " +
		"is very weak defensively, unable to take much of a beating.", SwashbucklerAbilities, new List<Ability>{Ability.None},3);
	public static readonly Dictionary<string,CharacterClass> classList = new Dictionary<string,CharacterClass> ()
	{
		{Knight.name, Knight}, {Mage.name, Mage}, {Swashbuckler.name, Swashbuckler}
	};
			
	private CharacterClass(int attack, int dodge, int minDefense, int maxDefense, int damage,
		int maxHP, int HPregen, int maxEnergy, int energyRegen, int move, string className, string classDescription,
		ClassAbilityGrid classGrid, List<Ability> classStartingAbilities, int classInventoryCapacity)
	{
		baseAttack = attack;
		baseDodge = dodge;
		baseMinDefense = minDefense;
		baseMaxDefense = maxDefense;
		baseDamage = damage;
		baseMaxHP = maxHP;
		baseHPRegen = HPregen;
		baseMaxEnergy = maxEnergy;
		baseEnergyRegen = energyRegen;
		baseMoveSpeed = move;
		name = className;
		description = classDescription;
		grid = classGrid;
		startingAbilities = classStartingAbilities;
		inventoryCapacity = classInventoryCapacity;
	}
}
