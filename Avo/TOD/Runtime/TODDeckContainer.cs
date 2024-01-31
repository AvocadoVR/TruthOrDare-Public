using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Avo.TOD.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TODDeckContainer : UdonSharpBehaviour
    {
        public string presetDeckName; //set in inspector to gen the button name
        public VRCUrl presetDeckURL; //address of the json file

        [HideInInspector] public DeckButton SetButton;
        
    }
}
