using System;
[Serializable]
public class SaveData
{
    public Buster.Weapon[] ObtainedWeapons = new Buster.Weapon[10];
    public int CompletedCastleStages;
    public CharControl.Character currentCharacter;
    public CharControl.upgrades[] ObtainedUpgrades = new CharControl.upgrades[12];
    public bool[] EquippedUpgrades = new bool[12];
    public int eTankStorage;
    public int bolts;
}
