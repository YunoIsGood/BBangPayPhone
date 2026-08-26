using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "RustyLake/ItemData")]
public sealed class ItemData : ScriptableObject
{
    [field: SerializeField, Tooltip("아이템 고유 ID")] 
    public string ItemID { get; private set; }
    
    [field: SerializeField, Tooltip("인벤토리 UI용 2D 아이콘")] 
    public Sprite UIIcon { get; private set; }
}