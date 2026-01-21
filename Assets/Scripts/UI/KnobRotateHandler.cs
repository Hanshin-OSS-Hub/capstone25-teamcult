using UnityEngine;
using UnityEngine.EventSystems; // 드래그 이벤트를 위해 필요

public class KnobRotateHandler : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [Header("Settings")]
    public GameSettingManager manager; // 아까 만든 매니저 연결
    public bool isVolumeKnob = true;   // 볼륨인지 밝기인지 체크

    private Vector2 centerPoint;

    // 드래그 시작 시 노브의 중심점을 계산합니다.
    public void OnBeginDrag(PointerEventData eventData)
    {
        centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
    }

    // 마우스를 드래그하는 동안 계속 호출됩니다.
    public void OnDrag(PointerEventData eventData)
    {
        if (manager == null) return;

        // 노브의 중심점을 구합니다.
        Vector2 center = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 dir = eventData.position - center;

        // 각도 계산 (12시 방향이 0도가 되도록 설정)
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 0~1 사이의 값으로 변환
        float value = angle / 360f;

        if (isVolumeKnob)
            manager.UpdateEnemyVolume(value);
        else
            manager.UpdateMapBrightness(value);
    }

}