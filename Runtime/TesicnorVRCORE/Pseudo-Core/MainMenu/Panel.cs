using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    [SerializeField]private Transform target = null;

    private float distanceThreshold = 3f;

    private float speed = 4;

    WaitForEndOfFrame frame = new WaitForEndOfFrame();

    private void Start()
    {
        StartCoroutine("update");
    }
    private IEnumerator update()
    {
        Debug.Log("STARTING UPDATE");
        while (true)
        {
            if (isMoving()) MoveSelf();
            yield return frame;
        }
    }

    public void MoveSelf()
    {
        gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, target.localPosition, Time.deltaTime * speed);
        if (hasArrived()) target = null;
    }

    public bool hasArrived()
    {
        if (Vector3.Distance(target.localPosition, gameObject.transform.localPosition) < distanceThreshold) return true;

        return false;
    }
    public bool isMoving()
    {
        if (target) return true;
        return false;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public Transform GetTarget()
    {
        return target;
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }

    public float GetSpeed()
    {
        return speed;
    }

}
