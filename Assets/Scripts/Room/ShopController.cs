using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour {
    [Header("아이템 설정")]
    [SerializeField] private List<GameObject> _itemPrefabs;

    [Header("배치 설정")]
    [SerializeField] private float _totalWidth = 12.0f;     
    [SerializeField] private int _spawnCount = 2;           

    [Header("가격 설정")]
    [SerializeField] private int _minPrice = 150;           
    [SerializeField] private int _maxPrice = 500;          

    [Header("현재 생성된 아이템")]
    [SerializeField] private List<GameObject> _spawnedItems = new List<GameObject>();

    [Header("UI 설정")]
    [SerializeField] private GameObject _priceTextPrefab; 
    [SerializeField] private Vector3 _textOffset = new Vector3(0, 1.5f, 0); 

    private void Start() {
        SpawnShopItems();
    }

    private void SpawnShopItems() {
        if (_itemPrefabs == null || _itemPrefabs.Count == 0) {
            Debug.LogError($"{gameObject.name}: _itemPrefabs 리스트가 비어 있어 아이템을 생성할 수 없습니다.");
            return;
        }

        if (_itemPrefabs.Count < _spawnCount) {
            int originalCount = _spawnCount;
            _spawnCount = _itemPrefabs.Count;
            Debug.Log($"{gameObject.name}: 아이템 프리팹 부족으로 생성 개수를 {originalCount}개에서 {_spawnCount}개로 조정했습니다.");
        }

        if (_spawnCount <= 0) {
            Debug.LogWarning($"{gameObject.name}: 생성할 아이템 개수가 0개입니다. 배치를 중단합니다.");
            return;
        }

        ClearOldItems();

        List<int> indices = GetRandomIndices(_itemPrefabs.Count, _spawnCount);
        float interval = _totalWidth / (_spawnCount + 1);
        float startX = -(_totalWidth / 2f);

        for (int i = 0; i < _spawnCount; i++) {
            float xOffset = startX + (interval * (i + 1));
            Vector3 spawnPos = transform.position + (transform.right * xOffset);

            GameObject itemObj = Instantiate(_itemPrefabs[indices[i]], spawnPos, transform.rotation);
            itemObj.transform.SetParent(this.transform);
            _spawnedItems.Add(itemObj);

            if (_priceTextPrefab != null) {
                Vector3 textWorldPos = spawnPos + _textOffset;

                GameObject priceObj = Instantiate(_priceTextPrefab, textWorldPos, Quaternion.identity);

                _spawnedItems.Add(priceObj);

                if (itemObj.TryGetComponent(out ItemPickUp pickUpScript)) {
                    int randomPrice = Random.Range(_minPrice, _maxPrice + 1);
                    var tmp = priceObj.GetComponent<TextMeshPro>();
                    pickUpScript.InitializeShopItem(randomPrice, tmp);
                }
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

    private void OnDrawGizmosSelected() {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); 
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_totalWidth, 1f, 0.1f));


        if (_spawnCount > 0) {
            Gizmos.color = new Color(0f, 1f, 0f, 0.7f); // 조금 더 진한 녹색

            float interval = _totalWidth / (_spawnCount + 1);
            float startX = -(_totalWidth / 2f);

            for (int i = 0; i < _spawnCount; i++) {
                float xOffset = startX + (interval * (i + 1));
                Vector3 localSpawnPos = new Vector3(xOffset, 0f, 0f);

                Matrix4x4 circleMatrix = Matrix4x4.TRS(
                    transform.TransformPoint(localSpawnPos), 
                    transform.rotation,                     
                    new Vector3(1f, 1f, 0.01f)              
                );
                Gizmos.matrix = circleMatrix;

                Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
            }
        }

        Gizmos.matrix = originalMatrix;
    }
}