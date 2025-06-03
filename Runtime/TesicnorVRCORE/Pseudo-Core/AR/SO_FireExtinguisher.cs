using UnityEngine;

[CreateAssetMenu(fileName = "Fire Extinguisher", menuName = "New Fire Extinguisher")]
public class SO_FireExtinguisher : ScriptableObject
{

    public enum ExtinguisherType { CO2,Powder, Water, PotassiumAcetate, Halotron, FireproofMaterial, ABF }
    [Header("El tipo de extintor")]
    public ExtinguisherType type = ExtinguisherType.CO2;

    [Header("Sistema de partículas asociado a este extintor.")]
    public GameObject Particles;

    
    [Header("Duración del extintor. Suele estar cerca de los 21 segundos.")]
    public float Duration;
    
    [Header("Tamaño inicial de las partículas de humo. Suele funcionar bien con 5.")]
    public float ParticlesStartSize;
    
    [Header("Tamaño final de las partículas de humo cuando se acaba. Suele funcionar bien con 0.3.")]
    public float ParticlesFinalSize;
    
    [Header("Habilita o no el manómetro, sólo a nivel visual.")]
    public bool HasManometer;

    [Header("Etiquetas del extintor.")]
    public Texture LabelES;
    public Texture LabelEN;
    
    [Header("Sonido del extintor.")]
    public AudioClip Sound;
    
    [Header("Indica si el extintor mancha o salpica.")]
    public bool HasSplatter;
    
    [Header("Objeto que representa las manchas o salpicaduras del agente extintor.")]
    public GameObject Splatter;

    [Header("Tiempo que transcurre entre manchas / salpicaduras.")]
    public GameObject TimeBetweenSplatters;

    [Header("El pitch del audio source al usar este extintor")]
    public float pitch;

    [Header("El volumen del audio source al usar este extintor")]
    public float volume;
}