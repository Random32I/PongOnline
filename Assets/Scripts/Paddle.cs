using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    [SerializeField] Rigidbody2D rig;
    [SerializeField] float force;

    [SerializeField] GameManager game;

    bool move = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) move = true;
        else move = false;
        if (move) game.UpdateServer();
        else game.serverBeingUpdated = false;
    }

    private void FixedUpdate()
    {
        if (move) rig.velocity = Vector3.right * Input.GetAxisRaw("Horizontal") * force;
        else rig.velocity = Vector3.zero;
    }
}
