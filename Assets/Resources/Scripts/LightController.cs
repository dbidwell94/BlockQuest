using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightController : MonoBehaviour {

    private GameObject sun;
    private Light myLight;
    private float range;
    private Color newColor;
    private bool breathe;
    private float breatheSpeed;
    private GameObject player;

	// Use this for initialization
	void Start () {
        sun = this.gameObject;
        myLight = GetComponent<Light>();
        range = 0;
        ColorChange();
        breathe = true;
        breatheSpeed = 1f;
        newColor = myLight.color;
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update () {
        sun.transform.LookAt(player.transform);
        range = Vector3.Distance(sun.transform.position, player.transform.position);
        myLight.range = range * 1.15f;

        if (Input.GetKeyDown(KeyCode.Space) || breathe)
        {
            ColorChange();
        }
	}

    void ColorChange()
    {
        float r = Random.Range(0, 255);
        float g = Random.Range(0, 255);
        float b = Random.Range(0, 255);
        r = r / 255;
        g = g / 255;
        b = b / 255;
        if (!breathe && Input.GetKeyDown(KeyCode.Space))
        {            
            newColor = new Color(r, g, b, 1);
            myLight.color = newColor;
        }

        if (breathe)
        {
            if (myLight.color == newColor)
            {
                newColor = new Color(r, g, b, 1);
            }
            myLight.color = Color.Lerp(myLight.color, newColor, breatheSpeed * Time.deltaTime);
        }
    }

    

}
