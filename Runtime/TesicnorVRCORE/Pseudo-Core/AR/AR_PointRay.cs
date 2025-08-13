using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AR_PointRay : MonoBehaviour
{
    #region PARAMETERS
    //Managers for AR raycasting and positioning
    ARRaycastManager ARRM;
    ARPlaneManager ARPM;

    //Origin of the GameObject
    GameObject Origin;

    //List for storing the hits
    List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    //Objeto a spawnear, cuidado con las referencias que no se limpian hasta cerrar la app
    public static SO_ARItem spawnObject;
    #endregion

    #region METHODS
    #region UNITY METHODS

    private void OnDestroy()
    {
        spawnObject = null;
    }
    private void Awake()
    {
        if (ARRM == null) ARRM = FindFirstObjectByType<ARRaycastManager>();
        if (ARPM == null) ARPM = FindFirstObjectByType<ARPlaneManager>();
    }

    public void SetRaycastOrigin(GameObject _origin)
    {
        Origin = _origin;
    }

    public Vector3 ARRaycast(Vector3 _direction, GameObject _origin = null)
    {
        if (_origin) Origin = _origin;

        Vector3 result = Vector3.zero;

        Ray _ray = new Ray(Origin.transform.position, _direction);

        ARRM.Raycast(_ray, Hits);

        foreach(var Hit in Hits)
        {
            Pose pose = Hit.pose;
            result = pose.position;
        }

        return result;
    }

    public GameObject ARSpawnObject(GameObject _prefab, Vector3 _direction, GameObject _origin = null)
    {
        GameObject result = null;

        Vector3 _position = ARRaycast(_direction, _origin);

        result = GameObject.Instantiate(_prefab, _position, Quaternion.identity);

        return result;
    }

    public GameObject ARSpawnObject(GameObject _prefab, Vector3 _direction, Vector3 _position)
    {
        GameObject result = null;

        result = GameObject.Instantiate(_prefab, _position, Quaternion.identity);

        return result;
    }

    public GameObject ARSpawnObject(GameObject _prefab, Vector3 _position, Quaternion _rotation)
    {
        return GameObject.Instantiate(_prefab, _position, _rotation);
    }

    public GameObject ARSpawnObject(Vector3 _position)
    {
        return GameObject.Instantiate(spawnObject.prefab, _position, Quaternion.identity);
    }

    public bool alreadySpawned = false;

    public GameObject ARSpawnObject(Vector3 _position, Quaternion _rotation)
    {
        alreadySpawned = true;
        return GameObject.Instantiate(spawnObject.prefab, _position, _rotation);
    }

    public GameObject ARSpawnObject(GameObject _prefab, Vector3 _direction, Quaternion _rotation, GameObject _origin = null)
    {
        GameObject result = null;

        Vector3 _position = ARRaycast(_direction, _origin);

        result = GameObject.Instantiate(_prefab, _position, _rotation);

        return result;
    }

    public GameObject ARSpawnObject(GameObject _prefab, Vector3 _direction, Transform _parent, GameObject _origin = null)
    {
        GameObject result = null;

        result = GameObject.Instantiate(_prefab,_parent);

        return result;
    }

    public GameObject ARSpawnObject(Vector3 _direction, GameObject _origin = null)
    {
        Vector3 _position = ARRaycast(_direction, _origin);

        return GameObject.Instantiate(spawnObject.prefab, _position, Quaternion.identity);
    }

    public GameObject ARSpawnObject(Vector3 _direction, Quaternion _rotation, GameObject _origin = null)
    {
        Vector3 _position = ARRaycast(_direction, _origin);

        return GameObject.Instantiate(spawnObject.prefab, _position, _rotation);
    }
    #endregion
    #endregion
}
