#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

namespace ImaginaryReactor
{
    [InitializeOnLoad]
    class EditorInitialization
    {
        static readonly string k_CustomDefine = "IMAGINARY_REACTOR";

        static EditorInitialization()
        {
            var definesStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defines = definesStr.Split(';').ToList();
            var found = defines.Find(define => define.Equals(k_CustomDefine));
            if (found == null)
            {
                defines.Add(k_CustomDefine);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defines.ToArray()));
            }
        }
    }
}
#endif
