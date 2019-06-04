using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaManager : MonoBehaviour
{
    private string _charaPath = "CharSout3"; //TEMP
    //Ref
    [SerializeField] private QuirkTable _quirkTable;

    //List All chara (added when they spawn)
    private List<GameObject> _allCharas;

    //Awake
    private void Awake()
    {
        _allCharas = new List<GameObject>();
    }

    //Spawn a Chara
    public void SpawnChara(Vector3 pos, string teamName, string playerName)
    {
        int[] quirks = GetNewSerializedQuirks();
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_SpawnChara", PhotonTargets.AllBuffered, pos.x, pos.y, pos.z, teamName, quirks, playerName);
        }
        else
        {
            RPC_SpawnChara(pos.x, pos.y, pos.z, teamName, quirks, playerName);
        }
    }
    [PunRPC] public GameObject RPC_SpawnChara(float x, float y, float z, string teamName, int[] quirks, string playerName)
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

        if(playerName != "")
        {
            PermissionsManager.Instance.spirit.InstantiateCharaRef(playerName, chara);
        }

        return chara;
    }

    //Init static
    public int[] GetNewSerializedQuirks()
    {
        List<Quirk> quirks = new List<Quirk>();
        //Decide quirk number
        int numberPhysical = Random.Range(1, 4); //1 à 3
        int numberMental = Random.Range(2, 5);  //2 à 4
        int numberJob = Random.Range(0, 2);     //0 à 1
        int numberApocExp = Random.Range(0, 2); //0 à 1
        //add Quirk
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.Physical, numberPhysical, quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.Mental, numberMental, quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.OldJob, numberJob, quirks);
        _quirkTable.GetRandomQuirksOfType(Quirk.QuirkType.ApocalypseExp, numberApocExp, quirks);
        //Serialize
        int[] serialized = new int[quirks.Count];
        for (int i = 0; i < quirks.Count; i++)
        {
            serialized[i] = _quirkTable.QuirkToId(quirks[i]);
        }
        return serialized;
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
        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_ReceiveMsg", PhotonTargets.AllBuffered, GetIdWithChara(chara), cc, intArgs, stringArgs, floatArgs);
        }
        else
        {
            RPC_ReceiveMsg(GetIdWithChara(chara), cc, intArgs, stringArgs, floatArgs);
        }
        
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
