using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinaX.XComponent;
using XLua;
using System;
using TinaX.Systems;

namespace TinaX.Lua
{
    [AddComponentMenu("TinaX/Lua/LuaBehaviour")]
    public class LuaBehaviour : XComponentScriptBase
    {
        public delegate void ReleaseTextAssetDelegate(TextAsset asset);

        public TextAsset LuaScript;

        public int UpdateOrder;
        public int LateUpdateOrder;
        public int FixedUpdateOrder;

        [HideInInspector]
        public LuaTable scriptData;

        private ILua m_LuaManager;
        private Internal.ILuaInternal m_LuaManagerInternal;

        private Action func_enable;
        private Action func_disable;

        private Action func_awake;
        private Action func_start;
        private Action func_destroy;

        private Action func_update;
        private Action func_fixedupdate;
        private Action func_lateupdate;

        private LuaFunction func_applicationFocus;
        private LuaFunction func_applicationPause;
        private Action func_applicationQuit;

        private LuaFunction func_message;

        private bool mAwaked = false;
        private bool mStarted = false;
        private bool mEnabled = false;
        private bool mDisabled = false;

        private Utils.DisposableGroup __DisposableGroup;
        private Utils.DisposableGroup m_DisposableGroup
        {
            get
            {
                if (__DisposableGroup == null)
                    __DisposableGroup = new Utils.DisposableGroup();
                return __DisposableGroup;
            }
        }

        private ITimeTicket mUpdateTicks;
        private ITimeTicket mLateUpdateTicks;
        private ITimeTicket mFixedUpdateTicks;

        private ReleaseTextAssetDelegate mAsset_Release_Callback;
        private bool SetLuaScript_Flag = false;

        /// <summary>
        /// Register Event Listener
        /// It will be auto unregister when luabehaviour destroy.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handler"></param>
        /// <param name="eventGroup"></param>
        public void RegisterEvent(string eventName, Action<object> handler, string eventGroup = XEvent.DefaultGroup)
        {
            m_DisposableGroup.RegisterEvent(eventName, handler, eventGroup);
        }

        public void SetLuaScript(TextAsset lua_text_asset, ReleaseTextAssetDelegate release_asset_callback)
        {
            if (this.LuaScript != null)
            {
                Debug.LogWarning($"[LuaBehaviour : {this.gameObject.name}] Lua Script already exist.");
                return;
            }
            if (lua_text_asset == null)
            {
                Debug.LogError($"[LuaBehaviour : {this.gameObject.name}] Set LuaScript Failed. Incoming parameter is empty");
                return;
            }
            this.LuaScript = lua_text_asset;
            InitLuaScript();

            if (mAwaked)
                func_awake?.Invoke();
            if (mStarted)
                func_start?.Invoke();
            if (mEnabled && this.enabled)
                func_enable?.Invoke();
            if (mDisabled && !this.enabled)
                func_disable?.Invoke();

            HandleUpdates();
            mAsset_Release_Callback = release_asset_callback;
            SetLuaScript_Flag = true;
        }

        public object[] Invoke(string functionName, params object[] args)
        {
            if (scriptData != null)
            {
                LuaFunction func = scriptData.Get<LuaFunction>(functionName);
                if(func != null)
                {
                    return func.Call(args);
                }
            }
            return null;
        }

        public bool TryInvoke(string functionName, out object[] result, params object[] args)
        {
            if (scriptData != null)
            {
                LuaFunction func = scriptData.Get<LuaFunction>(functionName);
                if (func != null)
                {
                    result = func.Call(args);
                    return true;
                }
            }
            result = null;
            return false;
        }

        public TResult Invoke<TResult>(string functionName)
        {
            if(scriptData != null)
            {
                var func = scriptData.Get<Func<TResult>>(functionName);
                if(func != null)
                {
                    return func();
                }
            }
            return default;
        }

        public void Invoke<T>(string functionName, T arg1)
        {
            if (scriptData != null)
                scriptData.Get<Action<T>>(functionName)?.Invoke(arg1);
        }

