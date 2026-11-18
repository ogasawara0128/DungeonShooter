//Scripts作成順
//① PlayerMove
//② LightController
//③ PlayerAttack

//④ ItemKeeper(hasArrows=100にしただけ
//⑤ CameraController-(Cinemachineで代替可能
//⑥ EnemyController
//⑦ UIController

//⑧ ItemData---(アイテム種類、項目
//⑨ ItemKeeper-(アイテム取得時の処理
//⑩ ItemBox----(宝箱開けたときの処理

//⑪ Exit-------(ワープ識別
//⑫ RoomManager(プレイヤーを適切な座標に配置
//⑬ 

using UnityEngine;
using UnityEngine.InputSystem;

// 方向
public enum Direction
{
    down = 0,    // 下
    left = 1,    // 左
    up = 2,    // 上
    right = 3,    // 右
}

public class PlayerMove : MonoBehaviour
{
    [Header("スピード・方向")]
    [SerializeField] private float _speed = 3.0f;      // 移動スピード
    [SerializeField] private Direction _direction = 0; // 移動方向

    private Vector2 _moveVec;        // 移動ベクトル
    private float _angleZ = -90.0f;  // 回転角度

    private Rigidbody2D _rbody;      // 指定したRigidbody2Dコンポ―ネントを参照できる変数
    private Animator _animator;      // 指定したAnimatorコンポ―ネントを参照できる変数

    public static float life = 1.0f; // プレイヤーのライフ
    private bool _inDamage = false;  // ダメージ中フラグ

    private UIController _uiCnt;

    // Moveアクションを受け取る
    //🔥【説明】スティックの傾き具合を取得したい
    //           InputSystemにおける「Move」に該当する入力があれば、
    //            引数valueに返った値をVector2型に変換して_moveVecに代入する、という内容
    //🔥【ルール】InputValue:InputSystem専用の型名          
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    void OnMove(InputValue value)
    {
        //🔥【ルール】Get<Vector2>():引数に返った入力の値をVector2データに変換するメソッド
        //            ここでは「Move」に該当する入力を変換して_moveVecに代入してる
        //            PlayerInputのBehavior(Inspectorの項目)がSendMessageになってる時使える
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        _moveVec = value.Get<Vector2>();
    }

    // Z軸の角度をfloatで返す
    public float GetAngle()
    {
        float angle = _angleZ;       // とりあえず前フレームの角度を代入
        if (_moveVec != Vector2.zero) //移動ベクトルがゼロでなければ書き替え
        {
            float rad = Mathf.Atan2(_moveVec.y, _moveVec.x);  // Atan2で現フレームの向きのZ軸の角度(ラジアン)を求める
            angle = rad * Mathf.Rad2Deg;                    // Z軸のラジアンをデグリー値（度）に変換
        }
        return angle;
    }

    // Z軸の角度からDirection値を算出して返す
    Direction AngleToDirection()
    {
        Direction dir;
        if (_angleZ >= -45 && _angleZ < 45) dir = Direction.right;  // 右向き
        else if (_angleZ >= 45 && _angleZ <= 135) dir = Direction.up;     // 上向き
        else if (_angleZ >= -135 && _angleZ <= -45) dir = Direction.down;   // 下向き
        else dir = Direction.left;   // 左向き
        return dir;
    }

    private void Awake()
    {
        if (life <= 0)
        {
            life = 1.0f;    // ライフが0の場合1に戻す
        }
    }

    void Start()
    {
        _rbody = GetComponent<Rigidbody2D>();    // PalyerのRigidbody2Dを変数rbodyで参照
        _animator = GetComponent<Animator>();    // PlayerのAnimatorを変数animatorで参照

        _uiCnt = GameObject.FindFirstObjectByType<UIController>();   // シーン中のUIController取得
    }

