using UnityEngine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks; // 비동기 처리를 위해 필요

public class CodeManager : MonoBehaviour
{
    [Header("Multitrack Setup")]
    public LiveSynth[] tracks;

    public class Globals
    {
        public double phase;
        public double time;
    }

    void Start()
    {
        // 자동 연결 (트랙이 비어있으면 자식들에서 찾기)
        if (tracks == null || tracks.Length == 0)
        {
            tracks = GetComponentsInChildren<LiveSynth>();
        }
    }

    // [핵심 수정] async 키워드 추가 (비동기 함수)
    public async void ApplyCodeToTrack(int trackIndex, string sourceCode)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Length) return;
        if (tracks[trackIndex] == null) return;

        try
        {
            // 1. 메인 스레드를 멈추지 않기 위해 'Task.Run'으로 다른 스레드에서 컴파일 수행
            var runner = await Task.Run(() => 
            {
                var options = ScriptOptions.Default.AddImports("System", "System.Math");
                var script = CSharpScript.Create<double>(sourceCode, options, typeof(Globals));
                return script.CreateDelegate();
            });

            // 2. 컴파일이 끝나면 다시 메인 스레드에서 함수 교체
            if (tracks[trackIndex] != null)
            {
                tracks[trackIndex].audioFunction = (p, t) =>
                {
                    return runner(new Globals { phase = p, time = t }).Result;
                };
            }
        }
        catch (Exception e)
        {
            // 컴파일 실패해도 게임은 안 멈춤, 로그만 남김
            Debug.LogWarning($"[Compile Skip] Track {trackIndex} Error: {e.Message}");
        }
    }
}