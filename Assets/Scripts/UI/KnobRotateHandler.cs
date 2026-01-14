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
        Vector2 touchPos = eventData.position;
        Vector2 direction = touchPos - centerPoint;

        // 중심점으로부터 마우스 위치의 각도를 구합니다. (유니티는 12시 방향이 0도)
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

        if (angle < 0) angle += 360f; // 0~360도로 변환

        // 각도(0~360)를 0~1 사이의 값으로 변환
        float value = angle / 360f;

        // 매니저에 값 전달
        if (isVolumeKnob)
            manager.UpdateEnemyVolume(value);
        else
            manager.UpdateMapBrightness(value);

        // 실제 노브 이미지도 마우스 방향에 맞춰 회전시키고 싶다면 아래 코드 추가
        // transform.localRotation = Quaternion.Euler(0, 0, -angle);
    }

}