using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaManager : MonoBehaviour
{
    private string _charaPath = "CharaTanguy"; //TEMP

    private List<GameObject> _allCharas;

    private void Awake()
    {
        _allCharas = new List<GameObject>();
    }

    //Spawn a Chara
    public void SpawnChara(Vector3 pos, string teamName)
    {
        GetComponent<PhotonView>().RPC("RPC_MasterSpawnChara", PhotonTargets.MasterClient, pos.x, pos.y, pos.z, teamName);
    }
    [PunRPC] private void RPC_MasterSpawnChara(float x, float y, float z, string teamName)
    {
        //Instancie sur le PhotonNetwork le Chara
        Vector3 pos = new Vector3(x, y, z);
        GameObject chara = Instantiate(Resources.Load<GameObject>(_charaPath), pos, Quaternion.identity);
        //Ajoute le chara dans la liste des charas
        _allCharas.Add(chara);
        //Initialise la team du chara (sur le network)
        chara.GetComponent<CharaPermissions>().SetTeam(teamName);
        //Initialise les stats du chara
        CharaRpg charRpg = chara.GetComponent<CharaRpg>();
        charRpg.Init();
        int[] quirks = charRpg.SerializeQuirks();
        //Finalise le chara chez les clients (rpg et liste)
        GetComponent<PhotonView>().RPC("RPC_ClientSpawnChara", PhotonTargets.OthersBuffered, x, y, z, teamName, quirks);
    }
    [PunRPC] private void RPC_ClientSpawnChara(float x, float y, float z, string teamName, int[] quirks)
    {
        //Instancie sur le PhotonNetwork le Chara
        Vector3 pos = new Vector3(x, y, z);
        GameObject chara = Instantiate(Resources.Load<GameObject>(_charaPath), pos, Quaternion.identity);
        //Ajoute le chara dans la liste des charas
        _allCharas.Add(chara);
        //Initialise la team du chara (sur le network)
        chara.GetComponent<CharaPermissions>().SetTeam(teamName);
        //Initialise les stats du chara
        chara.GetComponent<CharaRpg>().Init(quirks);

    }
    //When a chara spawns under a non masterclient
    public void AddToTeam(GameObject chara)
    {
        _allCharas.Add(chara);
    }

    //Get a Chara
    public int GetIdWithChara(GameObject chara)
    {
        for (int i = 0; i < _allCharas.Count; i++)
        {
            if (_allCharas[i] == chara)
            {
                return i;
            }
        }
        Debug.Log("GetIdWithChara: Chara was not found");
        return -1;
    }
    public GameObject GetCharaWithId(int id)
    {
        if (id < 0 || id >= _allCharas.Count) return null;

        return _allCharas[id];
    }
    //Command
    public void SendMsgTo(GameObject chara, int cc, int[] intArgs, string[] stringArgs, float[] floatArgs)
    {
        GetComponent<PhotonView>().RPC("RPC_ReceiveMsg", PhotonTargets.AllBuffered, GetIdWithChara(chara), cc, intArgs, stringArgs, floatArgs);
    }
    [PunRPC] public void RPC_ReceiveMsg(int id, int cc, int[] intArgs, string[] stringArgs, float[] floatArgs)
    {
        CharaConnect chara = GetCharaWithId(id).GetComponent<CharaConnect>();

        if(chara != null)
        {
            chara.ReceiveMsg(cc, intArgs, stringArgs, floatArgs);
        }
    }
}
