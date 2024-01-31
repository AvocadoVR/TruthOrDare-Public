using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Avo.TOD.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TODInteract : UdonSharpBehaviour
    {
        public UdonBehaviour _gameManager;
        public string eventName;
        
        public override void Interact()
        {
            _gameManager.SendCustomEvent(eventName);
        }
    }
}
