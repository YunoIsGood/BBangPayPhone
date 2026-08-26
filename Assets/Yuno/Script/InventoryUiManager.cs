using UnityEngine;

[DisallowMultipleComponent]
public sealed class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("아까 만든 인벤토리 그리드(컨테이너)")] 
    private Transform gridContainer;
    
    [SerializeField, Tooltip("아까 만든 슬롯 프리팹")] 
    private InventorySlotUI slotPrefab;

    private void OnEnable()
    {
        // 매니저가 초기화된 후 이벤트를 구독해야 하므로 Start 등에서 연결해도 됩니다.
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded += AddSlotUI;
            InventoryManager.Instance.OnItemConsumed += RemoveSlotUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemAdded -= AddSlotUI;
            InventoryManager.Instance.OnItemConsumed -= RemoveSlotUI;
        }
    }

    // 아이템을 주웠을 때 UI 슬롯 생성
    private void AddSlotUI(InventoryManager.InventoryItem newItem)
    {
        InventorySlotUI spawnedSlot = Instantiate(slotPrefab, gridContainer);
        spawnedSlot.Setup(newItem); // 아이콘 및 데이터 세팅
    }

    // 아이템을 사용(소모)했을 때 UI 슬롯 삭제
    private void RemoveSlotUI(ItemData consumedData)
    {
        // Grid 안의 모든 슬롯을 검사하여 소모된 데이터와 일치하는 슬롯 파괴
        foreach (Transform child in gridContainer)
        {
            if (child.TryGetComponent(out InventorySlotUI slot))
            {
                // Reflection이나 Public Getter로 확인 (InventorySlotUI에 현재 아이템 데이터를 반환하는 함수 필요)
                if (slot.CurrentData == consumedData) 
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }
}