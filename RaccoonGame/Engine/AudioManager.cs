using System;
using System.IO;
using System.Runtime.CompilerServices;
using FMOD;
using FMOD.Studio;

public class AudioManager
{
    private FMOD.Studio.System studioSystem;
    private FMOD.System coreSystem;

    //something something cache the banks to keep them in memory i dunno man
    private Bank masterBank;
    private Bank stringsBank;

    //APPARANTLY, people DONT like their ears bleeding, so I LEGALLY MUST INCLUDE VOLUME CONTROL.
    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    //something something dont be ass
    private EventInstance currentBgmInstance;
    private string currentBgmPath = "";

    /// <sumarry>
    /// Initializes FMOD for use in the project.
    /// </sumarry>
    /// <param name="banksFolderPath">The relative or absolution path to the exported FMOD shit</param>
    public void Initialize(string banksFolderPath)
    {
        //init systems
        FMOD.Studio.System.create(out studioSystem).CheckResult();
        studioSystem.getCoreSystem(out coreSystem).CheckResult();

        //create 32 virtual audio channels because i said so
        studioSystem.initialize(32, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero).CheckResult();

        //load in banks
        string masterPath = Path.Combine(banksFolderPath, "Master.bank");
        string stringsPath = Path.Combine(banksFolderPath, "Master.strings.bank");

        if(!File.Exists(masterPath) || !File.Exists(stringsPath))
        {
            throw new FileNotFoundException($"FMOD Banks not found. Ensure 'Master.bank' and 'Master.strings.bank' exist in: {banksFolderPath}");
        }

        studioSystem.loadBankFile(masterPath, LOAD_BANK_FLAGS.NORMAL, out masterBank).CheckResult();
        studioSystem.loadBankFile(stringsPath, LOAD_BANK_FLAGS.NORMAL, out stringsBank).CheckResult();

        //something about a pause menu... am i gonna implement one before i lose motivation? who fucking knows.
        studioSystem.getBus("bus:/", out masterBus);
        studioSystem.getBus("bus:/Music", out musicBus);
        studioSystem.getBus("bus:/SFX", out sfxBus);
    }


    /// <summary>
    /// does pretty much everything per frame
    /// </summary>
    public void Update()
    {
        studioSystem.update().CheckResult();
    }

    /// <summary>
    /// plays BGM base on an FMOD event path
    /// </summary>
    /// <param name="eventName"> the name of the event to play</param>
    public void PlayBGM(string eventName)
    {
        string fullEventPath = $"event:/BGM/{eventName}";

        //if the same shit needs to play, just keep playing it
        if (currentBgmPath == fullEventPath) return;

        //stop the previous song with whatever fadeout
        if (currentBgmInstance.isValid())
        {
            currentBgmInstance.stop(STOP_MODE.ALLOWFADEOUT);
            currentBgmInstance.release();
        }

        // find, create, and trigger the new event instance
        RESULT result = studioSystem.getEvent(fullEventPath, out EventDescription eventDesc);
        if(result == RESULT.ERR_EVENT_NOTFOUND)
        {
            Console.WriteLine($"[Audio Warning] BGM Event not found in banks: {fullEventPath}");
            currentBgmPath = "";
            return;
        }

        eventDesc.createInstance(out currentBgmInstance).CheckResult();
        currentBgmInstance.start().CheckResult();

        currentBgmPath = fullEventPath;
    }

    /// <summary>
    /// changes a param for the audio
    /// </summary>
    public void SetBgmParameter(string paramName, float value)
    {
        if(currentBgmInstance.isValid())
        {
            currentBgmInstance.setParameterByName(paramName, value).CheckResult();
        }
    }

    /// <summary>
    /// instantly triggers a one-off sound
    /// </summary>
    public void PlayOneShot(string sfxName)
    {
        string fullPath = $"event:/SFX/{sfxName}";
        if(studioSystem.getEvent(fullPath, out EventDescription eventDesc) == RESULT.OK)
        {
            eventDesc.createInstance(out EventInstance instance);
            instance.start();
            instance.release(); // immediately release because this is a one shot
        }
    }

    //mixer controls

    public void SetMasterVolume(float volume) => masterBus.setVolume(Math.Clamp(volume, 0.0f, 1.0f));
    public void SetMusicVolume(float volume) => musicBus.setVolume(Math.Clamp(volume, 0.0f, 1.0f));
    public void SetSfxVolume(float volume) => sfxBus.setVolume(Math.Clamp(volume, 0.0f, 1.0f));

    /// <summary>
    /// safely halts FMOD
    /// </summary>
    public void ShutDown()
    {
        if (currentBgmInstance.isValid())
        {
            currentBgmInstance.stop(STOP_MODE.IMMEDIATE);
            currentBgmInstance.release();
        }
        masterBank.unload();
        stringsBank.unload();
        studioSystem.release();
    }
}

/// <summary>
/// helper extension for error handling
/// </summary>
public static class FmodExtensions
{
    public static void CheckResult(this RESULT result)
    {
        if(result != RESULT.OK)
        {
            throw new Exception($"FMOD Core/Studio Error encountered: {result}");
        }
    }
}