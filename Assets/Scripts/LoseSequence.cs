using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Handles both game-end outcomes. Lose: explosion -> smoke -> panel (red text +
// red time-left). Win: panel shown immediately (green text + green time-left, no
// explosion/smoke). Same panel and buttons reused for both - only text/color change.
// Triggered automatically by GameManager.OnGameEnded.
public class LoseSequence : MonoBehaviour
{
    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject smokeEffect;
    [SerializeField] GameObject bombObject; // the whole device - hidden the moment it explodes
    [SerializeField] GameObject losePanel; // shared end-of-game panel, used for win too
    [SerializeField] TMP_Text resultText;  // "You lost !" / "You won !"
    [SerializeField] TMP_Text panelTimerText; // the time-left readout on the panel
    [SerializeField] Button restartButton;
    [SerializeField] Button quitButton;
    [SerializeField] float explosionToSmokeDelay = 1.5f;
    [SerializeField] float smokeToPanelDelay = 1.5f;
    [SerializeField] string loseMessage = "You lost !";
    [SerializeField] string winMessage = "You won !";
    [SerializeField] Color loseColor = Color.red;
    [SerializeField] Color winColor = Color.green;

    void Start()
    {
        GameManager.Instance.OnGameEnded += HandleGameEnded;

        if (explosionEffect != null) explosionEffect.SetActive(false);
        if (smokeEffect != null) smokeEffect.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        restartButton.onClick.AddListener(OnRestartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
    }

    void HandleGameEnded(GameManager.RunState state)
    {
        if (state == GameManager.RunState.Lost)
            StartCoroutine(PlayLoseSequence());
        else if (state == GameManager.RunState.Won)
            ShowResult(won: true);
    }

    IEnumerator PlayLoseSequence()
    {
        if (explosionEffect != null) explosionEffect.SetActive(true);

        yield return new WaitForSeconds(explosionToSmokeDelay);
        if (bombObject != null) bombObject.SetActive(false);
        if (smokeEffect != null) smokeEffect.SetActive(true);

        yield return new WaitForSeconds(smokeToPanelDelay);
        ShowResult(won: false);
    }

    void ShowResult(bool won)
    {
        Color color = won ? winColor : loseColor;

        if (resultText != null)
        {
            resultText.text = won ? winMessage : loseMessage;
            resultText.color = color;
        }

        if (panelTimerText != null)
        {
            panelTimerText.text = GameManager.FormatTime(GameManager.Instance.TimeRemaining);
            panelTimerText.color = color;
        }

        if (losePanel != null) losePanel.SetActive(true);
    }

    void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
