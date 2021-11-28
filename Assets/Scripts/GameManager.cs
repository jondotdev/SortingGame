using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Transform interactionCursor;

    public Text Score;
    public int score = 0;

    public bool paused = false;

    private Image cursor;
    private Text cursorHint;

    private SoundManager sm;

    private void Start()
    {
        cursor = interactionCursor.GetComponent<Image>();
        cursorHint = interactionCursor.Find("CursorHint").GetComponent<Text>();
        sm = transform.GetComponent<SoundManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) { Unpause(); }
            else { Pause(); }
        }
    }

    public void OnSuccess()
    {
        sm.OnSuccess();
        score++;
        Score.text = "Score: " + score.ToString();
    }

    public void OnFailure()
    {
        sm.OnFailure();
        score--;
        Score.text = "Score: "+ score.ToString();
    }

    public void Unpause()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pausePanel.SetActive(false);
        paused = false;
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pausePanel.SetActive(true);
        paused = true;
    }

    public void ShowInteractionCursor(string hintText) {
        cursor.color = new Color(.75f, .75f, .75f, .75f);
        cursorHint.text = hintText;
    }

    public void HideInteractionCursor() {
        cursor.color = new Color(.75f, .75f, .75f, .25f);
        cursorHint.text = "";
    }
}
