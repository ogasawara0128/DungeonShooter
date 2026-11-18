using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("体力・スピード・索敵範囲")]
    [SerializeField] private float _hp = 5.0f;
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private float _reactionDistance = 4.0f;
    [SerializeField] private GameObject _enemyBody;

    private float _axisH;
    private float _axisV;
    private Rigidbody2D _rbody;
    private Animator _animator;
    private bool _isActive = false;

    private GameObject _player;
    private bool _inDamage = false; // ダメージ中フラグ

    private string _id; //セーブ用ID

    void Start()
    {
        _rbody = GetComponent<Rigidbody2D>();
        _animator = _enemyBody.GetComponent<Animator>();

        _player = GameObject.FindGameObjectWithTag("Player");

        _id = SaveDataManager.MakeId(gameObject); //★★
        if (SaveDataManager.IsUsed(_id))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage)
        {
            if (_inDamage)
            {
                // ダメージ中点滅させる
                float val = Mathf.Sin(Time.time * 50);
                if (val > 0)
                {
                    _enemyBody.GetComponent<SpriteRenderer>().enabled = true;// スプライト表示
                }
                else
                {
                    _enemyBody.GetComponent<SpriteRenderer>().enabled = false;// スプライト非表示
                }
            }
            return;
        }

        if (_player != null)
        {
            //プレイヤーがあらかじめ決めた範囲より近ければ自分(敵)が動く
            float dist = Vector2.Distance(transform.position, _player.transform.position);
            if (dist < _reactionDistance)
            {
                _isActive = true;
            }
            else
            {
                _isActive = false;
            }

            //🔥【説明】Animator上でBool型の"IsActive"がtrueかfalseか(_isActive)
            //trueなら動くアニメ、falseなら寝るアニメになる
            _animator.SetBool("IsActive", _isActive);

            if (_isActive)
            {
                float dx = _player.transform.position.x - transform.position.x;
                float dy = _player.transform.position.y - transform.position.y;

                //🔥【説明】ラジアン使わず言い換え可能
                //Vector2 dir = new Vector2(dx, dy).normalized;
                //float angle = Vector2.SignedAngle(Vector2.right, dir);
                //           引数は(基準の向き,測りたい向き)
                //           Vector2.rightで基準は真右(ノーマルの基準)になる
                float rad = Mathf.Atan2(dy, dx);
                float angle = rad * Mathf.Rad2Deg;

                //🔥【説明】PlayerMove.csのenumを流用
                Direction direction;
                if (angle > -45.0f && angle <= 45.0f)
                {
                    direction = Direction.right;
                }
                else if (angle > 45.0f && angle <= 135.0f)
                {
                    direction = Direction.up;
                }
                else if (angle >= -135.0f && angle <= -45.0f)
                {
                    direction = Direction.down;
                }
                else
                {
                    direction = Direction.left;
                }

                _animator.SetInteger("Direction", (int)direction);

                //🔥【説明】距離によって移動スピードを変えないため正規化してる
                //           (移動スピードはFixedUpdate内で記述▼)  
                //▲でラジアン使わず言い換えた場合、こちらも言い換え可能
                //_axisH = dir.x;
                //_axisV = dir.y;
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                _axisH = Mathf.Cos(rad);
                _axisV = Mathf.Sin(rad);
            }
            else
            {
                _axisH = 0;
                _axisV = 0;
            }
        }
        else
        {
            _isActive = false;
        }
    }

    void FixedUpdate()
    {
        // ゲーム中以外とダメージ中は何もしない
        if (GameManager.gameState != GameState.InGame || _inDamage)
        {
            return;
        }

        if (_hp > 0)
        {
            //🔥【説明】正規化だけでは遅すぎるのでspeedかけてる
            _rbody.linearVelocity = new Vector2(_axisH, _axisV) * _speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Arrow")
        {
            GetDamage(collision.gameObject, "Arrow");
        }
        if (collision.gameObject.tag == "Sword")
        {
            GetDamage(collision.gameObject, "Sword");
        }
    }

    // ダメージ処理
    //🔥【説明】引数(①相手GameObjectそのもののデータ,②攻撃の種類(文字列))
    //①攻撃力(ダメージ量)、方向(吹っ飛ばされる方向)の取得
    //②文字列で受け取り、ifの条件分岐に使う
    void GetDamage(GameObject target, string attackType)
    {
        if (_inDamage) return; //すでにダメージ中なら重ねて処理はしない

        if (attackType == "Arrow")
        {
            ArrowController arrow = target.gameObject.GetComponent<ArrowController>();
            _hp -= arrow.attackPower;
        }
        else if (attackType == "Sword")
        {
            _hp -= 1;
        }

        //🔥【説明】PlayerMoveと同様の為、そちらを参照
        if (_hp > 0) //体力が残ればダメージ演出
        {
            _rbody.linearVelocity = new Vector2(0, 0);   // 移動停止
            // 攻撃の反対方向にヒットバックさせる
            Vector3 v = (transform.position - target.transform.position).normalized;
            _rbody.AddForce(new Vector2(v.x * 4, v.y * 4), ForceMode2D.Impulse);

            _inDamage = true;            // ダメージフラグ ON
            Invoke("DamageEnd", 0.25f);
        }
        else //死亡時
        {
            _isActive = false;
            GetComponent<CircleCollider2D>().enabled = false;
            _rbody.linearVelocity = new Vector2(0, 0);
            _rbody.gravityScale = 1;
            _rbody.AddForce(new Vector2(0, 5), ForceMode2D.Impulse);
            _animator.SetTrigger("IsDead");
            Destroy(gameObject, 1.0f);

            SaveDataManager.SetUsed(_id); //撃破したのでリストに刻まれる

            SoundManager.soundManager.SEPlay(SEType.EnemyKilled);//SEを鳴らす
        }
    }

    // ダメージ終了
    void DamageEnd()
    {
        _inDamage = false; // ダメージフラグ OFF
        _enemyBody.GetComponent<SpriteRenderer>().enabled = true; // スプライトを元に戻す
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _reactionDistance);
    }
}