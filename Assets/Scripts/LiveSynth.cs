using UnityEngine;
using System; // 수학 함수(Math)를 쓰기 위해 필요

public class LiveSynth : MonoBehaviour
{
    // 1. 소리의 높낮이 (AI가 건드릴 변수들)
    [Range(1, 20000)]
    public double frequency = 440.0; // 기본 '라(A)' 음
    public double gain = 0.1; // 볼륨 (0.0 ~ 1.0)

    private double increment;
    private double phase;
    private double sampling_frequency = 48000.0;

    // 2. [핵심] 소리를 만드는 '공식'을 담는 변수 (Delegate)
    // 처음에는 기본 사인파(Sine Wave) 공식을 넣어둡니다.
    public Func<double, double, double> audioFunction = (phase, time) => 
    {
        // 기본 공식: Sin(위상)
        return Math.Sin(phase); 
    };

    void Start()
    {
        sampling_frequency = AudioSettings.outputSampleRate;
    }

    // 3. 스피커로 소리를 내보내는 함수 (Unity가 자동으로 호출)
    void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2.0 * Math.PI / sampling_frequency;

        // 현재 시간 (변화하는 소리를 위해)
        double time = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            
            // 여기서 위에서 정의한 'audioFunction'을 실행합니다!
            // 나중에 AI가 이 audioFunction의 내용만 쏙 바꿔치기 할 겁니다.
            float value = (float)(gain * audioFunction(phase, time));

            data[i] = value;
            
            // 스테레오 설정 (왼쪽, 오른쪽 스피커 동일하게)
            if (channels == 2)
            {
                data[i + 1] = value;
            }

            if (phase > (Math.PI * 2))
            {
                phase = 0.0;
            }
        }
    }
    
    // 테스트용: 공식을 바꾸는 함수 (나중에 AI가 이 함수를 호출하게 됨)
    public void UpdateAudioCode()
    {
        // 예시: 버튼을 누르면 '톱니파(Sawtooth)' 소리로 변경
        audioFunction = (p, t) => 
        {
            return (p / Math.PI) - 1.0; // 톱니파 공식
        };
        Debug.Log("공식이 톱니파로 변경되었습니다!");
    }
}