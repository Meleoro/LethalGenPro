using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Parameters")] 
    private List<ICharacterComponent> characterComponents = new List<ICharacterComponent>();


    private void Start()
    {
        characterComponents = GetComponents<ICharacterComponent>().ToList();
    }


    private void Update()
    {
        // We execute the updates of all the components attached 
        for (int i = 0; i < characterComponents.Count; i++)
        {
            characterComponents[i].ComponentUpdate();
        }
    }
    
    private void FixedUpdate()
    {
        // We execute the fixed updates of all the components attached 
        for (int i = 0; i < characterComponents.Count; i++)
        {
            characterComponents[i].ComponentFixedUpdate();
        }
    }
    
    private void LateUpdate()
    {
        // We execute the late updates of all the components attached 
        for (int i = 0; i < characterComponents.Count; i++)
        {
            characterComponents[i].ComponentLateUpdate();
        }
    }
}
