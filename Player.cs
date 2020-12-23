using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
    //params
    [SerializeField] float runSpeed = 2f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;

    //攻撃が銃から発出されるよう微調整するためのパラメータ
    [SerializeField] float xOffset;
    [SerializeField] float yOffset;
    //攻撃プリファブ、攻撃のスピード
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float fireSpeed;

    //ダメージ受けたときにふっとぶ
    Vector2 deathKick;

    //ズリバイのスピード
    public float crowlSpeed;
    //梯子を見るためのRayを飛ばす距離
    public float distance = 1f;
    //レイヤー判定用の変数
    public LayerMask ladderLayer;
    //重力初期値格納用変数
    float gravityScaleAtFirst;
    //はしごにいるかどうか
    bool OnLadder = false;
    //右向きか左向きか
    bool lookRight = false;
    bool lookLeft = false;
    //生きているかどうか
    bool isAlive = true;
    //無敵状態かどうか
    bool isInvincible = false;

    //cache
    Rigidbody2D myRigidbody;
    Animator animator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;

    
    // Start is called before the first frame update
    void Start()
    {
        
        //各種コンポーネント取得
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        //重力値の初期化
        gravityScaleAtFirst = myRigidbody.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        //生きていないならば操作できない
        if (!isAlive) { return; }

        Run();
        ClimbLadder();
        Jump();
        Crouch();
        Attack();
        AttackMotionCancel();

        //ダメージ直後の無敵状態にはダメージを受けない
        if (isInvincible) { return; }
        Damaged();
    }


    private void Run()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal");

        //移動のスピードを決める
        //ズリバイしている時はスピード遅くなる
        if (Input.GetKey(KeyCode.DownArrow))
        {
            //rigidbody.y 、つまりy方向は物理法則のなすがままにしてあげる
            //一方、xにはズリバイスピードcrowlSpeedをかけて遅くしている
            Vector2 playerVelocity
                = new Vector2(controlThrow * runSpeed * crowlSpeed, myRigidbody.velocity.y);
            myRigidbody.velocity = playerVelocity;
        }
        else //しゃがんでない時
        {
            //rigidbody.y 、つまりy方向は物理法則のなすがままにしてあげる
            Vector2 playerVelocity
                = new Vector2(controlThrow * runSpeed, myRigidbody.velocity.y);
            myRigidbody.velocity = playerVelocity;

        }
        //移動のスピードここまで

        //次にアニメーションや右向き左向きの管理
        if (controlThrow > 0)
        {
            animator.SetBool("Walk", true);
            Flip(-1);//右向き
            lookRight = true;
            lookLeft = false;
        }
        else if (controlThrow < 0)
        {
            animator.SetBool("Walk", true);
            Flip(1);//左向き
            lookRight = false;
            lookLeft = true;
        }
        else if (controlThrow <= Mathf.Epsilon)//入力がほぼ０に等しい時
        {
            animator.SetBool("Walk", false);
        }

    }

    private void ClimbLadder()
    {

        //梯子があるかどうかの判定
        //プレイヤーから上方向に、distanceで指定した距離のレイを飛ばし、Ladderレイヤーがあるかどうかを判定
        RaycastHit2D ifLadderUp = Physics2D.Raycast(transform.position, Vector2.up, distance, ladderLayer);
        RaycastHit2D ifLadderDown = Physics2D.Raycast(transform.position, Vector2.down, distance, ladderLayer);


        //上下方向にいずれにも梯子がないならば
        if (ifLadderUp.collider == null &&
            ifLadderDown.collider == null)
        {
            //登り動作のアニメーションは表示しない
            animator.SetBool("Climb", false);
            //重力は元どおりにする
            myRigidbody.gravityScale = gravityScaleAtFirst;
            //下でいう「梯子モード」解除
            OnLadder = false;
            //これ以降は処理しない
            return;
        }

        //上下方向いずれかに梯子があるならば
        if (ifLadderUp.collider != null ||
            ifLadderDown.collider != null)
        {

            //梯子があるなら縦入力を受付
            float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");

            //上下いずれかの縦入力が一度あると、梯子モードに入る
            if (controlThrow < 0 || controlThrow > 0)
            {
                OnLadder = true;
            }

            //梯子モード
            if (OnLadder)
            {
                //重力を０に
                myRigidbody.gravityScale = 0f;
                //縦入力を移動に置き換え
                Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, controlThrow * climbSpeed);
                myRigidbody.velocity = climbVelocity;
                //梯子動作アニメーションに切り替え
                animator.SetBool("Climb", true);
            }

        }
    }


    //スプライトの右向き左向きを実行
    private void Flip(int direction)
    {
        transform.localScale = new Vector2(direction, transform.localScale.y);
    }


    private void Jump()
    {
        //空中ジャンプを防止
        //Groundにいない　かつ　Ladderにいないなら　足をついていないので
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) &&
            !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ladder")))
        {
            //ジャンプアニメーションにする
            animator.SetBool("Jump", true);
            //ジャンプ中ならここで処理抜ける
            return;
        }
        else
        {
            //以外（つまりどちからのレイヤーにいるなら足をついているので）ジャンプアニメーションにしない
            animator.SetBool("Jump", false);
        }
        //ジャンプ受け付ける
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidbody.velocity += jumpVelocityToAdd;
        }


    }

    private void Crouch()
    {
        float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
        //梯子の上ではしゃがめないのでここで処理を抜ける
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Ladder"))) { return; }
        //梯子の上でなければしゃがみアニメーションにうつる
        if (controlThrow < 0)
        {
            animator.SetBool("Crouch", true);

        }
        if (controlThrow >= 0)
        {
            animator.SetBool("Crouch", false);

        }
    }

    private void Attack()
    {
        //しゃがんでる間は攻撃できない
        if (Input.GetKey(KeyCode.DownArrow)) { return; }

        bool onAttack = CrossPlatformInputManager.GetButtonDown("Fire1");

        if (onAttack)
        {
     
            animator.SetBool("Attack", true);

            if (lookRight == true)
            {
                Vector3 offset = new Vector3(xOffset, yOffset, 0);
                GameObject laser = Instantiate
                    (laserPrefab,
                    transform.position + offset,
                    Quaternion.identity) as GameObject;
                laser.GetComponent<Rigidbody2D>().velocity = new Vector2(fireSpeed, 0);
            }
            else if (lookLeft == true)
            {
                Vector3 offset = new Vector3(-xOffset, yOffset, 0);
                GameObject laser = Instantiate
                    (laserPrefab,
                    transform.position + offset,
                    Quaternion.identity) as GameObject;
                laser.GetComponent<Rigidbody2D>().velocity = new Vector2(-fireSpeed, 0);
     
              }

        }

    }

    //attackでの攻撃アニメーションをキャンセルするメソッド
    private void AttackMotionCancel()
    {
        //たて、よこ、足がついていない状態なら攻撃アニメーションをキャンセル
        float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal");
        float verticalThrow = CrossPlatformInputManager.GetAxis("Vertical");
        if (controlThrow > 0 || controlThrow < 0 || verticalThrow > 0 || verticalThrow < 0 ||
            !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))

        {
            animator.SetBool("Attack", false);
        }
        //TODO ジャンプ攻撃のモーション
    }

    //ダメージを受けた時の処理
    private void Damaged()
    {
        //体もしくは足のコライダーがEnemyにタッチしたならば
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")) ||
           myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy")))
        {
            //アニメーションをダメージ状態にして
            animator.SetTrigger("Damaged");
            //はしごの上にいる状態なら下に落っこちる
            if (OnLadder)
            {
                //登り動作のアニメーションは表示しない
                animator.SetBool("Climb", false);
                //重力は元どおりにする
                myRigidbody.gravityScale = gravityScaleAtFirst;
                OnLadder = false;
            }
            //はしごにいないならdeathkickの方向に体がふっとぶ
            else
            {
                float xVec = 2f;
                float yVec = 8f;
                //右向きなら左方向（後ろ方向）に吹っ飛ぶ
                if(lookRight)
                {
                    deathKick = new Vector2(-xVec, yVec);
                }
                //左向きなら右方向（後ろ方向）に吹っ飛ぶ
                else if (lookLeft)
                {
                    deathKick = new Vector2(xVec, yVec);
                }
                //吹っ飛ぶ
                GetComponent<Rigidbody2D>().velocity = deathKick;
            }
            //操作を無効化する
            isAlive = false;
            //ハメ技をくらわないよう一旦無敵状態にする
            isInvincible = true;

            //コルーチンを回して一定秒後に状態復帰
            StartCoroutine(RecoverFromDamage());
        }

    }

    //ダメージ状態から復帰 微妙な挙動がポイント！
    IEnumerator RecoverFromDamage()
    {
        //0.5秒まつ
        yield return new WaitForSeconds(0.5f);
        //体が吹っ飛ぶ動きを止める（これがないとずっと吹っ飛び続けて不自然）
        GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        //さらに0.5秒まつ
        yield return new WaitForSeconds(0.5f);
        //アニメーションのダメージ状態から戻り
        animator.SetTrigger("Recover");
        //完全に戻り切るまで少し調整時間を置いてから
        yield return new WaitForSeconds(0.5f);
        //操作可能にする
        isAlive = true;
        //ちょっと時間を置いてから
        yield return new WaitForSeconds(0.5f);
        //無敵状態を直す
        isInvincible = false;
        yield break;
    }


}
