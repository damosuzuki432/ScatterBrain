using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;



//下から入る
//何もしなくても一方通行でOK
//上に到達
//下キーを押す
//platformEfector2DがDisableとなり、IsTriggerがonになる
//コライダからexitすると元どおりplatformEffecterがableとなりIstriggerがoffになる

public class LadderDown : MonoBehaviour
{

    PlatformEffector2D platformEffector2D;
    CompositeCollider2D compositeCollider2D;
    

    // Start is called before the first frame update
    void Start()
    {
        platformEffector2D = GetComponent<PlatformEffector2D>();
        compositeCollider2D = GetComponent<CompositeCollider2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        ClimbDown();
    }

    private void ClimbDown()
    {
        if (compositeCollider2D.IsTouchingLayers(LayerMask.GetMask("Player")))
        {
          
            float controlThrow = CrossPlatformInputManager.GetAxis("Vertical");
            if (controlThrow < 0)
            {
                platformEffector2D.enabled = false;
                compositeCollider2D.isTrigger = true;
                
            }
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        platformEffector2D.enabled = true;
        compositeCollider2D.isTrigger = false;

    }

}
