using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireControl : MonoBehaviour
{
    // 关联子弹点
    public Transform FirePoint;
    // 关联子弹预设体
    public GameObject BulletPre;
    public GameObject BigBulletPre;
    // 计时器
    private float timer;
    // 开火cd
    private float cd = 0.1f;
    private PlayerControl player;


    void Start()
    {
        player = PlayerControl.player;
    }

    void Update()
    {
        timer += Time.deltaTime;
        Debug.Log(player);
        if (timer >= cd && Input.GetKeyDown(KeyCode.K) && (player.hp>0) && (player.timer > 2.3f)) {
            timer = 0;
            // 创建子弹
            Instantiate(player.haveBluePotion ? BigBulletPre : BulletPre, FirePoint.position,FirePoint.rotation);
            //Instantiate( BigBulletPre, FirePoint.position,FirePoint.rotation);
        }
    }
}
