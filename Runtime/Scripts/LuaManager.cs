using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinaX.Lua.Const;
using TinaX.Lua.Internal;
using TinaX.Services;
using UnityEngine;
using XLua;

namespace TinaX.Lua
{
    public class LuaManager : ILua, ILuaInternal
    {
        private const string c_InternalLuaSign = @"{tinax}";
        private const string c_Internal_Lua_Extension = ".lua.txt";
        private const string c_InternalLuaEntryPath = @"{tinax}.init.init";

        private LuaConfig m_Config;
        private TinaX.Services.IAssetService m_Assets;

        private LuaEnv m_LuaVM;
        private LuaEnv.CustomLoader m_Loader;
        private static float m_lastGCTime = 0;
        private const float m_GCInterval = 1; //1 second

        private string m_Internal_Lua_Folder_Load_Path;
        private string m_Internal_Lua_Folder_Load_Path_withSlash;

        private string m_LuaExtension;
        private bool m_Inited;
        private TinaX.Systems.ITimeTicket m_UpdateTicket;

        private LuaFunction m_EntryFunc;

        private CustomLoadHandlerManager m_CustomLoadHandlerManager = new CustomLoadHandlerManager();

        public LuaManager(IAssetService buildInAssets)
        {
            m_Assets = buildInAssets;
            m_LuaVM = new LuaEnv();
        }

        ~LuaManager()
        {
            m_UpdateTicket?.Unregister();
        }

        public LuaEnv LuaVM => m_LuaVM;
        public bool Inited => m_Inited;

        public async Task<XException> Start()
        {
            if (m_Inited) return null;
            m_Config = XConfig.GetConfig<LuaConfig>(LuaConst.ConfigPath_Resources);
            if (m_Config == null)
                return new XException("[TinaX.ILRuntime] Connot found config file.");

            if (!m_Config.EnableLua) return null;

            m_Internal_Lua_Folder_Load_Path = m_Config.FrameworkInternalLuaFolderLoadPath;
            if (!m_Internal_Lua_Folder_Load_Path.IsNullOrEmpty())
            {
                if (m_Internal_Lua_Folder_Load_Path.EndsWith("/"))
                    m_Internal_Lua_Folder_Load_Path = m_Internal_Lua_Folder_Load_Path.Substring(0, m_Internal_Lua_Folder_Load_Path.Length - 1);
                m_Internal_Lua_Folder_Load_Path_withSlash = m_Internal_Lua_Folder_Load_Path + "/";
            }
            m_LuaExtension = m_Config.LuaFileExtensionName;
            if (!m_LuaExtension.StartsWith("."))
                m_LuaExtension = "." + m_LuaExtension;

            if(m_Assets == null)
                return new XException("[TinaX.ILRuntime]" + (IsChinese ? "没有任何服务实现了Framework中的内置的资产加载接口" : "No service implements the built-in asset loading interface in Framework"));

            m_Loader = LoadLuaFiles;
            m_LuaVM.AddLoader(m_Loader);

            try
            {
                await InitInternalEntry();
            }
            catch(XException e)
            {
                return e;
            }

            //准备好入口文件
            if(!m_Config.EntryLuaFileLoadPath.IsNullOrEmpty())
            {
                try
                {
                    TextAsset entry_ta = await m_Assets.LoadAsync<TextAsset>(m_Config.EntryLuaFileLoadPath);
                    m_EntryFunc = m_LuaVM.LoadString<LuaFunction>(entry_ta.bytes, m_Config.EntryLuaFileLoadPath);
                    m_Assets.Release(entry_ta);
                }
                catch(XException e)
                {
                    return e;
                }
            }

            if(m_UpdateTicket != null)
                m_UpdateTicket.Unregister();
            m_UpdateTicket = TimeMachine.RegisterUpdate(OnUpdate);

            m_Inited = true;
            return null;
        }

        public ILua ConfigureCustomLoadHandler(Action<CustomLoadHandlerManager> options)
        {
            options?.Invoke(m_CustomLoadHandlerManager);
            return this;
        }

        public void LuaRequireToPath(ref string fileName)
        {
            if (fileName.IndexOf('.') != -1)
                fileName = fileName.Replace('.', '/');

            bool framework_file = false;
            //转义
            if (fileName.StartsWith(c_InternalLuaSign))
            {
                fileName = fileName.Replace(c_InternalLuaSign, m_Internal_Lua_Folder_Load_Path);
                framework_file = true;
            }

            //后缀
            if (framework_file)
                fileName += c_Internal_Lua_Extension;
            else
                fileName += m_LuaExtension;

        }

        public void LoadStringAsync(string require_text, Action<LuaFunction,Exception> callback)
        {
            string final_path = require_text;
            LuaRequireToPath(ref final_path);
            m_Assets.LoadAsync(final_path, typeof(TextAsset), (asset, error) =>
            {
                if (error != null)
                {
                    callback?.Invoke(null, error);
                }
                else
                {
                    var func = m_LuaVM.LoadString<LuaFunction>(((TextAsset)asset).bytes, final_path);
                    callback?.Invoke(func, null);
                    m_Assets.Release(asset);
                }
            });
        }

        public void RequireEntryFile()
        {
            if (m_EntryFunc != null)
                m_EntryFunc.Call();
        }

        private byte[] LoadLuaFiles(ref string fileName)
        {
            if(m_CustomLoadHandlerManager.TryGetHandler(fileName,out var handler)) //自定义加载
            {
                return handler?.Invoke();
            }


            LuaRequireToPath(ref fileName);

            //使用同步接口加载资源
            try
            {
                var ta = m_Assets.Load<TextAsset>(fileName);
                byte[] code = ta.bytes;
                m_Assets.Release(ta);
                return code;
            }
            catch
            {
                Debug.LogWarning("Load Lua file failed: " + fileName);
                return null;
            }
        }

        private async Task InitInternalEntry()
        {
            try
            {
                string final_path = c_InternalLuaEntryPath;
                LuaRequireToPath(ref final_path);
                TextAsset ta = await m_Assets.LoadAsync<TextAsset>(final_path);
                object[] obj_result = m_LuaVM.DoString(ta.bytes, final_path);
                LuaTable table = (LuaTable)obj_result[0];
                List<string> init_list = table.Cast<List<string>>();

                List<Task> list_task = new List<Task>();
                foreach (var item in init_list)
                {
                    list_task.Add(require_init_file(item));
                }
                await Task.WhenAll(list_task);
                m_Assets.Release(ta);
            }
            catch(XException e)
            {
                throw e;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task require_init_file(string req_str)
        {
            string final_path = req_str;
            LuaRequireToPath(ref final_path);
            TextAsset ta = await m_Assets.LoadAsync<TextAsset>(final_path);
            m_LuaVM.DoString(ta.bytes, final_path);
            m_Assets.Release(ta);
        }

        private void OnUpdate()
        {
            if (Time.time - m_lastGCTime > m_GCInterval)
            {
                m_LuaVM.Tick();
                m_lastGCTime = Time.time;
            }
        }

        

        private static bool? _isChinese = false;
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

        private static bool? _nihongo_desuka = true;
        private static bool NihongoDesuka
        {
            get
            {
                if (_nihongo_desuka == null)
                    _nihongo_desuka = (Application.systemLanguage == SystemLanguage.Japanese);
                return _nihongo_desuka.Value;
            }
        }
    }
}
