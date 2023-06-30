using System;
using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;

namespace monogame_project.Helper_Tools;

public static class FmodController
{
    // FMOD
    private static readonly INativeFmodLibrary _nativeLibrary = new DesktopNativeFmodLibrary();
    private static Bank _masterBank;
    private static Bus _masterBus;
    private static FMOD.ChannelGroup _channelGroup;

    #region System
    public static void Init()
    {
        // FMOD Setup
        FmodManager.Init(_nativeLibrary, FmodInitMode.CoreAndStudio, "Content", preInitAction: PreInit);
    }

    private static void PreInit()
    {
        long seed = DateTime.Now.ToBinary();
        var advancedSettings = new ADVANCEDSETTINGS
        {
            randomSeed = ((uint)seed) ^ ((uint)(seed >> 32))
        };
        CoreSystem.Native.setAdvancedSettings(ref advancedSettings);
    }

    public static void LoadContent()
    {
        _masterBank = StudioSystem.LoadBank("ContactProtectFMOD/Build/Desktop/Master.bank");
        _masterBank = StudioSystem.LoadBank("ContactProtectFMOD/Build/Desktop/Master.strings.bank");
        _masterBank.LoadSampleData();
        
        // Loading busses must be after loading banks, for some reason
        _masterBus = StudioSystem.GetBus("bus:/Sounds");
    }

    public static void UnloadContent()
    {
        FmodManager.Unload();
        _masterBank.Unload();
    }

    public static void Update()
    {
        FmodManager.Update();
    }
    #endregion

    #region Game Specific

    public static void SetGameSpeed(float speed)
    {
        var native = _masterBus.Native;
        if (native.getChannelGroup(out _channelGroup) == RESULT.OK)
        {
            _masterBus.UnlockChannelGroup();
            _channelGroup.setPitch(speed);
        }
    }

    #endregion
}