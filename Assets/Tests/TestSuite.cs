using System.Collections.Generic;
using Chip8Sharp.Unity;
using Chip8Sharp.Core;
using NUnit.Framework;
using UnityEngine;

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
            _emulator.InitOnStart = false;
            _emulator.Init();
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
                0x60, 0x12,     // Set V[0] = 0x12
                0x40, 0x12,     // if (V[0] != 0x12) skipNextInstruction
                0x61, 0x69,     // Set V[1] = 0x69
                0x00, 0xE0      // Clear display
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[1] == 0x69);
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

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[0] == (vX - vY));

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            vX = CPU.V[4];
            vY = CPU.V[6];
            var result = (vX - vY) + 256;

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
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

            Assert.True(CPU.V[0xF] == 1);
            Assert.True(CPU.V[0] == (vY - vX));

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            vX = CPU.V[4];
            vY = CPU.V[6];
            var result = (vY - vX) + 256;

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 0);
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

        [Test]
        public void OpcodeBNNN()
        {
            var romBytes = new List<byte>()
            {
                0x60, 0x04,     // Set V[0] = 0x04
                0xB3, 0x02,     // PC = V[0] + 0x302 
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == 0x04);

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x306);
        }

        [Test]
        public void OpcodeCXNN()
        {
            var romBytes = new List<byte>()
            {
                0xC0, 0x33,     // Set V[0] = rand() & 0b00110011
                0xC0, 0x33,     // Set V[0] = rand() & 0b00110011
                0xC0, 0xFF,     // Set V[0] = rand() & 0b11111111
                0xC0, 0xFF,     // Set V[0] = rand() & 0b11111111
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            Assert.True((CPU.V[0] & 0b11001100) == 0);

            CPU.EmulateCycle();

            Assert.True((CPU.V[0] & 0b11001100) == 0);

            CPU.EmulateCycle();

            var rand1 = CPU.V[0];

            CPU.EmulateCycle();

            Assert.True(CPU.V[0] != rand1);
        }

        [Test]
        public void OpcodeDXYN()
        {
            var romBytes = new List<byte>()
            {
                0x61, 0x06,     // Set V[1] = 0x06
                0x62, 0x01,     // Set V[2] = 0x01
                0xD0, 0x05,     // Draw 0 (4x5) at (0,0)
                0xD1, 0x15,     // Draw 0 (4x5) at (6,6)
                0xD2, 0x25,     // Draw 0 (4x5) at (1,1)
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/debug/gfx.dat", CPU.GFX);

            for (int h = 0; h < 5; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    Assert.True((CPU.GFX[w + (h * 64)]) > 0 == (CPU.RAM[h] & (0b10000000 >> w)) > 0);
                }
            }
            Assert.True(CPU.V[0xF] == 0);

            CPU.EmulateCycle();

            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/debug/gfx2.dat", CPU.GFX);

            for (int h = 0; h < 5; h++)
            {
                for (int w = 0; w < 4; w++)
                {
                    Assert.True((CPU.GFX[6 + w + ((6 + h) * 64)]) > 0 == (CPU.RAM[h] & (0b10000000 >> w)) > 0);
                }
            }
            Assert.True(CPU.V[0xF] == 0);

            CPU.EmulateCycle();

            Assert.True(CPU.V[0xF] == 1);
        }

        [Test]
        public void OpcodeEX9E()
        {
            var romBytes = new List<byte>()
            {
                0x61, 0x06,     // Set V[1] = 0x06
                0xE0, 0x9E,     // if(key[V[0]] == 1)
                0x00, 0xE0,
                0x00, 0xE0,
                0xE0, 0x9E,     // if(key[V[0]] == 1)
                0x00, 0xE0,
                0x00, 0xE0,
                0xE1, 0x9E,     // if(key[V[1]] == 1)
                0x00, 0xE0,
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            CPU.Keys[0] = 1;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x206);

            CPU.EmulateCycle();

            CPU.Keys[0] = 0;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x20A);

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            CPU.Keys[0x6] = 1;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x212);
        }

        [Test]
        public void OpcodeEXA1()
        {
            var romBytes = new List<byte>()
            {
                0x61, 0x06,     // Set V[1] = 0x06
                0xE0, 0xA1,     // if(key[V[0]] != 1)
                0x00, 0xE0,
                0x00, 0xE0,
                0xE0, 0xA1,     // if(key[V[0]] != 1)
                0x00, 0xE0,
                0x00, 0xE0,
                0xE1, 0xA1,     // if(key[V[1]] != 1)
                0x00, 0xE0,
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            CPU.Keys[0] = 1;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x204);

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            CPU.Keys[0] = 0;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x20C);

            CPU.EmulateCycle();

            CPU.Keys[0x6] = 1;

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x210);
        }

        [Test]
        public void OpcodeFX07()
        {
            var romBytes = new List<byte>()
            {
                0x00, 0xE0,
                0x00, 0xE0,
                0x00, 0xE0,
                0xF1, 0x07,     // Set V[1] = delayTimer
                0x00, 0xE0,
                0x00, 0xE0,
                0xF5, 0x07,     // Set V[5] = delayTimer
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[1] == CPU.DelayTimer);

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[5] == CPU.DelayTimer);
        }

        [Test]
        public void OpcodeFX0A()
        {
            var romBytes = new List<byte>()
            {
                0xF2, 0x0A,     // Halt V[2] = getKey
                0x00, 0xE0,
                0xF5, 0x0A,     // Halt V[5] = getKey
                0x00, 0xE0,
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x202);

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x202);

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x202);

            CPU.Keys[0x3] = 1;
            CPU.SetKeys(true);

            CPU.EmulateCycle();
            
            Assert.True(CPU.V[2] == 0x3);
            Assert.True(CPU.PC == 0x204);

            CPU.Keys[0x3] = 0;
            CPU.Keys[0x7] = 1;
            CPU.SetKeys(true);

            CPU.EmulateCycle();
            CPU.SetKeys(true);

            Assert.True(CPU.PC == 0x206);
            Assert.True(CPU.V[5] == 0x7);

            CPU.EmulateCycle();

            Assert.True(CPU.PC == 0x208);
        }

        [Test]
        public void OpcodeFX15()
        {
            var romBytes = new List<byte>()
            {
                0x61, 0x32,     // V[1] = 0x32
                0x65, 0x66,     // V[5] = 0x66
                0x00, 0xE0,
                0xF1, 0x15,     // Set delayTimer = V[1]
                0x00, 0xE0,
                0xF5, 0x15,     // Set delayTimer = V[5]
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.DelayTimer == CPU.V[1]);

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.DelayTimer == CPU.V[5]);
        }

        [Test]
        public void OpcodeFX18()
        {
            var romBytes = new List<byte>()
            {
                0x61, 0x32,     // V[1] = 0x32
                0x65, 0x66,     // V[5] = 0x66
                0x00, 0xE0,
                0xF1, 0x18,     // Set soundTimer = V[1]
                0x00, 0xE0,
                0xF5, 0x18,     // Set soundTimer = V[5]
                0x00, 0xE0,
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.SoundTimer == CPU.V[1]);

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.SoundTimer == CPU.V[5]);
        }

        [Test]
        public void OpcodeFX1E()
        {
            var romBytes = new List<byte>()
            {
                0x64, 0x32,     // V[4] = 0x32
                0xA0, 0x04,     // I = 0x004
                0xF4, 0x1E,     // I += V[4]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.I == 0x036);
        }

        [Test]
        public void OpcodeFX29()
        {
            var romBytes = new List<byte>()
            {
                0x62, 0x02,     // V[2] = 0x02
                0x66, 0x06,     // V[6] = 0x06
                0x6D, 0x0D,     // V[D] = 0x0D
                0xF2, 0x29,     // I=sprite_addr[V[2]]
                0xF6, 0x29,     // I=sprite_addr[V[6]]
                0xFD, 0x29,     // I=sprite_addr[V[D]]
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            CPU.EmulateCycle();
            Assert.True(CPU.I == (5 * 2));

            CPU.EmulateCycle();
            Assert.True(CPU.I == (5 * 6));

            CPU.EmulateCycle();
            Assert.True(CPU.I == (5 * 13));
        }

        [Test]
        public void OpcodeFX33()
        {
            var romBytes = new List<byte>()
            {
                0xA1, 0x00,     // I = 0x100
                0x62, 0x7B,     // V[2] = 0d123
                0x66, 0x45,     // V[6] = 0d069
                0xF2, 0x33,     // set_BCD(V[2])
                0xF6, 0x33,     // set_BCD(V[6])
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            CPU.EmulateCycle();
            Assert.True(CPU.RAM[CPU.I] == 1);
            Assert.True(CPU.RAM[CPU.I+1] == 2);
            Assert.True(CPU.RAM[CPU.I+2] == 3);

            CPU.EmulateCycle();
            Assert.True(CPU.RAM[CPU.I] == 0);
            Assert.True(CPU.RAM[CPU.I + 1] == 6);
            Assert.True(CPU.RAM[CPU.I + 2] == 9);
        }

        [Test]
        public void OpcodeFX55()
        {
            var romBytes = new List<byte>()
            {
                0xA1, 0x00,     // I = 0x100
                0x60, 0x7B,     // V[0] = 0x7B
                0x61, 0x45,     // V[1] = 0x45
                0x62, 0x69,     // V[2] = 0x69
                0x63, 0xA2,     // V[3] = 0xA2
                0xF3, 0x55,     // reg_dump(V[3],&I)
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.RAM[CPU.I] == 0x7B);
            Assert.True(CPU.RAM[CPU.I + 1] == 0x45);
            Assert.True(CPU.RAM[CPU.I + 2] == 0x69);
            Assert.True(CPU.RAM[CPU.I + 3] == 0xA2);
        }

        [Test]
        public void OpcodeFX65()
        {
            var romBytes = new List<byte>()
            {
                0xA2, 0x08,     // I = 0x208
                0xF3, 0x65,     // reg_load(V[3],&I)
                0x00, 0xE0,
                0x00, 0xE0,
                0x69, 0x4D,     // V[0], V[1]
                0x3C, 0xFA,     // V[2], V[3] 
            };

            CPU.LoadGame(romBytes.ToArray());

            CPU.EmulateCycle();
            CPU.EmulateCycle();

            Assert.True(CPU.V[0] == 0x69);
            Assert.True(CPU.V[1] == 0x4D);
            Assert.True(CPU.V[2] == 0x3C);
            Assert.True(CPU.V[3] == 0xFA);
        }

    }
}
