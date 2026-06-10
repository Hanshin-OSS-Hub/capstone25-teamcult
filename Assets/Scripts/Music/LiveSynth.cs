using UnityEngine;
using System; 

public class LiveSynth : MonoBehaviour
{
    [Range(1, 20000)]
    public double frequency = 440.0; // 기본 라(A) 음
    public double gain = 0.1;

    private double increment;
    private double phase;
    private double sampling_frequency = 48000.0;

    public Func<double, double, double> audioFunction = (phase, time) => 
    {
        return Math.Sin(phase); 
    };

    void Start()
    {
        sampling_frequency = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2.0 * Math.PI / sampling_frequency;

        double time = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            float value = (float)(gain * audioFunction(phase, time));

            data[i] = value;
            
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
    
    public void UpdateAudioCode()
    {
        audioFunction = (p, t) => 
        {
            return (p / Math.PI) - 1.0; // 톱니파 공식
        };
        Debug.Log("공식이 톱니파로 변경되었습니다!");
    }
}