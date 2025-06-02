using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerationData : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
