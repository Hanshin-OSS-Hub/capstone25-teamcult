using UnityEngine;
using System;

public enum MusicGenre { DnB, Chiptune, Industrial, Synthwave }

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Current Status")]
    public MusicGenre currentGenre; // 현재 선택된 장르 (인스펙터 확인용)
    
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

    // 상태 변수
    [HideInInspector] public bool flameMode = false; 
    [HideInInspector] public bool isPlaying = true;
    [HideInInspector] public int tensionLevel = 0; 
    [HideInInspector] public bool lowHealthMode = false;

    // 내부 처리 변수
    private double sampleRate = 44100.0;
    private double nextTick = 0.0;
    private int stepIndex = 0;
    
    private Voice kickV = new Voice();
    private Voice snareV = new Voice();
    private Voice hihatV = new Voice();
    private Voice bassV = new Voice();
    private Voice leadV = new Voice(); 

    private float filterVel = 0.0f;
    private float filterPos = 0.0f;
    private System.Random rand = new System.Random();
    private float glitchIntensity = 0.0f;

    // AI 멜로디 및 스케일
    private int[] generatedMelody = new int[16];
    private int[] scale = { -12, -5, 0, 3, 5, 7, 10, 12, 15, 19 };

    // ★ 장르별 사운드 설정값 (Start에서 랜덤 결정됨)
    private int bassWaveType = 1; // 0:Sine, 1:Saw, 2:Square, 3:Noise
    private int leadWaveType = 2; 
    private float distortionAmount = 1.0f;
    private bool useSwing = false; // 스윙 리듬 여부

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        // ★★★ 게임 시작 시 장르 랜덤 선택 ★★★
        SetupRandomGenre();

        AudioSource source = GetComponent<AudioSource>();
        source.Stop(); source.loop = true; source.Play(); 
        
        GenerateMarkovMelody();
    }

    // 장르를 랜덤으로 뽑고, 악기 세팅을 바꿈
    void SetupRandomGenre()
    {
        Array values = Enum.GetValues(typeof(MusicGenre));
        currentGenre = (MusicGenre)values.GetValue(rand.Next(values.Length));

        switch (currentGenre)
        {
            case MusicGenre.DnB: // 빠르고, 톱니파 베이스
                bpm = 170.0;
                bassWaveType = 1; // Saw
                leadWaveType = 2; // Square
                distortionAmount = 1.5f;
                useSwing = false;
                break;

            case MusicGenre.Chiptune: // 8비트, 사각파 위주, 정박
                bpm = 140.0;
                bassWaveType = 2; // Square
                leadWaveType = 2; // Square
                distortionAmount = 1.0f; // 깔끔하게
                useSwing = false;
                cutoffFrequency = 20000f; // 필터 안 씀
                break;

            case MusicGenre.Industrial: // 느리고 무거움, 노이즈 섞임
                bpm = 100.0;
                bassWaveType = 1; // Saw
                leadWaveType = 3; // Noise/Weird
                distortionAmount = 5.0f; // 엄청난 왜곡
                useSwing = true; // 끈적하게
                break;

            case MusicGenre.Synthwave: // 몽환적, 사인파/톱니파
                bpm = 120.0;
                bassWaveType = 1; // Saw
                leadWaveType = 0; // Sine (부드러움)
                distortionAmount = 1.2f;
                useSwing = false;
                break;
        }
    }

    public void TriggerGlitch() { glitchIntensity = 1.0f; }

    public void GenerateMarkovMelody()
    {
        int currentNoteIndex = 2;
        for (int i = 0; i < 16; i++)
        {
            float currentDensity = density + ((i % 4 == 0) ? 0.2f : 0.0f);
            if (UnityEngine.Random.value > currentDensity) { generatedMelody[i] = -999; continue; }

            int move = (UnityEngine.Random.value < chaos) ? UnityEngine.Random.Range(-4, 5) : UnityEngine.Random.Range(-1, 2);
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
        
        // 스윙 리듬 처리 (짝수 박자를 살짝 늦게)
        if (useSwing && stepIndex % 2 == 1) samplesPerTick *= 1.2;

        float currentCutoff = Mathf.Clamp(cutoffFrequency, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
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
                
                if (stepIndex == 0 && rand.NextDouble() < 0.2) GenerateMarkovMelody();
            }

            double kick = GenKick(pitchShift);
            double snare = GenSnare(pitchShift);
            double hihat = GenHihat(pitchShift);
            
            // 베이스와 리드는 장르별 WaveType을 따름
            double bass = GenCustomBass(pitchShift, bassWaveType);
            double lead = 0.0;
            
            if (lowHealthMode) lead = GenAlarm(pitchShift); 
            else if (flameMode) lead = GenCinematicBrass(pitchShift);
            else lead = GenCustomLead(pitchShift, leadWaveType); // AI 멜로디

            double mix = kick + snare + hihat + bass + lead;
            
            if (tensionLevel == 0 && !flameMode) mix *= 0.6;
            
            // 장르별 왜곡도 적용
            mix = Math.Tanh(mix * distortionAmount); 
            mix *= masterVolume;

            // 필터
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

    void ProcessSequencer(int step)
    {
        bool kTrig = false, sTrig = false, hTrig = false;

        // ★★★ 장르별 리듬 패턴 변화 ★★★
        switch (currentGenre)
        {
            case MusicGenre.DnB: // 쿵--짝 --쿵짝
                if (step == 0 || step == 10) kTrig = true;
                if (step == 4 || step == 12) sTrig = true;
                if (step % 2 == 0) hTrig = true;
                break;

            case MusicGenre.Chiptune: // 쿵-짝-쿵-짝 (단순)
                if (step % 4 == 0) kTrig = true;
                if (step % 8 == 4) sTrig = true; // 4, 12
                if (step % 2 == 0) hTrig = true;
                break;

            case MusicGenre.Industrial: // 쿵---쿵--- (무거움)
                if (step == 0 || step == 8) kTrig = true; // 쿵 쿵
                if (step == 4 || step == 12) sTrig = true; // 짝 짝
                if (step % 4 == 0) hTrig = true; // 하이햇 적게
                break;

            case MusicGenre.Synthwave: // 쿵-쿵-쿵-쿵 (Four on the floor)
                if (step % 4 == 0) kTrig = true; // 매 박자마다 킥
                if (step == 4 || step == 12) sTrig = true;
                if (step % 2 == 0) hTrig = true;
                break;
        }

        // 공통 오버라이드 (각성/전투 등)
        if (flameMode && step % 4 == 0) kTrig = true;
        if (!flameMode && tensionLevel == 0 && step == 10) kTrig = false;
        if (!flameMode && tensionLevel == 0) sTrig = false;

        if (kTrig) Trigger(kickV);
        if (sTrig) Trigger(snareV);
        if (hTrig || tensionLevel >= 1) Trigger(hihatV);

        // BASS
        if (step == 0 || (currentGenre == MusicGenre.Synthwave && step % 4 == 2)) { // 신스웨이브는 엇박 베이스
            double note = (flameMode) ? 32.7 : ((tensionLevel >= 1) ? 43.65 : 32.7); 
            Trigger(bassV, note);
        }

        // LEAD
        if (lowHealthMode) {
            if (step % 4 == 0) Trigger(leadV, 880.0); 
        }
        else if (flameMode) {
            if (step % 8 == 0) Trigger(leadV, 55.0); 
        }
        else {
            int noteNum = generatedMelody[step];
            if (noteNum != -999) {
                double freq = 220.0 * Math.Pow(1.05946, noteNum);
                Trigger(leadV, freq);
            }
        }
    }

    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    // --- 파형 생성 함수들 (WaveType 적용) ---
    
    // 웨이브폼 선택 헬퍼: 0=Sine, 1=Saw, 2=Square, 3=Noise
    double GetWave(double phase, int type) {
        switch(type) {
            case 0: return Math.Sin(phase * 2.0 * Math.PI); // Sine
            case 1: return (phase % 1.0) * 2.0 - 1.0; // Saw
            case 2: return (phase % 1.0) < 0.5 ? 1.0 : -1.0; // Square
            case 3: return (rand.NextDouble() * 2.0 - 1.0); // Noise
            default: return 0.0;
        }
    }

    double GenCustomBass(double p, int type) {
        if(!bassV.active)return 0; 
        bassV.time+=p/sampleRate; 
        if(bassV.time>0.5)bassV.active=false; 
        bassV.phase+=bassV.freq*p/sampleRate; 
        
        // 두 개의 파형을 섞어 두껍게 (Detune)
        double w1 = GetWave(bassV.phase, type);
        double w2 = GetWave(bassV.phase * 1.01, type);
        
        return (w1+w2)*0.5 * Math.Exp(-bassV.time*4.0) * bassVol; 
    }

    double GenCustomLead(double p, int type) {
        if(!leadV.active)return 0; 
        leadV.time+=p/sampleRate; 
        if(leadV.time>0.3)leadV.active=false; 
        leadV.phase+=leadV.freq*p/sampleRate; 
        
        double w = GetWave(leadV.phase, type);
        return w * Math.Exp(-leadV.time*8.0) * leadVol; 
    }

    // 드럼은 장르 불문하고 타격감이 중요하므로 섞어서 사용
    double GenKick(double p) { 
        if(!kickV.active)return 0; kickV.time+=p/sampleRate; if(kickV.time>0.3)kickV.active=false; 
        double decay = (currentGenre == MusicGenre.Chiptune) ? 50.0 : 25.0; // 칩튠은 짧게
        double f=150*Math.Exp(-kickV.time*decay); kickV.phase+=f*p/sampleRate; 
        return Math.Tanh(Math.Sin(kickV.phase*2*Math.PI)*3)*Math.Exp(-kickV.time*8)*kickVol; 
    }
    double GenSnare(double p) { 
        if(!snareV.active)return 0; snareV.time+=p/sampleRate; if(snareV.time>0.2)snareV.active=false; 
        double t=Math.Sin(snareV.time*180*2*Math.PI*p)*Math.Exp(-snareV.time*15); 
        if(currentGenre == MusicGenre.Chiptune) t = ((t>0)?1:-1) * 0.5; // 칩튠은 톤도 사각파로
        double n=(rand.NextDouble()*2-1)*Math.Exp(-snareV.time*25); 
        return (t*0.4+n*0.6)*snareVol; 
    }
    double GenHihat(double p) { if(!hihatV.active)return 0; hihatV.time+=p/sampleRate; if(hihatV.time>0.05)hihatV.active=false; return (rand.NextDouble()*2-1)*Math.Exp(-hihatV.time*60)*hihatVol; }
    
    // 특수 효과음 (기존 유지)
    double GenCinematicBrass(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>1.5)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double s1=(leadV.phase%1.0)*2-1; double s2=((leadV.phase*1.01)%1.0)*2-1; double s3=((leadV.phase*0.99)%1.0)*2-1; double raw=(s1+s2+s3)*0.33; double env=Math.Min(1,leadV.time*5)*Math.Exp(-(leadV.time-0.2)*2); return Math.Tanh(raw*4)*env*leadVol; }
    double GenAlarm(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.15)leadV.active=false; double pm=1-(leadV.time*2); leadV.phase+=(leadV.freq*(1+pm*0.1)*p)/sampleRate; return ((leadV.phase%1.0)<0.5?1:-1)*0.6; }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}