using System;
using System.Net;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Avo.TOD.Runtime
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class URLLoader : UdonSharpBehaviour
    {
        #region Variables & Data

        [SerializeField] private DeckButton[] _deckButtons;
        [SerializeField] private TODDeckContainer[] _setContainers;
        [SerializeField] private int setsPerPage = 6;
        private int pages;
        private int singleEntries;
        private int currentPage;
   

        [Space] [Header("Game Instance")] public GameManager gameManager;

        [Space] [Header("URL Input")] 
        [SerializeField] private bool useDefaultURL;
        [SerializeField] private VRCUrl defaultURL;
        [SerializeField] private VRCUrlInputField _urlInputField;

        [Space]
        [Header("Buttons")]
        [SerializeField] private GameObject nextPageButton;
        [SerializeField] private GameObject lastPageButton;
        [SerializeField] private Button[] presets;


        [Space] 
        [Header("Master Lock")] 
        [SerializeField] private Toggle _masterLockToggle;

        [UdonSynced] public bool _IsMasterLocked = true;

        [Space] [Header("Loaded Set Info")] [SerializeField]
        private TextMeshProUGUI _deckName;

        [SerializeField] private TextMeshProUGUI _deckBy;
        [SerializeField] private TextMeshProUGUI _truthCount;
        [SerializeField] private TextMeshProUGUI _playerTruthCount;
        [SerializeField] private TextMeshProUGUI _dareCount;
        [SerializeField] private TextMeshProUGUI _playerDareCount;

        [Header("Error Handling")] [SerializeField]
        private TextMeshProUGUI _statusText;

        //Internal & Synced Variables
        private VRCPlayerApi _player;
        [UdonSynced] private VRCUrl _LoadedURL;

        #endregion Variables & Data

        #region Start, Master & Serialization

        void Start()
        {
            _player = Networking.LocalPlayer;
            GeneratePages();
            if (useDefaultURL) LoadURL(defaultURL);
        }

        public override void OnDeserialization()
        {
            _masterLockToggle.isOn = _IsMasterLocked;
            LoadURL(_LoadedURL);
        }

        public void MasterSwitch()
        {
            if (_IsMasterLocked && !_player.isMaster)
            {
                _masterLockToggle.isOn = _IsMasterLocked;
                Error("MasterLocked");
                return;
            }

            _IsMasterLocked = !_IsMasterLocked;
            _masterLockToggle.isOn = _IsMasterLocked;
            RequestSerialization();

        }

        #endregion Start, Master & Serialization

        #region Button Generatiom

        public void GeneratePages()
        {
            pages = _setContainers.Length / setsPerPage;
            singleEntries = _setContainers.Length % setsPerPage;
            GenData();
        }

        public void GenData()
        {
            // No Pages At All.
            if (pages == 0 && singleEntries == 0)
            {
                for (int i = singleEntries; i < setsPerPage; i++)
                {
                    _deckButtons[i].gameObject.SetActive(false);
                }
                return;
            }

            // If there are single entries on the last page
            if (currentPage == pages && singleEntries > 0)
            {
                for (int i = singleEntries; i < setsPerPage; i++)
                {
                    _deckButtons[i].gameObject.SetActive(false);
                }
                
                for (int i = pages + 1; i < singleEntries; i++)
                {
                    _deckButtons[i].SetTODSetContainer(_setContainers[i]);
                }
            }

            // If there are no pages but there are single entries
            if (pages == 0 && singleEntries > 0)
            {
                for (int i = singleEntries; i < setsPerPage; i++)
                {
                    _deckButtons[i].gameObject.SetActive(false);
                }
                
                for (int i = 0; i < singleEntries; i++)
                {
                    _deckButtons[i].SetTODSetContainer(_setContainers[i]);
                }
            }

            // If there are multiple pages or if currentPage is within valid range
            if (currentPage > 0 && currentPage <= pages)
            {
                for (int i = currentPage * pages; i < currentPage * pages + setsPerPage; i++)
                {
                    _deckButtons[i].SetTODSetContainer(_setContainers[i]);
                }
            }


            SetNextPageButton(currentPage < pages);
            SetLastPageButton(currentPage > 1);
        }

        public void NextPage()
        {
            currentPage++;
            GenData();
        }

        public void LastPage()
        {
            currentPage--;
            GenData();
        }
        
        public void SetNextPageButton(bool nextPage) => nextPageButton.SetActive(nextPage);
        public void SetLastPageButton(bool lastPage) => lastPageButton.SetActive(lastPage);


        #endregion

        #region URL Loading
        public void LoadURL(VRCUrl url)
        {
            VRCStringDownloader.LoadUrl(url, (IUdonEventReceiver)this);
        }

        public void LoadSetDataContainer(TODDeckContainer Instance)
        {
            Networking.SetOwner(_player, gameObject);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableRateLimit));
            _LoadedURL = Instance.presetDeckURL;
            LoadURL(_LoadedURL);
            RequestSerialization();
        }

        public void RequestURL()
        {
            if (_IsMasterLocked && !_player.isMaster)
            {
                Error("MasterLocked");
                return;
            }
            Networking.SetOwner(_player, gameObject);
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableRateLimit));
            _LoadedURL = _urlInputField.GetUrl();
            LoadURL(_LoadedURL);
            RequestSerialization();
        }
        #endregion URL Loading

        #region String Load Events
        public override void OnStringLoadSuccess(IVRCStringDownload WebRequest)
        {
            string json = WebRequest.Result;
            Debug.Log($"Successfully downloaded json {json}");

            if (VRCJson.TryDeserializeFromJson(json, out DataToken result))
            {
                result.DataDictionary.TryGetValue("DeckName", out DataToken deckName);
                result.DataDictionary.TryGetValue("DeckBy", out DataToken deckBy);
                result.DataDictionary.TryGetValue("Truths", out DataToken truths);
                result.DataDictionary.TryGetValue("Player_Truths", out DataToken pTruths);
                result.DataDictionary.TryGetValue("Dares", out DataToken dares);
                result.DataDictionary.TryGetValue("Player_Dares", out DataToken pDares);


                _deckName.text = deckName.String;
                _deckBy.text = deckBy.String;
                _truthCount.text = truths.DataList.Count.ToString();
                _playerTruthCount.text = pTruths.DataList.Count.ToString();
                _dareCount.text = dares.DataList.Count.ToString();
                _playerDareCount.text = pDares.DataList.Count.ToString();
                
                gameManager._truths = truths.DataList;
                gameManager._pTruths = pTruths.DataList;
                gameManager._dares = dares.DataList;
                gameManager._pDares = pDares.DataList;

                gameManager.playerDisplayedText.text = deckName.String;
                gameManager.questionDisplayedText.text = "By " + deckBy.String;

                SendCustomEventDelayedSeconds(nameof(DisableRateLimit), 10);
            }

        }

        public override void OnStringLoadError(IVRCStringDownload WebRequest)
        {
            Error("LoadError");
            gameManager.playerDisplayedText.text = "Error " + WebRequest.ErrorCode.ToString();
            gameManager.questionDisplayedText.text = WebRequest.Error;
            SendCustomEventDelayedSeconds(nameof(DisableRateLimit), 10);
        }
        #endregion String Load Events

        #region Rate Limiting
        public void EnableRateLimit()
        {
            _urlInputField.interactable = false;
            for (int i = 0; i < presets.Length; i++)
            {
                presets[i].interactable = false;
            }
        }

        public void DisableRateLimit()
        {
            _urlInputField.interactable = true;
            for (int i = 0; i < presets.Length; i++)
            {
                presets[i].interactable = true;
            }
        }

        #endregion Rate Limiting

        public void Error(string status)
        {
            switch (status)
            {
                case "MasterLocked":
                    _statusText.text = "[MasterLocked] Only instance master can currently do this.";
                    _statusText.color = Color.red;
                    break;
                case "LoadError":
                    _statusText.text = "Failed to Load JSON!";
                    _statusText.color = Color.red;
                    break;
            }
        }
    }

}
