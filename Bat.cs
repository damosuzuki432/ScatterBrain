using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Bat : MonoBehaviour
{
    //初期位置
    Vector2 startPoint;

    //プレイヤー参照
    Player player;
    Vector2 playerPos;

    //可動距離x軸 プラマイで使う
    [SerializeField] float moveRangeX;
    //可動距離y軸 プラマイで使う
    [SerializeField] float moveRangeY;

    //いったん動いたあとは近接条件は見ない（コルーチンが複数起動しないように）
    bool ifStarted = false;



    // Start is called before the first frame update
    void Start()
    {
        //プレイヤーを探す
        player = FindObjectOfType<Player>();
        
        //コウモリの初期位置
        startPoint = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        //プレイヤーの場所
        playerPos = player.transform.position;
        //プレイヤーとコウモリの距離をウオッチ
        float distance = Vector2.Distance(playerPos, startPoint);

        //もしプレイヤーが近接条件を満たしたら
        if (distance < 2 && ifStarted == false)
        {
            //飛び立つ
            StartCoroutine(Fly());
        }
    }

    IEnumerator Fly()
    {
        //飛び立ったフラグを立てる
        ifStarted = true;

        for (int i=0; i<=15;i++)
        {
            //四角形内のある一点をランダムで定義する
            float movePosX = Random.Range(-moveRangeX, moveRangeX);
            float movePosY = Random.Range(-moveRangeY, moveRangeY);
            Vector2 movePos = new Vector2(movePosX, movePosY);

            //コウモリの初期位置から、ランダム点分動かした到達点を指定
            Vector2 arrivalPos = movePos + startPoint;

            //その点に向かって、定速で移動するために距離を一定のレートで割り、それをdurationとする
            //これは数学的にそうだから覚えておくといい
            float distance = Vector2.Distance(transform.position, movePos);
            //このレートがコウモリの速さを決める
            float rate = 2.0f;
            float duration = distance / rate;

            //DoTweenで動かす
            gameObject.transform.DOLocalMove(arrivalPos, duration);

            //到達点に着くまで待つ **待ち時間をduration よりほんの少し短くすることで、次の移動まで間を開けない
            //なめらかな動きになる
            yield return new WaitForSeconds(duration - duration/10);

            //その点に着いたら、繰り返し

            //５回繰り返したら、次の移動点は初期基準点とする
            //帰還し、静止アニメーションに戻る

        }
        //スタート前に戻す
        ifStarted = false;
        yield break;

            }
}
