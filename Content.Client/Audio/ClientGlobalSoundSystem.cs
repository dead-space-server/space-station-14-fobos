using Content.Shared.Audio;
using Content.Shared.CCVar;
using Content.Shared.DeadSpace.CCCCVars; // DS14
using Content.Shared.GameTicking;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client.Audio;

public sealed class ClientGlobalSoundSystem : SharedGlobalSoundSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    // Admin music
    private bool _adminAudioEnabled = true;
    private List<EntityUid?> _adminAudio = new(1);

    // Event sounds (e.g. nuke timer)
    private bool _eventAudioEnabled = true;
    private Dictionary<StationEventMusicType, EntityUid?> _eventAudio = new(1);

    // Alert level sounds
    private float _alertLevelVolume = 1f; // DS14

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeNetworkEvent<AdminSoundEvent>(PlayAdminSound);
        Subs.CVar(_cfg, CCVars.AdminSoundsEnabled, ToggleAdminSound, true);

        SubscribeNetworkEvent<StationEventMusicEvent>(PlayStationEventMusic);
        SubscribeNetworkEvent<StopStationEventMusic>(StopStationEventMusic);
        Subs.CVar(_cfg, CCVars.EventMusicEnabled, ToggleStationEventMusic, true);

        SubscribeNetworkEvent<GameGlobalSoundEvent>(PlayGameSound);
        SubscribeNetworkEvent<AlertLevelSoundEvent>(PlayAlertLevelSound); // DS14
        Subs.CVar(_cfg, CCCCVars.AlertLevelVolume, SetAlertLevelVolume, true); // DS-14
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        ClearAudio();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ClearAudio();
    }

    private void ClearAudio()
    {
        foreach (var stream in _adminAudio)
        {
            _audio.Stop(stream);
        }
        _adminAudio.Clear();

        foreach (var stream in _eventAudio.Values)
        {
            _audio.Stop(stream);
        }

        _eventAudio.Clear();
    }

    private void PlayAdminSound(AdminSoundEvent soundEvent)
    {
        if(!_adminAudioEnabled) return;

        var stream = _audio.PlayGlobal(soundEvent.Specifier, Filter.Local(), false, soundEvent.AudioParams);
        _adminAudio.Add(stream?.Entity);
    }

    private void PlayStationEventMusic(StationEventMusicEvent soundEvent)
    {
        // Either the cvar is disabled or it's already playing
        if(!_eventAudioEnabled || _eventAudio.ContainsKey(soundEvent.Type)) return;

        var stream = _audio.PlayGlobal(soundEvent.Specifier, Filter.Local(), false, soundEvent.AudioParams);
        _eventAudio.Add(soundEvent.Type, stream?.Entity);
    }

    private void PlayGameSound(GameGlobalSoundEvent soundEvent)
    {
        _audio.PlayGlobal(soundEvent.Specifier, Filter.Local(), false, soundEvent.AudioParams);
    }

    // DS14-start
    private void PlayAlertLevelSound(AlertLevelSoundEvent soundEvent)
    {
        var audioParams = soundEvent.AudioParams ?? AudioParams.Default;
        audioParams = audioParams.AddVolume(SharedAudioSystem.GainToVolume(_alertLevelVolume));
        _audio.PlayGlobal(soundEvent.Specifier, Filter.Local(), false, audioParams);
    }
    // DS14-end

    private void StopStationEventMusic(StopStationEventMusic soundEvent)
    {
        if (!_eventAudio.TryGetValue(soundEvent.Type, out var stream))
            return;

        _audio.Stop(stream);
        _eventAudio.Remove(soundEvent.Type);
    }

    private void ToggleAdminSound(bool enabled)
    {
        _adminAudioEnabled = enabled;
        if (_adminAudioEnabled) return;
        foreach (var stream in _adminAudio)
        {
            _audio.Stop(stream);
        }
        _adminAudio.Clear();
    }

    private void ToggleStationEventMusic(bool enabled)
    {
        _eventAudioEnabled = enabled;
        if (_eventAudioEnabled) return;
        foreach (var stream in _eventAudio)
        {
            _audio.Stop(stream.Value);
        }
        _eventAudio.Clear();
    }

    // DS14-start
    private void SetAlertLevelVolume(float volume)
    {
        _alertLevelVolume = volume;
    }
    // DS14-end
}
