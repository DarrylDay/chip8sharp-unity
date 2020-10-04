using SimpleFileBrowser;
using UnityEngine;
using Chip8Sharp.Core;

namespace Chip8Sharp.Unity
{
    public class UnityChip8SharpEmu : MonoBehaviour
    {

        [SerializeField] private Texture2DScreenRenderer _unityScreenRenderer;
        [SerializeField] private UnityBeep _unityBeep;

        public bool StepThrough = false;

        private UnityUserInput _unityUserInput;
        private UnityRandomNumber _unityRandomNumber;
        private UnityLogger _unityLogger;

        public CPU CPU { get; private set; }

        public bool InitOnStart { get; set; } = true;

        private bool _gameLoaded = false;
        private float _deltaCounter;

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
        }

        void Start()
        {
            if (InitOnStart)
            {
                Init();

                FileBrowser.SetFilters(false, ".ch8");
                FileBrowser.ShowLoadDialog((files) =>
                {
                    var romBytes = System.IO.File.ReadAllBytes(files[0]);
                    CPU.LoadGame(romBytes);
                    _gameLoaded = true;
                },
                () => { },
                title: "Select Chip 8 Rom");
            }
        }

        void Update()
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
                _deltaCounter += Time.deltaTime;

                if (_deltaCounter >= (1f / 500f))
                {
                    _deltaCounter = 0f;

                    if (_gameLoaded)
                    {
                        CPU.EmulateCycle();
                        CPU.SetKeys();
                    }
                }
            }

        }
    }
}