using System;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Net.PlayMaker.FMETP
{
    [ActionCategory("FMETP/Network")]
    [HutongGames.PlayMaker.Tooltip("Parses an FMETP announce/log line or raw JSON payload and outputs ip, uuid, and headsetName.")]
    public class PM_FMETP_JSON_Parse : FsmStateAction
    {
        [RequiredField]
        [HutongGames.PlayMaker.Tooltip("Either the full log line (e.g. [PM_FMETP_AnnounceClient] Sent key='' payload={\"ip\":\"192.168.68.115\",\"uuid\":\"4567\",\"headsetName\":\"Aaron Here\"}) or just the JSON fragment { ... }.")]
        public FsmString text;

        [HutongGames.PlayMaker.Tooltip("If true, treat Text as a full log line and auto-extract the JSON between braces. If false, Text is assumed to be pure JSON.")]
        public FsmBool textIncludesLogPrefix;

        [HutongGames.PlayMaker.Tooltip("Outputs: IP address parsed from payload.")]
        public FsmString ip;

        [HutongGames.PlayMaker.Tooltip("Outputs: UUID parsed from payload.")]
        public FsmString uuid;

        [HutongGames.PlayMaker.Tooltip("Outputs: Headset/device name parsed from payload.")]
        public FsmString headsetName;

        [HutongGames.PlayMaker.Tooltip("Outputs: The raw JSON that was parsed (useful for debugging).")]
        public FsmString rawJsonOut;

        [HutongGames.PlayMaker.Tooltip("Fired when parsing succeeds.")]
        public FsmEvent successEvent;

        [HutongGames.PlayMaker.Tooltip("Fired when parsing fails.")]
        public FsmEvent errorEvent;

        [HutongGames.PlayMaker.Tooltip("If parsing fails, this is set with a short error message.")]
        public FsmString error;

        public override void Reset()
        {
            text = string.Empty;
            textIncludesLogPrefix = true;

            ip = new FsmString { UseVariable = true };
            uuid = new FsmString { UseVariable = true };
            headsetName = new FsmString { UseVariable = true };
            rawJsonOut = new FsmString { UseVariable = true };
            error = new FsmString { UseVariable = true };

            successEvent = null;
            errorEvent = null;
        }

        public override void OnEnter()
        {
            DoParse();
            Finish();
        }

        void DoParse()
        {
            var input = text != null ? (text.Value ?? string.Empty) : string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                Fail("Empty input.");
                return;
            }

            string json = input;

            if (textIncludesLogPrefix.Value)
            {
                int open = input.IndexOf('{');
                int close = input.LastIndexOf('}');
                if (open < 0 || close <= open)
                {
                    Fail("Could not locate JSON braces in input.");
                    return;
                }
                json = input.Substring(open, (close - open) + 1);
            }

            json = json.Trim();
            // Defensive: replace single quotes around keys/values if present in logs
            json = json.Replace("'", "\"");

            try
            {
                var data = JsonUtility.FromJson<Payload>(json);
                if (data == null)
                {
                    Fail("JsonUtility returned null (malformed JSON?)");
                    return;
                }

                if (ip != null && !ip.IsNone) ip.Value = data.ip ?? string.Empty;
                if (uuid != null && !uuid.IsNone) uuid.Value = data.uuid ?? string.Empty;
                if (headsetName != null && !headsetName.IsNone) headsetName.Value = data.headsetName ?? string.Empty;
                if (rawJsonOut != null && !rawJsonOut.IsNone) rawJsonOut.Value = json;

                if (successEvent != null) Fsm.Event(successEvent);
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        void Fail(string message)
        {
            if (error != null && !error.IsNone) error.Value = message;
            if (errorEvent != null) Fsm.Event(errorEvent);
        }

        [Serializable]
        class Payload
        {
            public string ip;
            public string uuid;
            public string headsetName;
        }
    }
}
