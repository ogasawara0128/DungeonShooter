using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    //シーンをまたぐ時、何番出口だったのかを記憶していくstatic変数
    //🔥【説明】Exit.csから引き継ぐ為のstatic変数
    public static int currentDoorNumber = 0;

    void Start()
    {
        //シーン名取得
        string sceneName = SceneManager.GetActiveScene().name;

        //取得した名前に応じてBGM切替
        //if (sceneName == "BossStage")
        //{
        //    SoundManager.soundManager.PlayBgm(BGMType.InBoss);
        //}
        //else
        //{
        //    SoundManager.soundManager.PlayBgm(BGMType.InGame);
        //}

        if (SaveDataManager.isStartFromSavePoint) //★★
        {
            //合図は1回きりなのですぐに戻す（戻さないと次のシーン移動でもセーブ位置に出てしまう）
            SaveDataManager.isStartFromSavePoint = false;

            //シーン上にいるプレイヤーを探してくる
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                //プレイヤーを加工した座標に配置
                player.transform.position = SaveDataManager.GetSavePointPosition();
            }
        }
        else
        {
            //全Exitタグを調べて該当したGameObjectを全て配列に格納
            //🔥【説明】Exitタグのオブジェクトたち(※sが付く)を見つけてexitsに代入
            GameObject[] exits = GameObject.FindGameObjectsWithTag("Exit");

            //配列の中を全点検
            //🔥【説明】forでも表現可能(教科書P.312)
            //          Exitタグのオブジェクトたち(exits)の中から1つずつ選んで(exitObj)点検していく
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            foreach (GameObject exitObj in exits)
            {
                //🔥【説明】exitObjが持ってる、Exitコンポーネント(スクリプト)に書かれてる、変数doorNumberが
                //           currentDoorNumberと一致すれば･･･
                //🔥【ルール】オブジェクト自体をFindしてもコンポーネントのGetは改めて必要
                //━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                if (currentDoorNumber == exitObj.GetComponent<Exit>().doorNumber)
                {
                    //🔥【説明】ワープした後、出口の上下左右1歩進んだ場所にプレイヤー出したい
                    //番号が一致したExitオブジェクトのX座標とY座標
                    float x = exitObj.transform.position.x;
                    float y = exitObj.transform.position.y;

                    //プレイヤーがいるべき座標として加工する
                    //🔥【説明】Exit.csのInspectorで選んだ方向によって▲のx,yの値を変える処理
                    //           Directionから選んだ1方向を入れる箱(direction)の、中身の実体(Direction)に対して処理をかける
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    switch (exitObj.GetComponent<Exit>().direction)
                    {
                        //🔥【ルール】{}内はチェック対象を記述
                        //             case〇〇:
                        //             やりたい処理;
                        //             break;←1個終わるごとに書く
                        //                     (break書かないと処理してもそのまま下の処理に続いてしまう) 
                        case Direction.up:
                            y += 1;
                            break;
                        case Direction.down:
                            y -= 1;
                            break;
                        case Direction.left:
                            x -= 1;
                            break;
                        //case Direction.right:
                        default:
                            x += 1;
                            break;
                    }

                    //シーン上にいるプレイヤーを探してくる
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        //プレイヤーを加工した座標に配置
                        player.transform.position = new Vector2(x, y);
                    }

                    //🔥【説明】見つかったら抜ける、見つからないなら見つかるまで続ける
                    //          いつまでも探すことなく探せたらforeachを抜けるためのbreak
                    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    break; //該当するExitが見つかったのでforeachを抜ける 
                }
            }
        }
    }
}