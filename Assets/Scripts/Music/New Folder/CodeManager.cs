using UnityEngine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks; 

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
        if (tracks == null || tracks.Length == 0)
        {
            tracks = GetComponentsInChildren<LiveSynth>();
        }
    }

    public async void ApplyCodeToTrack(int trackIndex, string sourceCode)
    {
        if (trackIndex < 0 || trackIndex >= tracks.Length) return;
        if (tracks[trackIndex] == null) return;

        try
        {
            var runner = await Task.Run(() => 
            {
                var options = ScriptOptions.Default.AddImports("System", "System.Math");
                var script = CSharpScript.Create<double>(sourceCode, options, typeof(Globals));
                return script.CreateDelegate();
            });

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
            Debug.LogWarning($"[Compile Skip] Track {trackIndex} Error: {e.Message}");
        }
    }
}