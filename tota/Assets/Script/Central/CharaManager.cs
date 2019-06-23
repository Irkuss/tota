using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaManager : MonoBehaviour
{
    private string _charaPath = "CharSout3"; //TEMP
    //Ref
    [SerializeField] private QuirkTable _quirkTable;

    [SerializeField] private TextAsset _textPrenoms = null;
    [SerializeField] private TextAsset _textNoms = null;

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

        string newFirstName = GetRandomFirstName();
        string newLastName = GetRandomLastName();

        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_SpawnChara", PhotonTargets.AllBuffered, pos.x, pos.y, pos.z, teamName, quirks, playerName, newFirstName, newLastName);
        }
        else
        {
            RPC_SpawnChara(pos.x, pos.y, pos.z, teamName, quirks, playerName, newFirstName, newLastName);
        }
    }
    [PunRPC]
    public GameObject RPC_SpawnChara(float x, float y, float z, string teamName, int[] quirks, string playerName, string newFirstName, string newLastName)
    {
        //Instancie sur le PhotonNetwork le Chara
        Vector3 pos = new Vector3(x, y, z);
        GameObject chara = Instantiate(Resources.Load<GameObject>(_charaPath), pos, Quaternion.identity);
        //Ajoute le chara dans la liste des charas
        _allCharas.Add(chara);

        //Initialise les stats du chara
        chara.GetComponent<CharaRpg>().SetIdentity(newFirstName, newLastName);
        chara.GetComponent<CharaRpg>().Init(quirks);


        if (teamName != "" && PhotonNetwork.isMasterClient)
        {
            //Initialise la team du chara (sur le network)
            chara.GetComponent<CharaPermissions>().SetTeam(teamName);
        }

        //if (playerName != "") PermissionsManager.Instance.spirit.InstantiateCharaRef(playerName, chara);

        return chara;
    }

    //Init static
    public int[] GetNewSerializedQuirks()
    {
        List<Quirk> quirks = new List<Quirk>();
        //Decide quirk number
        int numberPhysical = UnityEngine.Random.Range(1, 4); //1 à 3
        int numberMental = UnityEngine.Random.Range(2, 5);  //2 à 4
        int numberJob = UnityEngine.Random.Range(0, 2);     //0 à 1
        int numberApocExp = UnityEngine.Random.Range(0, 2); //0 à 1
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
        //Debug.Log("SendMsgTo: sending msg to");

        if (Mode.Instance.online)
        {
            GetComponent<PhotonView>().RPC("RPC_ReceiveMsg", PhotonTargets.AllBuffered, GetIdWithChara(chara), cc, intArgs, stringArgs, floatArgs);
        }
        else
        {
            RPC_ReceiveMsg(GetIdWithChara(chara), cc, intArgs, stringArgs, floatArgs);
        }

    }
    [PunRPC]
    public void RPC_ReceiveMsg(int id, int cc, int[] intArgs, string[] stringArgs, float[] floatArgs)
    {
        CharaConnect chara = GetCharaWithId(id).GetComponent<CharaConnect>();

        if (chara != null)
        {
            chara.ReceiveMsg(cc, intArgs, stringArgs, floatArgs);
        }
    }

    //Names static Generation
    public string GetRandomFirstName()
    {
        return GetRandomStringFromFile(_textPrenoms);
        //return GetRandomStringFromFile(12437, "Assets/Resources/Database/prenoms.txt");
    }
    public string GetRandomLastName()
    {
        return GetRandomStringFromFile(_textNoms);
        //return GetRandomStringFromFile(1000, "Assets/Resources/Database/noms.txt");
    }

    private string GetRandomStringFromFile(TextAsset textAsset)
    {
        Debug.Log("GetRandomStringFromFile: searching");

        string[] splitFile = new string[] { "\r\n", "\r", "\n" };

        //Getting the whole text as string
        string fileAsString = textAsset.text;

        //Getting the lines
        string[] fileLines = fileAsString.Split(splitFile, StringSplitOptions.None);

        //Choosing a line
        int chosenLineIndex = UnityEngine.Random.Range(0, fileLines.Length);

        string chosenLineString = fileLines[chosenLineIndex];

        //Cleaning shitty db ending with ' '
        if(chosenLineString[chosenLineString.Length - 1] == ' ')
        {
            chosenLineString = chosenLineString.Remove(chosenLineString.Length - 1);
        }

        return chosenLineString;
        /*
        using (StreamReader sr = new StreamReader(filePath))
        {
            int chosenLine = Random.Range(1, fileLineLength) + 1;
            for (int currentLine = 0; currentLine < chosenLine; currentLine++)
            {
                sr.ReadLine();
            }
            return sr.ReadLine();
        }*/
    }
}

