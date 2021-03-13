using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attributes : MonoBehaviour
{
    [Header("Identifiers")]

    public bool forUnit;
    public int unitIndex = -1;
    public bool forBuilding;
    public int unitType;
    public int attackType;

    [Header("Research Costs")]

    public float foodCost;
    public float woodCost;
    public float goldCost;
    public float stoneCost;
    public float ironCost;

    [Header("Attributes To Modify")]

    public float healthModifier;
    public float damageModifier;
    public float gatheringCapacityModifier, gatheringRateModifier, buildingStrengthModifier;
    public float speedModifier;
    public float resourceAmountModifier;
    public float standardFortificationModifier, siegeFortificationModifier;

    [Header("For Resource Type")]

    public NodeManager.ResourceTypes resourceToTarget;

    [Header("Research Identifier:")]

    public int researchIndex;
}
