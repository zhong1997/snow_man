using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SonControl : MonoBehaviour
{
    // 雪球预设体
    public GameObject BallPre;
    // 药水预设体
    public GameObject BluePotionPre;
    public GameObject RedPotionPre;
    public GameObject GreenPotionPre;
    public GameObject YellowPotionPre;
    public bool isFormBoss = true;
    private Rigidbody2D rbody;
    private Animator animator;
    private bool isGround = true;
    private bool isFirstGround = false;
    private bool isWall = false;
    private bool isRight = true;
    public bool isGoDie = false;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        if (isFormBoss) {
            rbody.AddForce(Vector2.left * 150);
        }
        
        // 左右随机
        isRight = Random.Range(0, 2) == 0;
    }

    void Update()
    {
        move();
        die();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground") {
            isGround = true;
            animator.SetBool("isMove", isGround);
        }
        if (collision.transform.tag == "Wall") {
            isWall = true;
            isRight = !isRight;
        }
        if (collision.gameObject.name == "floor_5") {
            isFirstGround = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground")
        {
            isGround = false;
            animator.SetBool("isMove", isGround);
        }
        if (collision.transform.tag == "Wall")
        {
            isWall = false;
        }
        if (collision.gameObject.name == "floor_5")
        {
            isFirstGround = false;
        }
    }

    private void move()
    {
        float speed = 0.3f * Time.deltaTime;
        Vector2 dir = Vector2.left;
        if (isRight) {
            dir = -dir;
            GetComponent<SpriteRenderer>().flipX = true;
        }
        transform.Translate(dir * speed);
    }

    private void die() {
        // 只有在第一层的地面上，撞到墙才会死
        if ((isFirstGround && isWall) || isGoDie)
        {
            Destroy(gameObject);
        }
    }

    public void createBall(GameObject param,GameObject bullet) {
        GameObject obj = Instantiate(BallPre);
        if (bullet.name == "BigBullet(Clone)")
        {
            obj.GetComponent<BallControl>().setHP(1);
        }
        obj.transform.position = param.transform.position;
        Destroy(param);
    }

    // 随机生成药水
    private void createPotion()
    {
        int r = Random.Range(0, 100);
        if (r <= 20)
        {
            GameObject obj = null;
            if (r > 0 && r <= 5)
            {
                obj = Instantiate(BluePotionPre);
            }
            else if (r > 5 && r <= 10)
            {
                obj = Instantiate(RedPotionPre);
            }
            else if (r > 10 && r <= 15)
            {
                obj = Instantiate(GreenPotionPre);
            }
            else if (r > 15 && r <= 520)
            {
                obj = Instantiate(YellowPotionPre);
            }
            if (obj != null)
            {
                obj.transform.position = transform.position;
            }
        }
    }
}
