using System;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;
using PixelOven.Debug;

namespace monogame_project.Helper_Tools;

public static class FmodController
{
    // FMOD
    private static readonly INativeFmodLibrary _nativeLibrary = new DesktopNativeFmodLibrary();
    private static Bank _masterBank;
    private static Bus _masterBus;
    private static FMOD.ChannelGroup _channelGroup;

    private static DSP _pitchDsp;
    private static bool initlizedPitchDsp = false;

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

    private static async Task LoadContentAsync()
    {
        var native = _masterBus.Native;
        while (native.getChannelGroup(out _channelGroup) != RESULT.OK) { await Task.Yield(); }
        
        _masterBus.UnlockChannelGroup();
        _channelGroup.setPitch(Game1.GameAudioSpeedMod);

        if (CoreSystem.Native.createDSPByType(DSP_TYPE.PITCHSHIFT, out _pitchDsp) == RESULT.OK)
        {
            if (_channelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, _pitchDsp) == RESULT.OK)
            {
                _pitchDsp.setParameterFloat((int)FMOD.DSP_PITCHSHIFT.PITCH, Game1.FmodDspPitchMod);
                initlizedPitchDsp = true;
            }
        }
    }

    public static void LoadContent()
    {
        _masterBank = StudioSystem.LoadBank("ContactProtectFMOD/Build/Desktop/Master.bank");
        _masterBank = StudioSystem.LoadBank("ContactProtectFMOD/Build/Desktop/Master.strings.bank");
        _masterBank.LoadSampleData();
        
        // Loading busses must be after loading banks, for some reason
        _masterBus = StudioSystem.GetBus("bus:/Sounds");

        // Loads FMOD DSPs which won't load until the update after FMOD is initialized
        LoadContentAsync();
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

    public static float SemitoneToSpeedMultiplier(float semitone)
    {
        return MathF.Pow(2.0f, semitone / 12f);
    }

    #endregion
    
}