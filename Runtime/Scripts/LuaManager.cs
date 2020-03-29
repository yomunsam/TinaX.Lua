using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinaX;
using TinaX.Lua.Const;
using TinaX.Lua.Internal;
using TinaX.Services;
using UnityEngine;
using XLua;

namespace TinaX.Lua
{
    public class LuaManager : ILua, ILuaInternal
    {
        private const string InternalLuaSign = @"{tinax}";
        private const string mInternal_Lua_Extension = ".lua.txt";
        private const string InternalLuaEntryPath = @"{tinax}.init.init";

        private LuaConfig mConfig;
        private XException mStartException;
        private TinaX.Services.IAssetService Assets;

        private LuaEnv mLuaVM;
        private LuaEnv.CustomLoader mLoader;
        private static float lastGCTime = 0;
        private const float GCInterval = 1; //1 second

        private string mInternal_Lua_Folder_Load_Path;
        private string mInternal_Lua_Folder_Load_Path_withSlash;

        private string mLuaExtension;
        private bool mInited;
        private TinaX.Systems.ITimeTicket mUpdateTicket;

        private LuaFunction mEntryFunc;

        public LuaManager()
        {
            mLuaVM = new LuaEnv();
        }

        public LuaEnv LuaVM => mLuaVM;
        public bool Inited => mInited;

        public async Task<bool> Start()
        {
            if (mInited) return true;
            mConfig = XConfig.GetConfig<LuaConfig>(LuaConst.ConfigPath_Resources);
            if (mConfig == null)
            {
                mStartException = new XException("[TinaX.ILRuntime] Connot found config file."); ;
                return false;
            }
            if (!mConfig.EnableLua) return true;

            mInternal_Lua_Folder_Load_Path = mConfig.FrameworkInternalLuaFolderLoadPath;
            if (!mInternal_Lua_Folder_Load_Path.IsNullOrEmpty())
            {
                if (mInternal_Lua_Folder_Load_Path.EndsWith("/"))
                    mInternal_Lua_Folder_Load_Path = mInternal_Lua_Folder_Load_Path.Substring(0, mInternal_Lua_Folder_Load_Path.Length - 1);
                mInternal_Lua_Folder_Load_Path_withSlash = mInternal_Lua_Folder_Load_Path + "/";
            }
            mLuaExtension = mConfig.LuaFileExtensionName;
            if (!mLuaExtension.StartsWith("."))
                mLuaExtension = "." + mLuaExtension;

            if(!XCore.MainInstance.TryGetBuiltinService(out Assets))
            {
                mStartException = new XException("[TinaX.ILRuntime]" + (IsChinese? "没有任何服务实现了Framework中的内置的资产加载接口": "No service implements the built-in asset loading interface in Framework")); 
                return false;
            }

            mLoader = LoadLuaFiles;
            mLuaVM.AddLoader(mLoader);

            try
            {
                await InitInternalEntry();
            }
            catch(XException e)
            {
                mStartException = e;
                return false;
            }

            //准备好入口文件
            if(!mConfig.EntryLuaFileLoadPath.IsNullOrEmpty())
            {
                try
                {
                    TextAsset entry_ta = await Assets.LoadAsync<TextAsset>(mConfig.EntryLuaFileLoadPath);
                    mEntryFunc = mLuaVM.LoadString<LuaFunction>(entry_ta.bytes, mConfig.EntryLuaFileLoadPath);
                    Assets.Release(entry_ta);
                }
                catch(XException e)
                {
                    mStartException = e;
                    return false;
                }
            }

            if(mUpdateTicket != null)
                mUpdateTicket.Unregister();
            mUpdateTicket = TimeMachine.RegisterUpdate(OnUpdate);

            await Task.Yield();
            mInited = true;
            return true;
        }

        public XException GetStartException() => mStartException;

        public void LuaRequireToPath(ref string fileName)
        {
            if (fileName.IndexOf('.') != -1)
                fileName = fileName.Replace('.', '/');

            bool framework_file = false;
            //转义
            if (fileName.StartsWith(InternalLuaSign))
            {
                fileName = fileName.Replace(InternalLuaSign, mInternal_Lua_Folder_Load_Path);
                framework_file = true;
            }

            //后缀
            if (framework_file)
                fileName += mInternal_Lua_Extension;
            else
                fileName += mLuaExtension;

        }

        public void LoadStringAsync(string require_text, Action<LuaFunction,Exception> callback)
        {
            string final_path = require_text;
            LuaRequireToPath(ref final_path);
            Assets.LoadAsync(final_path, typeof(TextAsset), (asset, error) =>
            {
                if (error != null)
                {
                    callback?.Invoke(null, error);
                }
                else
                {
                    var func = mLuaVM.LoadString<LuaFunction>(((TextAsset)asset).bytes, final_path);
                    callback?.Invoke(func, null);
                    Assets.Release(asset);
                }
            });
        }

        public void RequireEntryFile()
        {
            if (mEntryFunc != null)
                mEntryFunc.Call();
        }

        private byte[] LoadLuaFiles(ref string fileName)
        {
            LuaRequireToPath(ref fileName);

            //使用同步接口加载资源
            try
            {
                var ta = Assets.Load<TextAsset>(fileName);
                byte[] code = ta.bytes;
                Assets.Release(ta);
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
                string final_path = InternalLuaEntryPath;
                LuaRequireToPath(ref final_path);
                TextAsset ta = await Assets.LoadAsync<TextAsset>(final_path);
                object[] obj_result = mLuaVM.DoString(ta.bytes, final_path);
                LuaTable table = (LuaTable)obj_result[0];
                List<string> init_list = table.Cast<List<string>>();

                List<Task> list_task = new List<Task>();
                foreach (var item in init_list)
                {
                    list_task.Add(require_init_file(item));
                }
                await Task.WhenAll(list_task);
                Assets.Release(ta);
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
            TextAsset ta = await Assets.LoadAsync<TextAsset>(final_path);
            mLuaVM.DoString(ta.bytes, final_path);
            Assets.Release(ta);
        }

        private void OnUpdate()
        {
            if (Time.time - lastGCTime > GCInterval)
            {
                mLuaVM.Tick();
                lastGCTime = Time.time;
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
