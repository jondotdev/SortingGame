using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Transform interactionCursor;
    public Text Score;
    public Cinemachine.CinemachineVirtualCamera PlayerFollowCamera;
    public GameObject playerPrefab;
    public SpawnLocation spawnLocation = null;

    public bool paused = false;

    public int score = 0;

    private Image cursor;
    private Text cursorHint;

    private SoundManager sm;

    private void Start()
    {
        if(spawnLocation == null)
        {
            SpawnPlayer(Vector3.zero, Quaternion.identity);
        }
        else
        {
            SpawnPlayer(spawnLocation);
        }
        cursor = interactionCursor.GetComponent<Image>();
        cursorHint = interactionCursor.Find("CursorHint").GetComponent<Text>();
        sm = transform.GetComponent<SoundManager>();
        Unpause();
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

    public void SpawnPlayer(Vector3 position, Quaternion rotation)
    {
        GameObject go;
        go = Instantiate(playerPrefab, position, rotation);
        Player p = go.transform.GetComponent<Player>();
        InputHandler h = go.transform.GetComponent<InputHandler>();
        p.gm = this;
        h.gm = this;
        PlayerFollowCamera.Follow = go.transform.Find("PlayerCameraRoot");
    }

    public void SpawnPlayer(SpawnLocation s)
    {
        SpawnPlayer(s.transform.position, s.transform.rotation);
    }

    public void KillPlayer(GameObject player)
    {
        Destroy(player);
        if (spawnLocation == null)
        {
            SpawnPlayer(Vector3.zero, Quaternion.identity);
        }
        else
        {
            SpawnPlayer(spawnLocation);
        }
    }

    public void ShowInteractionCursor(string hintText)
    {
        cursor.color = new Color(.75f, .75f, .75f, .75f);
        cursorHint.text = hintText;
    }

    public void HideInteractionCursor()
    {
        cursor.color = new Color(.75f, .75f, .75f, .25f);
        cursorHint.text = "";
    }
}
