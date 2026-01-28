using UnityEngine;
using System.Collections.Generic;

public class ShoppingBasket : MonoBehaviour
{
    // Lista przedmiotów, które fizycznie s¹ w koszyku
    public List<ItemWorld> itemsInBasket = new List<ItemWorld>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ItemWorld item))
        {
            if (!itemsInBasket.Contains(item))
            {
                itemsInBasket.Add(item);
                Debug.Log($"W koszyku: {item.data.itemName} (+{item.data.price} z³)");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ItemWorld item))
        {
            if (itemsInBasket.Contains(item))
            {
                itemsInBasket.Remove(item);
                Debug.Log($"Wyjêto z koszyka: {item.data.itemName}");
            }
        }
    }

    public int GetTotalCost()
    {
        int total = 0;
        foreach (var item in itemsInBasket)
        {
            total += item.data.price;
        }
        return total;
    }
}