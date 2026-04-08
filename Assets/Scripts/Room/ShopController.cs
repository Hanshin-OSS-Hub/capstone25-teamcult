using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour {
    [Header("아이템 설정")]
    [SerializeField] private List<GameObject> _itemPrefabs;

    [Header("배치 설정")]
    [SerializeField] private float _totalWidth = 12.0f;     // 전체 관리 범위 (초기값 12)
    [SerializeField] private int _spawnCount = 2;           // 생성할 아이템 개수

    [Header("가격 설정")]
    [SerializeField] private int _minPrice = 150;           // 최소 가격
    [SerializeField] private int _maxPrice = 500;           // 최대 가격

    [Header("현재 생성된 아이템")]
    [SerializeField] private List<GameObject> _spawnedItems = new List<GameObject>();

    [Header("UI 설정")]
    [SerializeField] private GameObject _priceTextPrefab; // TMP 컴포넌트가 붙은 텍스트 프리팹
    [SerializeField] private Vector3 _textOffset = new Vector3(0, 1.5f, 0); // 아이템 머리 위 높이

    private void Start() {
        SpawnShopItems();
    }

    private void SpawnShopItems() {
        // 1. 프리팹 리스트 상태 확인
        if (_itemPrefabs == null || _itemPrefabs.Count == 0) {
            Debug.LogError($"{gameObject.name}: _itemPrefabs 리스트가 비어 있어 아이템을 생성할 수 없습니다.");
            return;
        }

        // 2. _spawnCount 조정 로직
        if (_itemPrefabs.Count < _spawnCount) {
            int originalCount = _spawnCount;
            _spawnCount = _itemPrefabs.Count;
            Debug.Log($"{gameObject.name}: 아이템 프리팹 부족으로 생성 개수를 {originalCount}개에서 {_spawnCount}개로 조정했습니다.");
        }

        // 3. 최종 개수가 0인 경우 체크
        if (_spawnCount <= 0) {
            Debug.LogWarning($"{gameObject.name}: 생성할 아이템 개수가 0개입니다. 배치를 중단합니다.");
            return;
        }

        // 기존 아이템 제거
        ClearOldItems();



        // 4. 배치 로직 수행
        List<int> indices = GetRandomIndices(_itemPrefabs.Count, _spawnCount);
        float interval = _totalWidth / (_spawnCount + 1);
        float startX = -(_totalWidth / 2f);

        for (int i = 0; i < _spawnCount; i++) {
            // [수정] xOffset을 기반으로 실제 소환 위치(spawnPos)를 계산합니다.
            float xOffset = startX + (interval * (i + 1));
            Vector3 spawnPos = transform.position + (transform.right * xOffset);

            // 1. 아이템 생성 (이제 계산된 spawnPos를 사용합니다)
            GameObject itemObj = Instantiate(_itemPrefabs[indices[i]], spawnPos, transform.rotation);
            itemObj.transform.SetParent(this.transform);
            _spawnedItems.Add(itemObj);

            // 2. 가격표 생성 및 아이템의 자식으로 설정
            if (_priceTextPrefab != null) {
                GameObject priceObj = Instantiate(_priceTextPrefab, itemObj.transform);
                priceObj.transform.localPosition = _textOffset; // 아이템 기준 상대 위치

                // 3. 컴포넌트 초기화
                if (itemObj.TryGetComponent(out ItemPickUp pickUpScript)) {
                    int randomPrice = Random.Range(_minPrice, _maxPrice + 1);

                    // 생성한 TMP를 전달하며 초기화 (TextMeshPro 컴포넌트 추출)
                    var tmp = priceObj.GetComponent<TextMeshPro>();
                    pickUpScript.InitializeShopItem(randomPrice, tmp);
                }
            }
            else {
                Debug.LogWarning($"{gameObject.name}: 가격표 프리팹(_priceTextPrefab)이 설정되지 않았습니다.");
            }
        }
    }

    private void ClearOldItems() {
        foreach (var item in _spawnedItems) {
            if (item != null) Destroy(item);
        }
        _spawnedItems.Clear();
    }

    private List<int> GetRandomIndices(int totalSize, int count) {
        List<int> tempList = new List<int>();
        for (int i = 0; i < totalSize; i++) tempList.Add(i);

        List<int> result = new List<int>();
        for (int i = 0; i < count; i++) {
            int randomIndex = Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }
        return result;
    }

    // 에디터 뷰에서 범위를 시각적으로 확인하기 위한 기즈모
    // 에디터 뷰에서 범위를 시각적으로 확인하기 위한 기즈모
    private void OnDrawGizmosSelected() {
        // Gizmos는 기본적으로 월드 좌표계를 따르므로, 
        // 오브젝트의 회전과 스케일을 반영하기 위해 행렬을 설정합니다.
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        // ---------------------------------------------------------
        // 1. 전체 관리 범위 시각화 (높이 1의 직사각형)
        // ---------------------------------------------------------
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 연한 녹색

        // Gizmos.DrawWireCube는 중심점 기준으로 그리므로, 로컬 (0,0,0)에 그립니다.
        // 사이즈는 (가로=_totalWidth, 세로=1, 두께=0.1 정도로 평면처럼 보이게)
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_totalWidth, 1f, 0.1f));


        // ---------------------------------------------------------
        // 2. 아이템 배치 예정 위치 시각화 (지름 1의 원)
        // ---------------------------------------------------------
        if (_spawnCount > 0) {
            Gizmos.color = new Color(0f, 1f, 0f, 0.7f); // 조금 더 진한 녹색

            // 배치 간격 계산 로직 (SpawnShopItems와 동일)
            float interval = _totalWidth / (_spawnCount + 1);
            float startX = -(_totalWidth / 2f);

            for (int i = 0; i < _spawnCount; i++) {
                // 로컬 좌표계에서의 X 위치 계산
                float xOffset = startX + (interval * (i + 1));
                Vector3 localSpawnPos = new Vector3(xOffset, 0f, 0f);

                // 지름이 1이므로 반지름은 0.5f
                // 3D 공간이므로 WireSphere를 사용하지만, 두께를 아주 얇게 눌러서 원처럼 보이게 합니다.
                // 이를 위해 잠시 기즈모 행렬을 해당 위치로 이동시키고 스케일을 조절합니다.
                Matrix4x4 circleMatrix = Matrix4x4.TRS(
                    transform.TransformPoint(localSpawnPos), // 월드 위치
                    transform.rotation,                     // 월드 회전
                    new Vector3(1f, 1f, 0.01f)              // Z축을 찌그러뜨려 원처럼 만듦
                );
                Gizmos.matrix = circleMatrix;

                Gizmos.DrawWireSphere(Vector3.zero, 0.5f); // 반지름 0.5 = 지름 1
            }
        }

        // Gizmos 행렬을 원래대로 복구 (다른 기즈모에 영향을 주지 않기 위해)
        Gizmos.matrix = originalMatrix;
    }
}