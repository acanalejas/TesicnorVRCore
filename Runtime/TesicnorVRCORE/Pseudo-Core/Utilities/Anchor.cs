using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Anchor : VRCollider, AnchorInterface
{
    #region PARAMETERS
    [Header("El jugador al que ancla")]
    [SerializeField] protected GameObject Player;

    [Header("El objeto del Warning")]
    [SerializeField] protected WarningObject Warning;

    [Header("La tag a comprobar para ver si es un punto de anclaje correcto")]
    [SerializeField] protected string CorrectAnchorageTag = "CorrectAnchorage";

    [Header("La distancia que permite alejarse el anclaje antes del warning")]
    [SerializeField] protected float MaxDistance = 1.5f;

    [Header("El margen de error a la hora de medir la distancia")]
    [SerializeField] protected float DistanceThreshold = 0.1f;
    #endregion

    #region METHODS
    protected virtual void Start()
    {
        onTargetReached.AddListener(CheckAnchorage);
        onTargetReached.AddListener(AnchorIt);
        
        //this.SetGrabbable(false);

        if(this.target)
            this.target.canBeCanceled = true;
        this.simulateOnDrop = false;

        if (this.targets.Length > 0)
            foreach (var tar in targets) tar.canBeCanceled = true;
    }

    protected virtual void CheckAnchorage(GameObject anchorage)
    {
        if (anchorage.tag == CorrectAnchorageTag)
        {
            Warning.DisableWarning();
        }
        else
        {
            Warning.EnableWarning();
        }
    }

    public bool IsAnchored()
    {
        bool result = false;
        if(this.target != null)
            result = this.target.conditionCompleted;
        if(this.targets.Length > 0)
        {
            foreach (var tar in targets) if (tar != null && tar.conditionCompleted) return true;
        }
        return result;
    }

    public void AnchorIt(GameObject _anchor)
    {
    }

    public void ReleaseIt(GameObject _anchor)
    {
    }

    public void CheckDistance()
    {
        if (!Player) return;

        Vector3 distance = Player.transform.position - this.transform.position;
        Vector3 proyection = new Vector3(distance.x, 0, distance.z);

        float distanceProyected = proyection.magnitude;

        if (distanceProyected > MaxDistance + DistanceThreshold) Warning.EnableWarning();
        else if (distanceProyected < MaxDistance - DistanceThreshold) Warning.DisableWarning();
    }

    public void EnableWarning()
    {
        Warning.EnableWarning();
    }

    public void DisableWarning()
    {
        Warning.DisableWarning();
    }
    #endregion
}
