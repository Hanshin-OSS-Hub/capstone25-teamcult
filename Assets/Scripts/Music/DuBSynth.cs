using UnityEngine;
using System;

// [변경] DarkFantasy 추가
public enum MusicGenre { DeepHouse, Synthwave, DarkFantasy }

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Current Status")]
    public MusicGenre currentGenre; 
    
    [Header("Cinematic Settings")]
    [Range(60, 200)] public double bpm = 120.0;
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
    
    // ★★★ [스케일 정의] ★★★
    private int[] currentScale; 
    
    // 1. DeepHouse: 마이너 펜타토닉 (깔끔함)
    private int[] minorPentatonic = { -12, -9, -5, 0, 3, 5, 7, 10, 12, 15 };
    
    // 2. Synthwave: 내추럴 마이너 (비장함)
    private int[] naturalMinor = { -12, -5, 0, 2, 3, 5, 7, 8, 10, 12 };

    // 3. [NEW] DarkFantasy: 하모닉 마이너 (기묘하고 사악함)
    // 0(C), 2(D), 3(Eb), 5(F), 7(G), 8(Ab), 11(B) -> 8과 11 사이의 증2도가 핵심
    private int[] harmonicMinor = { -12, -5, 0, 2, 3, 5, 7, 8, 11, 12 };

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
            case MusicGenre.DeepHouse: 
                bpm = 124.0; bassWaveType = 0; leadWaveType = 1; distortionAmount = 1.0f; useSwing = true; useUnison = false; 
                currentScale = minorPentatonic;
                break;

            case MusicGenre.Synthwave: 
                bpm = 110.0; bassWaveType = 1; leadWaveType = 0; distortionAmount = 1.2f; useSwing = false; useUnison = true; 
                currentScale = naturalMinor;
                break;

            // [NEW] DarkFantasy 설정
            case MusicGenre.DarkFantasy:
                bpm = 90.0; // 느리고 묵직하게
                bassWaveType = 1; // Saw (Drone용)
                leadWaveType = 0; // Sine (Bell용 FM 합성 베이스)
                distortionAmount = 1.1f; // 약간의 질감
                useSwing = false; // 정박의 엄숙함
                useUnison = true; // 웅장하게 (Drone이 넓게 퍼짐)
                currentScale = harmonicMinor; // 하모닉 마이너 적용
                break;
        }
    }

    public void TriggerGlitch() { glitchIntensity = 1.0f; }
    public void TriggerAttackGlitch() { attackGlitch = 1.0f; } 
    public void TriggerDamageGlitch() { damageGlitch = 1.0f; } 

    public void GenerateMarkovMelody()
    {
        int currentNoteIndex = 2; 
        for (int i = 0; i < 16; i++)
        {
            float currentDensity = density + ((i % 4 == 0) ? 0.2f : 0.0f);
            
            // DarkFantasy는 여백의 미가 중요하므로 밀도를 낮춤
            if (currentGenre == MusicGenre.DarkFantasy) currentDensity *= 0.7f;

            if (rand.NextDouble() > currentDensity) { generatedMelody[i] = -999; continue; }

            int move = (rand.NextDouble() < chaos) ? rand.Next(-4, 5) : rand.Next(-1, 2);
            if (pitchBias > 0.7f) move += 1;
            if (pitchBias < 0.3f) move -= 1;

            currentNoteIndex = Mathf.Clamp(currentNoteIndex + move, 0, currentScale.Length - 1);
            generatedMelody[i] = currentScale[currentNoteIndex];
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || sampleRate <= 0) return;

        double samplesPerTick = sampleRate * (60.0 / bpm) / 4.0; 
        if (useSwing && stepIndex % 2 == 1) samplesPerTick *= 1.2;

        float targetCutoff = cutoffFrequency;
        if (damageGlitch > 0.01f) { targetCutoff = 300f; } 
        float currentCutoff = Mathf.Clamp(targetCutoff, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
            lfoPhase += 1.0 / sampleRate;
            double pitchShift = 1.0;

            if (glitchIntensity > 0.001f) {
                pitchShift = 1.5 + (glitchIntensity * 1.0) + (rand.NextDouble() * 0.5);
                glitchIntensity *= 0.9995f; 
            }
            if (damageGlitch > 0.001f) {
                pitchShift -= 0.3; 
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
            
            // ★ [변경] 장르별 제네레이터 분기
            double bass = 0.0;
            double lead = 0.0;

            if (currentGenre == MusicGenre.DarkFantasy)
            {
                bass = GenDarkDrone(pitchShift); // 다크 판타지 전용 베이스
                lead = GenGhostBell(pitchShift); // 다크 판타지 전용 리드
            }
            else
            {
                bass = GenCustomBass(pitchShift, bassWaveType);
                lead = GenCustomLead(pitchShift, leadWaveType);
            }
            
            if (lowHealthMode) lead = GenAlarm(pitchShift); 
            else if (flameMode) lead = GenCinematicBrass(pitchShift);

            double mix = kick + snare + hihat + bass + lead;
            
            if (tensionLevel == 0 && !flameMode) mix *= 0.6;
            mix = Math.Tanh(mix * distortionAmount); 

            // 노이즈 효과
            if (attackGlitch > 0.001f) {
                double whiteNoise = (rand.NextDouble() * 2.0 - 1.0);
                mix += whiteNoise * attackGlitch * 0.4f; 
                attackGlitch *= 0.99f; 
            }
            if (damageGlitch > 0.001f) {
                double brokenNoise = (rand.NextDouble() * 2.0 - 1.0);
                brokenNoise = Math.Floor(brokenNoise * 5.0) / 5.0; 
                mix = (mix * 0.3) + (brokenNoise * damageGlitch * 0.8);
                damageGlitch *= 0.999f; 
            }

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

    void ProcessSequencer(int step)
    {
        bool kTrig = false, sTrig = false, hTrig = false;

        switch (currentGenre) {
            case MusicGenre.DeepHouse: 
                if (step % 4 == 0) kTrig = true; if (step % 4 == 2) hTrig = true; if (step == 4 || step == 12) sTrig = true; 
                break;
            case MusicGenre.Synthwave: 
                if (step % 4 == 0) kTrig = true; if (step == 4 || step == 12) sTrig = true; if (step % 2 == 0) hTrig = true; 
                break;
                
            // [NEW] DarkFantasy 리듬: 묵직하고 느린 쿵... 쿵...
            case MusicGenre.DarkFantasy:
                if (step == 0) kTrig = true; // 첫 박에 아주 무거운 킥
                if (step == 8) kTrig = true; // 중간에 한번 더
                // 스네어 대신 둔탁한 타격음 느낌으로 12번에만
                if (step == 12) sTrig = true; 
                // 하이햇 대신 기괴한 금속음 (확률적)
                if (rand.NextDouble() < 0.3) hTrig = true; 
                break;
        }

        if (flameMode && step % 4 == 0) kTrig = true;
        if (!flameMode && tensionLevel == 0 && step == 10) kTrig = false;
        if (!flameMode && tensionLevel == 0) sTrig = false;

        if (kTrig) Trigger(kickV);
        if (sTrig) Trigger(snareV);
        if (hTrig || tensionLevel >= 1) Trigger(hihatV);

        // Bass Trigger
        if (step == 0 || (currentGenre == MusicGenre.Synthwave && step % 4 == 2)) { 
            double note = (flameMode) ? 32.7 : ((tensionLevel >= 1) ? 43.65 : 32.7);
            if (currentGenre == MusicGenre.DarkFantasy) note = 32.7; // C1 (무조건 낮게)
            Trigger(bassV, note);
        }

        // Lead Trigger
        if (lowHealthMode) { if (step % 4 == 0) Trigger(leadV, 880.0); }
        else if (flameMode) { if (step % 8 == 0) Trigger(leadV, 55.0); }
        else {
            int noteNum = generatedMelody[step];
            // DarkFantasy는 멜로디가 너무 자주 나오면 분위기 깸 (확률적으로 스킵)
            if (currentGenre == MusicGenre.DarkFantasy && rand.NextDouble() < 0.3) return;

            if (noteNum != -999) Trigger(leadV, 220.0 * Math.Pow(1.05946, noteNum));
        }
    }

    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    // --- 기존 Generators ---
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
    double GetThickWave(double phase, int type, double timbre) {
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
        double morph = (speedMix * vibrato) + (chaos * 0.5f);
        return GetThickWave(leadV.phase, type, morph) * Math.Exp(-leadV.time*8.0) * leadVol; 
    }

    // ★★★ [NEW] Dark Fantasy 전용 악기 ★★★
    
    // 1. Dark Drone: 거대하고 지속적인 저음 (괴물의 숨소리)
    double GenDarkDrone(double p)
    {
        if (!bassV.active) return 0;
        bassV.time += p / sampleRate;
        if (bassV.time > 2.0) bassV.active = false; // 아주 길게 (2초)

        bassV.phase += bassV.freq * p / sampleRate;

        // 필터가 천천히 열렸다 닫히는 효과 (Wah-Wah)
        double sweep = Math.Sin(bassV.time * 2.0) * 0.5 + 0.5;
        
        // 톱니파(Saw)를 3개 겹쳐서 아주 두껍게 (Thick Unison)
        double w1 = (bassV.phase % 1.0) * 2.0 - 1.0;
        double w2 = ((bassV.phase * 1.01) % 1.0) * 2.0 - 1.0;
        double w3 = ((bassV.phase * 0.99) % 1.0) * 2.0 - 1.0;
        
        double raw = (w1 + w2 + w3) * 0.33;

        // Low Pass Filter 흉내 (Sweep 적용)
        raw = raw * (0.2 + sweep * 0.8); 

        return Math.Tanh(raw * 2.0) * bassVol * 1.2; // 약간의 디스토션
    }

    // 2. Ghost Bell: 차갑고 금속성인 종소리 (FM 합성)
    double GenGhostBell(double p)
    {
        if (!leadV.active) return 0;
        leadV.time += p / sampleRate;
        if (leadV.time > 1.5) leadV.active = false; // 길게 여운

        // FM 합성: 주파수를 다른 주파수로 흔듦
        // Modulator: 금속성 배음을 만드는 2.5배 주파수
        double modulator = Math.Sin(leadV.phase * 2.5 * 2.0 * Math.PI) * Math.Exp(-leadV.time * 5.0); 
        
        // Carrier: 실제 들리는 소리 + Modulator가 피치를 흔듦
        leadV.phase += leadV.freq * p / sampleRate;
        double carrier = Math.Sin(leadV.phase * 2.0 * Math.PI + modulator * 2.0);

        // 긴 잔향 (Reverb 느낌)
        return carrier * Math.Exp(-leadV.time * 1.5) * leadVol;
    }

    double GenKick(double p) { if(!kickV.active)return 0; kickV.time+=p/sampleRate; if(kickV.time>0.3)kickV.active=false; kickV.phase+=150*Math.Exp(-kickV.time*25.0)*p/sampleRate; return Math.Tanh(Math.Sin(kickV.phase*2*Math.PI)*3)*Math.Exp(-kickV.time*8)*kickVol; }
    double GenSnare(double p) { if(!snareV.active)return 0; snareV.time+=p/sampleRate; if(snareV.time>0.2)snareV.active=false; double t=Math.Sin(snareV.time*180*2*Math.PI*p)*Math.Exp(-snareV.time*15); double n=(rand.NextDouble()*2-1)*Math.Exp(-snareV.time*25); return (t*0.4+n*0.6)*snareVol; }
    double GenHihat(double p) { if(!hihatV.active)return 0; hihatV.time+=p/sampleRate; if(hihatV.time>0.05)hihatV.active=false; return (rand.NextDouble()*2-1)*Math.Exp(-hihatV.time*60)*hihatVol; }
    double GenCinematicBrass(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>1.5)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double raw=((leadV.phase%1.0)*2-1 + ((leadV.phase*1.01)%1.0)*2-1)*0.5; return Math.Tanh(raw*4)*Math.Min(1,leadV.time*5)*Math.Exp(-(leadV.time-0.2)*2)*leadVol; }
    double GenAlarm(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.15)leadV.active=false; leadV.phase+=(leadV.freq*(1+(1-leadV.time*2)*0.1)*p)/sampleRate; return ((leadV.phase%1.0)<0.5?1:-1)*0.6; }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}