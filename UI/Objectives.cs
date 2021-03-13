using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Objectives : MonoBehaviour
{
    public Text objectives;
    public GameObject enemyInfo;
    
    [TextArea]
    public string defaultObjective;

    public List<Subtitles> listOfObjectives;
}
