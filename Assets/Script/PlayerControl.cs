using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class PlayerControl : MonoBehaviour
{
    public static Collider2D c;
    public static PlayerControl player;
    // 人物移动的方向
    public static bool isRight;
    private static int score = 0;
    // 刚体组件
    private Rigidbody2D rbody;
    private Animator animator;
    // 是否在地面上
    private bool isGround;
    // 移动速度
    private float speed = 1;
    // 当前所在的地板
    private Collider2D floor;
    // 人物跳跃力度
    private float jump_power = 95;
    public float timer = 0;
    public int hp = 1;
    private bool isJump = false;
    private bool isMove = false;
    private float timeSpentInvincible;

    // 当前有没有碰到雪球
    private bool isTouchBall = false;
    // 是否是站在雪球上面
    private bool isStandBall = false;
    private GameObject ball;
    // 初始位置
    private Vector3 start_position;
    // 初始旋转
    private Quaternion start_rotation;
    // 渲染器
    private SpriteRenderer render;
    // 无敌时间
    private float GreenTime = 10f;

    

    // 蓝(强力子弹，伤害翻倍)，红(增加移速)，绿(限时无敌)，黄(增加子弹射程50%)
    public bool haveBluePotion;
    private bool haveRedPotion;
    private bool haveGreenPotion;
    public bool haveYellowPotion;

    private void Awake()
    {
        player = this;
    }

    void Start()
    {
        //player = this;
        render = GetComponent<SpriteRenderer>();
        c = GetComponent<Collider2D>();
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        start_position = transform.position;
        start_rotation = transform.rotation;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (hp > 0 && timer >= 2.3f)
        {
            move();
        }
        if (Input.GetKeyDown(KeyCode.K) && isTouchBall) {
            push(ball);
            if (isStandBall) {
                // 踢动雪球，玩家获得无敌时间
                haveGreenPotion = true;
            }
        }
    }

    private void FixedUpdate()
    {
        listenBlue();
        listenRed();
        listenGreen();
        listenYellow();
    }


    // 移动
    private void move() {
        
        if (Input.GetKey(KeyCode.D) && !isRight)
        {
            isRight = true;
            transform.Rotate(Vector3.up, 180);
        }
        if (Input.GetKey(KeyCode.A) && isRight)
        {
            isRight = false;
            transform.Rotate(Vector3.up, 180);
        }
        float x = Input.GetAxis("Horizontal");
        string type = speed == 2 ? "isRun" : "isMove";
        if (x != 0) {
            isMove = true;
            animator.SetBool(type, true);
            Vector3 dir = new Vector3(x,0,0);
            transform.position += dir * speed * Time.deltaTime;
        }else {
            isMove = false;
            animator.SetBool(type,false);
        }
        if (Input.GetButtonDown("Jump") && isGround) {
            isJump = true;
            rbody.AddForce(Vector2.up * jump_power);
            animator.SetBool("isJump",true);
        }
        if (Input.GetButtonUp("Jump")) {
            isJump = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.transform.tag;
        if (tag == "Ground") {
            foreach (ContactPoint2D point in collision.contacts)
            {
                float y = point.normal.y;
                if (y > 0.9f)
                {
                    // 人物脚部碰到地面
                    isGround = true;
                }
            }
            floor = collision.collider;
            animator.SetBool("isJump", false);
        }

        if (tag == "Enemy" || tag == "Boss") {
            hp = 0;
            isCollisionControl(true);
            checkHP();
        }

        if (tag == "Ball") {
            isTouchBall = true;
            animator.SetBool("isPush", isMove);
            ball = collision.gameObject;
            foreach (ContactPoint2D point in collision.contacts)
            {
                float y = point.normal.y;
                if (y < -0.9f && isJump)
                {
                    // 人物跳动时头部撞到雪球
                    collision.gameObject.GetComponent<BallControl>().UpFloor();
                }
                if (y >0.9f)
                {
                    // 人物脚部踩着雪球
                    isStandBall = true;
                }
            }
        }

        if (tag == "Potion") {
            string name = collision.gameObject.name;
            switch (name) {
                case "BluePotion(Clone)":
                    {
                        if (!haveBluePotion)
                        {
                            haveBluePotion = true;
                        }
                        else {
                            score += 2000;
                        }
                        break;
                    }
                    
                case "GreenPotion(Clone)":
                    {
                        if (!haveGreenPotion)
                        {
                            haveGreenPotion = true;
                        }
                        else
                        {
                            score += 2000;
                        }
                        break;
                    }
                case "RedPotion(Clone)":
                    {
                        if (!haveRedPotion)
                        {
                            haveRedPotion = true;
                        }
                        else
                        {
                            score += 2000;
                        }
                        break;
                    }
                case "YellowPotion(Clone)":
                    {
                        if (!haveYellowPotion)
                        {
                            haveYellowPotion = true;
                        }
                        else
                        {
                            score += 2000;
                        }
                        break;
                    }
            }
            Destroy(collision.gameObject);
        }

        if (tag == "Award") {
            AddScore(200);
            Destroy(collision.gameObject);
        }

    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.transform.tag == "Ground")
        {
            isGround = false;
        }
        if (collision.transform.tag == "Ball")
        {
            isTouchBall = false;
            isStandBall = false;
            animator.SetBool("isPush", false);
            ball = null;
        }
    }

    private void push(GameObject obj) {
        AddScore(500);
        obj.GetComponent<BallControl>().subHp(true);
    }

    private void checkHP() {
        if (hp <= 0) {
            animator.SetTrigger("dead");
        }
    }

    public void destroy() {
        Invoke("restart",0.5f);
    }

    // 重生
    private void restart() {
        transform.position = start_position;
        transform.rotation = start_rotation;
        isCollisionControl(false);
        animator.SetTrigger("restart");
        hp = 1;
        isRight = false;
        timer = 0;
        haveGreenPotion = true;
        GreenTime = 3f;
        isGround = true;
    }
    private void isCollisionControl(bool b) {
        Physics2D.IgnoreLayerCollision(8, 6,b);
        Physics2D.IgnoreLayerCollision(8, 7,b);
        Physics2D.IgnoreLayerCollision(8, 9,b);
    }

    private void listenBlue() {
        if (haveBluePotion) {
            Invoke("unBlue",10f);
        }
    }

    private void listenRed() {
        if (isGround)
        {
            if (!haveRedPotion)
            {
                speed = 1f;
            }
            if (haveRedPotion && speed != 2f)
            {
                speed = 2f;
                Invoke("unRed", 10f);
            }
        }
        else {
            speed = haveRedPotion ? 1f : 0.6f;
        }
        
    }

    private void listenGreen() {
        if (haveGreenPotion) {

            if (timeSpentInvincible == 0)
            {
                Physics2D.IgnoreLayerCollision(8, 6);
                Physics2D.IgnoreLayerCollision(8, 9);
                Invoke("unGreen", GreenTime);
            }
            timeSpentInvincible += Time.deltaTime;
            if (timeSpentInvincible % 1 > 0.5f)
            {
                render.material.color = new Color(248, 0, 15);
            }
            else
            {
                render.material.color = Color.yellow;
            }
            
        }
    }

    private void listenYellow()
    {
        if (haveYellowPotion)
        {
            Invoke("unYellow", 10f);
        }
    }

    private void unRed() {
        haveRedPotion = false;
        speed = 1;
        animator.SetBool("isRun", false);
    }

    private void unGreen() {
        haveGreenPotion = false;
        render.material.color = Color.white;
        Physics2D.IgnoreLayerCollision(8, 6,false);
        Physics2D.IgnoreLayerCollision(8, 9,false);
        timeSpentInvincible = 0;
        GreenTime = 10;
    }

    private void unBlue() {
        haveBluePotion = false;
    }

    private void unYellow()
    {
        haveYellowPotion = false;
    }

    public static void AddScore(int i) {
        score += i;
    }

    
}
