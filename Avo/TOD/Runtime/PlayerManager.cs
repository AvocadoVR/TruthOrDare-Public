using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Random = UnityEngine.Random;

namespace Avo.TOD.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PlayerManager : UdonSharpBehaviour
    {
        public int PlayerCount //number of players opted in
        {
            get
            {
                return _playersList.Length;
            }
        }

        [Header("Required Udon")]
        [SerializeField] private GameManager gameManager;
        [Space]

        [SerializeField] private GameObject joinButton;
        [SerializeField] private GameObject leaveButton;
        [Space]
        public GameObject[] templates;
        private TextMeshProUGUI[] _templateNames;
        #region Variables & Data

        // Synced Data
        [UdonSynced] private int[] _players;

        // Local Data
        private VRCPlayerApi _player;
        private bool _isRateLimited;
        private bool _hasJoined;
        private int[] _playersList;

        #endregion

        void Start()
        {
            _templateNames = new TextMeshProUGUI[templates.Length];

            for (int i = 0; i < templates.Length; i++)
            {
                _templateNames[i] = templates[i].GetComponentInChildren<TextMeshProUGUI>();
            }

            _players = new[] { -1 };
            _player = Networking.LocalPlayer;
        }

        public void RateLimit() => _isRateLimited = false;

        public void GetAllPlayers()
        {
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);

            int[] playerIDs = new int[VRCPlayerApi.GetPlayerCount()];

            for (var i = 0; i < players.Length; i++)
            {
                playerIDs[i] = players[i].playerId;
            }

            _players = playerIDs;
            _UpdateList();
            _UpdatePlayers();
        }

        public string GetRandomPlayer()
        {
            int randomIndex = Random.Range(0, _playersList.Length);

            return VRCPlayerApi.GetPlayerById(_playersList[randomIndex]).displayName;
        }

        public void Join()
        {
            if (!Utilities.IsValid(_player) || _isRateLimited) return;
            Networking.SetOwner(_player, gameObject);
            _hasJoined = true;
            _isRateLimited = true;

            joinButton.SetActive(false);
            leaveButton.SetActive(true);

            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            Add(_player.playerId);
        }

        public void Leave()
        {
            if (!Utilities.IsValid(_player) || _isRateLimited) return;
            Networking.SetOwner(_player, gameObject);
            _hasJoined = false;
            _isRateLimited = true;

            leaveButton.SetActive(false);
            joinButton.SetActive(true);

            SendCustomEventDelayedSeconds(nameof(RateLimit), 5);

            Remove(_player.playerId);
        }

        public void _UpdateList()
        {
            if (_players.Length == 1) return;

            int[] _temp = new int[_players.Length - 1];

            int j = 0;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] == _player.playerId) continue;
                _temp[j++] = _players[i];
            }

            _playersList = _temp;
            _UpdatePlayers();
        }

        public void _UpdatePlayers()
        {
            if (_players[0] == -1)
            {
                foreach (GameObject Template in templates)
                {
                    Template.SetActive(false);
                }
                return;
            }

            foreach (GameObject Template in templates)
            {
                Template.SetActive(false);
            }

            for (int i = 0; i < _players.Length; i++)
            {
                string playerName = _player.displayName;
                if (string.IsNullOrEmpty(playerName)) continue;

                _templateNames[i].text = playerName;
                templates[i].SetActive(true);
            }
        }

        public override void OnDeserialization()
        {
            if (gameManager.getAllPlayers)
            {
                GetAllPlayers();
            }
            else
            {
                _UpdateList();
                _UpdatePlayers();
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            RequestSerialization();
        }

        #region Add & Remove

        public void Add(int id)
        {
            if (_players.Length == 1 && _players[0] == -1)
            {
                _players[0] = id;
                RequestSerialization();
                _UpdateList();
                _UpdatePlayers();
            }
            else
            {
                int[] _temp = new int[_players.Length + 1];

                Array.Copy(_players, _temp, _players.Length);

                _temp[_temp.Length - 1] = id;

                _players = _temp;
                RequestSerialization();
                _UpdateList();
                _UpdatePlayers();
            }
        }

        public void Remove(int id)
        {
            if (_players.Length == 1 && _players[0] != -1)
            {
                _players[0] = -1;
                RequestSerialization();
                _UpdateList();
                _UpdatePlayers();
            }
            else
            {
                int[] _temp = new int[_players.Length - 1];
                int g = 0;

                for (int i = 0; i < _players.Length; i++)
                {

                    if (_players[i] == id) continue;
                    _temp[g++] = _players[i];
                }

                _players = _temp;
                RequestSerialization();
                _UpdateList();
                _UpdatePlayers();
            }
        }

        #endregion
    }
}
