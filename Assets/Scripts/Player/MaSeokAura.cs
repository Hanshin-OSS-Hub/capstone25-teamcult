using UnityEngine;

public class MaSeokAura : MonoBehaviour
{
    private ParticleSystem auraParticle;

    void Start()
    {
        GameObject particleObj = new GameObject("Aura");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = new Vector3(0, -0.3f, 0);

        auraParticle = particleObj.AddComponent<ParticleSystem>();

        var main = auraParticle.main;
        main.loop = true;
        main.startLifetime = 1.0f;
        main.startSpeed = 0.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = new Color(0.6f, 0.1f, 1f, 1f);
        main.gravityModifier = -0.15f;   // 위로 올라가게
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = auraParticle.emission;
        emission.rateOverTime = 6f;

        // ★ 아래에서 위로 올라오게 - Edge 타입으로 바닥선에서 생성
        var shape = auraParticle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
        shape.radius = 0.2f;             // 마석 너비만큼만
        shape.rotation = new Vector3(0, 0, 0); // 가로로 눕혀서 바닥처럼

        var colorOverLifetime = auraParticle.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.6f, 0.1f, 1f), 0f),
                new GradientColorKey(new Color(0.6f, 0.1f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var renderer = auraParticle.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingLayerName = "Player";
        renderer.sortingOrder = 10;

        auraParticle.Play();
    }
}