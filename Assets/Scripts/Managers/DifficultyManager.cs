using System.Collections.Generic;
using UnityEngine;



public class DifficultyManager : MonoBehaviour
{
    [SerializeField] List<DifficultyState> difficultyStates;

    public static DifficultyManager instance;


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
    

    public DifficultyState GetDifficultySettings(Difficulty difficulty)
    {
        foreach(DifficultyState state in difficultyStates)
        {
            if(state != null && state.difficulty == difficulty)
            {
                return state;
            }
        }
        return null;
    }
}
