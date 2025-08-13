using UnityEngine;

public class AdditionalAudioClip : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public AudioClip GetClip() => clip;

}
