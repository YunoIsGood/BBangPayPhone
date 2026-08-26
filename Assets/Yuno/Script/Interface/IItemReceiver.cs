using UnityEngine;
using PrimeTween;

[DisallowMultipleComponent]
public sealed class ItemReceiver : MonoBehaviour, IInteractable
{
    [Header("Requirement")]
    [SerializeField] private ItemData requiredItem;

    [Header("Action")]
    [SerializeField] private Transform targetDoor; // 열쇠로 열릴 자물쇠/문

    public bool CanInteract => InteractionStateManager.Instance != null && 
                               InteractionStateManager.Instance.CurrentState == GameState.Focused;

    public void Interact()
    {
        if (!CanInteract) return;

        ItemData currentItem = InventoryManager.Instance.SelectedItem;

        if (currentItem != null && currentItem == requiredItem)
        {
            // [성공] 올바른 아이템을 들고 클릭함
            InventoryManager.Instance.ConsumeSelectedItem();
            Tween.Rotation(targetDoor, Quaternion.Euler(0, 90f, 0), 0.8f, Ease.InOutSine);
            if (TryGetComponent(out Collider col)) col.enabled = false; 
        }
        else
        {
            // [실패] 빈손이거나 틀린 아이템 ➔ 텍스트 없이 덜컹거리는 애니메이션으로 피드백[cite: 1]
            Tween.ShakeLocalRotation(transform, strength: new Vector3(0, 0, 15f), duration: 0.3f, frequency: 10);
        }
    }
}