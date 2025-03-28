using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] Rigidbody2D rig;

    [SerializeField] float speed;
    [SerializeField] GameManager game;

    int hits;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        rig.velocity = new Vector3(game.ballXDir, game.ballYDir, 0) * (speed + hits * 0.1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            game.ballXDir *= -1;
        }
        else if (collision.transform.tag == "Player")
        {
            game.ballYDir *= -1;
            hits++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Finish")
        {
            transform.position = Vector3.zero;
            if (collision.name == "OppGoal")
            {
                game.AddScore(1);
                game.ballYDir = -1;
                game.ballXDir = 1;
            }
            else
            {
                game.AddScore(0);
                game.ballYDir = 1;
                game.ballXDir = -1;
            }
        }
    }
}
