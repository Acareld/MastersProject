using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private CanvasGroup canvas;

    private float fadeDuration = 2f;
    private float targetAlpha = 0f;

    private bool bShouldFade = false;

    private Coroutine textCoroutine;

    private static readonly string[] LoadingFrames =
    {
        "Loading",
        "Loading.",
        "Loading..",
        "Loading..."
    };

    void Awake()
    {
        canvas.alpha = 1f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        canvas.gameObject.SetActive(true);

        if (textCoroutine != null)
        {
            StopCoroutine(textCoroutine);
        }
        textCoroutine = StartCoroutine(TextCoroutine());
    }

    void Update()
    {
        if (bShouldFade)
        {
            UpdateFade();
        }


    }

    private IEnumerator TextCoroutine()
    {
        var delay = new WaitForSecondsRealtime(0.3f);
        int frame = 0;

        while (true)
        {
            text.SetText(LoadingFrames[frame]);

            frame = (frame + 1) % LoadingFrames.Length;

            yield return delay;
        }
    }

    public void StartFadeOut()
    {
        bShouldFade = true;
    }

    private void UpdateFade()
    {
        if (!canvas.gameObject.activeSelf)
            return;

        if (fadeDuration <= 0f)
        {
            canvas.alpha = targetAlpha;
        }
        else
        {
            canvas.alpha = Mathf.MoveTowards(canvas.alpha, targetAlpha, Time.unscaledDeltaTime / fadeDuration);
        }

        if (Mathf.Approximately(canvas.alpha, 0f))
        {
            if (textCoroutine != null) StopCoroutine(textCoroutine);
            canvas.gameObject.SetActive(false);

        }
    }
}
