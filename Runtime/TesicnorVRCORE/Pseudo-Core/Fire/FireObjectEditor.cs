using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TesicFire;

#if UNITY_EDITOR
[CustomEditor(typeof(FireObject), true)]
[CanEditMultipleObjects]
public class FireObjectEditor : Editor
{
    bool fireSettings = true;
    bool smokeSettings = false;
    bool otherVFX = false;

    void OnEnable()
    {
        FireObject manager = (FireObject)target;
        BoxCollider[] cols = manager.gameObject.GetComponents<BoxCollider>();
        foreach (BoxCollider col in cols) { if (col.isTrigger) manager.trigger = col; }
        if (!manager.trigger)
        {
            manager.trigger = manager.gameObject.AddComponent<BoxCollider>();
            manager.trigger.isTrigger = true;
        }
        //OnInspectorGUI();   
    }

    [InitializeOnEnterPlayMode]
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying) return;
        base.OnInspectorGUI();

        FireObject manager = (FireObject)target;

        manager.fire_MR = manager.GetComponent<MeshRenderer>();

        if (!manager.trigger)
        {
            BoxCollider[] cols = manager.gameObject.GetComponents<BoxCollider>();
            foreach (BoxCollider col in cols) { if (col.isTrigger) manager.trigger = col; }
            if (!manager.trigger)
            {
                manager.trigger = manager.gameObject.AddComponent<BoxCollider>();
                manager.trigger.isTrigger = true;
            }
        }

        #region Titulo
        GUILayout.BeginHorizontal();
        GUILayout.Label("FIRE OBJECT", EditorStyles.miniButtonMid, GUILayout.ExpandHeight(true));
        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(20);

        #region Botones
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Fire Settings", EditorStyles.miniButtonLeft))
        {
            fireSettings = true;
            smokeSettings = false;
            otherVFX = false;
        }
        if (GUILayout.Button("Smoke Settings", EditorStyles.miniButtonMid))
        {
            fireSettings = false;
            smokeSettings = true;
            otherVFX = false;
        }
        if (GUILayout.Button("Other VFX", EditorStyles.miniButtonRight))
        {
            fireSettings = false;
            smokeSettings = false;
            otherVFX = true;
        }

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(20);

        #region Zona de parámetros
        GUILayout.BeginVertical(EditorStyles.helpBox);

        #region Ajustes del fuego
        if (fireSettings)
        {
            GUILayout.Label("El prefab de las partículas de fuego");
            SerializedProperty firePrefab = serializedObject.FindProperty("fire_SystemPrefab");
            EditorGUILayout.PropertyField(firePrefab);

            GUILayout.Space(10);

            GUILayout.Label("Es el fuego inicial?", EditorStyles.boldLabel);
            manager.InitialFire = EditorGUILayout.Toggle(manager.InitialFire, EditorStyles.toggle);

            GUILayout.Space(10);

            GUILayout.Label("El tiempo que tarda en empezar el fuego", EditorStyles.boldLabel);
            manager.Delay = EditorGUILayout.FloatField(manager.Delay, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("La velocidad a la que se propaga el fuego");
            manager.FireSpeed = EditorGUILayout.FloatField(manager.FireSpeed, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("El tiempo que tarda en apagarse el fuego", EditorStyles.boldLabel);
            manager.MaxTimeToExtinguish = EditorGUILayout.FloatField(manager.MaxTimeToExtinguish, EditorStyles.miniTextField);
            manager.TimeToExtinguish = manager.MaxTimeToExtinguish;

            GUILayout.Space(10);

            GUILayout.Label("Solo para testear");
            EditorGUILayout.FloatField(manager.TimeToExtinguish, EditorStyles.label);

            GUILayout.Space(10);

            GUILayout.Label("El offset en el collider para la propagación del fuego", EditorStyles.boldLabel);
            manager.PropOffset = EditorGUILayout.Vector3Field("Propagation Offset", manager.PropOffset);

            GUILayout.Space(10);

            GUILayout.Label("La emissión de referencia", EditorStyles.boldLabel);
            manager.MaxEmission = EditorGUILayout.FloatField(manager.MaxEmission, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("El tamaño de referencia", EditorStyles.boldLabel);
            manager.MaxSize = EditorGUILayout.FloatField(manager.MaxSize, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("Evento que se lanza cuando se prende el objeto");
            SerializedProperty onFireStart = serializedObject.FindProperty("OnFireStarted");
            EditorGUILayout.PropertyField(onFireStart);

            GUILayout.Space(20);

            if (GUILayout.Button("Reset Fire", EditorStyles.miniButton))
            {
                Transform fire = manager.transform.Find("Fire_GO");
                if (!fire) fire = manager.transform.Find("Fire");
                if (fire)
                {
                    DestroyImmediate(fire.gameObject);
                }
            }

        }
        #region CheckIfHasFire
        Transform[] _children = manager.GetComponentsInChildren<Transform>();

        bool hasFire = false;
        foreach (Transform child in _children)
        {
            if (child != manager.transform)
            {
                if (child.name == "Fire_GO" && child.parent == manager.transform) hasFire = true;
            }
        }

        if (!hasFire)
        {
            GameObject fire = new GameObject("Fire_GO", typeof(ParticleSystem), typeof(MeshFilter), typeof(MeshRenderer), typeof(AudioSource));
            fire.transform.parent = manager.transform;
            fire.transform.localPosition = Vector3.zero;
            fire.transform.localRotation = Quaternion.Euler(Vector3.zero);
            fire.transform.localScale = Vector3.one;
            //fire.transform.forward = Vector3.up;
            fire.GetComponent<MeshRenderer>().enabled = false;
            manager.fire_System = fire.GetComponent<ParticleSystem>();
            manager.fire_GO = fire;
            manager.fire_SystemPrefab = AssetDatabase.LoadAssetAtPath("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Fire/ScriptableObjects/Fire Particles.asset", typeof(FireParticles)) as FireParticles;
            manager.fire_Source = fire.GetComponent<AudioSource>();
            manager.fire_Source.clip = AssetDatabase.LoadAssetAtPath("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Fire/Sounds/Big Fire.wav", typeof(AudioClip)) as AudioClip;
            manager.fire_Source.playOnAwake = false;

            CopyParticles(manager.fire_SystemPrefab.fire_System, manager.fire_System);
        }

        #endregion
        #endregion

        #region Ajustes del humo
        if (smokeSettings)
        {
            GUILayout.Label("Se usa el humo?", EditorStyles.boldLabel);
            manager.UsesSmoke = EditorGUILayout.Toggle(manager.UsesSmoke, EditorStyles.toggle);

            GUILayout.Space(10);

            GUILayout.Label("La densidad del humo", EditorStyles.boldLabel);
            manager.Density = EditorGUILayout.Slider(manager.Density, 1, 3);

            GUILayout.Space(10);

            GUILayout.Label("El color del humo", EditorStyles.boldLabel);
            manager.Smoke_Color = EditorGUILayout.ColorField(manager.Smoke_Color);

            GUILayout.Space(20);

            if (GUILayout.Button("Reset Smoke"))
            {
                Transform smoke = manager.transform.Find("Smoke");
                if (smoke) DestroyImmediate(smoke.gameObject);
            }
        }

        Transform[] children = manager.gameObject.GetComponentsInChildren<Transform>();
        bool smokeCreated = false;
        foreach (Transform child in children) { if (child.gameObject.name == "Smoke" && child.parent == manager.transform) smokeCreated = true; }

        if (!smokeCreated && manager.UsesSmoke)
        {
            GameObject smoke_go = new GameObject("Smoke", typeof(ParticleSystem), typeof(MeshFilter), typeof(MeshRenderer));
            smoke_go.transform.parent = manager.transform;
            smoke_go.transform.localRotation = Quaternion.Euler(Vector3.zero);
            smoke_go.transform.localScale = Vector3.one;
            //smoke_go.hideFlags = HideFlags.NotEditable;
            manager.smoke_System = smoke_go.GetComponent<ParticleSystem>();
            smoke_go.GetComponent<MeshRenderer>().enabled = false;

            CopyParticles(manager.fire_SystemPrefab.smoke_System, manager.smoke_System);
        }
        else if (smokeCreated && !manager.UsesSmoke)
        {
            Transform smoke_go = manager.transform.Find("Smoke");
            DestroyImmediate(smoke_go.gameObject);
        }
        #endregion

        #region Ajustes de VFX
        if (otherVFX)
        {
            GUILayout.Label("Se usan chispas?", EditorStyles.boldLabel);
            manager.UsesSparks = EditorGUILayout.Toggle(manager.UsesSparks, EditorStyles.toggle);

            GUILayout.Space(20);

            GUILayout.Button("Reset Sparks", EditorStyles.miniButton);
        }

        Transform sparks = manager.transform.Find("Sparks");
        if (sparks)
            if (sparks.parent != manager.transform) sparks = null;

        if (manager.UsesSparks && !sparks)
        {
            GameObject sparks_go = new GameObject("Sparks", typeof(ParticleSystem));
            sparks_go.transform.parent = manager.transform;
            sparks_go.transform.localPosition = Vector3.zero;
            sparks_go.transform.localRotation = Quaternion.identity;
            sparks_go.transform.localScale = Vector3.one;
            sparks_go.hideFlags = HideFlags.HideInHierarchy;
            manager.sparks_System = sparks_go.GetComponent<ParticleSystem>();

            CopyParticles(manager.fire_SystemPrefab.sparks_System, manager.sparks_System);
        }
        else if (!manager.UsesSparks && sparks)
        {
            DestroyImmediate(sparks.gameObject);
        }

        #endregion

        GUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
        #endregion

        //Para asegurarse de que tiene los colliders correctos
        Collider[] colliders = manager.GetComponents<Collider>();
        if (colliders.Length > 0)
        {
            bool hasTrigger = false;
            bool hasCol = false;
            foreach (Collider col in colliders)
            {
                if (col.isTrigger) hasTrigger = true;
                else hasCol = true;
            }
            if (!hasTrigger)
            {
                Collider trigger = manager.gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
            }
            if (!hasCol)
            {
                Collider col = manager.gameObject.AddComponent<BoxCollider>();
            }
        }
    }

    void CopyParticles(ParticleSystem ps, ParticleSystem _target)
    {
        FireObject manager = (FireObject)target;

        var main = _target.main;
        var main_p = ps.main;
        main.duration = main_p.duration;
        main.startLifetime = main_p.startLifetime;
        main.simulationSpace = main_p.simulationSpace;
        main.cullingMode = main_p.cullingMode;
        main.emitterVelocityMode = main_p.emitterVelocityMode;
        main.emitterVelocity = main_p.emitterVelocity;
        main.gravityModifier = main_p.gravityModifier;
        main.maxParticles = main_p.maxParticles;
        main.gravityModifierMultiplier = main_p.gravityModifierMultiplier;
        main.playOnAwake = main_p.playOnAwake;
        main.prewarm = main_p.prewarm;
        main.startRotation = main_p.startRotation;
        main.loop = main_p.loop;
        main.flipRotation = main_p.flipRotation;
        main.scalingMode = main_p.scalingMode;
        main.startColor = main_p.startColor;
        main.startDelay = main_p.startDelay;
        main.startSize = main_p.startSize;
        main.startSpeed = main_p.startSpeed;


        var shape = _target.shape;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.meshShapeType = ParticleSystemMeshShapeType.Edge;
        shape.meshRenderer = _target.GetComponent<MeshRenderer>();

        var col = _target.colorOverLifetime;
        var col_p = ps.colorOverLifetime;
        col.color = col_p.color;
        col.enabled = col_p.enabled;

        var emission = _target.emission;
        var emission_p = ps.emission;
        emission.burstCount = emission_p.burstCount;
        emission.rateMultiplier = emission_p.rateMultiplier;
        emission.rateOverTimeMultiplier = emission_p.rateOverTimeMultiplier;
        emission.rateOverDistanceMultiplier = emission_p.rateOverDistanceMultiplier;
        emission.rate = emission_p.rate;
        emission.rateOverDistance = emission_p.rateOverDistance;
        emission.rateOverTime = emission_p.rateOverTime;
        emission.type = emission_p.type;

        var sol = _target.sizeOverLifetime;
        var sol_p = ps.sizeOverLifetime;
        sol.size = sol_p.size;
        sol.sizeMultiplier = sol_p.sizeMultiplier;
        sol.separateAxes = sol_p.separateAxes;
        sol.x = sol_p.x; sol.y = sol_p.y; sol.z = sol_p.z;
        sol.xMultiplier = sol_p.zMultiplier; sol.yMultiplier = sol_p.yMultiplier; sol.zMultiplier = sol_p.zMultiplier;
        sol.enabled = sol_p.enabled;

        Renderer renderer = _target.GetComponent<Renderer>();
        Renderer renderer_p = ps.GetComponent<Renderer>();
        renderer.sharedMaterial = renderer_p.sharedMaterial;

        var tsa = _target.textureSheetAnimation;
        var tsa_p = ps.textureSheetAnimation;
        tsa.fps = tsa_p.fps;
        tsa.animation = tsa_p.animation;
        tsa.mode = tsa_p.mode;
        tsa.rowMode = tsa_p.rowMode;
        tsa.cycleCount = tsa_p.cycleCount;
        tsa.frameOverTime = tsa_p.frameOverTime;
        tsa.startFrame = tsa_p.startFrame;
        tsa.speedRange = tsa_p.speedRange;
        tsa.enabled = tsa_p.enabled;
        tsa.numTilesX = tsa_p.numTilesX;
        tsa.numTilesY = tsa_p.numTilesY;

        var noise = _target.noise;
        var noise_p = ps.noise;
        noise.damping = noise_p.damping;
        noise.frequency = noise_p.frequency;
        noise.octaveScale = noise_p.octaveScale;
        noise.octaveMultiplier = noise_p.octaveMultiplier;
        noise.quality = noise_p.quality;
        noise.enabled = noise_p.enabled;
        noise.strength = noise_p.strength;

        var vol = _target.velocityOverLifetime;
        var vol_p = ps.velocityOverLifetime;
        vol.orbitalX = vol_p.orbitalX;
        vol.orbitalY = vol_p.orbitalY;
        vol.orbitalZ = vol_p.orbitalZ;
        vol.space = vol_p.space;
        vol.x = vol_p.x; vol.y = vol_p.y; vol.z = vol_p.z;
        vol.radial = vol_p.radial;
        vol.orbitalOffsetX = vol_p.orbitalOffsetX;
        vol.orbitalOffsetY = vol_p.orbitalOffsetY;
        vol.orbitalOffsetZ = vol_p.orbitalOffsetZ;
        vol.speedModifier = vol_p.speedModifier;
        vol.enabled = vol_p.enabled;
    }
}
#endif