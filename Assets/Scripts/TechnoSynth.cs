using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class TechnoSynth : MonoBehaviour
{
    // ... (기존 설정 변수들 동일) ...
    [Header("Global Settings")]
    [Range(60, 180)] public double bpm = 128.0;
    [Range(0, 1)] public float masterVolume = 0.5f;

    // ... (Kick, Bass, Lead 설정 변수들 동일) ...
    [Header("Kick Settings")]
    public float kickVol = 1.0f;
    [Range(0.01f, 1.0f)] public float kickDecay = 0.3f;
    [Range(0f, 1f)] public float kickDistortion = 0.0f;

    [Header("Bass Settings")]
    public float bassVol = 0.6f;
    public WaveType bassWave = WaveType.Saw;
    public float bassCutoff = 0.5f;
    [Range(0.01f, 2.0f)] public float bassAttack = 0.01f;
    [Range(0.01f, 2.0f)] public float bassDecay = 0.2f;
    [Range(0.0f, 1.0f)] public float bassSustain = 0.0f;
    [Range(0.01f, 2.0f)] public float bassRelease = 0.1f;

    [Header("Lead Settings")]
    public float leadVol = 0.4f;
    public WaveType leadWave = WaveType.Square;
    [Range(0.01f, 2.0f)] public float leadAttack = 0.05f;
    [Range(0.01f, 2.0f)] public float leadDecay = 0.1f;
    [Range(0.0f, 1.0f)] public float leadSustain = 0.5f;
    [Range(0.01f, 2.0f)] public float leadRelease = 0.3f;

    // ==========================================
    // [변주용 스위치 추가]
    // ==========================================
    [HideInInspector] public bool variationMode = false; // 거리 5 이내일 때 켜짐

    [HideInInspector] public bool isPlaying = true;
    
    // ... (내부 변수들 동일) ...
    private double sampleRate;
    private double nextTick = 0.0;
    private int stepIndex = 0; 
    private VoiceState kickState = new VoiceState();
    private VoiceState bassState = new VoiceState();
    private VoiceState leadState = new VoiceState();

    public enum WaveType { Sine, Square, Saw, Triangle, Noise }

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying) return;

        double samplesPerTickCurrent = sampleRate * (60.0 / bpm) / 4.0;

        for (int i = 0; i < data.Length; i += channels)
        {
            nextTick += 1.0;
            if (nextTick >= samplesPerTickCurrent)
            {
                nextTick = 0;
                stepIndex = (stepIndex + 1) % 16;
                TriggerSequencer(stepIndex);
            }

            double kickOut = GenerateKick();
            double bassOut = GenerateVoice(bassState, bassWave, bassVol) * (0.2 + bassCutoff * 0.8);
            double leadOut = GenerateVoice(leadState, leadWave, leadVol);

            double mix = (kickOut + bassOut + leadOut) * masterVolume;
            if (mix > 1.0) mix = 1.0;
            if (mix < -1.0) mix = -1.0;

            data[i] = (float)mix;
            if (channels == 2) data[i + 1] = (float)mix;
        }
    }

    // ==========================================
    // [핵심 수정] 시퀀서 로직
    // ==========================================
    void TriggerSequencer(int step)
    {
        // 1. KICK: 4/4 정박 (항상 일정)
        if (step % 4 == 0) TriggerKick();

        // 2. BASS: 변주 모드에 따라 패턴이 바뀜
        if (variationMode) 
        {
            // [변주 모드] 16비트 롤링 베이스 (두두두두) - 엄청 긴박해짐
            // 킥이 나오는 타이밍(0, 4, 8, 12)은 피해서 그루브를 줌 (사이드체인 느낌)
            if (step % 4 != 0) 
            {
                TriggerNote(bassState, 55.0); 
            }
        }
        else 
        {
            // [일반 모드] 엇박 (쿵-짝-쿵-짝)
            if (step % 4 == 2) 
            {
                TriggerNote(bassState, 55.0); 
            }
        }

        // 3. LEAD: 변주 모드일 때만 비상벨처럼 울림
        if (variationMode)
        {
            if (step % 8 == 0) // 긴 주기로 삐--- 
            {
                TriggerNote(leadState, 440.0); // 높은 A음
            }
        }
    }

    // ... (GenerateKick, TriggerKick, GenerateVoice, TriggerNote 등 나머지 함수는 이전과 100% 동일) ...
    // ... 코드가 길어지니 생략합니다. 이전 답변의 Generate 함수들을 그대로 쓰시면 됩니다 ...
    
    // (아래는 편의를 위해 다시 첨부하는 Generate 함수들입니다)
    void TriggerKick() { kickState.phase = 0.0; kickState.envTime = 0.0; kickState.active = true; }
    double GenerateKick() {
        if (!kickState.active) return 0.0;
        kickState.envTime += 1.0 / sampleRate;
        if (kickState.envTime > kickDecay) kickState.active = false;
        double pitch = 50.0 + 150.0 * Math.Exp(-kickState.envTime * 50.0);
        kickState.phase += pitch / sampleRate;
        double wave = Math.Sin(kickState.phase * 2.0 * Math.PI);
        double env = Math.Exp(-kickState.envTime * (10.0 / kickDecay));
        if (kickDistortion > 0) wave = Math.Tanh(wave * (1.0 + kickDistortion * 5.0));
        return wave * env * kickVol;
    }
    void TriggerNote(VoiceState state, double freq) {
        state.frequency = freq; state.phase = 0.0; state.envTime = 0.0; state.adsrState = 1; state.currentAmp = 0.0f;
    }
    double GenerateVoice(VoiceState state, WaveType type, float vol) {
        if (state.adsrState == 0) return 0.0;
        state.envTime += 1.0 / sampleRate;
        switch (state.adsrState) {
            case 1: state.currentAmp += (float)(1.0 / (bassAttack * sampleRate)); if (state.currentAmp >= 1.0f) { state.currentAmp = 1.0f; state.adsrState = 2; } break;
            case 2: float decayStep = (float)(1.0 / (bassDecay * sampleRate) * (1.0 - bassSustain)); state.currentAmp -= decayStep; if (state.currentAmp <= bassSustain) { state.currentAmp = bassSustain; state.adsrState = 3; } break;
            case 3: state.adsrState = 4; break;
            case 4: state.currentAmp -= (float)(1.0 / (bassRelease * sampleRate)); if (state.currentAmp <= 0.0f) { state.currentAmp = 0.0f; state.adsrState = 0; } break;
        }
        state.phase += state.frequency / sampleRate; if (state.phase > 1.0) state.phase -= 1.0;
        double osc = 0.0;
        switch (type) {
            case WaveType.Sine: osc = Math.Sin(state.phase * 2.0 * Math.PI); break;
            case WaveType.Square: osc = state.phase < 0.5 ? 1.0 : -1.0; break;
            case WaveType.Saw: osc = 2.0 * state.phase - 1.0; break;
            case WaveType.Triangle: osc = Mathf.PingPong((float)state.phase * 2.0f, 1.0f) * 2.0f - 1.0f; break;
            case WaveType.Noise: osc = (new System.Random().NextDouble() * 2.0) - 1.0; break;
        }
        return osc * state.currentAmp * vol;
    }
    class VoiceState { public bool active = false; public double phase = 0.0; public double frequency = 440.0; public double envTime = 0.0; public int adsrState = 0; public float currentAmp = 0.0f; }
}