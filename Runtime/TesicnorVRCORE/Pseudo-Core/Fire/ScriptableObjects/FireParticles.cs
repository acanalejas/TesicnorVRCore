using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire Particles", menuName = "Fire Particles")]
public class FireParticles : ScriptableObject
{
    public ParticleSystem fire_System;
    public ParticleSystem smoke_System;
    public ParticleSystem sparks_System;
}
