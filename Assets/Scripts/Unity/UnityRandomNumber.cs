using Chip8Sharp.Core;
using UnityEngine;

namespace Chip8Sharp.Unity
{
    public class UnityRandomNumber : IRandomNumber
    {
        public byte RandomNumber()
        {
            return (byte)Random.Range(0, 255);
        }
    }
}
