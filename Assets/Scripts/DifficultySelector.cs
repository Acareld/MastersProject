using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelector : MonoBehaviour
{
    private void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayVeryEasy()
    {
        PersistantManager.instance.SetFixedDifficulty(Difficulty.VERYEASY);
        PlayGame();
    }

    public void PlayEasy()
    {
        PersistantManager.instance.SetFixedDifficulty(Difficulty.EASY);
        PlayGame();
    }
    public void PlayMedium()
    {
        PersistantManager.instance.SetFixedDifficulty(Difficulty.MEDIUM);
        PlayGame();
    }
    public void PlayHard()
    {
        PersistantManager.instance.SetFixedDifficulty(Difficulty.HARD);
        PlayGame();
    }
    public void PlayVeryHard()
    {
        PersistantManager.instance.SetFixedDifficulty(Difficulty.VERYHARD);
        PlayGame();
    }
}
