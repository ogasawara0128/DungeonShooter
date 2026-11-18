//🔥【説明】Inspectorの話～Floorの親GridのTransform.zが2なのでExitが現れてる。0にすると消える

using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [Header("移動先となるシーン名")]
    [SerializeField] private string _sceneName;

    [Header("出入り口の識別番号・方向")]
    public int doorNumber;
    //🔥【説明】出口ごとに出口のどっち側にプレイヤー出すかあらかじめInspectorで決めとく
    //          PlayerMoveのenumを使いまわす
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    public Direction direction;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //static変数に自分の識別番号を授ける
            //🔥【説明】シーンまたいでも覚えておけるようstatic変数に委ねる
            RoomManager.currentDoorNumber = doorNumber;

            //🔥【説明】Invokeで食い込む感じを表現
            //          秒数長くするとプレイヤーが通過してしまう危険性あり
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            Invoke("Load", 0.2f); //0.2秒後に目的に切替
        }
    }

    //シーンを切り替えるメソッド
    private void Load()
    {
        SceneManager.LoadScene(_sceneName);
    }
}
