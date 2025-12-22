using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Global Settings")]
    [Range(120, 200)] public double bpm = 170.0;
    [Range(0, 1)] public float masterVolume = 0.5f;

    [Header("Health Effect")]
    [Range(100f, 22000f)] public float cutoffFrequency = 22000f;
    public float resonance = 1.0f;

    [Header("Drum Levels")]
    public float kickVol = 0.8f;
    public float snareVol = 0.6f;
    public float hihatVol = 0.3f;

    [Header("Synth Levels")]
    public float bassVol = 0.5f;
    public float leadVol = 0.4f;
    public float alarmVol = 0.6f;
    
    // [수정됨] 누락되었던 변수 추가!
    public float bassDistortion = 0.0f;

    // ==========================================
    // [Control Flags]
    // ==========================================
    [HideInInspector] public bool isPlaying = true;
    [HideInInspector] public int tensionLevel = 0; 
    [HideInInspector] public bool lowHealthMode = false;

    private double sampleRate;
    private double nextTick = 0.0;
    private int stepIndex = 0;

    // 악기 상태
    private DrumState kickState = new DrumState();
    private DrumState snareState = new DrumState();
    private DrumState hihatState = new DrumState();
    private SynthState bassState = new SynthState();
    private SynthState leadState = new SynthState(); 

    private float filterVel = 0.0f;
    private float filterPos = 0.0f;

    // 미니멀 리프용 마이너 스케일 (A Minor)
    private int[] minorRiffNotes = { 0, 0, 3, 7, 0, 3, 10, 7 }; 

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying) return;

        double samplesPerTick = sampleRate * (60.0 / bpm) / 4.0; 
        float cutoff = Mathf.Clamp(cutoffFrequency, 50f, 20000f); 

        for (int i = 0; i < data.Length; i += channels)
        {
            nextTick += 1.0;
            if (nextTick >= samplesPerTick)
            {
                nextTick = 0;
                stepIndex = (stepIndex + 1) % 16;
                ProcessSequencer(stepIndex);
            }

            double kick = GenerateKick();
            double snare = GenerateSnare();
            double hihat = GenerateHihat();
            double bass = GenerateReeseBass();
            
            double lead = 0.0;
            if (lowHealthMode) lead = GenerateAlarm(); 
            else lead = GenerateLead();

            double mix = kick + snare + hihat + bass + lead;
            mix *= masterVolume;

            if (cutoff < 20000f) 
            {
                float f = 2.0f * Mathf.Sin((float)(Math.PI * cutoff / sampleRate));
                filterPos += filterVel * f;
                filterVel += ((float)mix - filterPos - filterVel * (2.0f - resonance)) * f;
                mix = filterPos;
            }

            if (mix > 1.0) mix = 1.0;
            if (mix < -1.0) mix = -1.0;

            data[i] = (float)mix;
            if (channels == 2) data[i + 1] = (float)mix;
        }
    }

    void ProcessSequencer(int step)
    {
        bool kTrig = (step == 0) || (step == 10) || (tensionLevel >= 1 && step == 7);
        if (kTrig) TriggerDrum(kickState);

        bool sTrig = (step == 4) || (step == 12);
        if (tensionLevel == 2 && (step == 14 || step == 15)) sTrig = true;
        if (lowHealthMode) sTrig = false; 
        if (sTrig) TriggerDrum(snareState);

        if ((step % 2 == 0 || tensionLevel >= 1) && !lowHealthMode) TriggerDrum(hihatState);

        if (step == 0) 
        {
            double note = (tensionLevel >= 1) ? 55.0 : 43.65; 
            TriggerSynth(bassState, note);
        }

        if (lowHealthMode)
        {
            if (step % 4 == 0) TriggerSynth(leadState, 880.0); 
        }
        else if (tensionLevel > 0)
        {
            if (step % 3 == 0) 
            {
                double battlePitch = (tensionLevel == 2) ? 880.0 : 440.0;
                TriggerSynth(leadState, battlePitch);
            }
        }
        else
        {
            if (step % 2 == 0) 
            {
                int noteIdx = (step / 2) % minorRiffNotes.Length;
                int semitone = minorRiffNotes[noteIdx];
                double freq = 220.0 * Math.Pow(1.05946, semitone);
                TriggerSynth(leadState, freq);
            }
        }
    }

    void TriggerDrum(DrumState state) { state.envTime = 0.0; state.active = true; }
    void TriggerSynth(SynthState state, double freq) { state.freq = freq; state.active = true; state.amp = 1.0f; state.envTime = 0.0; }

    double GenerateKick() { if (!kickState.active) return 0.0; kickState.envTime += 1.0/sampleRate; if(kickState.envTime>0.3)kickState.active=false; double t=kickState.envTime; double p=60+200*Math.Exp(-t*25); kickState.phase+=p/sampleRate; return Math.Tanh(Math.Sin(kickState.phase*2*Math.PI)*2.0)*Math.Exp(-t*10)*kickVol; }
    double GenerateSnare() { if (!snareState.active) return 0.0; snareState.envTime += 1.0/sampleRate; if(snareState.envTime>0.2)snareState.active=false; double t=snareState.envTime; return (Math.Sin(t*180*2*Math.PI)*Math.Exp(-t*15)*0.4 + (new System.Random().NextDouble()*2-1)*Math.Exp(-t*25)*0.6)*snareVol; }
    double GenerateHihat() { if (!hihatState.active) return 0.0; hihatState.envTime += 1.0/sampleRate; if(hihatState.envTime>0.05)hihatState.active=false; double n=(new System.Random().NextDouble()*2-1); double hp=n-hihatState.lastVal; hihatState.lastVal=n; return hp*Math.Exp(-hihatState.envTime*80)*hihatVol; }
    
    // 여기서 bassDistortion 변수를 사용합니다!
    double GenerateReeseBass() { 
        if (!bassState.active) return 0.0; 
        bassState.phase+=bassState.freq/sampleRate; 
        bassState.phase2+=(bassState.freq+0.5)/sampleRate; 
        double s1=(bassState.phase%1.0)*2-1; 
        double s2=(bassState.phase2%1.0)*2-1; 
        double combined = (s1+s2)*0.5;
        if (bassDistortion > 0) combined = Math.Tanh(combined * (1.0 + bassDistortion));
        bassState.amp -= (float)(1.0/(0.5*sampleRate)); 
        if(bassState.amp<0)bassState.active=false; 
        return combined*bassState.amp*bassVol; 
    }

    double GenerateLead()
    {
        if (!leadState.active) return 0.0;
        leadState.envTime += 1.0 / sampleRate;
        float decay = (tensionLevel > 0) ? 0.2f : 0.4f; 
        if (leadState.envTime > decay) leadState.active = false;
        leadState.phase += leadState.freq / sampleRate;
        double tri = Mathf.PingPong((float)leadState.phase * 2.0f, 1.0f) * 2.0f - 1.0f;
        double sine = Math.Sin(leadState.phase * 2.0 * Math.PI);
        double wave = (tri * 0.7 + sine * 0.3);
        double env = Math.Exp(-leadState.envTime * (5.0 / decay));
        return wave * env * leadVol;
    }

    double GenerateAlarm()
    {
        if (!leadState.active) return 0.0;
        leadState.envTime += 1.0 / sampleRate;
        if (leadState.envTime > 0.15) leadState.active = false; 
        double pitchMod = 1.0 - (leadState.envTime * 2.0); 
        double currentFreq = leadState.freq * (1.0 + pitchMod * 0.1);
        leadState.phase += currentFreq / sampleRate;
        double square = (leadState.phase % 1.0) < 0.5 ? 1.0 : -1.0;
        return square * alarmVol * 0.8; 
    }

    class DrumState { public bool active; public double envTime; public double phase; public double lastVal; }
    class SynthState { public bool active; public double freq; public double phase; public double phase2; public float amp; public double envTime; }
}