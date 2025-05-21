using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelAnchorManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("El GameObject que sirve de anclaje")]
    [SerializeField] Transform Anchor;

    [Header("El GameObject del panel que se tiene que anclar")]
    [SerializeField] Transform Panel;

    [Header("La velocidad con la que sigue al anclaje")]
    [SerializeField] float Speed = 1;
    #endregion

    #region METHODS
    private void Start()
    {
        StartCoroutine(nameof(update));
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    IEnumerator update()
    {
        while (true)
        {
            if(Panel != null && Anchor != null)
            {
                Panel.position = Vector3.Lerp(Panel.position, Anchor.position, Time.deltaTime * Speed);
                Panel.rotation = Quaternion.Lerp(Panel.rotation, Anchor.rotation, Time.deltaTime * Speed);
            }
            yield return frame;
        }
    }

    public void SetPanel(GameObject _panel)
    {
        Panel = _panel.transform;
    }
    public void SetPanel(Transform _panel)
    {
        Panel = _panel;
    }

    public void SetAnchor(GameObject _anchor)
    {
        Anchor = _anchor.transform;
    }
    public void SetAcnhor(Transform _anchor)
    {
        Anchor = _anchor;
    }
    #endregion
}
