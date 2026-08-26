using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightFrame;
    
    private InventoryManager.InventoryItem _slotData;
    public ItemData CurrentData => _slotData?.Data;

    public void Setup(InventoryManager.InventoryItem item)
    {
        _slotData = item;
        iconImage.sprite = item.Data.UIIcon;
        highlightFrame.enabled = false;
        
        InventoryManager.Instance.OnItemSelected += HandleItemSelected;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemSelected -= HandleItemSelected;
    }

    private void HandleItemSelected(ItemData selectedItem)
    {
        highlightFrame.enabled = (_slotData != null && _slotData.Data == selectedItem);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_slotData == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // [좌클릭] 아이템 장착(선택)
            InventoryManager.Instance.SelectItem(_slotData.Data);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 🚨 핵심 방어막 1: 이미 어떤 아이템이든 관찰 모드가 켜져 있다면, 중복 우클릭을 완벽히 무시! (겹침 방지)
            if (InteractionStateManager.Instance != null && 
                InteractionStateManager.Instance.CurrentState == GameState.Inspect) 
                return;

            InspectStoredItem();
        }
    }

    private void InspectStoredItem()
    {
        if (_slotData.InspectableModel == null || InspectViewer.Instance == null) return;

        Transform targetT = ((MonoBehaviour)_slotData.InspectableModel).transform;
        
        // 🚨 핵심 방어막 2: 스케일을 1로 고정하는 코드를 '삭제'했습니다.
        // 이제 씬 바닥에 놓여있던 원본 크기(예: 0.1)를 그대로 유지하므로 거대해지지 않습니다.
        targetT.position = InspectViewer.Instance.transform.position;
        targetT.gameObject.SetActive(true);

        InspectViewer.Instance.StartInspect(_slotData.InspectableModel);

        void OnInspectEnded(GameState state)
        {
            if (state != GameState.Inspect)
            {
                InteractionStateManager.Instance.OnStateChanged -= OnInspectEnded;
                
                // 관찰 모드가 끝나면 즉시 화면에서 숨김
                if (targetT != null) targetT.gameObject.SetActive(false);
            }
        }
        InteractionStateManager.Instance.OnStateChanged += OnInspectEnded;
    }
}