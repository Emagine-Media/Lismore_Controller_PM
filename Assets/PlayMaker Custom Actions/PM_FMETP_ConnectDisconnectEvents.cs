// PM_FMETP_ConnectDisconnectEvents.cs (minimal implementation)
// Category: FMETP

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker;

/*
 * This minimal PlayMaker action listens to FMETP client connect and disconnect events
 * and fires the chosen PlayMaker transitions for each event. It provides only the
 * essential fields: the target FMETP manager, optional component type restriction,
 * the names of the events to listen for, and the PlayMaker events to fire.
 *
 * Assign this action to an FSM state and set:
 *   - targetObject: the GameObject with your FMNetworkManager or server component.
 *   - clientConnectedEventName: name of the event for client connect (default "OnClientConnected").
 *   - clientDisconnectedEventName: name of the event for client disconnect (default "OnClientDisconnected").
 *   - onConnectedEvent: the PlayMaker event to send when a client connects.
 *   - onDisconnectedEvent: the PlayMaker event to send when a client disconnects.
 */
[HutongGames.PlayMaker.ActionCategory("FMETP")]
[HutongGames.PlayMaker.Tooltip("Subscribes to FMETP connect/disconnect events and fires PlayMaker transitions for each.")]
public class PM_FMETP_ConnectDisconnectEvents : FsmStateAction
{
    [HutongGames.PlayMaker.RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject with your FMETP manager/server component (e.g., FMNetworkManager / FMServerComponent).")]
    public FsmOwnerDefault targetObject;

    [HutongGames.PlayMaker.Tooltip("Restrict binding to this component type name (e.g., 'FMNetworkManager' or 'FMServerComponent'). Leave blank to search all components.")]
    public FsmString restrictToComponentTypeName;

    [HutongGames.PlayMaker.Tooltip("Name of the event for client connect (e.g., OnClientConnected or OnClientConnectedEvent).")]
    public FsmString clientConnectedEventName;

    [HutongGames.PlayMaker.Tooltip("Name of the event for client disconnect (e.g., OnClientDisconnected or OnClientDisconnectedEvent).")]
    public FsmString clientDisconnectedEventName;

    [HutongGames.PlayMaker.Tooltip("PlayMaker event to send when a client connects.")]
    public FsmEvent onConnectedEvent;

    [HutongGames.PlayMaker.Tooltip("PlayMaker event to send when a client disconnects.")]
    public FsmEvent onDisconnectedEvent;

    // Internal binding state
    private Component _boundComponent;
    private Delegate _subConn, _subDisc;
    private object _unityEventConn, _unityEventDisc;
    private Delegate _unityEventConnDel, _unityEventDiscDel;

    public override void Reset()
    {
        targetObject = null;
        restrictToComponentTypeName = new FsmString { UseVariable = true };
        clientConnectedEventName = "OnClientConnected";
        clientDisconnectedEventName = "OnClientDisconnected";
        onConnectedEvent = null;
        onDisconnectedEvent = null;

        _boundComponent = null;
        _subConn = _subDisc = null;
        _unityEventConn = _unityEventDisc = null;
        _unityEventConnDel = _unityEventDiscDel = null;
    }

    public override void OnEnter()
    {
        var go = Fsm.GetOwnerDefaultTarget(targetObject);
        if (go == null)
        {
            Debug.LogError("[PM_FMETP_ConnectDisconnectEvents] Target GameObject is null.");
            Finish();
            return;
        }

        // Attempt to bind to events on components of the target object
        var comps = go.GetComponents<MonoBehaviour>().Where(c => c != null && ComponentAllowed(c.GetType())).ToArray();
        bool bound = false;
        foreach (var comp in comps)
        {
            if (TryBindBoth(comp))
            {
                _boundComponent = comp;
                bound = true;
                break;
            }
        }
        if (!bound)
        {
            Debug.LogError($"[PM_FMETP_ConnectDisconnectEvents] Could not bind to '{clientConnectedEventName.Value}' / '{clientDisconnectedEventName.Value}'.");
            Finish();
            return;
        }
    }

    public override void OnExit()
    {
        Unsubscribe();
    }

    // --- Binding helpers ---

    private bool TryBindBoth(Component comp)
    {
        var t = comp.GetType();
        bool ok1 = TryBindStringEvent(comp, t, clientConnectedEventName.Value, true);
        bool ok2 = TryBindStringEvent(comp, t, clientDisconnectedEventName.Value, false);
        return ok1 && ok2;
    }

    private bool TryBindStringEvent(Component comp, Type t, string name, bool isConnect)
    {
        if (string.IsNullOrEmpty(name)) return false;

        // Try C# event first
        var evt = t.GetEvent(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (evt != null)
        {
            var handler = isConnect
                ? GetType().GetMethod(nameof(HandleClientConnected), BindingFlags.Instance | BindingFlags.NonPublic)
                : GetType().GetMethod(nameof(HandleClientDisconnected), BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                var del = Delegate.CreateDelegate(evt.EventHandlerType, this, handler);
                evt.AddEventHandler(comp, del);
                if (isConnect) _subConn = del; else _subDisc = del;
                return true;
            }
            catch
            {
                // signature mismatch
            }
        }

        // Try UnityEvent<string>
        var uevt = GetUnityEventObject(t, comp, name);
        if (uevt != null)
        {
            var add = uevt.GetType().GetMethod("AddListener", BindingFlags.Instance | BindingFlags.Public);
            if (add != null)
            {
                var unityActionType = add.GetParameters().FirstOrDefault()?.ParameterType;
                if (unityActionType != null)
                {
                    var handler = isConnect
                        ? GetType().GetMethod(nameof(HandleClientConnected), BindingFlags.Instance | BindingFlags.NonPublic)
                        : GetType().GetMethod(nameof(HandleClientDisconnected), BindingFlags.Instance | BindingFlags.NonPublic);
                    var del = Delegate.CreateDelegate(unityActionType, this, handler);
                    add.Invoke(uevt, new object[] { del });

                    if (isConnect) { _unityEventConn = uevt; _unityEventConnDel = del; }
                    else           { _unityEventDisc = uevt; _unityEventDiscDel = del; }
                    return true;
                }
            }
        }
        return false;
    }

    private object GetUnityEventObject(Type t, Component comp, string memberName)
    {
        var p = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(p.PropertyType)) return p.GetValue(comp);
        var f = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(f.FieldType)) return f.GetValue(comp);
        return null;
    }

