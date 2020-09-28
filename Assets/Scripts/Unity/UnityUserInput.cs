using Chip8Sharp.Input;

namespace Chip8Sharp.Unity
{
    public class UnityUserInput : IUserInput
    {
        public void SetKeys(byte[] keys)
        {
            keys[0x1] = (byte)(UnityEngine.Input.GetKey("1") ? 1 : 0);
            keys[0x2] = (byte)(UnityEngine.Input.GetKey("2") ? 1 : 0);
            keys[0x3] = (byte)(UnityEngine.Input.GetKey("3") ? 1 : 0);
            keys[0xC] = (byte)(UnityEngine.Input.GetKey("4") ? 1 : 0);

            keys[0x4] = (byte)(UnityEngine.Input.GetKey("q") ? 1 : 0);
            keys[0x5] = (byte)(UnityEngine.Input.GetKey("w") ? 1 : 0);
            keys[0x6] = (byte)(UnityEngine.Input.GetKey("e") ? 1 : 0);
            keys[0xD] = (byte)(UnityEngine.Input.GetKey("r") ? 1 : 0);

            keys[0x7] = (byte)(UnityEngine.Input.GetKey("a") ? 1 : 0);
            keys[0x8] = (byte)(UnityEngine.Input.GetKey("s") ? 1 : 0);
            keys[0x9] = (byte)(UnityEngine.Input.GetKey("d") ? 1 : 0);
            keys[0xE] = (byte)(UnityEngine.Input.GetKey("f") ? 1 : 0);

            keys[0xA] = (byte)(UnityEngine.Input.GetKey("z") ? 1 : 0);
            keys[0x0] = (byte)(UnityEngine.Input.GetKey("x") ? 1 : 0);
            keys[0xB] = (byte)(UnityEngine.Input.GetKey("c") ? 1 : 0);
            keys[0xF] = (byte)(UnityEngine.Input.GetKey("v") ? 1 : 0);
        }
    }
}

