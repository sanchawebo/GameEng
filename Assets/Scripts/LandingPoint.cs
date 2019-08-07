using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LandingPoint : MonoBehaviour
{
    private ParticleSystem splash;

    void Start()
    {
        splash = GetComponent<ParticleSystem>();
    }
    
    public void Splash ()
    {
        splash.Emit(100);
    }
}
