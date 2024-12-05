using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterComponent 
{
    void ComponentUpdate();

    void ComponentFixedUpdate();
    
    void ComponentLateUpdate();
}
