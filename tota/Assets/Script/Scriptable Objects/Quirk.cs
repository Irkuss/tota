using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuirk", menuName = "Quirk/Quirk")]
public class Quirk : ScriptableObject
{
    public enum QuirkType
    {
        Physical,
        Mental,
        ApocalypseExp,
        OldJob,
    }

    public QuirkType type = QuirkType.Physical;
    //name and description
    public string quirkName = "quirk name";
    public string quirkDescription = "quirk description here";

    //Main stats modifiers
    [Header("Main stats modifiers (Main stat: 0 -> 100 (base 0))")]
    public int strength = 0;
    public int intelligence = 0;
    public int perception = 0;
    public int mental = 0;
    public int social = 0;
    //Stats Level modifers
    [Header("Skills modifiers (Skil: -1 -> 10 (base 0))")]
    public int doctor = 0; //Unlock new operations + higher speed operations
    public int farmer = 0; //Unlock different plants to farm
    public int carpenter = 0; //higher speed carpenter + unlock some complex build
    public int scavenger = 0; //higher speed scavenging
    public int electrician = 0; //higher speed electrician + unlock some complex build
    public int marksman = 0; //higher accuracy at shooting
    //Specific modifiers
    [Header("Stats level modifiers (-2 -> 2 (base 0))")]
    public int stamina = 0; //
    //Quirk non compatible avec ce quirk
    [Header("Non compatible quirks")]
    public Quirk[] nonCompatQuirks;

    //Getter
    public CharaRpg.Stats GetStats()
    {
        return new CharaRpg.Stats(
            new int[CharaRpg.c_statNumber]
            {
                //Main stats
                strength,
                intelligence,
                perception,
                mental,
                social,
                //Skills
                doctor,
                farmer,
                carpenter,
                scavenger,
                electrician,
                marksman,
                //Specific modifiers (level)
                stamina
            }
            );
    }
    //Compatibility
    public bool IsCompatibleWith(List<Quirk> quirks)
    {
        foreach(Quirk nonCompatQuirk in nonCompatQuirks)
        {
            if(quirks.Contains(nonCompatQuirk))
            {
                return false;
            }
        }
        return true;
    }

}
