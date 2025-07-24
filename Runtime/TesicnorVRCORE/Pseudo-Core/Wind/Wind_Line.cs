using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Line : Anchor
{
    #region PARAMETERS
    [Header("El line renderer que hace de cable")]
    [SerializeField] protected LineRenderer lineRenderer;

    [Header("Opcional solo por seguridad, el material del line renderer")]
    [SerializeField] protected Material lineMaterial;

    [Header("El punto de anclaje en el anclaje de la linea")]
    [SerializeField] protected Transform anchorageLineAnchor;

    [Header("El punto de anclaje en el arnés de la linea")]
    [SerializeField] protected Transform harnessLineAnchor;
    #endregion

    #region METHODS
    public override void Awake()
    {
        base.Awake();

        if (!lineRenderer) lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        if (lineMaterial) lineRenderer.material = lineMaterial;
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(nameof(CustomUpdate));
    }


    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    protected virtual IEnumerator CustomUpdate()
    {
        while (true)
        {
            SetLineVisuals();
            yield return frame;
        }
    }

    protected virtual void SetLineVisuals()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, anchorageLineAnchor.position);
        lineRenderer.SetPosition(1, harnessLineAnchor.position);
    }
    #endregion
}
