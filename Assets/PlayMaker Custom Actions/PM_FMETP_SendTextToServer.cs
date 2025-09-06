using HutongGames.PlayMaker;
using UnityEngine;
using FMSolution.FMNetwork;

namespace Net.PlayMaker.FMETP
{
    [ActionCategory("FMETP/Network")]
    [HutongGames.PlayMaker.Tooltip("Send a text message from a client to the FMETP server.")]
    public class PM_FMETP_SendTextToServer : FsmStateAction
    {
        [RequiredField, HutongGames.PlayMaker.Tooltip("GameObject with FMNetworkManager component")]
        public FsmGameObject fmNetworkObject;

        [RequiredField, UIHint(UIHint.TextArea)]
        public FsmString message;

        [HutongGames.PlayMaker.Tooltip("If true, send every frame.")]
        public FsmBool everyFrame;

        FMNetworkManager _fm;

        public override void Reset()
        {
            fmNetworkObject = null;
            message = string.Empty;
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
                Debug.LogWarning("[PM_FMETP_SendTextToServer] fmNetworkObject is null.");
                return false;
            }
            _fm = go.GetComponent<FMNetworkManager>();
            if (_fm == null)
            {
                Debug.LogWarning("[PM_FMETP_SendTextToServer] FMNetworkManager not found on GameObject.");
                return false;
            }
            return true;
        }

        void DoSend()
        {
            if (_fm == null) return;
            _fm.SendToServer(message.Value ?? string.Empty);
        }
    }
}
