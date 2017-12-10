using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotScript : MonoBehaviour {

    public Vector3[] patrolPoints;
    private Light botLight;
    private float intensity;
    private float maxIntensity;
    private bool lightSwitch;

	// Use this for initialization
	void Start () {
        botLight = GetComponentInChildren<Light>();
        maxIntensity = 10;
        intensity = Random.Range(1, maxIntensity);
        int randomNum = Random.Range(1, 2);
        switch (randomNum)
        {
            default: lightSwitch = true;
                break;
            case 1: lightSwitch = true;
                break;
            case 2: lightSwitch = false;
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
        Breathe();
	}

    void Breathe()
    {
        if (lightSwitch)
        {
            intensity += Time.deltaTime * 10;
        }
        else if (!lightSwitch)
        {
            intensity -= Time.deltaTime * 10;
        }
        if (lightSwitch && intensity >= maxIntensity)
        {
            lightSwitch = false;
        }
        else if (!lightSwitch && intensity <= 1)
        {
            lightSwitch = true;
        }
        botLight.intensity = intensity;
    }
}
