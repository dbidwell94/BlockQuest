using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public Joystick Joystick;
    private Vector3 playerStartPos;
    private Vector3 playerPosRounded, playerPos;
    private GameObject player;

	// Use this for initialization
	void Start () {
        playerPosRounded = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        playerPos = playerPosRounded;
        playerStartPos = playerPosRounded;
        player = this.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        MovePlayer();
	}

    void MovePlayer()
    {
        if (player.transform.position == playerPosRounded)
        {
            Vector3 joystickMove = new Vector3(Joystick.JoystickOutput.x, 0, Joystick.JoystickOutput.y);
            Vector3 moveTo = player.transform.position + joystickMove;
            Vector3 Direction = moveTo - player.transform.position;
            Ray ray = new Ray(player.transform.position, Direction);
            if (!Physics.Raycast(ray, Mathf.Round(Direction.magnitude)))
            {
                playerPos = new Vector3(moveTo.x, player.transform.position.y, moveTo.z);
                playerPosRounded = new Vector3(Mathf.Round(playerPos.x), player.transform.position.y, Mathf.Round(playerPos.z));
            }
        }
        player.transform.position = Vector3.MoveTowards(player.transform.position, playerPosRounded, Time.deltaTime * 5);

    }

    public void RefreshSpawn(Vector3 spawnLoc)
    {
        playerPosRounded = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        playerPos = playerPosRounded;
        playerStartPos = playerPosRounded;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            player.transform.position = playerStartPos;
            playerPos = playerStartPos;
            playerPosRounded = playerStartPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Goal")
        {
            GameManager._Instance.HitGoal(other.gameObject);
        }
    }
}
