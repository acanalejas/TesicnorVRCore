using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Line : Anchor
{
    #region PARAMETERS
    [Header("El line renderer que hace de cable")]
    [SerializeField] protected LineRenderer lineRenderer;

    [Header("La anchura del line renderer")]
    [SerializeField] protected float LineWidth = 0.012f;

    [Header("La cantidad de puntos necesarios para hacer la linea")]
    [SerializeField] protected int LinePoints = 10;

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
        if (!lineRenderer) lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        if (lineMaterial) lineRenderer.material = lineMaterial;

        lineRenderer.positionCount = LinePoints + 1;

        base.Awake();
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
            yield return frame;
            SetLineVisuals();
        }
    }

    protected virtual void SetLineVisuals()
    {
        //lineRenderer.positionCount = 2;
        //lineRenderer.SetPosition(0, anchorageLineAnchor.position);
        //lineRenderer.SetPosition(1, harnessLineAnchor.position);
        MakeLineCurvature();

        lineRenderer.endWidth = LineWidth;
        lineRenderer.startWidth = LineWidth;
    }

    protected virtual void MakeLineCurvature()
    {
        Vector3 init = harnessLineAnchor.position;
        Vector3 final = anchorageLineAnchor.position;

        float currentDistance = Vector3.Distance(init, final); //Mathf.Clamp(Vector3.Distance(init, final), 0, MaxDistance);
        float distanceDiff = MaxDistance - Mathf.Clamp(currentDistance, 0, MaxDistance);

        Vector3 distance = final - init;
        Vector3 direction = distance.normalized;

        Vector3 halfPoint = init + distance.magnitude / 2 * direction + new Vector3(0, -distanceDiff, 0);

        for(int ratio = 0; ratio <= LinePoints; ratio++)
        {
            Vector3 tangent1 = Vector3.Lerp(init, halfPoint, ratio / LinePoints);
            Vector3 tangent2 = Vector3.Lerp(halfPoint, final, ratio / LinePoints);

            Vector3 point = Vector3.Lerp(tangent1, tangent2, ratio / LinePoints);

            lineRenderer.SetPosition(ratio, point);
        }
    }
    #endregion
}
