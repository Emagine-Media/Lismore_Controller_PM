using HutongGames.PlayMaker;
using UnityEngine;
using FMSolution.FMNetwork;

namespace Net.PlayMaker.FMETP
{
    [ActionCategory("FMETP/Network")]
    [HutongGames.PlayMaker.Tooltip("Send a text message to a specific target IP (client). Use on the server or client as needed.")]
    public class PM_FMETP_SendTextToTarget : FsmStateAction
    {
        [RequiredField, HutongGames.PlayMaker.Tooltip("GameObject with FMNetworkManager component")]
        public FsmGameObject fmNetworkObject;

        [RequiredField, UIHint(UIHint.TextArea)]
        public FsmString message;

        [RequiredField, HutongGames.PlayMaker.Tooltip("Destination IPv4, e.g. 192.168.1.25")]
        public FsmString targetIP;

        [HutongGames.PlayMaker.Tooltip("If true, send every frame.")]
        public FsmBool everyFrame;

        FMNetworkManager _fm;

        public override void Reset()
        {
            fmNetworkObject = null;
            message = string.Empty;
            targetIP = "127.0.0.1";
            everyFrame = false;
        }

        public override void OnEnter()
        {
            if (!TryCache()) { Finish(); return; }
            DoSend();
            if (!everyFrame.Value) Finish();
        }

        public override void OnUpdate()
        {
            if (everyFrame.Value) DoSend();
        }

        bool TryCache()
        {
            var go = fmNetworkObject.Value;
            if (go == null)
            {
                Debug.LogWarning("[PM_FMETP_SendTextToTarget] fmNetworkObject is null.");
                return false;
            }
            _fm = go.GetComponent<FMNetworkManager>();
            if (_fm == null)
            {
                Debug.LogWarning("[PM_FMETP_SendTextToTarget] FMNetworkManager not found on GameObject.");
                return false;
            }
            return true;
        }

        void DoSend()
        {
            if (_fm == null) return;
            var ip = targetIP.Value ?? "127.0.0.1";
            _fm.SendToTarget(message.Value ?? string.Empty, ip);
        }
    }
}
