// Assets/Editor/ExportPlayMakerFSM.cs
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HutongGames.PlayMaker;

public class ExportPlayMakerFSM : Editor
{
    [MenuItem("Tools/PlayMaker/Export Selected FSMs to JSON")]
    static void ExportSelectedFSMs()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("Select a GameObject with one or more PlayMakerFSM components.");
            return;
        }

        var fsms = go.GetComponents<PlayMakerFSM>();
        if (fsms == null || fsms.Length == 0)
        {
            Debug.LogError("No PlayMakerFSM components found on the selected GameObject.");
            return;
        }

        var wrapper = new FsmDumpWrapper { items = new List<FsmDump>() };

        foreach (var pm in fsms)
        {
            var fsm = pm.Fsm;
            if (fsm == null)
                continue;

            // States
            var states = (fsm.States ?? Array.Empty<FsmState>())
                .Select(s => new FsmStateDump
                {
                    name = s.Name,
                    actions = (s.Actions ?? Array.Empty<FsmStateAction>())
                                .Select(a => a != null ? a.GetType().Name : "null")
                                .ToArray(),
                    transitions = (s.Transitions ?? Array.Empty<FsmTransition>())
                                .Select(t => new FsmTransitionDump
                                {
                                    eventName = t?.EventName ?? "",
                                    toState   = t?.ToState   ?? ""
                                })
                                .ToArray()
                })
                .ToArray();

            // Global transitions
            var globals = (fsm.GlobalTransitions ?? Array.Empty<FsmTransition>())
                .Select(t => new FsmTransitionDump
                {
                    eventName = t?.EventName ?? "",
                    toState   = t?.ToState   ?? ""
                })
                .ToArray();

            // Variables
            var vars = fsm.Variables;
            var vDump = new FsmVariablesDump
            {
                bools   = (vars.BoolVariables    ?? Array.Empty<FsmBool>())   .Select(v => v.Name).ToArray(),
                ints    = (vars.IntVariables     ?? Array.Empty<FsmInt>())    .Select(v => v.Name).ToArray(),
                floats  = (vars.FloatVariables   ?? Array.Empty<FsmFloat>())  .Select(v => v.Name).ToArray(),
                strings = (vars.StringVariables  ?? Array.Empty<FsmString>()) .Select(v => v.Name).ToArray(),
                vectors = (vars.Vector3Variables ?? Array.Empty<FsmVector3>()).Select(v => v.Name).ToArray(),
                colors  = (vars.ColorVariables   ?? Array.Empty<FsmColor>())  .Select(v => v.Name).ToArray(),
                objects = (vars.ObjectVariables  ?? Array.Empty<FsmObject>()) .Select(v => v.Name).ToArray(),
                arrays  = (vars.ArrayVariables   ?? Array.Empty<FsmArray>())  .Select(v => v.Name).ToArray()
            };

            // Events
            var events = (fsm.Events ?? Array.Empty<FsmEvent>())
                .Select(e => e?.Name ?? "")
                .ToArray();

            wrapper.items.Add(new FsmDump
            {
                gameObject   = go.name,
                fsmName      = fsm.Name,
                startState   = fsm.StartState,
                events       = events,
                states       = states,
                globalTransitions = globals,
                variables    = vDump
            });
        }

        if (wrapper.items.Count == 0)
        {
            Debug.LogWarning("Nothing to export (no valid FSMs).");
            return;
        }

        var path = EditorUtility.SaveFilePanel(
            "Export FSM JSON",
            Application.dataPath,
            go.name + "_FSM.json",
            "json"
        );
        if (string.IsNullOrEmpty(path)) return;

        // JsonUtility only serializes fields; these classes below use public fields.
        var json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
        Debug.Log($"FSM exported → {path}");
        EditorUtility.RevealInFinder(path);
    }

    // ---------- Serializable DTOs for JsonUtility ----------

    [Serializable]
    public class FsmDumpWrapper
    {
        public List<FsmDump> items;
    }

    [Serializable]
    public class FsmDump
    {
        public string gameObject;
        public string fsmName;
        public string startState;
        public FsmTransitionDump[] globalTransitions;
        public string[] events;
        public FsmStateDump[] states;
        public FsmVariablesDump variables;
    }

    [Serializable]
    public class FsmStateDump
    {
        public string name;
        public string[] actions;
        public FsmTransitionDump[] transitions;
    }

    [Serializable]
    public class FsmTransitionDump
    {
        public string eventName;
        public string toState;
    }

    [Serializable]
    public class FsmVariablesDump
    {
        public string[] bools;
        public string[] ints;
        public string[] floats;
        public string[] strings;
        public string[] vectors;
        public string[] colors;
        public string[] objects;
        public string[] arrays;
    }
}