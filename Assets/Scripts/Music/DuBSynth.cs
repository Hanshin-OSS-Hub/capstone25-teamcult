using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Cinematic Settings")]
    [Range(60, 200)] public double bpm = 160.0;
    [Range(0, 1)] public float masterVolume = 0.5f;

    [Header("Filter Control")]
    public float cutoffFrequency = 20000f;
    [HideInInspector] public bool flameMode = false; 

    [Header("Mix Levels")]
    public float kickVol = 0.8f; 
    public float snareVol = 0.5f;
    public float hihatVol = 0.3f;
    public float bassVol = 0.7f; 
    public float leadVol = 0.6f; 

    [HideInInspector] public bool isPlaying = true;
    [HideInInspector] public int tensionLevel = 0; 
    [HideInInspector] public bool lowHealthMode = false;

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

    // [NEW] 하이 피치 글리치 변수
    private float glitchIntensity = 0.0f; 

    private int[] minorRiffNotes = { 0, 0, 3, 7, 0, 3, 10, 7 }; 
    private int[] flameDarkNotes = { -12, -12, -12, -12, -5, -5, -7, -7 };

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        AudioSource source = GetComponent<AudioSource>();
        source.Stop();
        source.loop = true;
        source.Play(); 
    }

    // [외부 호출] 데미지 입으면 호출
    public void TriggerGlitch()
    {
        glitchIntensity = 1.0f; // 강도 초기화
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying) return;
        if (sampleRate <= 0) return;

        double samplesPerTick = sampleRate * (60.0 / bpm) / 4.0; 
        float currentCutoff = Mathf.Clamp(cutoffFrequency, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
            // === 글리치 로직 (하이 피치) ===
            double pitchShift = 1.0;
            
            if (glitchIntensity > 0.01f)
            {
                // 강도가 높을수록 피치를 확 올려버림 (1.5배 ~ 2.5배)
                // 랜덤성을 줘서 "끼릭-끽-" 거리는 느낌
                double randomJitter = rand.NextDouble() * 0.5;
                pitchShift = 1.5 + (glitchIntensity * 1.0) + randomJitter;

                // 글리치 강도 서서히 감소
                glitchIntensity *= 0.9995f; 
            }
            
            // 시퀀서 타이밍 계산 (피치가 높으면 시간도 빨리 감)
            nextTick += 1.0 * pitchShift; 

            if (nextTick >= samplesPerTick)
            {
                nextTick = 0;
                stepIndex = (stepIndex + 1) % 16;
                ProcessSequencer(stepIndex);
            }

            // 소리 합성 (freq는 바꾸지 않고, phase 증가 속도를 pitchShift로 조절)
            double kick = GenKick(pitchShift);
            double snare = GenSnare(pitchShift);
            double hihat = GenHihat(pitchShift);
            double bass = GenReeseBass(pitchShift);
            
            double lead = 0.0;
            if (lowHealthMode) lead = GenAlarm(pitchShift); 
            else if (flameMode) lead = GenCinematicBrass(pitchShift);
            else lead = GenPluck(pitchShift);

            double mix = kick + snare + hihat + bass + lead;
            
            if (tensionLevel == 0 && !flameMode) mix *= 0.6;

            // 글리치 상태일 때 소리를 좀 더 찢어지게 (Distortion)
            if (glitchIntensity > 0.1f) mix = Math.Tanh(mix * 3.0);
            else mix = Math.Tanh(mix * 1.5); 

            mix *= masterVolume;

            // Filter
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
        // KICK
        bool kTrig = (step == 0) || (step == 10);
        if (flameMode && step % 4 == 0) kTrig = true; 
        if (!flameMode && tensionLevel == 0 && step == 10) kTrig = false; 
        if (kTrig) Trigger(kickV);

        // SNARE
        bool sTrig = (step == 4) || (step == 12);
        if (!flameMode && tensionLevel == 0) sTrig = false; 
        if (sTrig) Trigger(snareV);

        // HI-HAT
        if (step % 2 == 0 || tensionLevel >= 1 || flameMode) Trigger(hihatV);

        // BASS
        if (step == 0) 
        {
            double note = (flameMode) ? 32.7 : ((tensionLevel >= 1) ? 43.65 : 32.7); 
            Trigger(bassV, note);
        }

        // LEAD
        if (lowHealthMode) {
            if (step % 4 == 0) Trigger(leadV, 880.0); 
        }
        else if (flameMode) {
            if (step % 8 == 0) {
                int noteIdx = (step / 8) % flameDarkNotes.Length;
                double freq = 55.0 * Math.Pow(1.05946, flameDarkNotes[noteIdx]);
                Trigger(leadV, freq);
            }
        }
        else if (tensionLevel > 0) {
            if (step % 4 == 0) {
                double battlePitch = (tensionLevel == 2) ? 880.0 : 440.0;
                Trigger(leadV, battlePitch);
            }
        }
        else {
            if (step % 2 == 0) {
                int noteIdx = (step / 2) % minorRiffNotes.Length;
                double freq = 220.0 * Math.Pow(1.05946, minorRiffNotes[noteIdx]);
                Trigger(leadV, freq);
            }
        }
    }

    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    // --- Generators with Pitch Shift Support ---
    
    // 모든 악기 함수에 pitch 인자를 받아서 위상(Phase) 증가 속도를 곱해줍니다.

    double GenKick(double pitch) {
        if (!kickV.active) return 0.0;
        kickV.time += (1.0/sampleRate) * pitch; // 시간도 빨리 감
        if(kickV.time>0.3) kickV.active=false;
        double freq = 150.0 * Math.Exp(-kickV.time * 25.0); 
        kickV.phase += (freq * pitch)/sampleRate; // 피치 적용
        return Math.Tanh(Math.Sin(kickV.phase * 2.0 * Math.PI) * 3.0) * Math.Exp(-kickV.time * 8.0) * kickVol;
    }

    double GenSnare(double pitch) {
        if (!snareV.active) return 0.0;
        snareV.time += (1.0/sampleRate) * pitch; 
        if(snareV.time>0.2) snareV.active=false;
        double tone = Math.Sin(snareV.time * 180.0 * 2.0 * Math.PI * pitch) * Math.Exp(-snareV.time * 15.0);
        double noise = (rand.NextDouble()*2.0-1.0) * Math.Exp(-snareV.time * 25.0);
        return (tone * 0.4 + noise * 0.6) * snareVol;
    }

    double GenHihat(double pitch) {
        if (!hihatV.active) return 0.0;
        hihatV.time += (1.0/sampleRate) * pitch; 
        if(hihatV.time>0.05) hihatV.active=false;
        return (rand.NextDouble()*2.0-1.0) * Math.Exp(-hihatV.time * 60.0) * hihatVol;
    }
    
    double GenReeseBass(double pitch) {
        if (!bassV.active) return 0.0;
        bassV.time += (1.0/sampleRate) * pitch; 
        if (bassV.time > 0.5) bassV.active = false;
        bassV.phase += (bassV.freq * pitch) / sampleRate;
        double saw1 = (bassV.phase % 1.0) * 2.0 - 1.0;
        double saw2 = ((bassV.phase * 1.01) % 1.0) * 2.0 - 1.0; 
        return (saw1 + saw2) * 0.5 * Math.Exp(-bassV.time * 5.0) * bassVol;
    }

    double GenPluck(double pitch) {
        if (!leadV.active) return 0.0;
        leadV.time += (1.0/sampleRate) * pitch; 
        if(leadV.time>0.3) leadV.active=false;
        leadV.phase += (leadV.freq * pitch)/sampleRate;
        double square = (leadV.phase % 1.0) < 0.5 ? 1.0 : -1.0;
        return square * Math.Exp(-leadV.time * 10.0) * leadVol * 0.7;
    }

    double GenCinematicBrass(double pitch) {
        if (!leadV.active) return 0.0;
        leadV.time += (1.0/sampleRate) * pitch;
        if (leadV.time > 1.5) leadV.active = false; 
        leadV.phase += (leadV.freq * pitch) / sampleRate;
        double s1 = (leadV.phase % 1.0) * 2.0 - 1.0; 
        double s2 = ((leadV.phase * 1.01) % 1.0) * 2.0 - 1.0;
        double s3 = ((leadV.phase * 0.99) % 1.0) * 2.0 - 1.0;
        double raw = (s1 + s2 + s3) * 0.33;
        double attack = Math.Min(1.0, leadV.time * 5.0); 
        double decay = Math.Exp(-(leadV.time - 0.2) * 2.0);
        return Math.Tanh(raw * 4.0) * attack * decay * leadVol; 
    }

    double GenAlarm(double pitch) {
        if (!leadV.active) return 0.0;
        leadV.time += (1.0/sampleRate) * pitch; 
        if(leadV.time>0.15) leadV.active=false;
        double pMod = 1.0 - (leadV.time * 2.0);
        leadV.phase += (leadV.freq * (1.0 + pMod * 0.1) * pitch)/sampleRate;
        return ((leadV.phase % 1.0) < 0.5 ? 1.0 : -1.0) * 0.6;
    }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}