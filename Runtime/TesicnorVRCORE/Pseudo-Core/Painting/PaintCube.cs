using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintCube : MonoBehaviour
{
    #region PARAMETERS
    public string paintTag = "Floor";

    public bool destroyOnPaint = true;
    #endregion
    #region METHODS
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == paintTag)
        {
            GetUVCoordinates(other);
            if (destroyOnPaint) Destroy(this.gameObject);
        }
    }
    public Vector2 GetUVCoordinates(Collider other)
    {
        int lastLayer = other.gameObject.layer;
        other.gameObject.layer = 20;

        RaycastHit hit = new RaycastHit();
        Vector3 direction = this.transform.forward;
        Ray ray = new Ray(this.transform.position, direction);

        //this.GetComponent<BoxCollider>().enabled = false;

        bool coll = Physics.Raycast(ray, out hit, this.GetComponent<BoxCollider>().bounds.extents.x * 1.5f);

        Debug.Log(other.gameObject.name);
        if (coll)
            Debug.Log(hit.collider.GetType().Name + " " + hit.collider.gameObject.name);

        Vector2 result = new Vector2(hit.textureCoord.x, hit.textureCoord.y);

        other.gameObject.layer = lastLayer;

        if (coll && hit.collider.gameObject.name == other.name)
            ObjectPainter.Instance.Paint(result, hit.collider.gameObject);

        return result;
    }
    #endregion
}
