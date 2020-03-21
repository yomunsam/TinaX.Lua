using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.Lua.Const;
using TinaX.Lua.Internal;
using TinaXEditor.Lua.Const;
using UnityEditor;
using UnityEngine;

namespace TinaXEditor.Lua.Internal
{
    public static class LuaProjectSetting
    {
        private static bool mDataRefreshed = false;
        private static LuaConfig mConfig;
        private static string[] _extNames;
        

        [SettingsProvider]
        public static SettingsProvider XRuntimeSetting()
        {
            return new SettingsProvider(LuaEditorConst.ProjectSetting_Node, SettingsScope.Project, new string[] { "Nekonya", "TinaX", "Lua", "TinaX.Lua", "xLua" })
            {
                label = "X Lua",
                guiHandler = (searchContent) =>
                {
                    if (!mDataRefreshed) refreshData();
                    if (mConfig == null)
                    {
                        GUILayout.Space(20);
                        GUILayout.Label(I18Ns.NoConfig);
                        if (GUILayout.Button(I18Ns.BtnCreateConfigFile, Styles.style_btn_normal, GUILayout.MaxWidth(120)))
                        {
                            mConfig = XConfig.CreateConfigIfNotExists<LuaConfig>(LuaConst.ConfigPath_Resources, AssetLoadType.Resources);
                            refreshData();
                        }
                    }
                    else
                    {
                        GUILayout.Space(20);

                        //Enable Lua
                        mConfig.EnableLua = EditorGUILayout.ToggleLeft(I18Ns.EnableLua, mConfig.EnableLua);


                        //Entry File
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(I18Ns.EntryFilePath);
                        EditorGUILayout.BeginHorizontal();
                        mConfig.EntryLuaFileLoadPath = EditorGUILayout.TextField(mConfig.EntryLuaFileLoadPath);
                        if (GUILayout.Button("Select",Styles.style_btn_normal, GUILayout.Width(55)))
                        {
                            var path = EditorUtility.OpenFilePanel("Select Lua Entry File", "Assets/", "");
                            if (!path.IsNullOrEmpty())
                            {
                                var root_path = Directory.GetCurrentDirectory().Replace("\\", "/");
                                if (path.StartsWith(root_path))
                                {
                                    path = path.Substring(root_path.Length + 1, path.Length - root_path.Length - 1);
                                    path = path.Replace("\\", "/");
                                    mConfig.EntryLuaFileLoadPath = path;
                                }
                                else
                                    Debug.LogError("Invalid Path: " + path);
                            }
                            
                        }
                        EditorGUILayout.EndHorizontal();


                        //Framework Lua Folder Load Path 
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(I18Ns.FrameworkInternalLuaFolderLoadPath);
                        EditorGUILayout.BeginHorizontal();
                        mConfig.FrameworkInternalLuaFolderLoadPath = EditorGUILayout.TextField(mConfig.FrameworkInternalLuaFolderLoadPath);
                        EditorGUILayout.EndHorizontal();


                        //extension
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(I18Ns.LuaExtension, GUILayout.MaxWidth(120));
                        mConfig.LuaFileExtensionName = EditorGUILayout.TextField(mConfig.LuaFileExtensionName, GUILayout.MaxWidth(120));
                        if(mConfig.LuaFileExtensionName != ".lua.txt")
                        {
                            if (GUILayout.Button(".lua.txt", Styles.style_btn_normal, GUILayout.Width(75)))
                                mConfig.LuaFileExtensionName = ".lua.txt";
                        }
                        if (mConfig.LuaFileExtensionName != ".lua.bytes")
                        {
                            if (GUILayout.Button(".lua.bytes", Styles.style_btn_normal, GUILayout.Width(75)))
                                mConfig.LuaFileExtensionName = ".lua.bytes";
                        }
                        if (mConfig.LuaFileExtensionName != ".txt")
                        {
                            if (GUILayout.Button(".txt", Styles.style_btn_normal, GUILayout.Width(75)))
                                mConfig.LuaFileExtensionName = ".txt";
                        }

                        EditorGUILayout.EndHorizontal();
                        
                    }
                },
                deactivateHandler = () =>
                {
                    if (mConfig != null)
                    {
                        if (!mConfig.LuaFileExtensionName.IsNullOrEmpty())
                        {
                            mConfig.LuaFileExtensionName = mConfig.LuaFileExtensionName.ToLower();
                            if (!mConfig.LuaFileExtensionName.StartsWith("."))
                                mConfig.LuaFileExtensionName = "." + mConfig.LuaFileExtensionName;
                        }
                        EditorUtility.SetDirty(mConfig);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            };
        }



        private static void refreshData()
        {
            mConfig = XConfig.GetConfig<LuaConfig>(LuaConst.ConfigPath_Resources, AssetLoadType.Resources, false);



            mDataRefreshed = true;
        }


        static class Styles
        {
            private static GUIStyle _style_btn_normal; //字体比原版稍微大一号
            public static GUIStyle style_btn_normal
            {
                get
                {
                    if (_style_btn_normal == null)
                    {
                        _style_btn_normal = new GUIStyle(GUI.skin.button);
                        _style_btn_normal.fontSize = 13;
                    }
                    return _style_btn_normal;
                }
            }


        }
        static class I18Ns
        {
            private static bool? _isChinese;
            private static bool IsChinese
            {
                get
                {
                    if (_isChinese == null)
                    {
                        _isChinese = (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified);
                    }
                    return _isChinese.Value;
                }
            }

            private static bool? _nihongo_desuka;
            private static bool NihongoDesuka
            {
                get
                {
                    if (_nihongo_desuka == null)
                        _nihongo_desuka = (Application.systemLanguage == SystemLanguage.Japanese);
                    return _nihongo_desuka.Value;
                }
            }

            public static string NoConfig
            {
                get
                {
                    if (IsChinese)
                        return "在首次使用TinaX Lua的设置工具前，请先创建配置文件";
                    if (NihongoDesuka)
                        return "TinaX Luaセットアップツールを初めて使用する前に、構成ファイルを作成してください";
                    return "Before using the TinaX Lua setup tool for the first time, please create a configuration file";
                }
            }

            public static string BtnCreateConfigFile
            {
                get
                {
                    if (IsChinese)
                        return "创建配置文件";
                    if (NihongoDesuka)
                        return "構成ファイルを作成する";
                    return "Create Configure File";
                }
            }

            public static string EnableLua
            {
                get
                {
                    if (IsChinese)
                        return "启用 Lua Runtime";
                    if (NihongoDesuka)
                        return "Luaランタイムを有効にする";
                    return "Enable Lua Runtime";
                }
            }

            public static string EntryFilePath
            {
                get
                {
                    if (IsChinese)
                        return "Lua启动入口文件加载路径";
                    if (NihongoDesuka)
                        return "Lua起動ファイルのロードパス";
                    return "Lua startup file load path";
                }
            }

            public static string FrameworkInternalLuaFolder
            {
                get
                {
                    if (IsChinese)
                        return "Framework 内置Lua文件根目录";
                    if (NihongoDesuka)
                        return "フレームワークの組み込みLuaファイルのルートディレクトリ";
                    return "Framework built-in Lua files root directory";
                }
            }

            public static string FrameworkInternalLuaFolderLoadPath
            {
                get
                {
                    if (IsChinese)
                        return "Framework 内置Lua文件根目录在加载时的路径";
                    if (NihongoDesuka)
                        return "ロード時のフレームワーク組み込みLuaファイルのルートパス";
                    return "Framework built-in Lua file root path at load time";
                }
            }

            public static string FrameworkInternalLuaFolder_Tips
            {
                get
                {
                    if (IsChinese)
                        return "TinaX.Lua需要将内置的Lua文件导入到工程中，并确保Framework内置的资源加载方法可以顺利的加载到它们。";
                    if (NihongoDesuka)
                        return "TinaX.Luaは、組み込みのLuaファイルをプロジェクトにインポートし、Frameworkの組み込みリソースの読み込み方法がスムーズにそれらに読み込まれるようにする必要があります。";
                    return "TinaX.Lua needs to import the built-in Lua files into the project, and ensure that the built-in resource loading method of the Framework can be smoothly loaded into them.";
                }
            }

            public static string ImportBuildinLuaFilesToThisPath
            {
                get
                {
                    if (IsChinese)
                        return "导入内置Lua文件到该目录";
                    if (NihongoDesuka)
                        return "ビルトインLuaファイルをこのディレクトリにインポートします";
                    return "Import built-in Lua files into this directory";
                }
            }

            public static string LuaExtension
            {
                get
                {
                    if (IsChinese)
                        return "Lua文件后缀名";
                    if (NihongoDesuka)
                        return "Luaファイル拡張子";
                    return "Lua file extension";
                }
            }
            
            
        }
    }
}