    private bool ComponentAllowed(Type tt)
    {
        if (tt == null) return false;
        if (!restrictToComponentTypeName.IsNone && !string.IsNullOrEmpty(restrictToComponentTypeName.Value))
            return string.Equals(tt.Name, restrictToComponentTypeName.Value, StringComparison.Ordinal) ||
                   string.Equals(tt.FullName, restrictToComponentTypeName.Value, StringComparison.Ordinal);
        return true;
    }

    // --- Event handlers ---

    // Simply send the onConnectedEvent when a client connects.
    private void HandleClientConnected(string payload)
    {
        if (onConnectedEvent != null) Fsm.Event(onConnectedEvent);
    }

    // Simply send the onDisconnectedEvent when a client disconnects.
    private void HandleClientDisconnected(string payload)
    {
        if (onDisconnectedEvent != null) Fsm.Event(onDisconnectedEvent);
    }

    // --- Cleanup ---
    private void Unsubscribe()
    {
        if (_boundComponent == null) return;
        var t = _boundComponent.GetType();

        TryRemoveEventHandler(t, _boundComponent, clientConnectedEventName.Value, _subConn);
        TryRemoveEventHandler(t, _boundComponent, clientDisconnectedEventName.Value, _subDisc);
        _subConn = _subDisc = null;

        TryRemoveUnityListener(_unityEventConn, _unityEventConnDel);
        TryRemoveUnityListener(_unityEventDisc, _unityEventDiscDel);
        _unityEventConn = _unityEventDisc = null;
        _unityEventConnDel = _unityEventDiscDel = null;

        _boundComponent = null;
    }

    private void TryRemoveEventHandler(Type t, Component comp, string name, Delegate del)
    {
        if (del == null || string.IsNullOrEmpty(name)) return;
        try
        {
            var evt = t.GetEvent(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (evt != null) evt.RemoveEventHandler(comp, del);
        }
        catch
        {
            // ignore
        }
    }

    private void TryRemoveUnityListener(object unityEvent, Delegate del)
    {
        if (unityEvent == null || del == null) return;
        var remove = unityEvent.GetType().GetMethod("RemoveListener", BindingFlags.Instance | BindingFlags.Public);
        if (remove != null)
        {
            try { remove.Invoke(unityEvent, new object[] { del }); } catch { }
        }
    }
}