using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PreloadedROMs", menuName = "Tools/Create Preloaded ROMs Object", order = 1)]
public class PreloadedROMData : ScriptableObject
{
    public byte[] BlitzROM;
    public byte[] BreakoutROM;
    public byte[] PongROM;
    public byte[] SpaceInvadersROM;
    public byte[] TankROM;
    public byte[] TetrisROM;
    public byte[] UFOROM;

#if UNITY_EDITOR
    [MenuItem("Tools/LoadROMBytes")]
    public static void LoadROMBytes()
    {
        var data = Resources.Load<PreloadedROMData>("PreloadedROMData");
        data.BlitzROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Blitz [David Winter].ch8");
        data.BreakoutROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Breakout (Brix hack) [David Winter, 1997].ch8");
        data.PongROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Pong [Paul Vervalin, 1990].ch8");
        data.SpaceInvadersROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Space Invaders [David Winter].ch8");
        data.TankROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Tank.ch8");
        data.TetrisROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/Tetris [Fran Dachille, 1991].ch8");
        data.UFOROM = System.IO.File.ReadAllBytes(Application.dataPath + @"/StreamingAssets/Roms/UFO [Lutz V, 1992].ch8");
    }
#endif

}
