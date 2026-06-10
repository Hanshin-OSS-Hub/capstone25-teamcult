using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    private Material mat;

    private string hitParam = "_HitAmount";

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        mat.SetFloat(hitParam, 1f);

        yield return new WaitForSeconds(0.1f);

        mat.SetFloat(hitParam, 0f);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Flash();
        }
    }
}