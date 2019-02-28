using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaOutline : MonoBehaviour
{
    //Recuperer le permissions manager component
    private PermissionsManager permManager;

    

    [SerializeField]
    private Outline outline;
    private Outline.Mode selectedMod = Outline.Mode.OutlineAndSilhouette;
    private Outline.Mode deselectedMod = Outline.Mode.SilhouetteOnly;
    private Outline.Mode nullMod = Outline.Mode.OutlineVisible;

    private bool clientIsInOurTeam = false;

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
            if (team.ContainsPlayer(permManager.GetPlayerWithName(PhotonNetwork.player.name)))
            {
                //Cas ou le Chara a une équipe et le client est dedans
                clientIsInOurTeam = true;

                outline.OutlineMode = deselectedMod;
            }
            else
            {
                //Cas ou le Chara a une équipe mais le client n'est pas dedans
                outline.OutlineMode = nullMod;

                int[] color = team.LinkedColor;
                outline.OutlineColor = new Color32((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
            }
        }
        else
        {
            //Cas ou le Chara n'a pas d'équipe (IA)
            outline.OutlineMode = nullMod;

            outline.OutlineColor = Color.white;
        }
    }

    //Public Methods
    [PunRPC]
    public void ChangeColorTo(int[] color)
    {
        if (clientIsInOurTeam)
        {
            outline.OutlineColor = new Color32((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]);
        }
    }

    [PunRPC]
    public void SetOutlineToSelected()
    {
        if (clientIsInOurTeam)
        {
            outline.OutlineMode = selectedMod;
        }
        
    }

    [PunRPC]
    public void SetOutlineToNotSelected()
    {
        if (clientIsInOurTeam)
        {
            outline.OutlineMode = deselectedMod;
        }
    }
}
