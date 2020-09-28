namespace Chip8Sharp.Graphics
{
    public interface IScreenRenderer
    {
        void Initialize();

        void DrawGFX(byte[] gfx);
    }
}