        private void InitLuaScript()
        {
            if(XCore.MainInstance == null || !XCore.GetMainInstance().TryGetService(out m_LuaManager) || !XCore.GetMainInstance().TryGetService(out m_LuaManagerInternal))
            {
                Debug.LogError("[LuaBehaviour] Lua Service not ready.");
                return;
            }
            scriptData = m_LuaManagerInternal.LuaVM.NewTable();
            LuaTable meta = m_LuaManagerInternal.LuaVM.NewTable();
            //元表，让luaBehaviour可以访问_G
            meta.Set("__index", m_LuaManager.LuaVM.Global);
            scriptData.SetMetaTable(meta);
            meta.Dispose();

            scriptData.Set("self", this);

            //注入绑定对象
            if(base.UObjectBindInfos != null)
            {
                foreach(var item in base.UObjectBindInfos)
                {
                    if (!item.Name.IsNullOrEmpty() & item.Object != null)
                        scriptData.Set(item.Name, item.Object);
                }
            }
            if(base.TypeBindInfos != null)
            {
                foreach(var item in base.TypeBindInfos)
                {
                    if (!item.Name.IsNullOrEmpty())
                    {
                        if (XComponents.TryGetValue(item, out object value))
                            scriptData.Set(item.Name, value);
                    }
                }
            }

            m_LuaManagerInternal.LuaVM.DoString(LuaScript.bytes, LuaScript.name, scriptData);

            scriptData.Get("OnEnable", out func_enable);
            scriptData.Get("OnDisable", out func_disable);

            scriptData.Get("Awake", out func_awake);
            scriptData.Get("Start", out func_start);
            scriptData.Get("OnDestroy", out func_destroy);

            scriptData.Get("Update", out func_update);
            scriptData.Get("FixedUpdate", out func_fixedupdate);
            scriptData.Get("LateUpdate", out func_lateupdate);

            scriptData.Get("OnApplicationFocus", out func_applicationFocus);
            scriptData.Get("OnApplicationPause", out func_applicationPause);
            scriptData.Get("OnApplicationQuit", out func_applicationQuit);

            scriptData.Get("OnMessage", out func_message);
        }

        private void HandleUpdates()
        {
            if (mUpdateTicks != null)
            {
                mUpdateTicks.Unregister();
                mUpdateTicks = null;
            }
            if (func_update != null)
                mUpdateTicks = TimeMachine.RegisterUpdate(OnUpdate, UpdateOrder);

            if (mLateUpdateTicks != null)
            {
                mLateUpdateTicks.Unregister();
                mLateUpdateTicks = null;
            }
            if (func_lateupdate != null)
                mLateUpdateTicks = TimeMachine.RegisterLateUpdate(OnLateUpdate, LateUpdateOrder);

            if (mFixedUpdateTicks != null)
            {
                mFixedUpdateTicks.Unregister();
                mFixedUpdateTicks = null;
            }
            if (func_fixedupdate != null)
                mFixedUpdateTicks = TimeMachine.RegisterFixedUpdate(OnFixedUpdate, FixedUpdateOrder);
        }

        private void Awake()
        {
            if (LuaScript == null)
                return;

            InitLuaScript();

            if (func_awake != null)
                func_awake.Invoke();
            mAwaked = true;

            HandleUpdates();
        }

        private void Start()
        {
            mStarted = true;
            func_start?.Invoke();
        }

        private void OnDestroy()
        {

            __DisposableGroup?.Dispose();
            //Updates
            if(mUpdateTicks != null)
            {
                mUpdateTicks.Unregister();
                mUpdateTicks = null;
            }
            if(mLateUpdateTicks != null)
            {
                mLateUpdateTicks.Unregister();
                mLateUpdateTicks = null;
            }
            if(mFixedUpdateTicks != null)
            {
                mFixedUpdateTicks.Unregister();
                mFixedUpdateTicks = null;
            }

            func_destroy?.Invoke();

            if(scriptData != null)
            {
                scriptData.Dispose();
                scriptData = null;
            }
            
            if(LuaScript != null)
            {
                if (mAsset_Release_Callback != null && SetLuaScript_Flag)
                    mAsset_Release_Callback(this.LuaScript);
            }
        }

        private void OnEnable()
        {
            mEnabled = true;
            func_enable?.Invoke();
        }

        private void OnDisable()
        {
            mDisabled = true;
            func_disable?.Invoke();
        }

        private void OnUpdate()
        {
            func_update?.Invoke();
        }

        private void OnLateUpdate()
        {
            func_lateupdate?.Invoke();
        }

        private void OnFixedUpdate()
        {
            func_fixedupdate?.Invoke();
        }

        private void OnApplicationFocus(bool focus)
        {
            this.func_applicationFocus?.Call(focus);
        }

        private void OnApplicationPause(bool pause)
        {
            this.func_applicationPause?.Call(pause);
        }

        private void OnApplicationQuit()
        {
            this.func_applicationQuit?.Invoke();
        }

        public override void SendMsg(string messageName, params object[] args)
        {
            if(scriptData!= null)
            {
                //先找找看有没有独立的function 
                var func = scriptData.Get<LuaFunction>(messageName);
                if(func != null)
                {
                    func.Call(args);
                    return;
                }
                func_message?.Call(args);
            }
        }

        public override void SendQueueMsg(string messageName, params object[] param)
        {
            this.SendMsg(messageName, param);
        }

    }

}
