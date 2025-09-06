// PM_Debug_ListMembers.cs
// Lists candidate events/properties/fields on a component to help bind PlayMaker actions.
// Category: FMETP

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker;

[HutongGames.PlayMaker.ActionCategory("FMETP")]
[HutongGames.PlayMaker.Tooltip("Logs events, UnityEvents, properties and fields on the target component so you can see exact names.")]
public class PM_Debug_ListMembers : FsmStateAction
{
    [HutongGames.PlayMaker.RequiredField]
    [HutongGames.PlayMaker.Tooltip("GameObject that has your FMETP component.")]
    public FsmOwnerDefault targetObject;

    [HutongGames.PlayMaker.Tooltip("Restrict to this component type name (e.g., 'FMNetworkManager' or 'FMServerComponent').")]
    public FsmString restrictToComponentTypeName;

    public override void OnEnter()
    {
        var go = Fsm.GetOwnerDefaultTarget(targetObject);
        if (go == null) { Debug.LogError("[PM_Debug_ListMembers] Target null"); Finish(); return; }

        var comps = go.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            if (!Filter(t, restrictToComponentTypeName.Value)) continue;

            Debug.Log($"[PM_Debug_ListMembers] ==== {t.FullName} on {go.name} ====");

            // C# events
            foreach (var e in t.GetEvents(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                Debug.Log($"[PM_Debug_ListMembers] Event: {e.EventHandlerType?.Name} {e.Name}");

            // UnityEvent props
            foreach (var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(p.PropertyType))
                    Debug.Log($"[PM_Debug_ListMembers] UnityEvent (prop): {p.PropertyType.Name} {p.Name}");

            // UnityEvent fields
            foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(f.FieldType))
                    Debug.Log($"[PM_Debug_ListMembers] UnityEvent (field): {f.FieldType.Name} {f.Name}");

            // Handy scalars
            foreach (var p in t.GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                if (p.PropertyType == typeof(int) || p.PropertyType == typeof(bool))
                    Debug.Log($"[PM_Debug_ListMembers] Prop: {p.PropertyType.Name} {p.Name}");
            foreach (var f in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
                if (f.FieldType == typeof(int) || f.FieldType == typeof(bool))
                    Debug.Log($"[PM_Debug_ListMembers] Field: {f.FieldType.Name} {f.Name}");
        }
        Finish();
    }

    private bool Filter(Type t, string restrict)
    {
        if (string.IsNullOrEmpty(restrict)) return true;
        return string.Equals(t.Name, restrict, StringComparison.Ordinal) ||
               string.Equals(t.FullName, restrict, StringComparison.Ordinal);
    }
}