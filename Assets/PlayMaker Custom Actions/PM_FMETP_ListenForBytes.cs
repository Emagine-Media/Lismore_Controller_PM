using System;
using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Events;
using FMSolution.FMNetwork;

namespace Net.PlayMaker.FMETP
{
    [ActionCategory("FMETP/Network")]
    [HutongGames.PlayMaker.Tooltip("Subscribes to FMETP byte[] UnityEvents and fires a PlayMaker event. Stores length in an Int.")]
    public class PM_FMETP_ListenForBytes : FsmStateAction
    {
        [RequiredField, HutongGames.PlayMaker.Tooltip("GameObject with FMNetworkManager")]
        public FsmGameObject fmNetworkObject;

        [HutongGames.PlayMaker.Tooltip("Store the received byte length here (optional).")]
        [UIHint(UIHint.Variable)] public FsmInt storeLength;

        [RequiredField, HutongGames.PlayMaker.Tooltip("PlayMaker event to fire on receive.")]
        public FsmEvent receivedEvent;

        [HutongGames.PlayMaker.Tooltip("Only react when this instance is Server.")]
        public FsmBool onlyWhenServer;

        [HutongGames.PlayMaker.Tooltip("Only react when this instance is Client.")]
        public FsmBool onlyWhenClient;

        [HutongGames.PlayMaker.Tooltip("Log receives to Console.")]
        public FsmBool debugLog;

        FMNetworkManager _fm;
        readonly List<UnityEvent<byte[]>> _boundEvents = new List<UnityEvent<byte[]>>();
        UnityAction<byte[]> _handler;

        public override void Reset()
        {
            fmNetworkObject = null;
            storeLength = null;
            receivedEvent = null;
            onlyWhenServer = false;
            onlyWhenClient = false;
            debugLog = false;
        }

        public override void OnEnter()
        {
            var go = fmNetworkObject.Value;
            if (go == null)
            {
                Debug.LogWarning("[PM_FMETP_ListenForBytes] fmNetworkObject is null.");
                Finish();
                return;
            }

            _fm = go.GetComponent<FMNetworkManager>();
            if (_fm == null)
            {
                Debug.LogWarning("[PM_FMETP_ListenForBytes] FMNetworkManager not found on GameObject.");
                Finish();
                return;
            }

            _handler = OnBytes;

            BindByteUnityEvents(_fm);

            if (_boundEvents.Count == 0)
            {
                Debug.LogWarning("[PM_FMETP_ListenForBytes] No UnityEvent<byte[]> found on FMNetworkManager.");
            }
        }

        public override void OnExit()
        {
            foreach (var ev in _boundEvents)
            {
                try { ev.RemoveListener(_handler); } catch { }
            }
            _boundEvents.Clear();
        }

        void OnBytes(byte[] data)
        {
            if (!PassesRoleFilter()) return;

            int len = data != null ? data.Length : 0;
            if (storeLength != null) storeLength.Value = len;
            Fsm.EventData.IntData = len;
            if (debugLog.Value) Debug.Log($"[FMETP → PlayMaker] Bytes: {len}");

            Fsm.Event(receivedEvent);
        }

        bool PassesRoleFilter()
        {
            if (_fm == null) return true;
            if (onlyWhenServer.Value && _fm.NetworkType != FMNetworkType.Server) return false;
            if (onlyWhenClient.Value && _fm.NetworkType != FMNetworkType.Client) return false;
            return true;
        }

        void BindByteUnityEvents(FMNetworkManager fm)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var fi in fm.GetType().GetFields(flags))
            {
                if (typeof(UnityEvent<byte[]>).IsAssignableFrom(fi.FieldType))
                {
                    var ev = fi.GetValue(fm) as UnityEvent<byte[]>;
                    if (ev != null)
                    {
                        ev.AddListener(_handler);
                        _boundEvents.Add(ev);
                    }
                }
            }
            foreach (var pi in fm.GetType().GetProperties(flags))
            {
                if (!pi.CanRead) continue;
                if (typeof(UnityEvent<byte[]>).IsAssignableFrom(pi.PropertyType))
                {
                    var ev = pi.GetValue(fm, null) as UnityEvent<byte[]>;
                    if (ev != null)
                    {
                        ev.AddListener(_handler);
                        _boundEvents.Add(ev);
                    }
                }
            }
        }
    }
}
