using Chip8Sharp.Graphics;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chip8Sharp.Unity
{
    [RequireComponent(typeof(RawImage))]
    public class Texture2DScreenRenderer : MonoBehaviour, IScreenRenderer
    {

        private Texture2D _screenTexture;

        public void Initialize()
        {
            if (_screenTexture != null)
                Texture2D.Destroy(_screenTexture);

            _screenTexture = new Texture2D(64, 32, TextureFormat.RGBA32, false);

            _screenTexture.anisoLevel = 1;
            _screenTexture.filterMode = FilterMode.Point;

            var rawImage = GetComponent<RawImage>();

            if (rawImage != null)
            {
                rawImage.texture = _screenTexture;
            }
            else
            {
                throw new Exception("Raw Image Component is NULL");
            }
        }

        public void DrawGFX(byte[] gfx)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    _screenTexture.SetPixel(x, (31 - y), new Color32(255, 255, 255, (byte)(255 * gfx[x + (y * 64)])));
                }
            }

            _screenTexture.Apply();
        }

    }
}

