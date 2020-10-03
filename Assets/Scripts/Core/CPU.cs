using Chip8Sharp.Graphics;
using Chip8Sharp.Input;
using Chip8Sharp.Debug;
using Chip8Sharp.Sound;

namespace Chip8Sharp.Core
{
    public class CPU
    {

        public ushort AddressPointer => I;

        // Opcode
        public ushort Opcode { get; private set; }

        // Memory
        public byte[] RAM { get; private set; } = new byte[4096];
        public ushort I { get; private set; }
        public ushort PC { get; private set; }

        // Registers
        public byte[] V { get; private set; } = new byte[16];

        // Graphics
        public byte[] GFX { get; private set; } = new byte[64 * 32];

        // Timers
        public byte DelayTimer { get; private set; }
        public byte SoundTimer { get; private set; }
        private byte _60HzCounter;
        private float _clockSpeed = 500f;

        // Stack
        public ushort[] Stack { get; private set; } = new ushort[16];
        public ushort StackPointer { get; private set; }

        // Input
        public byte[] Keys { get; private set; } = new byte[16];
        private bool _keyPressHault = false;
        private byte _keyVIndex;

        // Interfaces
        private IScreenRenderer _screenRenderer;
        private IUserInput _userInput;
        private IBeep _beep;
        private IRandomNumber _randomNumber;
        private ILogger _logger;

