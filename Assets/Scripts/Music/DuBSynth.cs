using UnityEngine;
using System;

public enum MusicGenre { DnB, Chiptune, DeepHouse, Synthwave }

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

    // 내부 처리 변수
    private double sampleRate = 44100.0;
    private double nextTick = 0.0;
    private int stepIndex = 0;
    private double lfoPhase = 0.0; 

    private Voice kickV = new Voice();
    private Voice snareV = new Voice();
    private Voice hihatV = new Voice();
    private Voice bassV = new Voice();
    private Voice leadV = new Voice(); 

    private float filterVel = 0.0f;
    private float filterPos = 0.0f;
    
    private System.Random rand = new System.Random();
    
    // 글리치 변수
    private float glitchIntensity = 0.0f;
    private float attackGlitch = 0.0f;
    private float damageGlitch = 0.0f;

    private int[] generatedMelody = new int[16];
    private int[] scale = { -12, -5, 0, 3, 5, 7, 10, 12, 15, 19 };

    private int bassWaveType = 1; 
    private int leadWaveType = 2; 
    private float distortionAmount = 1.0f;
    private bool useSwing = false; 
    private bool useUnison = false; 

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        SetupRandomGenre();
        GenerateMarkovMelody();

        AudioSource source = GetComponent<AudioSource>();
        if (source.clip == null) source.clip = AudioClip.Create("ProceduralAudio", 1, 1, 44100, false);
        source.Stop(); source.loop = true; source.Play(); 
    }

    void SetupRandomGenre()
    {
        Array values = Enum.GetValues(typeof(MusicGenre));
        currentGenre = (MusicGenre)values.GetValue(rand.Next(values.Length));

        switch (currentGenre)
        {
            case MusicGenre.DnB: 
                bpm = 170.0; bassWaveType = 0; leadWaveType = 2; distortionAmount = 1.5f; 
                useSwing = false; useUnison = true; 
                break;
            case MusicGenre.Chiptune: 
                bpm = 140.0; bassWaveType = 2; leadWaveType = 2; distortionAmount = 1.0f; 
                useSwing = false; useUnison = false; 
                cutoffFrequency = 20000f; 
                break;
            case MusicGenre.DeepHouse: 
                bpm = 124.0; bassWaveType = 0; leadWaveType = 1; distortionAmount = 1.0f; 
                useSwing = true; useUnison = false; 
                break;
            case MusicGenre.Synthwave: 
                bpm = 120.0; bassWaveType = 1; leadWaveType = 0; distortionAmount = 1.2f; 
                useSwing = false; useUnison = true; 
                break;
        }
    }

    public void TriggerGlitch() { glitchIntensity = 1.0f; }
    
    // ★★★ [중요] 공격 시 attackGlitch를 1.0으로 설정
    public void TriggerAttackGlitch() { attackGlitch = 1.0f; } 
    
    // ★★★ [중요] 피격 시 damageGlitch를 1.0으로 설정
    public void TriggerDamageGlitch() { damageGlitch = 1.0f; } 

    public void GenerateMarkovMelody()
    {
        int currentNoteIndex = 2;
        for (int i = 0; i < 16; i++)
        {
            float currentDensity = density + ((i % 4 == 0) ? 0.2f : 0.0f);
            if (rand.NextDouble() > currentDensity) { generatedMelody[i] = -999; continue; }

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

        float targetCutoff = cutoffFrequency;
        
        // 피격 시 필터 닫힘 (먹먹해짐)
        if (damageGlitch > 0.01f) { 
            targetCutoff = 300f; 
        }
        float currentCutoff = Mathf.Clamp(targetCutoff, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
            lfoPhase += 1.0 / sampleRate;
            double pitchShift = 1.0;

            // 글리치 피치 변조 (기존 유지)
            if (glitchIntensity > 0.001f) {
                pitchShift = 1.5 + (glitchIntensity * 1.0) + (rand.NextDouble() * 0.5);
                glitchIntensity *= 0.9995f; 
            }
            
            // 피격 시 음악 속도 느려짐 (Tape Stop 효과)
            if (damageGlitch > 0.001f) {
                pitchShift -= 0.3; // 0.3배 느려짐
            }
            
            nextTick += 1.0 * pitchShift;
            if (nextTick >= samplesPerTick)
            {
                nextTick = 0;
                stepIndex = (stepIndex + 1) % 16;
                ProcessSequencer(stepIndex);
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

            // =========================================================
            // ★★★ [NEW] 노이즈 주입 파트 (확실한 효과음) ★★★
            // =========================================================

            // 1. 공격 (Attack) -> "치익!" 하는 날카로운 노이즈 추가
            if (attackGlitch > 0.001f)
            {
                // 화이트 노이즈 생성
                double whiteNoise = (rand.NextDouble() * 2.0 - 1.0);
                
                // attackGlitch 값 자체가 볼륨이 됨 (1.0 -> 0.0)
                mix += whiteNoise * attackGlitch * 0.4f; // 음악 위에 40% 볼륨으로 얹음
                
                // 빠르게 사라짐 (Short Decay)
                attackGlitch *= 0.99f; 
            }

            // 2. 피격 (Damage) -> "콰직!" 하는 깨진 노이즈 + 음악 볼륨 다운
            if (damageGlitch > 0.001f)
            {
                // 비트크러쉬 노이즈 (저해상도 노이즈)
                double brokenNoise = (rand.NextDouble() * 2.0 - 1.0);
                brokenNoise = Math.Floor(brokenNoise * 5.0) / 5.0; // 5단계로 깎음

                // 음악 볼륨을 줄이고(Ducking), 노이즈를 크게 키움
                mix = (mix * 0.3) + (brokenNoise * damageGlitch * 0.8);
                
                // 천천히 사라짐 (Long Decay)
                damageGlitch *= 0.999f; 
            }
            // =========================================================

            mix *= masterVolume;

            if (currentCutoff < 20000f) {
                float f = 2.0f * Mathf.Sin((float)(Math.PI * currentCutoff / sampleRate));
                filterPos += filterVel * f;
                filterVel += ((float)mix - filterPos - filterVel * 1.0f) * f;
                mix = filterPos;
            }

            data[i] = (float)mix;
            if (channels == 2) data[i + 1] = (float)mix;
        }
    }

    // --- 나머지 기존 시퀀서 및 제네레이터 코드들 (유지) ---
    void ProcessSequencer(int step)
    {
        bool kTrig = false, sTrig = false, hTrig = false;

        switch (currentGenre) {
            case MusicGenre.DnB: if (step==0 || step==10) kTrig=true; if (step==4 || step==12) sTrig=true; if (step%2==0) hTrig=true; break;
            case MusicGenre.Chiptune: if (step%4==0) kTrig=true; if (step%8==4) sTrig=true; if (step%2==0) hTrig=true; break;
            case MusicGenre.DeepHouse: if (step%4==0) kTrig=true; if (step%4==2) hTrig=true; if (step==4 || step==12) sTrig=true; break;
            case MusicGenre.Synthwave: if (step%4==0) kTrig=true; if (step==4 || step==12) sTrig=true; if (step%2==0) hTrig=true; break;
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
            if (noteNum != -999) Trigger(leadV, 220.0 * Math.Pow(1.05946, noteNum));
        }
    }

    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    double GetAdvancedWave(double phase, int type, double timbre) {
        double grit = (rand.NextDouble() * 2.0 - 1.0) * 0.05 * timbre; 
        switch(type) {
            case 0: return Math.Sin(phase * 2.0 * Math.PI + Math.Sin(phase * 4.0 * Math.PI) * timbre * 3.0) + grit; 
            case 1: double rs = (phase % 1.0) * 2.0 - 1.0; return Math.Sin(rs * (1.0 + timbre * 4.0)) + grit; 
            case 2: double width = 0.5 + Math.Sin(lfoPhase * 2.0) * 0.4 * timbre; return ((phase % 1.0) < width ? 1.0 : -1.0) + grit; 
            case 3: double n = (rand.NextDouble() * 2.0 - 1.0); if (timbre > 0.1) { double s = Math.Max(1.0, 30.0 - timbre * 28.0); n = Math.Floor(n * s) / s; } return n; 
            default: return 0.0;
        }
    }

    double GetThickWave(double phase, int type, double timbre)
    {
        if (useUnison) {
            double center = GetAdvancedWave(phase, type, timbre);
            double left = GetAdvancedWave(phase * 0.995, type, timbre); 
            double right = GetAdvancedWave(phase * 1.005, type, timbre); 
            return (center + left * 0.7 + right * 0.7) * 0.45; 
        } else {
            return GetAdvancedWave(phase, type, timbre);
        }
    }

    double GenCustomBass(double p, int type) {
        if(!bassV.active)return 0; bassV.time+=p/sampleRate; if(bassV.time>0.6)bassV.active=false; bassV.phase+=bassV.freq*p/sampleRate; 
        double morph = 0.2 + (heavyMix * 0.6) + (tensionLevel * 0.2);
        double subOsc = Math.Sin(bassV.phase * 0.5 * 2.0 * Math.PI) * 0.6; 
        double mainOsc = GetThickWave(bassV.phase, type, morph);
        double mainOsc2 = GetThickWave(bassV.phase * 1.01, type, morph);
        return (mainOsc + mainOsc2 + subOsc) * 0.4 * Math.Exp(-bassV.time*4.0) * bassVol; 
    }

    double GenCustomLead(double p, int type) {
        if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.4)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; 
        double vibrato = (Math.Sin(lfoPhase * 6.0) + 1.0) * 0.5;
        double morph = (speedMix * vibrato) + (chaos * 0.5f); if (currentGenre == MusicGenre.Chiptune) morph *= 0.5;
        return GetThickWave(leadV.phase, type, morph) * Math.Exp(-leadV.time*8.0) * leadVol; 
    }

    double GenKick(double p) { if(!kickV.active)return 0; kickV.time+=p/sampleRate; if(kickV.time>0.3)kickV.active=false; double decay=(currentGenre==MusicGenre.Chiptune)?50.0:25.0; kickV.phase+=150*Math.Exp(-kickV.time*decay)*p/sampleRate; return Math.Tanh(Math.Sin(kickV.phase*2*Math.PI)*3)*Math.Exp(-kickV.time*8)*kickVol; }
    double GenSnare(double p) { if(!snareV.active)return 0; snareV.time+=p/sampleRate; if(snareV.time>0.2)snareV.active=false; double t=Math.Sin(snareV.time*180*2*Math.PI*p)*Math.Exp(-snareV.time*15); if(currentGenre==MusicGenre.Chiptune)t=((t>0)?1:-1)*0.5; double n=(rand.NextDouble()*2-1)*Math.Exp(-snareV.time*25); return (t*0.4+n*0.6)*snareVol; }
    double GenHihat(double p) { if(!hihatV.active)return 0; hihatV.time+=p/sampleRate; if(hihatV.time>0.05)hihatV.active=false; return (rand.NextDouble()*2-1)*Math.Exp(-hihatV.time*60)*hihatVol; }
    double GenCinematicBrass(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>1.5)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double raw=((leadV.phase%1.0)*2-1 + ((leadV.phase*1.01)%1.0)*2-1)*0.5; return Math.Tanh(raw*4)*Math.Min(1,leadV.time*5)*Math.Exp(-(leadV.time-0.2)*2)*leadVol; }
    double GenAlarm(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.15)leadV.active=false; leadV.phase+=(leadV.freq*(1+(1-leadV.time*2)*0.1)*p)/sampleRate; return ((leadV.phase%1.0)<0.5?1:-1)*0.6; }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}