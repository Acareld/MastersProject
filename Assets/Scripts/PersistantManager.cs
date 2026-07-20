using UnityEditor;
using UnityEngine;

public class PersistantManager : MonoBehaviour
{
    public static PersistantManager instance;

    private Difficulty selectedDifficulty;
    private bool bUseFixedDifficulty = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public bool UseFixedDifficulty()
    {
        return bUseFixedDifficulty;
    }

    public Difficulty GetFixedDifficulty()
    {
        return selectedDifficulty;
    }

    public void SetFixedDifficulty(Difficulty difficulty)
    {
        bUseFixedDifficulty = true;
        selectedDifficulty = difficulty;
    }

}
