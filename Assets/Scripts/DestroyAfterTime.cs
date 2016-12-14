using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {

    public float timeToDestroy = 3;
	
    private float timer;

    void Awake()
    {
        timer = timeToDestroy;
    }

	void Update () {
        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            GameObject.Destroy(this.gameObject);
        }
	}
}
