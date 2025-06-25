//  Copyright © 2025 CAS.AI. All rights reserved.

#if UNITY_ANDROID || (CASDeveloper && UNITY_EDITOR)
using System;
using UnityEngine;

namespace CAS.Android
{
    internal sealed class CASViewClient : CASViewBase, CASCallback.Handler
    {
        private readonly AndroidJavaObject _bridge;
        private CASCallback _callback;

        public override bool isReady
        {
            get { return _bridge.Call<bool>("isReady"); }
        }

        public override int refreshInterval
        {
            get { return _bridge.Call<int>("getRefreshInterval"); }
            set { _bridge.Call("setRefreshInterval", value); }
        }

        internal CASViewClient(CASManagerBase manager, AdSize size)
            : base(manager, size)
        {
            _callback = new CASCallback(this);
            _bridge = new AndroidJavaObject(CASJavaBridge.AdViewClass, (int)size, _callback, manager.managerID);
        }

        internal override void DestroyNative()
        {
            _bridge.Call("destroy");
        }

        internal override void Enable()
        {
            _bridge.Call("enable");
        }

        public override void LoadNative()
        {
            _bridge.Call("load");
        }

        public override void SetActive(bool active)
        {
            if (active)
            {
                _bridge.Call("show");
                return;
            }
            _bridge.Call("hide");
        }

        protected override void SetPositionNative(int position, int x, int y)
        {
            _bridge.Call("setPosition", position, x, y);
        }

        protected override void SetPositionPxNative(int position, int x, int y)
        {
            _bridge.Call("setPositionPx", position, x, y);
        }
    }
}
#endif