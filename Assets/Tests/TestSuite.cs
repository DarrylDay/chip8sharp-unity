using System.Collections;
using System.Collections.Generic;
using Chip8Sharp.Unity;
using Chip8Sharp.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestSuite
    {

        private CPU CPU => _emulator.CPU;

        private UnityChip8SharpEmu _emulator;

        [SetUp]
        public void Setup()
        {
            _emulator = MonoBehaviour.Instantiate(Resources.Load<UnityChip8SharpEmu>("Emulator"));
            _emulator.Init();

            CPU.Reset();
        }

        [TearDown]
        public void Teardown()
        {
            GameObject.Destroy(_emulator.gameObject);
        }

        [Test]
        public void Opcode00E0()
        {
            var romBytes = new List<byte>()
            {
                0xD0, 0x05,
                0x00, 0xE0
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            bool pixelWasDrawn = false;

            foreach (var b in CPU.GFX)
            {
                if (b != 0x00)
                {
                    pixelWasDrawn = true;
                    break;
                }
            }

            if (!pixelWasDrawn)
            {
                Assert.Fail();
            }

            CPU.EmulateCycle();

            foreach (var b in CPU.GFX)
            {
                if (b != 0x00)
                {
                    Assert.Fail();
                    break;
                }
            }

            Assert.Pass();
        }

        [Test]
        public void Opcode00EE()
        {
            var romBytes = new List<byte>()
            {
                0x22, 0x08,     // Call subroutine at 0x208
                0x00, 0xE0,
                0x00, 0xE0,
                0x00, 0xE0,
                0x00, 0xE0,     // Subroutine start
                0x00, 0xEE,     // Return from subroutine
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            Assert.True(CPU.StackPointer == 1);

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x202, "PC == 0x202, was " + CPU.PC.ToString("x"));
            Assert.True(CPU.StackPointer == 0);
        }

        [Test]
        public void Opcode1NNN()
        {
            var romBytes = new List<byte>()
            {
                0x00, 0xE0,
                0x12, 0x08,     // Go to 0x208
                0x00, 0xE0,
                0x00, 0xE0,
                0x60, 0x12,     // Set V[0] = 0x12
                0x00, 0xE0,
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x208);
            Assert.True(CPU.Stack[0] != 0x202);
            Assert.True(CPU.StackPointer == 0);

            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == 0x12);
        }

        [Test]
        public void Opcode2NNN()
        {
            var romBytes = new List<byte>()
            {
                0x00, 0xE0,
                0x22, 0x08,     // Call subroutine at 0x0208
                0x00, 0xE0,
                0x00, 0xE0,
                0x60, 0x12,     // Set V[0] = 0x12
                0x00, 0xE0,
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x208);
            Assert.True(CPU.Stack[0] == 0x202);
            Assert.True(CPU.StackPointer == 1);

            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == 0x12);
        }

        [Test]
        public void Opcode3XNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x12,     // Set V[0] = 0x12
                0x30, 0x12,     // if (V[0] == 0x12) skipNextInstruction
                0x61, 0x69,     // Set V[1] = 0x69
                0x00, 0xE0      // Clear display
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[1] != 0x69);
        }

        [Test]
        public void Opcode4XNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x10,     // Set V[0] = 0x12
                0x40, 0x12,     // if (V[0] != 0x12) skipNextInstruction
                0x61, 0x69,     // Set V[1] = 0x69
                0x00, 0xE0      // Clear display
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[1] != 0x69);
        }

        [Test]
        public void Opcode5XNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x12,     // Set V[0] = 0x12
                0x61, 0x12,     // Set V[1] = 0x12
                0x50, 0x10,     // if (V[0] == 0x12) skipNextInstruction
                0x61, 0x69,     // Set V[2] = 0x69
                0x00, 0xE0      // Clear display
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[2] != 0x69);
        }

        [Test]
        public void Opcode6XNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x12,     // Set V[0] = 0x12
                0x6A, 0x24,     // Set V[A] = 0x24
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0x0] == 0x12);
            Assert.True(CPU.V[0xA] == 0x24);
        }

        [Test]
        public void Opcode7XNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x04,     // Set V[0] = 0x04
                0x70, 0x04,     // Set V[0] += 0x04
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == 0x08);
        }

        [Test]
        public void Opcode8XY0()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x10,     // V[0] = V[1]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == CPU.V[1]);
        }

        [Test]
        public void Opcode8XY1()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x11,     // V[0] = V[0] | V[1]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == (CPU.V[0] | CPU.V[1]));
        }

        [Test]
        public void Opcode8XY2()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x12,     // V[0] = V[0] & V[1]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == (CPU.V[0] & CPU.V[1]));
        }

        [Test]
        public void Opcode8XY3()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x13,     // V[0] = V[0] ^ V[1]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            var vX = CPU.V[0];
            var vY = CPU.V[1];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == (vX ^ vY));
        }

        [Test]
        public void Opcode8XY4()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x14,     // V[0] += V[1]
                0x64, 0xDD,     // Set V[4] = 0xDD
                0x66, 0xFF,     // Set V[6] = 0xFF
                0x84, 0x64,     // V[4] += V[6]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            var vX = CPU.V[0];
            var vY = CPU.V[1];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
            Assert.True(CPU.V[0] == (vX + vY));

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            vX = CPU.V[4];
            vY = CPU.V[6];
            var result = (vX + vY) - 256;

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[4] == result);
        }

        [Test]
        public void Opcode8XY5()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x44,     // Set V[0] = 0x33
                0x61, 0x33,     // Set V[1] = 0x44
                0x80, 0x15,     // V[0] -= V[1]
                0x64, 0xDD,     // Set V[4] = 0xDD
                0x66, 0xFF,     // Set V[6] = 0xFF
                0x84, 0x65,     // V[4] -= V[6]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            var vX = CPU.V[0];
            var vY = CPU.V[1];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
            Assert.True(CPU.V[0] == (vX - vY));

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            vX = CPU.V[4];
            vY = CPU.V[6];
            var result = (vX - vY) + 256;

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[4] == result);
        }

        [Test]
        public void Opcode8XY6()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0xF1,     // Set V[0] = 0xF1
                0x80, 0x16,     // V[F] = LSB(V[0]), V[0] >>= 1
                0x60, 0x10,     // Set V[0] = 0x10
                0x80, 0x16,     // V[F] = LSB(V[0]), V[0] >>= 1
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            var vX = CPU.V[0];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[0] == (vX >>= 1));

            CPU.EmulateCycle();

            vX = CPU.V[0];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
            Assert.True(CPU.V[0] == (vX >>= 1));
        }

        [Test]
        public void Opcode8XY7()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x33,     // Set V[0] = 0x33
                0x61, 0x44,     // Set V[1] = 0x44
                0x80, 0x17,     // V[0] = V[1] - V[0]
                0x64, 0xFF,     // Set V[4] = 0xFF
                0x66, 0xDD,     // Set V[6] = 0xDD
                0x84, 0x67,     // V[4] = V[6] - V[4]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            var vX = CPU.V[0];
            var vY = CPU.V[1];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
            Assert.True(CPU.V[0] == (vY - vX));

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            vX = CPU.V[4];
            vY = CPU.V[6];
            var result = (vY - vX) + 256;

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[4] == result);
        }

        [Test]
        public void Opcode8XYE()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0xF1,     // Set V[0] = 0xF1
                0x80, 0x1E,     // V[F] = MSB(V[0]), V[0] <<= 1
                0x60, 0x10,     // Set V[0] = 0x10
                0x80, 0x1E,     // V[F] = MSB(V[0]), V[0] <<= 1
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            var vX = CPU.V[0];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0b10000000);
            Assert.True(CPU.V[0] == (vX <<= 1));

            CPU.EmulateCycle();

            vX = CPU.V[0];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
            Assert.True(CPU.V[0] == (vX <<= 1));
        }

        [Test]
        public void OpcodeANNN()
        {
            var romBytes = new List<byte>()
            {
                0xA6, 0x96,     // I = 0x696
                0xA0, 0x22,     // I = 0x022 
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            Assert.True(CPU.I == 0x696);

            CPU.EmulateCycle();

            Assert.True(CPU.I == 0x022);
        }
    }
}
