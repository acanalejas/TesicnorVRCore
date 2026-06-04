#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

[InitializeOnLoad]
public static class BuildProfileChangeDetector
{
    public static Action<string, PlatformType> onBuildProfileChanged;
    private static string lastProfile = string.Empty;

    public static PlatformType currentPlatform = PlatformType.Meta;

    static BuildProfileChangeDetector()
    {
        EditorApplication.update -= CheckBuildProfile;
        EditorApplication.update += CheckBuildProfile;

        onBuildProfileChanged = null;
        onBuildProfileChanged += (s, type) =>
        {
            lastProfile = s;
            currentPlatform = type;
        };
        CheckBuildProfile();
    }

    static void CheckBuildProfile()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) 
            return;
        try
        {
            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
            if (activeProfile == null) return;

            string profileName = activeProfile.name;
            if (bProfileChanged(profileName))
            {
                CheckBuildProfileName(profileName);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Excepcion al recoger el build profile activo : " + e.Message);
        }
    }

    static void CheckBuildProfileName(string _profile)
    {
        foreach (var p in Enum.GetValues(typeof(PlatformType)))
        {
            if (_profile.Contains(p.ToString()))
            {
                onBuildProfileChanged.Invoke(_profile, (PlatformType) p);
            }
        }
    }

    static bool bProfileChanged(string _currentProfile)
    {
        return _currentProfile != lastProfile;
    }
}
#endif
