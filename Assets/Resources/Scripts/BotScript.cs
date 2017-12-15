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
        maxIntensity = 30;
        intensity = Random.Range(1, maxIntensity);
        botLight.intensity = intensity;
        lightSwitch = Random.Range(1, 2) > 1;
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
        if (botLight.intensity >= maxIntensity)
        {
            lightSwitch = true;
        }
        else if (botLight.intensity <= 0)
        {
            lightSwitch = false;
        }
        float multiplier = (lightSwitch) ? -1f : 1f;
        intensity = botLight.intensity + (Time.deltaTime * 50) * multiplier;
        botLight.intensity = intensity;
    }

    void Patrol()
    {
        botTrans.position = Vector3.MoveTowards(botTrans.position, toGo, Time.deltaTime * 5f);
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
