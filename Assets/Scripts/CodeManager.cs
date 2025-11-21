using UnityEngine;
using Microsoft.CodeAnalysis.CSharp.Scripting; // 방금 설치한 패키지
using Microsoft.CodeAnalysis.Scripting;
using System;

public class CodeManager : MonoBehaviour
{
    // 우리가 1단계에서 만든 신디사이저 연결
    public LiveSynth synth;

    // 컴파일할 때 사용할 전역 변수 설정 (AI가 'phase'나 'time'을 쓸 수 있게)
    public class Globals
    {
        public double phase;
        public double time;
    }

    // 테스트를 위해 Inspector에서 직접 코드를 입력해 볼 공간
    [TextArea(5, 10)]
    public string codeInput = "return Math.Sin(phase);";

    // 이 함수를 실행하면 텍스트가 진짜 코드로 변해서 적용됨!
    [ContextMenu("Apply Code")] // 컴포넌트 우클릭 메뉴로 실행 가능
    public async void ApplyCode()
    {
        if (synth == null)
        {
            Debug.LogError("LiveSynth가 연결되지 않았습니다!");
            return;
        }

        try
        {
            // 1. 스크립트 옵션 설정 (System.Math 같은 기본 라이브러리 사용 허용)
            var options = ScriptOptions.Default
                .AddImports("System", "System.Math"); // Math.Sin 처럼 쓰기 위해

            // 2. 텍스트를 실제 함수(Func)로 컴파일
            // 입력값: Globals(phase, time), 출력값: double
            var script = CSharpScript.Create<double>(codeInput, options, typeof(Globals));
            var runner = script.CreateDelegate();

            // 3. LiveSynth의 오디오 함수 교체 (Hot Swap)
            synth.audioFunction = (p, t) =>
            {
                // AI가 만든 코드를 실행!
                // RunAsync는 무거우니 미리 컴파일된 runner를 씁니다.
                // 다만 runner는 Task를 반환하므로 동기식으로 값을 가져옵니다.
                // (성능 최적화를 위해선 구조를 더 다듬어야 하지만 일단은 이렇게 갑니다)
                return runner(new Globals { phase = p, time = t }).Result;
            };

            Debug.Log("✅ 코드 적용 성공!");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 컴파일 오류: {e.Message}");
        }
    }
}