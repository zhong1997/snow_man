using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossControl : MonoBehaviour
{
    // 小弟发射点
    public Transform SonPoint;
    // 小弟预设体
    public GameObject SonPrefab;
    public static Collider2D c;
    private Rigidbody2D rbody;
    private Animator animator;
    private float timer;
    private bool isGround;
    private Collider2D floor;
    private int hp = 2000;
    // 渲染器
    private SpriteRenderer render;
    //private bool isTwinkle = false;
    //private float timeSpentInvincible = 0;


    int count = 0;

    private void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        c = GetComponent<Collider2D>();
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (hp > 0) {
            jump();
        }
    }

    private void FixedUpdate()
    {
        //twinkle();
    }


    private void jump() {
        // 6s 大跳一下
        if ((timer / 2) >= 1 && count >= 3 && isGround)
        {
            rbody.AddForce(Vector2.up * 150);
            animator.SetBool("isBigJump", true);
            count = 0;
            timer = 0;
            if (floor.name == "floor_4")
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(),floor);
                Invoke("regain",1.4f);
            }
            return;
        }
        // 2s 小跳一下
        if ((timer / 2) >= 1 && isGround)
        {
            rbody.AddForce(Vector2.up * 90);
            animator.SetBool("isJump",true);
            ++count;
            timer = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Ground") {
            floor = collision.collider;
            isGround = true;
            animator.SetBool("isJump", false);
            animator.SetBool("isBigJump", false);
        }

        if (hp == 0) {
            Destroy(gameObject);
        }
    }


    // 攻击，也就是发射小弟
    public void Attack() {
        GameObject bullet = Instantiate(SonPrefab, SonPoint.position, SonPoint.rotation);
    }

    // 恢复碰撞
    private void regain()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), floor, false);
    }

    public void subHP(string name) {
        if (hp <= 0) {
            return;
        }
        switch (name) {
            case "Bullet(Clone)":
                --hp;
                break;
            case "BigBullet(Clone)":
                hp -= 5;
                break;
            case "Ball(Clone)":
                hp -= 200;
                break;
        }
        //isTwinkle = true;
        checkHP();
    }

    private void checkHP() {
        if (hp <= 0) {
            Physics2D.IgnoreCollision(PlayerControl.c, GetComponent<Collider2D>());
            animator.SetTrigger("dead");
        }
    }

    private void Destroy() {
        Destroy(gameObject);
    }

    //private void twinkle() {
    //    if (isTwinkle)
    //    {
    //        timeSpentInvincible += Time.deltaTime;
    //        if (timeSpentInvincible % 1 > 0.5f)
    //        {
    //            render.material.color = Color.blue;
    //        }
    //        else
    //        {
    //            render.material.color = Color.white;
    //        }
    //    }
    //}
    //private void unTwinkle() {
    //    isTwinkle = false;
    //}

}
