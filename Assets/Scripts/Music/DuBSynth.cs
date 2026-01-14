using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public class DnBSynth : MonoBehaviour
{
    [Header("Cinematic Settings")]
    [Range(60, 200)] public double bpm = 160.0;
    [Range(0, 1)] public float masterVolume = 0.5f;
    public float cutoffFrequency = 20000f;

    [Header("AI Generation Params (LLM Controls This)")]
    // 이 변수들을 나중에 LLM이 조절하게 됩니다.
    [Range(0, 1)] public float chaos = 0.2f;    // 0: 순차적, 1: 랜덤 도약
    [Range(0, 1)] public float density = 0.8f;  // 0: 쉼표 많음, 1: 꽉 채움
    [Range(0, 1)] public float pitchBias = 0.5f;// 0: 저음 위주, 1: 고음 위주

    [Header("Dynamic Layers")]
    public float kickVol = 0.8f; 
    public float snareVol = 0.5f;
    public float hihatVol = 0.3f;
    public float bassVol = 0.7f; 
    public float leadVol = 0.6f; 

    [HideInInspector] public bool flameMode = false; 
    [HideInInspector] public bool isPlaying = true;
    [HideInInspector] public int tensionLevel = 0; 
    [HideInInspector] public bool lowHealthMode = false;

    // 내부 변수
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

    // AI가 생성할 멜로디를 담을 공간 (16스텝)
    private int[] generatedMelody = new int[16];
    
    // C Minor Scale (사용할 재료)
    private int[] scale = { -12, -5, 0, 3, 5, 7, 10, 12, 15, 19 };

    // 글리치
    private float glitchIntensity = 0.0f;

    void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
        if (sampleRate <= 0) sampleRate = 44100.0;

        AudioSource source = GetComponent<AudioSource>();
        source.Stop(); source.loop = true; source.Play(); 
        
        // 시작할 때 첫 멜로디 생성
        GenerateMarkovMelody();
    }

    public void TriggerGlitch() { glitchIntensity = 1.0f; }

    // ★★★ 마르코프 체인 작곡 알고리즘 ★★★
    // LLM이 설정한 chaos, density 값을 바탕으로 다음 노트를 확률적으로 결정
    public void GenerateMarkovMelody()
    {
        int currentNoteIndex = 2; // 시작은 중간음(0) 근처에서

        for (int i = 0; i < 16; i++)
        {
            // 1. Density 체크 (소리를 낼 것인가 쉼표를 둘 것인가)
            // 리듬감을 위해 4의 배수 박자는 확률 높임
            float currentDensity = density + ((i % 4 == 0) ? 0.2f : 0.0f);
            
            if (UnityEngine.Random.value > currentDensity)
            {
                generatedMelody[i] = -999; // 쉼표
                continue;
            }

            // 2. Markov Transition (다음 음 결정)
            int move = 0;

            if (UnityEngine.Random.value < chaos)
            {
                // 혼돈(Chaos)이 높으면: 크게 도약 (-3칸 ~ +3칸 이상)
                move = UnityEngine.Random.Range(-4, 5);
            }
            else
            {
                // 질서(Order)가 높으면: 인접한 음으로 이동 (-1, 0, +1)
                move = UnityEngine.Random.Range(-1, 2);
            }

            // 3. Pitch Bias 적용 (고음/저음 선호도)
            // bias가 높을수록 위로 갈 확률 증가
            if (pitchBias > 0.7f) move += 1;
            if (pitchBias < 0.3f) move -= 1;

            // 인덱스 적용 및 범위 제한
            currentNoteIndex = Mathf.Clamp(currentNoteIndex + move, 0, scale.Length - 1);
            generatedMelody[i] = scale[currentNoteIndex];
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying || sampleRate <= 0) return;

        double samplesPerTick = sampleRate * (60.0 / bpm) / 4.0; 
        float currentCutoff = Mathf.Clamp(cutoffFrequency, 50f, 20000f);

        for (int i = 0; i < data.Length; i += channels)
        {
            // 글리치 효과 (피치 시프트)
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
                
                // [자동 작곡] 16박자(한 마디)가 끝날 때마다 새로운 패턴 생성 시도
                // 단, 너무 자주 바뀌면 정신없으니 8마디(128스텝)마다 한 번씩
                // 여기선 데모를 위해 stepIndex가 0일 때 (16스텝마다) 20% 확률로 변주
                if (stepIndex == 0 && rand.NextDouble() < 0.2) 
                {
                    GenerateMarkovMelody();
                }
            }

            double kick = GenKick(pitchShift);
            double snare = GenSnare(pitchShift);
            double hihat = GenHihat(pitchShift);
            double bass = GenReeseBass(pitchShift);
            
            double lead = 0.0;
            if (lowHealthMode) lead = GenAlarm(pitchShift); 
            else if (flameMode) lead = GenCinematicBrass(pitchShift); // 각성 모드
            else lead = GenPluck(pitchShift); // 일반 모드 (AI 작곡 멜로디)

            double mix = kick + snare + hihat + bass + lead;
            
            if (tensionLevel == 0 && !flameMode) mix *= 0.6;
            if (glitchIntensity > 0.1f) mix = Math.Tanh(mix * 3.0);
            else mix = Math.Tanh(mix * 1.5); 
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

    void ProcessSequencer(int step)
    {
        // ... (드럼, 베이스 로직은 기존과 동일) ...
        bool kTrig = (step == 0) || (step == 10);
        if (flameMode && step % 4 == 0) kTrig = true; 
        if (!flameMode && tensionLevel == 0 && step == 10) kTrig = false; 
        if (kTrig) Trigger(kickV);

        bool sTrig = (step == 4) || (step == 12);
        if (!flameMode && tensionLevel == 0) sTrig = false; 
        if (sTrig) Trigger(snareV);

        if (step % 2 == 0 || tensionLevel >= 1 || flameMode) Trigger(hihatV);

        if (step == 0) {
            double note = (flameMode) ? 32.7 : ((tensionLevel >= 1) ? 43.65 : 32.7); 
            Trigger(bassV, note);
        }

        // LEAD (여기가 AI 멜로디 적용되는 곳)
        if (lowHealthMode) {
            if (step % 4 == 0) Trigger(leadV, 880.0); 
        }
        else if (flameMode) {
            // 각성 모드는 고정된 웅장한 패턴 (AI 적용 안 함, 테마 유지)
            if (step % 8 == 0) Trigger(leadV, 55.0); 
        }
        else {
            // [일반/전투 모드] 마르코프 체인이 만든 멜로디 연주
            int noteNum = generatedMelody[step];
            if (noteNum != -999) // 쉼표가 아니면 재생
            {
                double freq = 220.0 * Math.Pow(1.05946, noteNum);
                Trigger(leadV, freq);
            }
        }
    }

    // ... (Gen 함수들과 Helper 함수들은 기존 시네마틱 버전과 동일하게 유지) ...
    void Trigger(Voice v, double freq = 0) { v.active = true; v.time = 0.0; v.phase = 0.0; if(freq>0) v.freq = freq; }

    double GenKick(double p) { if(!kickV.active)return 0; kickV.time+=p/sampleRate; if(kickV.time>0.3)kickV.active=false; double f=150*Math.Exp(-kickV.time*25); kickV.phase+=f*p/sampleRate; return Math.Tanh(Math.Sin(kickV.phase*2*Math.PI)*3)*Math.Exp(-kickV.time*8)*kickVol; }
    double GenSnare(double p) { if(!snareV.active)return 0; snareV.time+=p/sampleRate; if(snareV.time>0.2)snareV.active=false; double t=Math.Sin(snareV.time*180*2*Math.PI*p)*Math.Exp(-snareV.time*15); double n=(rand.NextDouble()*2-1)*Math.Exp(-snareV.time*25); return (t*0.4+n*0.6)*snareVol; }
    double GenHihat(double p) { if(!hihatV.active)return 0; hihatV.time+=p/sampleRate; if(hihatV.time>0.05)hihatV.active=false; return (rand.NextDouble()*2-1)*Math.Exp(-hihatV.time*60)*hihatVol; }
    double GenReeseBass(double p) { if(!bassV.active)return 0; bassV.time+=p/sampleRate; if(bassV.time>0.5)bassV.active=false; bassV.phase+=bassV.freq*p/sampleRate; double s1=(bassV.phase%1.0)*2-1; double s2=((bassV.phase*1.01)%1.0)*2-1; return (s1+s2)*0.5*Math.Exp(-bassV.time*5)*bassVol; }
    double GenPluck(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.3)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double s=(leadV.phase%1.0)<0.5?1:-1; return s*Math.Exp(-leadV.time*10)*leadVol*0.7; }
    double GenCinematicBrass(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>1.5)leadV.active=false; leadV.phase+=leadV.freq*p/sampleRate; double s1=(leadV.phase%1.0)*2-1; double s2=((leadV.phase*1.01)%1.0)*2-1; double s3=((leadV.phase*0.99)%1.0)*2-1; double raw=(s1+s2+s3)*0.33; double env=Math.Min(1,leadV.time*5)*Math.Exp(-(leadV.time-0.2)*2); return Math.Tanh(raw*4)*env*leadVol; }
    double GenAlarm(double p) { if(!leadV.active)return 0; leadV.time+=p/sampleRate; if(leadV.time>0.15)leadV.active=false; double pm=1-(leadV.time*2); leadV.phase+=(leadV.freq*(1+pm*0.1)*p)/sampleRate; return ((leadV.phase%1.0)<0.5?1:-1)*0.6; }

    class Voice { public bool active; public double time; public double phase; public double freq; }
}