    void Update()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage)
        {
            if (_inDamage)
            {
                // ダメージ中点滅させる
                //🔥【ルール】Mathfクラス→引数(ラジアン)の角度を正と負で表すルールを持つ
                //🔥【説明】  Time.time:Timeクラスのtimeプロパティが持ってる。
                //             「必ず一定スピードで経過する」という特性を利用して角度として使ってる
                //             50かけることでリズム早くしてる(50×経過時間)             
                float val = Mathf.Sin(Time.time * 50);
                if (val > 0)
                {
                    GetComponent<SpriteRenderer>().enabled = true;// スプライト表示
                }
                else
                {
                    GetComponent<SpriteRenderer>().enabled = false;// スプライト非表示
                }
            }
            return;
        }

        _angleZ = GetAngle();  // 現フレームの角度を取得
        Direction dir = AngleToDirection(); // 現フレームのDirection値を取得

        //🔥【説明】前フレームと向きが異なっていたら
        if (dir != _direction)　// 前フレームと向いている方向が違う
        {
            //🔥【説明】次フレームの為に_directionを新しくしておく
            _direction = dir;      // 向いている方向を更新

            //🔥【ルール】変数.Set+パラメーター種類(パラメーター名,その値)
            //             Integer:Animatorでのint型のこと
            //🔥【説明】  enumの値(0,1,2,3)をint型として扱わせる為に変換
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            _animator.SetInteger("Direction", (int)_direction);　//アニメーション更新（intに型変換)
        }
    }

    void FixedUpdate()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage)
        {
            return;
        }

        //🔥【説明】_moveVec:OnMove()でVector2と定義済
        //🔥【ルール】InputSystemではVectorによる方向入力を-1～1の値に変換して返す
        //🔥【説明】ので、そのままじゃ弱いかもなのでspeedかけてる
        _rbody.linearVelocity = _moveVec * _speed; //velocity値を決定
    }

    //🔥【ルール】引数collisionには衝突に関する情報が入っている
    //      例1:collision.relativeVelocity ──► どれくらいの凄まじいスピード（速度）でぶつかったか
    //      例2:collision.contacts         ──► 相手の体の「どの位置（座標）」に当たったか
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Enemyタグとぶつかったら
        //🔥ルール】collision.gameObject:衝突した相手
        if (collision.gameObject.tag == "Enemy")
        {
            //ダメージを負う
            //衝突以外でのダメージに使いまわせるためメソッドに分ける
            GetDamage(collision.gameObject);
        }
    }

    // ダメージ処理
    // 🔥【ルール】引数に相手オブジェクトを受け取ることで、
    //              敵の種類に応じたダメージ計算や、ヒットバックの方向を算出できる。
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    void GetDamage(GameObject target)
    {
        if (GameManager.gameState == GameState.InGame)
        {
            //SoundManager.soundManager.SEPlay(SEType.GetDamage);//SEを鳴らす

            life -= 0.25f; //ライフを減らす

            //🔥【説明】ダメージ受けたらUIControllerでHPスライドの位置を更新
            //ライフを更新
            _uiCnt.UpdateLife(life);

            if (life > 0) //もしライフが残っていれば
            {
                _rbody.linearVelocity = new Vector2(0, 0);   // 移動停止

                // ぶつかった相手の反対方向にヒットバックさせる
                //方向を取得して1に整える（正規化）
                //🔥【説明】正規化した上で迫力出すために4倍してる
                Vector3 v = (transform.position - target.transform.position).normalized;
                _rbody.AddForce(new Vector2(v.x * 4, v.y * 4), ForceMode2D.Impulse); //正規化されているので4倍 
                _inDamage = true;            // ダメージフラグ ON
                Invoke("DamageEnd", 0.25f);
            }
            else
            {
                GameOver();　//体力がなければゲームオーバー
            }
        }
    }
    // ダメージ終了
    void DamageEnd()
    {
        _inDamage = false; // ダメージフラグ OFF
        GetComponent<SpriteRenderer>().enabled = true; // スプライトを元に戻す
    }

    // ゲームオーバー
    void GameOver()
    {
        GameManager.gameState = GameState.GameOver; //ゲームステータスをGameOverに変更

        // ゲームオーバー演出
        GetComponent<CircleCollider2D>().enabled = false;       // 当たり判定を消す
        _rbody.linearVelocity = new Vector2(0, 0);               // 移動停止
        _rbody.gravityScale = 1;                                 // 重力を戻す
        _rbody.AddForce(new Vector2(0, 5), ForceMode2D.Impulse); // プレイヤーを上に少し跳ね上げる

        _animator.SetTrigger("IsDead");                          // アニメーションを切り替える

        // Playerマップの無効化とUIマップの有効化
        PlayerInput input = GetComponent<PlayerInput>();        // PlayerInput取得
        input.currentActionMap.Disable();                       // Player無効化
        input.SwitchCurrentActionMap("UI");                     // アクションマップをUIに変更
        input.currentActionMap.Enable();                        // UIマップ有効化

        Invoke("PlayerGravityEnd", 2.0f); //アニメーション演出が終わるころ落下は停止

        //SoundManager.soundManager.StopBgm();//BGM停止
        //SoundManager.soundManager.SEPlay(SEType.GameOver);//ゲームオーバーSEを鳴らす
    }

    void PlayerGravityEnd()
    {
        _rbody.gravityScale = 0; // 重力をなくして静止させる
    }
}