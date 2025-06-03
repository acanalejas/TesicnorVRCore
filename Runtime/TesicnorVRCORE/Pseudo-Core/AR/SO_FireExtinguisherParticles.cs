using UnityEngine;

[CreateAssetMenu(fileName = "Fire Extinguisher Particles", menuName = "New Fire Extinguisher Particles")]
public class SO_FireExtinguisherParticles : ScriptableObject
{
    [Header("Sistema de partículas del extintor.")]
    public ParticleSystem ParticleSystem;
}