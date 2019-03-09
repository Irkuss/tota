using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaOutline : MonoBehaviour
{
    //Description de ce que doit fait Outline:
    //Soit une team A et une team B,
    //-Un joueur de la team A voit les Charas de la team B avec une outLine d'une même couleur (celle associé à la team B).
    //-Il voit les Charas de sa team sans outline sauf s'ils sont selectionnés 
    // alors il verra une outline de la couleur associée au joueur qui a selectionné le Chara en question.
    //-Les Charas sans équipe (controlé par l'IA) n'ont pas d'Outline

    //Recuperer le permissions manager component
    private PermissionsManager permManager;

    //Recuperer le Outline Component attaché au Chara
    [SerializeField] private Outline outline = null;

    //Différent mode possible
    private Outline.Mode _selectedMod = Outline.Mode.OutlineAndSilhouette;
    private Outline.Mode _deselectedMod = Outline.Mode.SilhouetteOnly;
    private Outline.Mode _nullMod = Outline.Mode.OutlineVisible;

    //Variable locale pour savoir si le client est dans l'équipe qui possède ce Chara
    //Initialisé dans OnFinishedPermissions()
    private bool _clientIsInOurTeam = false;

    //Appelé dans le Start() de CharaPermissions
    //NB: on ne doit pas le mettre directement dans Start() car il est possible que CharaPermissions ne se soit pas encore chargé.
    public void OnFinishedPermissions()
    {
        //Called by Start in CharaPermissions

        permManager = GameObject.Find("eCentralManager").GetComponent<PermissionsManager>(); //pas ouf comm methode, mieux vaux avec un tag

        

        GetComponent<PhotonView>().RPC("UpdateTeamColor", PhotonTargets.AllBuffered);
    }

    //Init Team Color
    [PunRPC]
    private void UpdateTeamColor()
    {
        PermissionsManager.Team team = GetComponent<CharaPermissions>().GetTeam();

        if (team != null)
        {
            if (team.ContainsPlayer(permManager.GetPlayerWithName(PhotonNetwork.player.NickName)))
            {
                //Cas ou le Chara a une équipe et le client est dedans
                _clientIsInOurTeam = true;

                outline.OutlineMode = _deselectedMod;
            }
            else
            {
                //Cas ou le Chara a une équipe mais le client n'est pas dedans
                outline.OutlineMode = _nullMod;

                int[] color = team.LinkedColor;
                outline.OutlineColor = new Color32((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
            }
        }
        else
        {
            //Cas ou le Chara n'a pas d'équipe (IA)
            outline.OutlineMode = _nullMod;

            outline.OutlineColor = Color.white;
        }
    }

    //Public Methods
    [PunRPC]
    public void ChangeColorTo(int[] color)
    {
        if (_clientIsInOurTeam)
        {
            outline.OutlineColor = new Color32((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
        }
    }

    [PunRPC]
    public void SetOutlineToSelected()
    {
        if (_clientIsInOurTeam)
        {
            outline.OutlineMode = _selectedMod;
        }
        
    }

    [PunRPC]
    public void SetOutlineToNotSelected()
    {
        if (_clientIsInOurTeam)
        {
            outline.OutlineMode = _deselectedMod;
        }
    }
}
