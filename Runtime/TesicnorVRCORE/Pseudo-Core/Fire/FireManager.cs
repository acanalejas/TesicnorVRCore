using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TesicFire
{
    public class FireManager : MonoBehaviour
    {
        #region SINGLETON
        private static FireManager instance;
        public static FireManager Instance { get { return instance; } }

        void CheckSingleton()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }
        #endregion
        #region PARAMETERS
        /// <summary>
        /// Es aleatorio el comienzo del fuego?
        /// </summary>
        [HideInInspector] public bool RandomInitialFire = false;

        /// <summary>
        /// Tiempo que tarda en empezar el fuego
        /// </summary>
        [HideInInspector] public float Delay = 4;

        #endregion

        #region FUNCTIONS
        private void Awake()
        {
            CheckSingleton();
        }
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FireManager), true)]
    public class FireManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            FireManager manager = (FireManager)target;

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Es el fuego inicial aleatorio?");
            manager.RandomInitialFire = EditorGUILayout.Toggle(manager.RandomInitialFire, EditorStyles.toggle);

            GUILayout.Space(10);

            GUILayout.Label("El tiempo que tarda en encenderse el primer fuego");
            manager.Delay = EditorGUILayout.FloatField(manager.Delay, EditorStyles.numberField);

            GUILayout.EndVertical();
        }
    }
#endif
}
