using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using Random = UnityEngine.Random;

namespace Avo.TOD.Runtime
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class GameManager : UdonSharpBehaviour
    {
        #region Variables & Data

        [SerializeField] private PlayerManager playerManager;

        [Space]

        [Header("Displays")]
        [SerializeField] public TextMeshProUGUI questionDisplayedText;
        [SerializeField] public TextMeshProUGUI playerDisplayedText;

        [Space]

        [Header("Game Settings")]
        [SerializeField] private bool usePlaceholders;
        [SerializeField] public bool getAllPlayers;
        [SerializeField] private string playerPlaceholder = "<player>";

        // Truth & Dares
        [HideInInspector] public DataList _truths = new DataList();
        [HideInInspector] public DataList _dares = new DataList();
        [HideInInspector] public DataList _pTruths = new DataList();
        [HideInInspector] public DataList _pDares = new DataList();

        // Game Logic & Local Player
        private VRCPlayerApi _player;
        private int _id;

        // Synced Data
        [UdonSynced] private string _question;
        [UdonSynced] public int _playerID = -1;

        #endregion Variables & Data

        #region Start
        void Start()
        {
            _player = Networking.LocalPlayer;
        }
        #endregion Start

        #region Truth
        public void Truth()
        {
            Networking.SetOwner(_player, gameObject);
            _playerID = _player.playerId;

            if (usePlaceholders && playerManager.PlayerCount >= 3)
            {
                switch (Chance())
                {
                    case false:
                        _id = Random.Range(0, _truths.Count);
                        SetQuestion(1);
                        break;
                    case true:
                        _id = Random.Range(0, _pTruths.Count);
                        SetQuestion(2);
                        break;
                }
            }
            else
            {
                _id = Random.Range(0, _truths.Count);
                SetQuestion(1);
            }
        }
        #endregion Truth

        #region Dare
        public void Dare()
        {
            Networking.SetOwner(_player, gameObject);
            _playerID = _player.playerId;

            if (usePlaceholders && playerManager.PlayerCount >= 3)
            {
                switch (Chance())
                {
                    case false:
                        _id = Random.Range(0, _dares.Count);
                        SetQuestion(3);
                        break;
                    case true:
                        _id = Random.Range(0, _pDares.Count);
                        SetQuestion(4);
                        break;
                }
            }
            else
            {
                _id = Random.Range(0, _dares.Count);
                SetQuestion(3);
            }
        }
        #endregion Dare

        #region Chance and Set Question
        private bool Chance()
        {
            int chance = Random.Range(1, 2);

            switch (chance)
            {
                case 1:
                    return false;
                case 2:
                    return true;
            }

            return false;
        }

        private void SetQuestion(int value)
        {
            switch (value)
            {
                case 1:
                    _question = _truths[_id].String;
                    break;
                case 2:
                    _question = _pTruths[_id].String;
                    _question.Replace(playerPlaceholder, playerManager.GetRandomPlayer());
                    break;
                case 3:
                    _question = _dares[_id].String;
                    break;
                case 4:
                    _question = _pDares[_id].String;
                    _question.Replace(playerPlaceholder, playerManager.GetRandomPlayer());
                    break;
            }
            _UpdateQuestion();
        }
        #endregion Chance and Set Question

        #region Update Question and Serialization
        private void _UpdateQuestion()
        {
            playerDisplayedText.text = VRCPlayerApi.GetPlayerById(_playerID).displayName;
            questionDisplayedText.text = _question;
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (!Utilities.IsValid(VRCPlayerApi.GetPlayerById(_playerID))) return;
            playerDisplayedText.text = VRCPlayerApi.GetPlayerById(_playerID).displayName;
            questionDisplayedText.text = _question;
        }
        
        #endregion Update Question and Serialization
    }
}
