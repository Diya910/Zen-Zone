using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class AttackBehavour : MonoBehaviour
{
    public Animator animController1; 

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            animController1.SetBool("Hitting", true);
        }
          if (Input.GetMouseButtonUp(0)) {
            animController1.SetBool("Hitting", false);
        }
    }
}
