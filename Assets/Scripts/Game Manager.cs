using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] List<AudioSource> unpausableAudioSources = new List<AudioSource>();
    [SerializeField] List<Object> componentsToPause = new List<Object>();
    private List<Behaviour> fullyPausableBehaviours = new List<Behaviour>();
    List<Behaviour> previouslyEnabledBehaviours = new List<Behaviour>();
    float timeScale;
    public bool GamePaused => paused;
    private bool paused = false;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        timeScale=Time.timeScale;
        foreach (AudioSource audioSource in unpausableAudioSources)
        {
            audioSource.ignoreListenerPause=true;
        }
        foreach (Object obj in componentsToPause)
        {
            if (obj is Behaviour behaviour)
            fullyPausableBehaviours.Add(behaviour);
        }
    }
    public void PauseGame()
    {
        if (!paused)
        {
            timeScale=Time.timeScale;
            Time.timeScale=0;
            AudioListener.pause=true;
            fullyPausableBehaviours.RemoveAll(b => b == null);
            foreach (Behaviour behaviour in fullyPausableBehaviours)
            {
                if (behaviour.enabled)
                {
                    previouslyEnabledBehaviours.Add(behaviour);
                    behaviour.enabled=false;
                }
            }
            paused=true;
        }
    }
    public void UnpauseGame()
    {
        if (paused)
        {
            Time.timeScale=timeScale;
            AudioListener.pause=false;
            previouslyEnabledBehaviours.RemoveAll(b => b == null);
            foreach (Behaviour behaviour in previouslyEnabledBehaviours)
            {
                behaviour.enabled=true;
            }
            previouslyEnabledBehaviours.Clear();
            paused=false;
        }
    }
    public void TogglePause()
    {
        if (paused)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }
}
