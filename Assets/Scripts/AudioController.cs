using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

	static AudioController instance;

    public float menuVolume = 0.3f, gameVolume = 0.4f;

    private AudioSource[] sources;
    public AudioClip menuMusic, puzzleMusic, defenceMusic, sandboxMusic, logicMusic;
    public float fadeFactor = 0.02f;
    private float stayFactor;

	void Awake()
	{
		if (instance == null) {
			//Debug.Log("Assigning instance of Audio Controller");
			instance = this;//new GameObject("Audio Controller").AddComponent<AudioController>();
            sources = GetComponents<AudioSource>();
            sources[0].clip = menuMusic;
            sources[0].Play();
            stayFactor = 1 - fadeFactor;
		} else {
			Destroy (gameObject);
		}
		DontDestroyOnLoad(this);
	}
	
	public void OnApplicationQuit()
	{
		//Debug.Log("Audio Controller destroyed");
		instance = null;
		Destroy(this);
	}

    void Update()
    {
        if (Application.loadedLevelName.Contains("Screen"))
        {
            sources[0].volume = sources[0].volume * stayFactor + menuVolume * fadeFactor;
            sources[1].volume = sources[1].volume * stayFactor;
        }
        else if (Application.loadedLevelName.Contains("Puzzle"))
        {
            if (sources[1].clip != puzzleMusic)
            {
                if (puzzleMusic != null)
                {
                    sources[1].clip = puzzleMusic;
                    sources[1].Stop();
                    sources[1].Play();
                }
            }
            sources[0].volume = sources[0].volume * stayFactor;
            sources[1].volume = sources[1].volume * stayFactor + gameVolume * fadeFactor;
        }
        else if (Application.loadedLevelName.Contains("Defence"))
        {
            if (sources[1].clip != defenceMusic)
            {
                if (defenceMusic != null)
                {
                    sources[1].clip = defenceMusic;
                    sources[1].Stop();
                    sources[1].Play();
                }
            }
            sources[0].volume = sources[0].volume * stayFactor;
            sources[1].volume = sources[1].volume * stayFactor + gameVolume * fadeFactor;
        }
        else if (Application.loadedLevelName.Contains("Sandbox"))
        {
            if (sources[1].clip != sandboxMusic)
            {
                if (sandboxMusic != null)
                {
                    sources[1].clip = sandboxMusic;
                    sources[1].Stop();
                    sources[1].Play();
                }
            }
            sources[0].volume = sources[0].volume * stayFactor;
            sources[1].volume = sources[1].volume * stayFactor + gameVolume * fadeFactor;
        }
        else if (Application.loadedLevelName.Contains("OR") || Application.loadedLevelName.Contains("AND"))
        {
            if (sources[1].clip != logicMusic)
            {
                if (logicMusic != null)
                {
                    sources[1].clip = logicMusic;
                    sources[1].Stop();
                    sources[1].Play();
                }
            }
            sources[0].volume = sources[0].volume * stayFactor;
            sources[1].volume = sources[1].volume * stayFactor + gameVolume * fadeFactor;
        }
        if (sources[0].volume < 0.001f)
        {
            sources[0].Stop();
        }
        else
        {
            if (!sources[0].isPlaying)
            {
                sources[0].Play();
            }
        }
        if (sources[1].volume < 0.001f)
        {
            sources[1].Stop();
        }
    }
}
