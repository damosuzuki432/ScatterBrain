using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{
    //動くスピード
    [SerializeField] float moveSpeed = 1.0f;
    //ヒットポイント
    [SerializeField] int hitPoint = 3;
    //足下判定のレイキャストのためのベクトル
    Vector2 checkOnFloor;
    float xDirection = 1f;
    float yDirection = -1f;
    //レイキャストの距離（地面）
    float distanceF = 1f;
    //レイキャストの距離（壁)
    float distanceW = 0.3f;
    //落下中の重力
    float fallGravity = 4;
    //通常の重力
    float gravityScaleAtFirst;
    //groundレイヤーマスク
    public LayerMask groundLayer;

    Rigidbody2D myRigidbody2D;
    BoxCollider2D boxCollider2D;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        checkOnFloor = new Vector2(xDirection, yDirection);
        gravityScaleAtFirst = myRigidbody2D.gravityScale;
        
    }

    // Update is called once per frame
    void Update()
    {
        //足下にgounrdレイヤーがあるか常に探索
        CheckFloor();
        //空中に浮いてないか常にチェック
        CheckInAir();
        
        if(IsFacingRight())
        {
            //左向きに進む
            myRigidbody2D.velocity = new Vector2(moveSpeed, 0f);
            checkOnFloor = new Vector2(1, -1);
            //壁にぶつかっていないか常にチェック
            CheckAgainstWall(Vector2.right);
        }
        else
        {
            //右向きに進む
            myRigidbody2D.velocity = new Vector2(-moveSpeed, 0f);
            checkOnFloor = new Vector2(-1, -1);
            //壁にぶつかっていないか常にチェック
            CheckAgainstWall(Vector2.left);
        }
        
    }

   
    private void CheckFloor()
    {
        //レイを斜め下に飛ばして、groundレイヤーがあるかどうかを見る
        RaycastHit2D ifFloor = Physics2D.Raycast(transform.position, checkOnFloor, distanceF, groundLayer);
        //もしgroundレイヤーがあれば
        if(ifFloor.collider != null)
        {
            //処理抜ける
            return;
        }
        //グラウンドレイヤーがなければ
        else
        {
            //反転させる
            transform.localScale = new Vector2(-(Mathf.Sign(myRigidbody2D.velocity.x)), 1f);
        }
    }

    private void CheckInAir()
    {
        //groundに接していなければ、浮いているものとみなして
        if (!boxCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            //重力スケールを一気にあげてストンと落とす
            myRigidbody2D.gravityScale =fallGravity;
        }
        //それ以外は、接地しているものとみなして通常の重力に
        else
        {
            myRigidbody2D.gravityScale = gravityScaleAtFirst;
        }
    }

    private void CheckAgainstWall(Vector2 vec2)
    {
        //進行方向にレイを飛ばして
        RaycastHit2D ifWall = Physics2D.Raycast(transform.position, vec2, distanceW, groundLayer);
        //壁（グラウンドレイヤー）があれば反転
        if (ifWall.collider != null)
        {
            transform.localScale = new Vector2(-(Mathf.Sign(myRigidbody2D.velocity.x)), 1f);
        }
    }

    //どっちを向いているかをxの値で判断
    bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "NormalShot")
        {
            int damagePoint = 1;
            DamageManager(damagePoint);
        }
    }

    //当たることで呼ばれる
    //shotのスクリプトからダメージを受け取る
    private void DamageManager(int damagePoint)
    {
        //HPを減らす
        hitPoint = hitPoint - damagePoint;

        //０以下になったならデストロイ
        if (hitPoint <=0)
        {
            Destroy(gameObject);
        }
    }


}
