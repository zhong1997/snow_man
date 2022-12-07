using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : MonoBehaviour
{
    private Rigidbody2D rbody;
    private float timer;
    //private float speed = 1.5f;
    private float speed = 2f;
    private PlayerControl player;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        player = PlayerControl.player;
    }

    void Update()
    {
        timer += Time.deltaTime;
        fire();
        rbody.gravityScale = 0.3f;
        rbody.isKinematic = true;
        float time = 0.4f;
        if (player.haveYellowPotion && time != 0.6f)
        {
            time = 0.6f;
        }
        if (timer > time)
        {
            speed = 0.1f;
            rbody.isKinematic = false;
            Invoke("destroy", 0.5f);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        if (obj.tag == "Boss")
        {
            PlayerControl.AddScore(10);
            obj.GetComponent<BossControl>().subHP(gameObject.name);
        }

        if (obj.tag == "Enemy")
        {
            PlayerControl.AddScore(10);
            collision.transform.GetComponent<SonControl>().createBall(collision.gameObject,gameObject);
        }
        if (obj.tag == "Ball")
        {
            BallControl ball = collision.gameObject.GetComponent<BallControl>();
            if (ball.getHP() > 1) {
                PlayerControl.AddScore(10);
            }
            ball.subHp(false);
        }
        if (obj.tag != "Bullet")
        {
            Destroy(gameObject);
        }

    }

    private void fire() {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void destroy() {
        Destroy(gameObject);
    }
}