        // Font Set
        private readonly byte[] _chip8Fontset = new byte[80]
        {
          0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
          0x20, 0x60, 0x20, 0x20, 0x70, // 1
          0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
          0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
          0x90, 0x90, 0xF0, 0x10, 0x10, // 4
          0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
          0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
          0xF0, 0x10, 0x20, 0x40, 0x40, // 7
          0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
          0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
          0xF0, 0x90, 0xF0, 0x90, 0x90, // A
          0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
          0xF0, 0x80, 0x80, 0x80, 0xF0, // C
          0xE0, 0x90, 0x90, 0x90, 0xE0, // D
          0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
          0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public CPU(
            IScreenRenderer screenRenderer, 
            IUserInput userInput,
            IBeep beep,
            IRandomNumber randomNumber,
            ILogger logger = null)
        {
            _screenRenderer = screenRenderer;
            _userInput = userInput;
            _beep = beep;
            _randomNumber = randomNumber;
            _logger = logger;
        }

        public void Reset()
        {
            PC = 0x200;            // Program counter starts at 0x200
            Opcode = 0;            // Reset current opcode	
            I = 0;                 // Reset index register
            StackPointer = 0;      // Reset stack pointer
            DelayTimer = 0;        // Reset delay timer
            SoundTimer = 0;        // Reset sound timer

            // Init display
            for (int i = 0; i < (64 * 32); i++)
            {
                GFX[i] = 0;
            }
            _screenRenderer.Initialize();

            // Clear stack and registers
            for (int i = 0; i < 16; i++)
            {
                Stack[i] = 0;
                V[i] = 0;
            }

            // Clear RAM
            for (int i = 0; i < 4096; i++)
            {
                RAM[i] = 0;
            }

            // Load fontset
            for (int i = 0; i < 0x50; ++i)
                RAM[i] = _chip8Fontset[i];

            _logger?.Log("Chip 8 Emulator Reset");
        }

        public void LoadGame(byte[] romBytes)
        {
            // Load rom bytes into RAM
            for (int i = 0; i < romBytes.Length; ++i)
            {
                RAM[i + 0x200] = romBytes[i];
            }

            // Halt at end of program
            RAM[(0x200 + romBytes.Length)] = 0x0E;
            RAM[(0x200 + romBytes.Length + 1)] = 0xFD;

            _logger?.Log("Chip 8 ROM Loaded - " + romBytes.Length + " Bytes");
        }

        public void EmulateCycle()
        {
            if (!_keyPressHault)
            {
                bool skipNextInstruction = false;
                bool incrementCounter = true;
                bool drawFlag = false;

                // Fetch
                Opcode = (ushort)(RAM[PC] << 8 | RAM[PC + 1]);
                byte F = 0xF;
                byte X = (byte)((Opcode & 0x0F00) >> 8);
                byte Y = (byte)((Opcode & 0x00F0) >> 4);
                byte N = (byte)(Opcode & 0x000F);
                byte NN = (byte)(Opcode & 0x00FF);
                ushort NNN = (ushort)(Opcode & 0x0FFF);

                // Decode & Excute
                switch ((Opcode) & 0xF000)
                {
                    case 0x0000:

                        switch ((Opcode) & 0xFFF)
                        {
                            case 0x0E0:

                                // Clears the screen.
                                for (int i = 0; i < (64 * 32); i++)
                                {
                                    GFX[i] = 0;
                                }

                                drawFlag = true;

                                break;
                            case 0x0EE:

                                // Returns from a subroutine.
                                StackPointer--;
                                PC = Stack[StackPointer];

                                break;
                            default:

                                _logger?.Log("Invalid opcode = " + Opcode.ToString());

                                break;
                        }

                        break;
                    case 0x1000:

                        // Jumps to address NNN.
                        PC = NNN;
                        incrementCounter = false;

                        break;
                    case 0x2000:

                        // Calls subroutine at NNN.
                        Stack[StackPointer] = PC;
                        ++StackPointer;
                        PC = NNN;
                        incrementCounter = false;

                        break;
                    case 0x3000:

                        // Skips the next instruction if VX equals NN. (Usually the next instruction is a jump to skip a code block)
                        if (V[X] == NN)
                        {
                            skipNextInstruction = true;
                        }

                        break;
                    case 0x4000:

                        // Skips the next instruction if VX doesn't equal NN. (Usually the next instruction is a jump to skip a code block)
                        if (V[X] != NN)
                        {
                            skipNextInstruction = true;
                        }

                        break;
                    case 0x5000:

                        // Skips the next instruction if VX equals VY. (Usually the next instruction is a jump to skip a code block)
                        if (V[X] == V[Y])
                        {
                            skipNextInstruction = true;
                        }

                        break;
                    case 0x6000:

                        // Sets VX to NN.
                        V[X] = NN;
                        break;

                    case 0x7000:

                        // Adds NN to VX. (Carry flag is not changed)
                        V[X] += NN;
                        break;

                    case 0x8000:

                        switch (Opcode & 0x00F)
                        {
                            case 0x000:

                                // Sets VX to the value of VY.
                                V[X] = V[Y];

                                break;
                            case 0x001:

                                // Sets VX to VX or VY. (Bitwise OR operation)
                                V[X] = (byte)(V[X] | V[Y]);

                                break;
                            case 0x002:

                                // Sets VX to VX and VY. (Bitwise AND operation)
                                V[X] = (byte)(V[X] & V[Y]);

                                break;
                            case 0x003:

                                // Sets VX to VX xor VY.
                                V[X] = (byte)(V[X] ^ V[Y]);

                                break;
                            case 0x004:

                                // Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                                int sumVxVy = V[X] + V[Y];

                                if (sumVxVy > 255)
                                {
                                    sumVxVy -= 256;
                                    V[F] = 0x01;
                                }
                                else
                                {
                                    V[F] = 0x00;
                                }

                                V[X] = (byte)sumVxVy;

                                break;
                            case 0x005:

                                // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                int subVxVy = V[X] - V[Y];

                                if (subVxVy < 0)
                                {
                                    subVxVy += 256;
                                    V[F] = 0x01;
                                }
                                else
                                {
                                    V[F] = 0x00;
                                }

                                V[X] = (byte)subVxVy;

                                break;
                            case 0x006:

                                // Stores the least significant bit of VX in VF and then shifts VX to the right by 1.[b]
                                V[F] = (byte)(V[X] & 0b00000001);
                                V[X] >>= 1;

                                break;
                            case 0x007:

                                // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                                int subVyVx = V[Y] - V[X];

                                if (subVyVx < 0)
                                {
                                    subVyVx += 256;
                                    V[F] = 0x01;
                                }
                                else
                                {
                                    V[F] = 0x00;
                                }

                                V[X] = (byte)subVyVx;

                                break;
                            case 0x00E:

                                // 	Stores the most significant bit of VX in VF and then shifts VX to the left by 1.[b]
                                V[F] = (byte)(V[X] & 0b10000000);
                                V[X] <<= 1;

                                break;
                            default:

                                _logger?.Log("Invalid opcode = " + Opcode.ToString());

                                break;
                        }

                        break;

                    case 0x9000:

                        // Skips the next instruction if VX doesn't equal VY. (Usually the next instruction is a jump to skip a code block)
                        if (V[X] != V[Y])
                        {
                            skipNextInstruction = true;
                        }

                        break;
                    case 0xA000:

                        // Sets I to the address NNN.
                        I = NNN;

                        break;
                    case 0xB000:

                        // 	Jumps to the address NNN plus V0.
                        PC = (ushort)(V[0] + NNN);
                        incrementCounter = false;

                        break;
                    case 0xC000:

                        // Sets VX to the result of a bitwise and operation on a random number (Typically: 0 to 255) and NN.
                        V[X] = (byte)(_randomNumber.RandomNumber() & NN);

                        break;
                    case 0xD000:

                        // Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels. 
                        // Each row of 8 pixels is read as bit-coded starting from memory location I; I value doesn’t change 
                        // after the execution of this instruction. As described above, VF is set to 1 if any screen pixels 
                        // are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen
                        int height = N;
                        ushort pixel;
                        
                        V[0xF] = 0;
                        for (int yline = 0; yline < height; yline++)
                        {
                            pixel = RAM[I + yline];
                            for (int xline = 0; xline < 8; xline++)
                            {
                                if ((pixel & (0b10000000 >> xline)) != 0)
                                {
                                    int posX = V[X] + xline;
                                    int posY = V[Y] + yline;

                                    // Check if sprite overflows screen
                                    if (posX >= 64)
                                        posX -= 64;
                                    if (posY >= 32)
                                        posY -= 32;

                                    // Check if pixel is already set, if so raise V[F] flag
                                    if (GFX[(posX + (posY * 64))] == 1)
                                    {
                                        V[0xF] = 1;
                                    }   

                                    GFX[posX + (posY * 64)] ^= 1;
                                }
                            }
                        }
                        drawFlag = true;

                        break;
                    case 0xE000:

                        switch (Opcode & 0x0FF)
                        {
                            case 0x09E:

                                // Skips the next instruction if the key stored in VX is pressed. (Usually the next instruction is a jump to skip a code block)
                                if (Keys[V[X]] == 1)
                                    skipNextInstruction = true;

                                break;
                            case 0x0A1:

                                // Skips the next instruction if the key stored in VX isn't pressed. (Usually the next instruction is a jump to skip a code block)
                                if (Keys[V[X]] == 0)
                                    skipNextInstruction = true;

                                break;
                            default:

                                _logger?.Log("Invalid opcode = " + Opcode.ToString());

                                break;
                        }

                        break;
                    case 0xF000:

                        switch (Opcode & 0x00FF)
                        {
                            case 0x007:

                                // Sets VX to the value of the delay timer.
                                V[X] = DelayTimer;

                                break;
                            case 0x00A:

                                // A key press is awaited, and then stored in VX. 
                                // (Blocking Operation. All instruction halted until next key event)
                                _keyPressHault = true;
                                _keyVIndex = X;

                                break;
                            case 0x015:

                                // Sets the delay timer to VX.	
                                DelayTimer = V[X];

                                break;
                            case 0x018:

                                // Sets the sound timer to VX.
                                SoundTimer = V[X];

                                break;
                            case 0x01E:

                                // Adds VX to I. VF is not affected.
                                I += V[X];

                                break;
                            case 0x029:

                                // Sets I to the location of the sprite for the character in VX. 
                                // Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                                I = (ushort)(V[X] * 5);

                                break;
                            case 0x033:

                                // Stores the binary-coded decimal representation of VX, with the most significant of three 
                                // digits at the address in I, the middle digit at I plus 1, and the least significant digit 
                                // at I plus 2. (In other words, take the decimal representation of VX, place the hundreds digit 
                                // in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.)
                                RAM[I] = (byte)(V[X] / 100);
                                RAM[I + 1] = (byte)((V[X] / 10) % 10);
                                RAM[I + 2] = (byte)((V[X] % 100) % 10);

                                break;
                            case 0x055:

                                // Stores V0 to VX (including VX) in memory starting at address I. 
                                // The offset from I is increased by 1 for each value written, but I itself is left unmodified.[d]
                                for (byte i = 0; i <= X; i++)
                                {
                                    RAM[I + i] = V[i];
                                }

                                break;
                            case 0x065:

                                // Fills V0 to VX (including VX) with values from memory starting at address I. 
                                // The offset from I is increased by 1 for each value written, but I itself is left unmodified.[d]
                                for (byte i = 0; i <= X; i++)
                                {
                                    V[i] = RAM[I + i];
                                }

                                break;
                            default:

                                _logger?.Log("Invalid opcode = " + Opcode.ToString("X"));

                                break;
                        }

                        break;
                }

                if (!_keyPressHault)
                {
                    if (skipNextInstruction)
                    {
                        PC += 4;
                    }
                    else if (incrementCounter)
                    {
                        PC += 2;
                    }
                }

                if (drawFlag)
                {
                    _screenRenderer.DrawGFX(GFX);
                }
            }

            _60HzCounter--;
            if (_60HzCounter <= 0)
            {
                _60HzCounter = (byte)(_clockSpeed / 60f);

                // Update timers
                if (DelayTimer > 0)
                    --DelayTimer;

                if (SoundTimer > 0)
                {
                    if (SoundTimer == 1)
                        _beep.Beep();
                    --SoundTimer;
                }
            }

        }

        public void SetKeys()
        {
            bool keyActive = false;
            byte keyNumber = 0;

            _userInput.SetKeys(Keys);

            // Check if a key is active
            for (byte i = 0; i < 16; i++)
            {
                if (Keys[i] == 1)
                {
                    keyActive = true;
                    keyNumber = i;
                }
            }

            // End key press hault if a key is pressed
            if (keyActive && _keyPressHault)
            {
                V[_keyVIndex] = (byte)keyNumber;
                _keyPressHault = false;
            }
        }

    }

}