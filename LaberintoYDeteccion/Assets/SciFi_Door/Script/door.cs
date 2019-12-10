using UnityEngine;
using System.Collections;

public class door : MonoBehaviour {
	GameObject thedoor;
    AudioSource doorSound;

    private void Awake()
    {
        doorSound = GetComponent<AudioSource>();
    }

    void OnTriggerEnter ( Collider obj  )
    {
	    thedoor= GameObject.FindWithTag("SF_Door");
	    thedoor.GetComponent<Animation>().Play("open");
        doorSound.Stop();
        doorSound.Play();
    }

void OnTriggerExit ( Collider obj  )
    {
	    thedoor= GameObject.FindWithTag("SF_Door");
	    thedoor.GetComponent<Animation>().Play("close");
        doorSound.Stop();
        doorSound.Play();
    }
}