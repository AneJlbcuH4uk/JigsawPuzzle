using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeStateWhenActive : MonoBehaviour
{
    [SerializeField] private InputControl IC;
    [SerializeField] private int active_state;
    [SerializeField] private int inactive_state;
    private void OnEnable() 
    {
        IC.ChangeState(active_state);
    }

    private void OnDisable()
    {
        IC.ChangeState(inactive_state);
    }

}
