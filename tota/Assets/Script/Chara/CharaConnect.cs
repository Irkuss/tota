using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaConnect : MonoBehaviour
{
    private CharaManager manager;

    public enum CharaCommand
    {
        RPC_SetStoppingDistance,
        RPC_SetDestination,
        UpdateTeamColor,
        ChangeColorTo,
        SetOutlineToSelected,
        SetOutlineToNotSelected,
        RPC_SetTeam,
        SetOwner,
        SetOwnerNull,
        ModifyCountWithId,
        ReceiveAddWound,
        TrainStat
    }
    // Start is called before the first frame update
    void Awake()
    {
        manager = GameObject.Find("eCentralManager").GetComponent<CharaManager>();
    }
    public void SendMsg(CharaCommand cc, int[] intArgs = null, string[] stringArgs = null, float[] floatArgs = null)
    {
        manager.SendMsgTo(this.gameObject, (int)cc, intArgs, stringArgs, floatArgs);
    }
    public void ReceiveMsg(int cc, int[] i, string[] s, float[] f)
    {
        switch((CharaCommand) cc)
        {
            case CharaCommand.RPC_SetStoppingDistance:  GetComponent<CharaMovement>().RPC_SetStoppingDistance(f[0]); break;
            case CharaCommand.RPC_SetDestination: GetComponent<CharaMovement>().RPC_SetDestination(i[0], f[0],f[1],f[2]); break;
            case CharaCommand.UpdateTeamColor: GetComponent<CharaOutline>().UpdateTeamColor(); break;
            case CharaCommand.ChangeColorTo: GetComponent<CharaOutline>().ChangeColorTo(i); break;
            case CharaCommand.SetOutlineToSelected: GetComponent<CharaOutline>().SetOutlineToSelected(); break;
            case CharaCommand.SetOutlineToNotSelected: GetComponent<CharaOutline>().SetOutlineToNotSelected(); break;
            case CharaCommand.RPC_SetTeam: GetComponent<CharaPermissions>().RPC_SetTeam(s[0]); break;
            case CharaCommand.SetOwner: GetComponent<CharaPermissions>().SetOwner(s[0]); break;
            case CharaCommand.SetOwnerNull: GetComponent<CharaPermissions>().SetOwnerNull(); break;
            case CharaCommand.ModifyCountWithId: GetComponent<CharaInventory>().ModifyCountWithId(i[0],i[1]); break;
            case CharaCommand.ReceiveAddWound: GetComponent<CharaRpg>().ReceiveAddWound(i[0], i[1], s[0], s[1], f[0]); break;
            case CharaCommand.TrainStat: GetComponent<CharaRpg>().RPC_TrainStat((CharaRpg.Stat)i[0], f[0]); break;
        }
    }
}
