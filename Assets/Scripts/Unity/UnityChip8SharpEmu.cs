using Chip8Sharp.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR || UNITY_STANDALONE
using SimpleFileBrowser;
#endif

namespace Chip8Sharp.Unity
{
    public class UnityChip8SharpEmu : MonoBehaviour
    {

        [SerializeField] private Texture2DScreenRenderer _unityScreenRenderer;
        [SerializeField] private UnityBeep _unityBeep;

        [SerializeField] private GameObject _titlePanel;
        [SerializeField] private GameObject _preloadedPanel;
        [SerializeField] private Button _selectROMButton;

        public bool StepThrough = false;

        private UnityUserInput _unityUserInput;
        private UnityRandomNumber _unityRandomNumber;
        private UnityLogger _unityLogger;

        public CPU CPU { get; private set; }

        public bool InitOnStart { get; set; } = true;

        private bool _gameLoaded = false;

        public void Init()
        {
            _unityUserInput = new UnityUserInput();
            _unityRandomNumber = new UnityRandomNumber();
            _unityLogger = new UnityLogger();

            CPU = new CPU(
                _unityScreenRenderer,
                _unityUserInput,
                _unityBeep,
                _unityRandomNumber,
                _unityLogger
                );

            QualitySettings.vSyncCount = 0;
        }

        void Start()
        {
#if UNITY_WEBGL
            _selectROMButton.gameObject.SetActive(false);

            Application.targetFrameRate = -1;
#endif

            if (InitOnStart)
            {
                Init();
            }
        }

        void Update()
        {
            if (_gameLoaded)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetKeyDown(KeyCode.Backspace))
                {
                    CPU.Reset();
                    _gameLoaded = false;

                    _titlePanel.SetActive(true);
                }
            }
        }

        void FixedUpdate()
        {
            if (StepThrough)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                {
                    UnityEngine.Debug.Log(CPU.Opcode.ToString("x"));
                    CPU.EmulateCycle();
                    CPU.SetKeys();
                }
            }
            else
            {
                if (_gameLoaded)
                {
                    CPU.EmulateCycle();
                    CPU.SetKeys();
                }
            }
        }

        public void SelectROMFile()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            FileBrowser.SetFilters(false, ".ch8");
            FileBrowser.ShowLoadDialog((files) =>
            {
                try
                {
                    var romBytes = System.IO.File.ReadAllBytes(files[0]);
                    LoadGame(romBytes);
                }
                catch (Exception e)
                {
                }
            },
            () => { },
            title: "Select Chip 8 Rom");
#endif
        }

        public void OpenPreloadedGame(string gameName)
        {
            StartCoroutine(LoadBytesFromStreamingAsset("Roms/" + gameName + ".ch8", (bytes) =>
            {
                if (bytes != null)
                {
                    LoadGame(bytes);
                }
            }
            ));
        }

        private void LoadGame(byte[] bytes)
        {
            CPU.LoadGame(bytes);
            _gameLoaded = true;

            _titlePanel.SetActive(false);
            _preloadedPanel.SetActive(false);
        }

        private IEnumerator LoadBytesFromStreamingAsset(string fileName, Action<byte[]> onFinished = null)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

            byte[] bytes;
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                var www = new WWW(filePath);
                yield return www;
                bytes = www.bytes;
            }
            else
            {
                bytes = System.IO.File.ReadAllBytes(filePath);
            }

            onFinished?.Invoke(bytes);
        }

    }
}