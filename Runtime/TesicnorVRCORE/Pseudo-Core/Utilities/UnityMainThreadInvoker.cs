using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnityMainThreadInvoker : MonoBehaviour
{
    #region SINGLETON
    private static UnityMainThreadInvoker instance;
    public static UnityMainThreadInvoker Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion

    #region PARAMETERS
    private static Queue<Action> executionQueue = new Queue<Action>();
    #endregion

    #region FUNCTIONS
    public void Update()
    {
        lock (executionQueue)
        {
            while(executionQueue.Count > 0)
            {
                executionQueue?.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    public IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
    #endregion
}
