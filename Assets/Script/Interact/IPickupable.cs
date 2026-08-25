public interface IPickupable : IInteractable
{
    string ItemID { get; } // 인벤토리 데이터와 매핑될 ID
    void Pickup(); // 클릭 즉시 호출됨 (360도 관찰 없음)
}