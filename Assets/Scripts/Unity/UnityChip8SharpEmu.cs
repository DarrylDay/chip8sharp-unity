using SimpleFileBrowser;
using UnityEngine;
using Chip8Sharp.Core;


namespace Chip8Sharp.Unity
{
    public class UnityChip8SharpEmu : MonoBehaviour
    {

        [SerializeField] private Texture2DScreenRenderer _unityScreenRenderer;
        [SerializeField] private UnityBeep _unityBeep;

        private UnityUserInput _unityUserInput;
        private UnityRandomNumber _unityRandomNumber;
        private UnityLogger _unityLogger;

        private CPU _cpu;

        private bool _gameLoaded = false;
        private float _deltaCounter;

        void Start()
        {
            _unityUserInput = new UnityUserInput();
            _unityRandomNumber = new UnityRandomNumber();
            _unityLogger = new UnityLogger();

            _cpu = new CPU(
                _unityScreenRenderer,
                _unityUserInput,
                _unityBeep,
                _unityRandomNumber,
                _unityLogger
                );

            _cpu.Reset();

            FileBrowser.SetFilters(false, ".ch8");
            FileBrowser.ShowLoadDialog((files) => 
            {
                var romBytes = System.IO.File.ReadAllBytes(files[0]);
                _cpu.LoadGame(romBytes);
                _gameLoaded = true;
            }, 
            () => { }, 
            title: "Select Chip 8 Rom");
            
        }

        void Update()
        {
            _deltaCounter += Time.deltaTime;

            if (_deltaCounter >= (1f / 500f))
            {
                _deltaCounter = 0f;

                if (_gameLoaded)
                {
                    _cpu.EmulateCycle();
                    _cpu.SetKeys();
                }
            }
        }
    }
}