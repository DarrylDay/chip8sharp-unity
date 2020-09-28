namespace Chip8Sharp.Debug 
{
    public interface ILogger
    {
        void Log(string message);

        void DumpRAM(byte[] ram);
        void DumpGFX(byte[] gfx);
        void DumpRegisters(byte[] regs);
    }
}

