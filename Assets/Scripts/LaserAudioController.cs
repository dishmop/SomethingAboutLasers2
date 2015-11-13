using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class LaserAudioController : MonoBehaviour {

    AudioSource audioSource;

	// Use this for initialization
	void Start () {
        audioSource = gameObject.GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(Mathf.Pow(transform.GetChild(0).GetChild(0).localScale.x / Global.UnitBeamRadius, 2.0f) * 3.0f / 8.0f);
        audioSource.pitch = 2.0f;// Mathf.Pow(transform.GetChild(0).GetChild(0).localScale.x / Global.UnitBeamRadius, 2.0f) * 3.0f / 8.0f;
	}
}
