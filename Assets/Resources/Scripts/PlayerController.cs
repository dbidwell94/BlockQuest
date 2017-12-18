using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public Joystick Joystick;
    private Vector3 playerStartPos;
    private Vector3 playerPosRounded, playerPos;
    private GameObject player;
    public GameObject deathParts;
    public GameObject camHolder, sun1Holder, sun2Holder, sun1, sun2;
    private bool isDead = false;
    private int levelDeathCount;
    public float sensitivity;

	// Use this for initialization
	void Start () {
        levelDeathCount = 0;
        playerPosRounded = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        playerPos = playerPosRounded;
        playerStartPos = playerPosRounded;
        player = this.gameObject;
        sensitivity = (PlayerPrefs.HasKey("Joystick Sensitivity")) ?
            sensitivity = PlayerPrefs.GetFloat("Joystick Sensitivity") :
            sensitivity = 1f;
	}
	
	// Update is called once per frame
	void Update () {
        if (!isDead)
        {
            MovePlayer();
        }
        if (isDead)
        {
            if (GameObject.Find("deathparts") == null)
            {
                isDead = false;
                player.transform.position = playerStartPos;
                playerPos = playerStartPos;
                playerPosRounded = playerStartPos;
                this.GetComponent<MeshRenderer>().enabled = true;
                this.GetComponent<BoxCollider>().isTrigger = false;
                this.GetComponent<Rigidbody>().useGravity = true;
            }
        }
        if (levelDeathCount >= 10)
        {
            GooglePlayGames.PlayGamesPlatform.Instance.ReportProgress(GPGSIds.achievement_persistent, 100, (bool success) => { });
        }
	}

    void MovePlayer()
    {
        if (player.transform.position == playerPosRounded)
        {
            Vector3 joystickMove = (new Vector3(Joystick.JoystickOutput.x, 0, Joystick.JoystickOutput.y)) * sensitivity;          
            joystickMove = (joystickMove.magnitude > 1) ? joystickMove = joystickMove.normalized : joystickMove;
            Vector3 moveTo = (player.transform.position + joystickMove) + DPad._Instance.DPadOutput;
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
        levelDeathCount = 0;
        playerPosRounded = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        playerPos = playerPosRounded;
        playerStartPos = playerPosRounded;
    }

    void Die()
    {
        levelDeathCount++;
        isDead = true;
        GameObject parts = Instantiate(deathParts, this.transform.position, new Quaternion());
        parts.name = "deathparts";
        this.GetComponent<BoxCollider>().isTrigger = true;
        this.GetComponent<Rigidbody>().useGravity = false;
        this.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Goal")
        {
            GameManager._Instance.HitGoal(other.gameObject);
        }
    }

    public void UpdateSensitivity()
    {
        sensitivity = (PlayerPrefs.HasKey("Joystick Sensitivity")) ?
            sensitivity = PlayerPrefs.GetFloat("Joystick Sensitivity") :
            sensitivity = 1f;
    }
}
