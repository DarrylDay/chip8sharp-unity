using Chip8Sharp.Debug;
using UnityEngine;

namespace Chip8Sharp.Unity
{
    public class UnityLogger : Debug.ILogger
    {
        public void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void DumpRAM(byte[] ram)
        {
            throw new System.NotImplementedException();
        }

        public void DumpGFX(byte[] gfx)
        {
            throw new System.NotImplementedException();
        }

        public void DumpRegisters(byte[] regs)
        {
            throw new System.NotImplementedException();
        }
    }
}

