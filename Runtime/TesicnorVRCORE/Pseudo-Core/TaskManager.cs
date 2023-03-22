using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class TaskManager : MonoBehaviour
{
    #region SINGLETON
    public static TaskManager Instance { get { return instance; } }
    private static TaskManager instance;

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    #endregion
    #region PARAMETERS
    /// <summary>
    /// Lista total de las tareas a realizar
    /// </summary>
    [Header("Lista total de las tareas a realizar")]
    [HideInInspector]public VR_Task[] totalTasks;

    /// <summary>
    /// (OPCIONAL)La tarea que se queda ejecutandose cuando ya se han terminado todas las demas
    /// </summary>
    [Header("(OPCIONAL)La tarea que se queda ejecutandose cuando ya se han terminado todas las demás")]
    [HideInInspector] public VR_Task idleTask;

    /// <summary>
    /// Si las tareas de la lista totalTasks están en orden
    /// </summary>
    [Header("Si las tareas de la lista totalTasks están en orden")]
    [HideInInspector] public bool isInOrder = true;

    /// <summary>
    /// Lista de índices de la lista totalTasks en el orden que se tienen que ejecutar
    /// </summary>
    [Header("Rellenar solo si NO están en orden")]
    [Tooltip("Lista de índices de la lista totalTasks en el orden que se tienen que ejecutar")]
    [HideInInspector] public int[] orderOfIndexes;

    /// <summary>
    /// Accion usada cuando se verifica que todas las tareas han sido completadas
    /// </summary>
    [HideInInspector] public UnityEvent onAllTasksCompleted;

    /// <summary>
    /// Tiene dos posibles usos:
    /// 1. Si las tareas están en orden, es el índice de la tarea actual
    /// 2. Si están desordenadas, es el indice del array orderOfIndexes, de la que sería la tarea actual
    /// </summary>
    [HideInInspector]public int activeTask = 0;

    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        CheckSingleton();
        SetInitialTask();

        onAllTasksCompleted.AddListener(SetFinalTask);
    }

    /// <summary>
    /// Setea todo para la primera tarea
    /// </summary>
    private void SetInitialTask()
    {
        foreach(VR_Task task in totalTasks)
        {
            task.enabled = false;
        }

        int initialIndex = 0;
        if (!isInOrder) initialIndex = orderOfIndexes[0];

        totalTasks[initialIndex].enabled = true;
    }

    private void SetFinalTask()
    {
        totalTasks[activeTask].enabled = false;
        totalTasks[activeTask].StopAllCoroutines();

        if(idleTask) idleTask.enabled = true;
    }
    /// <summary>
    /// Va a la siguiente tarea, como están en un orden específico, avanza a la siguiente
    /// </summary>
    public void GoToNextTask()
    {
        //Desactiva y para las coroutines, simplemente por seguridad
        if (isInOrder)
        {
            totalTasks[activeTask].enabled = false;
            totalTasks[activeTask].StopAllCoroutines();
        }
        else
        {
            totalTasks[orderOfIndexes[activeTask]].enabled = false;
            totalTasks[orderOfIndexes[activeTask]].StopAllCoroutines();
        }

        activeTask++;
        if (!isInOrder) totalTasks[orderOfIndexes[activeTask]].enabled = true;
        else totalTasks[activeTask].enabled = true;
    }

    /// <summary>
    /// Va a la tarea anterior, como están en orden, simplemente retrocede
    /// </summary>
    public void GoToLastTask()
    {
        if (isInOrder)
        {
            totalTasks[activeTask].enabled = false;
            totalTasks[activeTask].StopAllCoroutines();
        }
        else
        {
            totalTasks[orderOfIndexes[activeTask]].enabled = false;
            totalTasks[orderOfIndexes[activeTask]].StopAllCoroutines();
        }

        activeTask--;
        if (!isInOrder) totalTasks[orderOfIndexes[activeTask]].enabled = true;
        else totalTasks[activeTask].enabled = true;
    }

    public void GoToTask(VR_Task task)
    {
        bool exists = totalTasks.Contains(task);
        if (!exists) return;

        totalTasks[activeTask].StopAllCoroutines();
        totalTasks[activeTask].enabled = false;

        for(int i = 0; i < totalTasks.Length; i++)
        {
            if (totalTasks[i] == task) activeTask = i;
        }
        totalTasks[activeTask].enabled = true;
    }

    public void GoToTask(int index)
    {
        if (index > totalTasks.Length || index < 0) return;

        totalTasks[activeTask].enabled = false;
        totalTasks[activeTask].StopAllCoroutines();

        activeTask = index;

        totalTasks[activeTask].enabled = true;
    }

    /// <summary>
    /// Bool para checkear si todas las tareas estan completadas
    /// </summary>
    /// <returns></returns>
    public bool AllTasksCompleted()
    {
        foreach(VR_Task task in totalTasks)
        {
            if (!task.taskCompleted) return false;
        }
        return true;
    }

    /// <summary>
    /// Método para comprobar si están todas las misiones completadas.
    /// Se comprobará cada vez que se complete una tarea;
    /// </summary>
    public void CheckAllTasksCompleted()
    {
        if (AllTasksCompleted())
        {
            onAllTasksCompleted?.Invoke();
        }
    }
    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(TaskManager), true)]
public class TaskManagerEditor : Editor
{
    [HideInInspector] public TaskManager manager;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        manager = target as TaskManager;

        SerializedProperty totalTasks = serializedObject.FindProperty("totalTasks");
        EditorGUILayout.PropertyField(totalTasks, new GUIContent("List of tasks"));

        SerializedProperty idleTask = serializedObject.FindProperty("idleTask");
        EditorGUILayout.PropertyField(idleTask, new GUIContent("OPCIONAL : Idle task"));

        GUILayout.Label("Las tareas de la lista están en orden?", EditorStyles.boldLabel);
        manager.isInOrder = GUILayout.Toggle(manager.isInOrder, "Is in order?");

        GUILayout.Space(10);

        SerializedProperty onAllTasksCompleted = serializedObject.FindProperty("onAllTasksCompleted");
        EditorGUILayout.PropertyField(onAllTasksCompleted, new GUIContent("On all tasks completed"));

        if (!manager.isInOrder)
        {
            SerializedProperty orderOfIndexes = serializedObject.FindProperty("orderOfIndexes");
            EditorGUILayout.PropertyField(orderOfIndexes, new GUIContent("Order of indexes"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
