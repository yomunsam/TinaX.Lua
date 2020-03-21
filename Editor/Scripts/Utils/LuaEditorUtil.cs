using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using TinaX;
using TinaX.IO;
using UnityEditor;
using TinaXEditor.Lua.Const;

namespace TinaXEditor.Lua
{
    public static class LuaEditorUtil
    {
        //public static void CopyInternalLuaFilesToProjectAssets(string target_folder_path)
        //{
        //    if (!target_folder_path.StartsWith("Assets/"))
        //    {
        //        Debug.LogError("Target path invalid : " + target_folder_path);
        //        return;
        //    }

        //    string target_path = Path.Combine(Directory.GetCurrentDirectory(), target_folder_path);
        //    XDirectory.DeleteIfExists(target_path);
        //    FileUtil.CopyFileOrDirectory(LuaEditorConst.InternalLuaFilesFolderInPackages, target_folder_path);
        //    AssetDatabase.Refresh();
        //}
    }
}
