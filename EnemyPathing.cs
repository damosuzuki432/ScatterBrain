using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour
{
    WaveConfig waveConfig; //referred as "this."
    List<Transform> wayPoints;
    
    int wayPointindex = 0;

    // Start is called before the first frame update
    void Start()
    {
        //動線（wayPoints）をwaveConfigから取得。
        wayPoints = waveConfig.GetWayPoints();
        //スタート時点でインデックス０の場所に移動
        transform.position = wayPoints[wayPointindex].transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void SetWaveConfig(WaveConfig waveConfig)
    {
        //this. はこのクラスで定義したwaveConfigを指しているという意味。
        //右辺のwaveConfigは（）内の引数のwaveConfig.
        //つまり「このクラスでいうwaveConfigとは、引数のwaveConfigですよ、その引数はEnemySpawnerから
        //やってきますよ」と言っている。これが無いとEnemyはMove関数にてwaveConfigが設定されていないので、実行できない。
        this.waveConfig = waveConfig;
    }


    private void Move()
    {
        //インデックスは０からスタート。インデックスの長さ（Count)より小さければ
        if (wayPointindex <= wayPoints.Count - 1)
        {
            //ターゲットをインデックス０において
            var targetPosition = wayPoints[wayPointindex].transform.position;
            //動く速さをデルタタイムに指定して
            var movementThisFrame = waveConfig.GetmoveSpeed() * Time.deltaTime;
            //動かす。movetowardsを使うと現在地からターゲットまで指定の速さで移動する
            transform.position = Vector2.MoveTowards
                (transform.position, targetPosition, movementThisFrame);

            //指定のターゲットに到着したら次のターゲットを狙う
            if (transform.position == targetPosition)
            {
                wayPointindex++;
            }
        }
        else
        {
            //ターゲットに到着したらオブジェクトを破壊
            Destroy(gameObject);
        }
    }
}
