using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallControl : MonoBehaviour
{
    public GameObject SonPre;
    // 药水预设体
    public GameObject BluePotionPre;
    public GameObject RedPotionPre;
    public GameObject GreenPotionPre;
    public GameObject YellowPotionPre;

    public GameObject CakePre;
    public GameObject CandyPre;
    public GameObject IceCreamPre;
    public GameObject LollyPre;
    public GameObject MushRoomPre;
    public GameObject SandWichPre;

    private Rigidbody2D rbody;
    private Animator animator;
    private bool isFirstGround = false;
    private bool isWall = false;
    private bool isLeftWall = false;
    private float gravityScale = 1f;
    // 是否滚动
    private bool isScroll;
    private Vector2 scrollDir;
    private Collider2D player;

    private float timer = 0;
    private int hp = 3;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        player = PlayerControl.c;

        // 雪球在没有变成圆形之前，和角色、boss、小怪之间没有碰撞
        isCollisionControl(player,false);
        isCollisionControl(BossControl.c, false);
        Physics2D.IgnoreLayerCollision(7, 6);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (!isScroll) addHP(timer);
        scroll(isScroll);
    }

    void FixedUpdate() {
        listen();
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        Transform t = collision.transform;
        if (t.tag == "Wall")
        {
            PlayerControl.AddScore(10);
            foreach (ContactPoint2D c in collision.contacts) {
                float x = c.normal.x;
                if (x > 0.9f) {
                    // 雪球碰到墙的右边，说明雪球撞到左边的墙体了
                    isLeftWall = true;

                }
                if (x < -0.9f) {
                    // 雪球碰到墙的左边，说明雪球撞到右边的墙体了
                    isLeftWall = false;
                }
            }
            isWall = true;
            // 改变滚动方向
            scrollDir.x = -scrollDir.x;
        }
        if (t.tag == "Ground" && hp > 1) {
            // 不完整体雪球状态，设置速度为0，同时没有重力，达到固定在原地的效果，不会因为物理惯性漂移
            unMove();
        }
        if (t.name == "floor_5")
        {
            isFirstGround = true;
        }
        if (t.tag == "Enemy" && isScroll) {
            PlayerControl.AddScore(500);
            t.GetComponent<SonControl>().isGoDie = true;
            createPotionAndAward();
        }
        if (t.tag == "Boss" && isScroll)
        {
            // 扣血
            t.GetComponent<BossControl>().subHP(transform.name);
            Destroy(gameObject);
        }
        die();
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        Transform t = collision.transform;
        if (t.tag == "Wall")
        {
            isWall = false;
        }
        if (t.name == "floor_5")
        {
            isFirstGround = false;
        }
    }

    public void subHp(bool isPush)
    {
        if (hp > 1)
        {
            --hp;
        }
        if (hp == 1 && isPush)
        {
            --hp;
            isCollisionControl(BossControl.c, true);
            // 此处决定推动时的方向
            if (isWall)
            {
                // 靠墙，那就让它直接往墙的反方向滚动，不依赖玩家的方向
                scrollDir = isLeftWall ? Vector2.right : Vector2.left;
                PlayerControl.AddScore(10);
            }
            else
            {
                // 不靠墙，该往哪边往哪边
                scrollDir = PlayerControl.isRight ? Vector2.right : Vector2.left;
            }

            isScroll = true;
        }
    }

    private void addHP(float f)
    {
        // 通过名称获取图层，并设置
        //gameObject.layer = LayerMask.LayerToName("");
            
        // 5s回一次血
        if (f >= 10 && hp <= 3)
        {
            unMove();
            // 回血了，关闭和玩家、小怪的碰撞
            isCollisionControl(player, false);
            Physics2D.IgnoreLayerCollision(7, 6);
            ++hp;
            timer = 0;
        }
    }

    public void setHP(int hpValue) {
        hp = hpValue;
    }

    private void listen() {
        animator.SetFloat("hp", hp);
        if (hp == 1) {
            // 恢复重力
            rbody.gravityScale = gravityScale;
            // 圆形雪球时，开启与玩家的碰撞，才能被推动
            isCollisionControl(player, true);
        }
        if (hp > 3)
        {
            // 大于3，说明雪球已经完全融化了，得把小怪放回来
            GameObject son = Instantiate(SonPre);
            son.GetComponent<SonControl>().isFormBoss = false;
            son.transform.position = gameObject.transform.position;
            Destroy(gameObject);
        }
    }

    private void scroll(bool b)
    {
        if (b)
        {
            // 滚动状态下不与玩家产生碰撞
            isCollisionControl(player, false);
            // 滚动状态下会对敌人造成伤害，开启碰撞检测
            Physics2D.IgnoreLayerCollision(7, 6, false);
            transform.Translate(scrollDir * 2 * Time.deltaTime);
            // 获取物体体积
            //Vector2 volume = GetComponent<Collider2D>().bounds.size;
            //Collider[] arr = Physics.OverlapBox(transform.position, volume, Quaternion.identity, LayerMask.GetMask("Ball"));
            // 获取当前物体周围0.01范围内的碰撞体
            Collider2D[] arr = Physics2D.OverlapCircleAll(transform.position, 0.01f);
            foreach (Collider2D c in arr)
            {
                BallControl ball = c.GetComponent<BallControl>();
                if (ball == null) continue;
                if (ball.tag == "Ball" && ball.hp == 1 && !ball.isScroll)
                {
                    // 是完整雪球并且没有滚动，那么我就带动该雪球滚动
                    ball.scrollDir = scrollDir;
                    ball.subHp(true);
                }
                if (ball.tag == "Ball" && ball.hp > 1)
                {
                    // 不是完整体雪球，则销毁该雪球
                    Destroy(ball.gameObject);
                    PlayerControl.AddScore(500);
                    createPotionAndAward();
                }
            }
        }
    }

    // 雪球与其他物体的碰撞开关
    private void isCollisionControl(Collider2D other,bool b) {
        Physics2D.IgnoreCollision(other, GetComponent<Collider2D>(), !b);
    }

    private void die()
    {
        // 只有在第一层的地面上，撞到墙才会死
        if (isFirstGround && isWall)
        {
            Destroy(gameObject);
        }
    }

    public void UpFloor()
    {
        rbody.AddForce(Vector2.up * 50);
    }

    private void unMove() {
        rbody.velocity = Vector2.zero;
        rbody.gravityScale = 0;
    }

    // 随机生成药水、奖励
    private void createPotionAndAward() {
        int r = Random.Range(0, 100);
        if (r <= 40)
        {
            GameObject obj = null;
            if (r > 0 && r <= 4)
            {
                obj = Instantiate(BluePotionPre);
            }
            else if (r > 4 && r <= 8)
            {
                obj = Instantiate(RedPotionPre);
            }
            else if (r > 8 && r <= 12)
            {
                obj = Instantiate(GreenPotionPre);
            }
            else if (r > 12 && r <= 16)
            {
                obj = Instantiate(YellowPotionPre);
            }
            else if (r > 16 && r <= 20)
            {
                obj = Instantiate(CakePre);
            }
            else if (r > 20 && r <= 24)
            {
                obj = Instantiate(CandyPre);
            }
            else if (r > 24 && r <= 28)
            {
                obj = Instantiate(IceCreamPre);
            }
            else if (r > 28 && r <= 32)
            {
                obj = Instantiate(LollyPre);
            }
            else if (r > 32 && r <= 36)
            {
                obj = Instantiate(MushRoomPre);
            }
            else if (r > 36 && r <= 40)
            {
                obj = Instantiate(SandWichPre);
            }
            if (obj != null) {
                obj.transform.position = transform.position;
            }
        }
    }

    public int getHP() {
        return hp;
    }
}
