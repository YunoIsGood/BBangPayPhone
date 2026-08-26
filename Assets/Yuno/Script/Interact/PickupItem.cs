using UnityEngine;
<<<<<<< Updated upstream:Assets/Yuno/Script/Interact/PickupItem.cs
using PrimeTween;
using Script.Interact;
=======
>>>>>>> Stashed changes:Assets/Script/Interact/PickupItem.cs

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    private IInspectable _inspectableComponent;

    public bool CanInteract => InteractionStateManager.Instance != null && 
                               InteractionStateManager.Instance.CurrentState == GameState.Focused;

    private void Awake()
    {
        _inspectableComponent = GetComponent<IInspectable>();
    }

    public void Interact()
    {
        if (!CanInteract || InventoryManager.Instance == null) return;

        // 매니저에 데이터 등록
        InventoryManager.Instance.AddItem(itemData, _inspectableComponent);
        
        // 애니메이션 없이 즉시 화면에서 비활성화 (스케일 꼬임 원천 차단)
        gameObject.SetActive(false);
    }
}