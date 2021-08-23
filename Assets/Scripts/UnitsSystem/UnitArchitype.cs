using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitArchitype", menuName = "Unit Architype")]
public class UnitArchitype : ScriptableObject
{
    //Unit's specs
    [SerializeField]
    int speed = 3;
    [SerializeField]
    int health = 3;

    //Unit's specs
    [SerializeField]
    AbilityType[] setOfAbilities;

    [SerializeField]
    SubUnitMovement[] subunitsPrefabs;
    [SerializeField]
    Banner bannerPrefab;
    [SerializeField]
    BattleFormation battleFormation = BattleFormation.Single;

    public int Speed { get => speed; }
    public int Health { get => health; }
    public AbilityType[] SetOfAbbilities { get => (AbilityType[])setOfAbilities.Clone(); }
    public SubUnitMovement[] SubunitsPrefabs { get => (SubUnitMovement[])subunitsPrefabs.Clone(); }
    public Banner BannerPrefab { get => bannerPrefab; }
    public BattleFormation BattleFormation { get => battleFormation; }

}
