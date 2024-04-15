#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;


namespace ZBDNative
{
    public static class PostProcessor
    {
        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;
            Debug.Log("ZBD: Handling post process for iOS");

            var projPath = PBXProject.GetPBXProjectPath(buildPath);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.GetUnityMainTargetGuid();
            var fingerPrintGuid = proj.AddRemotePackageReferenceAtVersion("https://github.com/fingerprintjs/fingerprintjs-pro-ios", "2.1.5");
            proj.AddRemotePackageFrameworkToProject(targetGuid, "FingerprintPro", fingerPrintGuid, false);
            proj.WriteToFile(projPath);
        }
    }
}
#endif