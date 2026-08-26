using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // 아이템 데이터와 3D 모델 원본을 묶어서 보관
    public sealed class InventoryItem
    {
        public ItemData Data { get; }
        public IInspectable InspectableModel { get; }

        public InventoryItem(ItemData data, IInspectable model)
        {
            Data = data;
            InspectableModel = model;
        }
    }

    private readonly List<InventoryItem> _inventory = new();
    
    // 현재 손에 쥔(선택된) 아이템
    public ItemData SelectedItem { get; private set; } 

    public event Action<InventoryItem> OnItemAdded;
    public event Action<ItemData> OnItemSelected;
    public event Action<ItemData> OnItemConsumed;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void AddItem(ItemData data, IInspectable model)
    {
        var newItem = new InventoryItem(data, model);
        _inventory.Add(newItem);
        OnItemAdded?.Invoke(newItem);
    }

    public void SelectItem(ItemData data)
    {
        SelectedItem = data;
        OnItemSelected?.Invoke(data);
    }

    public void ConsumeSelectedItem()
    {
        if (SelectedItem == null) return;
        
        ItemData consumed = SelectedItem;
        _inventory.RemoveAll(x => x.Data == consumed);
        SelectedItem = null;
        
        OnItemConsumed?.Invoke(consumed);
        OnItemSelected?.Invoke(null); 
    }
}