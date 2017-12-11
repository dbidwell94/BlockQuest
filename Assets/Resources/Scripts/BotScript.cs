using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotScript : MonoBehaviour {

    private Transform botTrans;
    public Vector3[] patrolPoints;
    private Dictionary<Vector3, bool> pointsToGo;
    private Light botLight;
    private float intensity;
    private float maxIntensity;
    private bool lightSwitch;
    private Vector3 toGo;

	// Use this for initialization
	void Start () {
        botTrans = this.GetComponent<Transform>();
        toGo = botTrans.position;
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
        if (patrolPoints != null)
        {
            List<Vector3> tempPoints = new List<Vector3>();
            foreach (Vector3 t in patrolPoints)
            {
                tempPoints.Add(t);
            }

            tempPoints.Add(botTrans.position);
            patrolPoints = tempPoints.ToArray();

            pointsToGo = new Dictionary<Vector3, bool>();
            foreach (Vector3 point in patrolPoints)
            {
                pointsToGo.Add(point, false);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        Breathe();
        if (patrolPoints != null)
        {
            Patrol();
        }
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

    void Patrol()
    {
        botTrans.position = Vector3.MoveTowards(botTrans.position, toGo, Time.deltaTime * 5);
        if (pointsToGo.ContainsKey(botTrans.position))
        {
            pointsToGo[botTrans.position] = true;
            foreach (KeyValuePair<Vector3, bool> p in pointsToGo)
            {
                Vector3 Direction = p.Key - botTrans.position;
                Ray ray = new Ray(botTrans.position, Direction);
                if (!p.Value && !Physics.Raycast(ray, Direction.magnitude))
                {
                    toGo = p.Key;
                }
            }

            if (!pointsToGo.ContainsValue(false))
            {
                Dictionary<Vector3, bool> tempDictionary = new Dictionary<Vector3, bool>();
                foreach(KeyValuePair<Vector3, bool> p in pointsToGo)
                {
                    tempDictionary.Add(p.Key, false);
                }
                pointsToGo = tempDictionary;
                if (pointsToGo.ContainsKey(botTrans.position))
                {
                    pointsToGo[botTrans.position] = true;
                }
            }
        }
    }
}
