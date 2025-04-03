using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SaveData
{
    public bool[] ObtainedWeapons = new bool[10];
    public int CompletedCastleStages;
    public bool[] ObtainedUpgrades = new bool[12];
    public int eTankStorage;
    public int bolts;
}
