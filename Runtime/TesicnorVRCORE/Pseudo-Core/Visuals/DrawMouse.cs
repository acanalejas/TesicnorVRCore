using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMouse : MonoBehaviour
{
    public Texture2D toPaint;
    public Texture2D normal;
    public Vector2 size;
    public float radius = 0.7f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider)
                {
                    
                    if(hit.collider.gameObject)
                    DrawOverMaterial.DrawOverMaterialByCollisionPoint(hit.point, toPaint, hit.collider.gameObject, hit.textureCoord2, radius);
                }
            }
        }
    }
}
