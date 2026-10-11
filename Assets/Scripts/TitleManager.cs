using UnityEngine;


/// <summary>
/// チーム選択やシーンの移動まで、タイトルにかかわるすべてのことを操作するマネージャークラス。SneceChangerは消す予定
/// </summary>
public class TitleManager : MonoBehaviour
{

    [SerializeField] GameObject TitlePanel;
    [SerializeField] GameObject SelectPanel;



    public void SelectAkazukinTeam()
    {
        GameData.PlayerSelectedTeam = Team.Akazukin;
    }
    public void SelectMomotaroTeam()
    {
        GameData.PlayerSelectedTeam = Team.Momotaro;
    }

    public void StartSelectPanel()
    {
        TitlePanel.SetActive(false);
        SelectPanel.SetActive(true);
    }

}
