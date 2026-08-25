public interface IItemReceiver : IInteractable
{
    bool CanReceiveItem(string itemID); // 선택한 아이템이 열쇠인지 검증
    void ReceiveItem(string itemID);
}