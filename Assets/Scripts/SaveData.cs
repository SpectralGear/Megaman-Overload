using System;
[Serializable]
public class SaveData
{
    public Buster.Weapon[] ObtainedWeapons = new Buster.Weapon[1]{Buster.Weapon.MegaBuster};
    public int CompletedCastleStages = 0;
    public CharControl.Character currentCharacter = CharControl.Character.Megaman;
    public CharControl.upgrades[] ObtainedUpgrades = new CharControl.upgrades[12];
    public bool[] EquippedUpgrades = new bool[12];
    public int eTankStorage = 0;
    public int bolts = 0;
}
