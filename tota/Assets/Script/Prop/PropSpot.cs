using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPropSpot", menuName = "Prop/PropSpot")]
public class PropSpot : ScriptableObject
{
    [Header("Characterizes the placement of a plot on a propMarker")]
    //PropSpot characterizes a way of placing a plot on a propMarker

    //List of every possible Props
    [Tooltip("List of every possible Props")]
    public Prop[] possibleProps = null;
    //Liste des chances sur 100 de chaque prop de spawn
    //NB: si la somme des chances est inférieur à 100, il est possible que rien ne spawn
    [Tooltip("Liste des chances sur 100 de chaque prop de spawn")]
    public int[] randomChances = null;
    //Indique si le prop doit s'aligner avec le marker ou à une rotation libre
    [Tooltip("Indique si le prop doit s'aligner avec le marker ou à une rotation libre")]
    public bool alignedWithMarker = false;

    //ex:
    //Soit possibleProps.Length = randomChances.Length = 1
    //Soit possibleProps[0] est un prop quelconque, propX
    //Soit randomChances[0] = 50
    //Au moment de décider du prop à spawn au niveau d'un PropMarker possédant ce PropSpot
    //il y aura une chance sur deux de faire spawner propX, et une chance sur deux de ne rien faire spawner

    //Choisis le Prop a spawn (NB on pourrait faire passer des trucs en parametres pour rendre le choix plus interressant)
    public Prop GetChosenProp()
    {
        //Jet d'un D100 (dé à 100 faces)
        int rng = Random.Range(0, 100);
        //Choix du prop
        for (int i = 0; i < randomChances.Length; i++)
        {
            rng = rng - randomChances[i];

            if (rng < 0)
            {
                //Un Prop a été choisi
                return possibleProps[i];
            }
        }
        //choix: ne rien faire apparaître
        return null;
    }
}
