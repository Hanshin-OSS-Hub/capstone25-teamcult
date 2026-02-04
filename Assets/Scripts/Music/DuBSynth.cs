using UnityEngine;
using System;

public enum MusicGenre { DnB, Chiptune, Industrial, Synthwave }

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Current Status")]
    public MusicGenre currentGenre; 
    
    [Header("Cinematic Settings")]
    [Range(60, 200)] public double bpm = 160.0;
    [Range(0, 1)] public float masterVolume = 0.5f;
    public float cutoffFrequency = 20000f;

    [Header("AI Params")]
    [Range(0, 1)] public float chaos = 0.2f;
    [Range(0, 1)] public float density = 0.8f;
    [Range(0, 1)] public float pitchBias = 0.5f;

    [Header("Dynamic Layers")]
    public float kickVol = 0.8f; 
    public float snareVol = 0.5f;
    public float hihatVol = 0.3f;
    public float bassVol = 0.7f; 
    public float leadVol = 0.6f; 

    [Header("Enemy Mix (Auto Update)")]
    public float heavyMix = 0.0f; 
    public float speedMix = 0.0f; 
    public float weirdMix = 0.0f; 

    [HideInInspector] public bool flameMode = false; 
    [HideInInspector] public bool isPlaying = true;
    [HideInInspector] public int tensionLevel = 0; 
    [HideInInspector] public bool lowHealthMode = false;

    private double sampleRate = 44100.0;
    private double nextTick = 0.0;
    private int stepIndex = 0;
    
    // 오디오 쓰레드 안전용 LFO 위상
    private double lfoPhase = 0.0; 

    private Voice kickV = new Voice();
    private Voice snareV = new Voice();
    private Voice hihatV = new Voice();
    private Voice bassV = new Voice();
    private Voice leadV = new Voice(); 

    private float filterVel = 0.0f;
    private float filterPos = 0.0f;
    
    // ★ System.Random은 오디오 쓰레드에서도 안전합니다.
    private System.Random rand = new System.Random();
    private float glitchIntensity = 0.0f;

    private int[] generatedMelody = new int[16];
    private int[] scale = { -12, -5, 0, 3, 5, 7, 10, 12, 15, 19 };

    private int bassWaveType = 1; 
    private int leadWaveType = 2; 
    private float distortionAmount = 1.0f;
    private bool useSwing = false; 

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        SetupRandomGenre();
        GenerateMarkovMelody(); // 시작할 때 한 번 생성

        AudioSource source = GetComponent<AudioSource>();
        
        // [안전장치] 클립이 없으면 오디오 엔진이 안 도는 경우가 있음. 빈 클립 생성.
        if (source.clip == null) {
            source.clip = AudioClip.Create("ProceduralAudio", 1, 1, 44100, false);
        }

        source.Stop(); 
        source.loop = true; 
        source.Play(); 
    }

    void SetupRandomGenre()
    {
        // Start는 메인쓰레드라 UnityEngine.Random 써도 되지만, 통일성을 위해 rand 사용
        Array values = Enum.GetValues(typeof(MusicGenre));
        currentGenre = (MusicGenre)values.GetValue(rand.Next(values.Length));

        switch (currentGenre)
        {
            case MusicGenre.DnB: 
                bpm = 170.0;
                bassWaveType = 0; // Sine (FM)
                leadWaveType = 2; // Square
                distortionAmount = 1.5f;
                useSwing = false;
                break;
            case MusicGenre.Chiptune: 
                bpm = 140.0;
                bassWaveType = 2; // Square (PWM)
                leadWaveType = 2; // Square
                distortionAmount = 1.0f; 
                useSwing = false;
                cutoffFrequency = 20000f; 
                break;
            case MusicGenre.Industrial: 
                bpm = 100.0;
                bassWaveType = 1; // Saw (Fold)
                leadWaveType = 3; // Noise (Crush)
                distortionAmount = 4.0f; 
                useSwing = true; 
                break;
            case MusicGenre.Synthwave: 
                bpm = 120.0;
                bassWaveType = 1; // Saw
                leadWaveType = 0; // Sine
                distortionAmount = 1.2f;
                useSwing = false;
                break;
        }
    }

    public void TriggerGlitch() { glitchIntensity = 1.0f; }

    // ★★★ [수정됨] 오디오 쓰레드 안전 버전 ★★★
    // UnityEngine.Random -> System.Random (rand)으로 교체
    public void GenerateMarkovMelody()
    {
        int currentNoteIndex = 2;
        for (int i = 0; i < 16; i++)
        {
            float currentDensity = density + ((i % 4 == 0) ? 0.2f : 0.0f);
            
            // rand.NextDouble()은 0.0 ~ 1.0 사이 값을 줍니다.
            if (rand.NextDouble() > currentDensity) { 
                generatedMelody[i] = -999; 
                continue; 
            }

            int move = (rand.NextDouble() < chaos) ? rand.Next(-4, 5) : rand.Next(-1, 2);
            
            if (pitchBias > 0.7f) move += 1;
            if (pitchBias < 0.3f) move -= 1;

            currentNoteIndex = Mathf.Clamp(currentNoteIndex + move, 0, scale.Length - 1);
            generatedMelody[i] = scale[currentNoteIndex];
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || sampleRate <= 0) return;

        double samplesPerTick = sampleRate * (60.0 / bpm) / 4.0; 
        if (useSwing && stepIndex % 2 == 1) samplesPerTick *= 1.2;

        float currentCutoff = Mathf.Clamp(cutoffFrequency, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
            // LFO 위상 증가 (Time.time 대신 사용 - 오디오 쓰레드 안전)
            lfoPhase += 1.0 / sampleRate;

            double pitchShift = 1.0;
            if (glitchIntensity > 0.01f) {
                pitchShift = 1.5 + (glitchIntensity * 1.0) + (rand.NextDouble() * 0.5);
                glitchIntensity *= 0.9995f; 
            }
            
            nextTick += 1.0 * pitchShift;
            if (nextTick >= samplesPerTick)
            {
                nextTick = 0;
                stepIndex = (stepIndex + 1) % 16;
                ProcessSequencer(stepIndex);
                
                // 오디오 쓰레드 안에서 호출되므로, 반드시 안전한 함수여야 함
                if (stepIndex == 0 && rand.NextDouble() < 0.2) GenerateMarkovMelody();
            }

            double kick = GenKick(pitchShift);
            double snare = GenSnare(pitchShift);
            double hihat = GenHihat(pitchShift);
            double bass = GenCustomBass(pitchShift, bassWaveType);
            double lead = 0.0;
            
            if (lowHealthMode) lead = GenAlarm(pitchShift); 
            else if (flameMode) lead = GenCinematicBrass(pitchShift);
            else lead = GenCustomLead(pitchShift, leadWaveType);

            double mix = kick + snare + hihat + bass + lead;
            
            if (tensionLevel == 0 && !flameMode) mix *= 0.6;
            
            mix = Math.Tanh(mix * distortionAmount); 
            mix *= masterVolume;

            if (currentCutoff < 20000f) 
            {
                float f = 2.0f * Mathf.Sin((float)(Math.PI * currentCutoff / sampleRate));
                filterPos += filterVel * f;
                filterVel += ((float)mix - filterPos - filterVel * 1.0f) * f;
                mix = filterPos;
            }

            data[i] = (float)mix;
            if (channels == 2) data[i + 1] = (float)mix;
        }
    }

    // ... (ProcessSequencer는 기존과 동일하여 생략, Generator로 바로 넘어갑니다) ...
    void ProcessSequencer(int step)
    {
        bool kTrig = false, sTrig = false, hTrig = false;

        switch (currentGenre)
        {
            case MusicGenre.DnB: 
                if (step == 0 || step == 10) kTrig = true;
                if (step == 4 || step == 12) sTrig = true;
                if (step % 2 == 0) hTrig = true;
                break;
            case MusicGenre.Chiptune: 
                if (step % 4 == 0) kTrig = true;
                if (step % 8 == 4) sTrig = true; 
                if (step % 2 == 0) hTrig = true;
                break;
            case MusicGenre.Industrial: 
                if (step == 0 || step == 8) kTrig = true; 
                if (step == 4 || step == 12) sTrig = true; 
                if (step % 4 == 0) hTrig = true; 
                break;
            case MusicGenre.Synthwave: 
                if (step % 4 == 0) kTrig = true; 
                if (step == 4 || step == 12) sTrig = true;
                if (step % 2 == 0) hTrig = true;
                break;
        }

        if (flameMode && step % 4 == 0) kTrig = true;
        if (!flameMode && tensionLevel == 0 && step == 10) kTrig = false;
        if (!flameMode && tensionLevel == 0) sTrig = false;

        if (kTrig) Trigger(kickV);
        if (sTrig) Trigger(snareV);
        if (hTrig || tensionLevel >= 1) Trigger(hihatV);

        if (step == 0 || (currentGenre == MusicGenre.Synthwave && step % 4 == 2)) { 
            double note = (flameMode) ? 32.7 : ((tensionLevel >= 1) ? 43.65 : 32.7); 
            Trigger(bassV, note);
        }

        if (lowHealthMode) { if (step % 4 == 0) Trigger(leadV, 880.0); }
        else if (flameMode) { if (step % 8 == 0) Trigger(leadV, 55.0); }
        else {
            int noteNum = generatedMelody[step];
            if (noteNum != -999) {
                double freq = 220.0 * Math.Pow(1.05946, noteNum);
                Trigger(leadV, freq);
            }
        }
    }

    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    // ★★★ Time.time 대신 lfoPhase 사용 (안전함) ★★★
    double GetAdvancedWave(double phase, int type, double timbre) 
    {
        switch(type) {
            case 0: // Sine + FM
                double fmAmount = timbre * 3.0; 
                return Math.Sin(phase * 2.0 * Math.PI + Math.Sin(phase * 4.0 * Math.PI) * fmAmount);
            case 1: // Saw + Fold
                double rawSaw = (phase % 1.0) * 2.0 - 1.0;
                double foldAmt = 1.0 + (timbre * 4.0); 
                return Math.Sin(rawSaw * foldAmt); 
            case 2: // Square + PWM
                // Time.time 대신 lfoPhase 사용
                double lfo = Math.Sin(lfoPhase * 2.0) * 0.4 * timbre;
                double width = 0.5 + lfo; 
                return (phase % 1.0) < width ? 1.0 : -1.0;
            case 3: // Noise + Bitcrush
                double noise = (rand.NextDouble() * 2.0 - 1.0);
                if (timbre > 0.1) {
                    double steps = 30.0 - (timbre * 28.0);
                    if(steps < 1.0) steps = 1.0; // 0 나누기 방지
                    noise = Math.Floor(noise * steps) / steps;
                }
                return noise;
            default: return 0.0;
        }
    }

    double GenCustomBass(double p, int type) {
        if(!bassV.active)return 0; 
        bassV.time+=p/sampleRate; 
        if(bassV.time>0.6)bassV.active=false; 
        bassV.phase+=bassV.freq*p/sampleRate; 
        
        double morph = 0.2 + (heavyMix * 0.6) + (tensionLevel * 0.2);
        double w1 = GetAdvancedWave(bassV.phase, type, morph);
        double w2 = GetAdvancedWave(bassV.phase * 1.01, type, morph); 
        return (w1+w2)*0.5 * Math.Exp(-bassV.time*4.0) * bassVol; 
    }

    double GenCustomLead(double p, int type) {
        if(!leadV.active)return 0; 
        leadV.time+=p/sampleRate; 
        if(leadV.time>0.4)leadV.active=false; 
        leadV.phase+=leadV.freq*p/sampleRate; 
        
        double vibrato = (Math.Sin(lfoPhase * 6.0) + 1.0) * 0.5; // lfoPhase 사용
        double morph = (speedMix * vibrato) + (chaos * 0.5f);
        if (currentGenre == MusicGenre.Chiptune) morph *= 0.5;

        double w = GetAdvancedWave(leadV.phase, type, morph);
        return w * Math.Exp(-leadV.time*8.0) * leadVol; 
    }

    double GenKick(double p) { if(!kickV.active)return 0; kickV.time+=p/sampleRate; if(kickV.time>0.3)kickV.active=false; double decay = (currentGenre == MusicGenre.Chiptune) ? 50.0 : 25.0; double f=150*Math.Exp(-kickV.time*decay); kickV.phase+=f*p/sampleRate; return Math.Tanh(Math.Sin(kickV.phase*2*Math.PI)*3)*Math.Exp(-kickV.time*8)*kickVol; }
    double GenSnare(double p) { if(!snareV.active)return 0; snareV.time+=p/sampleRate; if(snareV.time>0.2)snareV.active=false; double t=Math.Sin(snareV.time*180*2*Math.PI*p)*Math.Exp(-snareV.time*15); if(currentGenre == MusicGenre.Chiptune) t = ((t>0)?1:-1) * 0.5; double n=(rand.NextDouble()*2-1)*Math.Exp(-snareV.time*25); return (t*0.4+n*0.6)*snareVol; }
    double GenHihat(double p) { if(!hihatV.active)return 0; hihatV.time+=p/sampleRate; if(hihatV.time>0.05)hihatV.active=false; return (rand.NextDouble()*2-1)*Math.Exp(-hihatV.time*60)*hihatVol; }
    double GenCinematicBrass(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>1.5)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double s1=(leadV.phase%1.0)*2-1; double s2=((leadV.phase*1.01)%1.0)*2-1; double s3=((leadV.phase*0.99)%1.0)*2-1; double raw=(s1+s2+s3)*0.33; double env=Math.Min(1,leadV.time*5)*Math.Exp(-(leadV.time-0.2)*2); return Math.Tanh(raw*4)*env*leadVol; }
    double GenAlarm(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.15)leadV.active=false; double pm=1-(leadV.time*2); leadV.phase+=(leadV.freq*(1+pm*0.1)*p)/sampleRate; return ((leadV.phase%1.0)<0.5?1:-1)*0.6; }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}