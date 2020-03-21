using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using UnityEngine;

namespace TinaX.Lua.Internal
{
    public class LuaConfig : ScriptableObject
    {
        public bool EnableLua = true;

        public string LuaFileExtensionName = ".lua.txt";


        public string FrameworkInternalLuaFolderLoadPath = "Assets/TinaX/Lua"; //内置资源文件夹在加载时的根目录路径

        public string EntryLuaFileLoadPath; //入口文件地址
    }
}
