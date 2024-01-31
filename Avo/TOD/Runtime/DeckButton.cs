using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using VRC.SDKBase;
using VRC.Udon;


namespace Avo.TOD.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DeckButton : UdonSharpBehaviour
    {
        public URLLoader UIController;
        public TODDeckContainer assignedSet;
        public TextMeshProUGUI buttonText;

        private VRCPlayerApi _player;

        public void Start()
        {
            _player = Networking.LocalPlayer;
        }

        public void OnClick()
        {
            if (UIController._IsMasterLocked && !_player.isMaster)
            {
                UIController.Error("MasterLocked");
                return;
            }
            UIController.LoadSetDataContainer(assignedSet);
        }

        public void SetTODSetContainer(TODDeckContainer InputWorldData)
        {
            assignedSet = InputWorldData;
            buttonText.text = InputWorldData.presetDeckName;
        }
    }
}