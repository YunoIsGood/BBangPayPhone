using UnityEngine;
using PrimeTween;

// 월드에서 클릭하면 360도 뷰 없이 즉시 인벤토리로 들어가는 아이템
public sealed class PickupItem : MonoBehaviour, IPickupable
{
    [field: SerializeField] public string ItemID { get; private set; }
    
    // 줌인(Focused) 상태에서만 집을 수 있음
    public bool CanInteract => InteractionStateManager.Instance.CurrentState == GameState.Focused;

    public void Interact() => Pickup();

    public void Pickup()
    {
        if (!CanInteract) return;
        
        // TODO: InventoryManager.Instance.AddItem(ItemID);
        Debug.Log($"[{ItemID}] 인벤토리 획득 완료!");

        // 빨려 들어가는 연출 후 삭제 (PrimeTween)
        Tween.Scale(transform, Vector3.zero, 0.3f, Ease.InBack)
             .OnComplete(() => Destroy(gameObject));
    }
}