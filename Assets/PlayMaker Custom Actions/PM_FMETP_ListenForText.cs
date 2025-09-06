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
    [HutongGames.PlayMaker.Tooltip("Subscribes to FMETP string-received UnityEvents and fires a PlayMaker event when text arrives.")]
    public class PM_FMETP_ListenForText : FsmStateAction
    {
        [RequiredField, HutongGames.PlayMaker.Tooltip("GameObject with FMNetworkManager")]
        public FsmGameObject fmNetworkObject;

        [HutongGames.PlayMaker.Tooltip("Store the received text here (optional).")]
        [UIHint(UIHint.Variable)] public FsmString storeMessage;

        [RequiredField, HutongGames.PlayMaker.Tooltip("PlayMaker event to fire on receive.")]
        public FsmEvent receivedEvent;

        [HutongGames.PlayMaker.Tooltip("Only react when this instance is Server.")]
        public FsmBool onlyWhenServer;

        [HutongGames.PlayMaker.Tooltip("Only react when this instance is Client.")]
        public FsmBool onlyWhenClient;

        [HutongGames.PlayMaker.Tooltip("Log receives to Console.")]
        public FsmBool debugLog;

        FMNetworkManager _fm;
        readonly List<UnityEvent<string>> _boundEvents = new List<UnityEvent<string>>();
        UnityAction<string> _handler;

        public override void Reset()
        {
            fmNetworkObject = null;
            storeMessage = null;
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
                Debug.LogWarning("[PM_FMETP_ListenForText] fmNetworkObject is null.");
                Finish();
                return;
            }

            _fm = go.GetComponent<FMNetworkManager>();
            if (_fm == null)
            {
                Debug.LogWarning("[PM_FMETP_ListenForText] FMNetworkManager not found on GameObject.");
                Finish();
                return;
            }

            _handler = OnText;

            // Bind to any UnityEvent<string> exposed by FMNetworkManager (works with current FMETP)
            BindStringUnityEvents(_fm);

            if (_boundEvents.Count == 0)
            {
                Debug.LogWarning("[PM_FMETP_ListenForText] No UnityEvent<string> found on FMNetworkManager. " +
                                 "If your FMETP version renamed events, update this action or use a simple relay MonoBehaviour.");
                // Stay active to avoid immediate Finish; user might switch scenes.
            }
        }

        public override void OnExit()
        {
            foreach (var ev in _boundEvents)
            {
                try { ev.RemoveListener(_handler); } catch { /* ignore */ }
            }
            _boundEvents.Clear();
        }

        void OnText(string value)
        {
            if (!PassesRoleFilter()) return;

            if (storeMessage != null) storeMessage.Value = value;
            Fsm.EventData.StringData = value; // convenient access inside transitions
            if (debugLog.Value) Debug.Log($"[FMETP → PlayMaker] Text: {value}");

            Fsm.Event(receivedEvent);
        }

        bool PassesRoleFilter()
        {
            if (_fm == null) return true;
            if (onlyWhenServer.Value && _fm.NetworkType != FMNetworkType.Server) return false;
            if (onlyWhenClient.Value && _fm.NetworkType != FMNetworkType.Client) return false;
            return true;
        }

        void BindStringUnityEvents(FMNetworkManager fm)
        {
            // Look for public or private fields of type UnityEvent<string>
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var fi in fm.GetType().GetFields(flags))
            {
                if (typeof(UnityEvent<string>).IsAssignableFrom(fi.FieldType))
                {
                    var ev = fi.GetValue(fm) as UnityEvent<string>;
                    if (ev != null)
                    {
                        ev.AddListener(_handler);
                        _boundEvents.Add(ev);
                    }
                }
            }

            // Also scan readable properties of type UnityEvent<string>
            foreach (var pi in fm.GetType().GetProperties(flags))
            {
                if (!pi.CanRead) continue;
                if (typeof(UnityEvent<string>).IsAssignableFrom(pi.PropertyType))
                {
                    var ev = pi.GetValue(fm, null) as UnityEvent<string>;
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
