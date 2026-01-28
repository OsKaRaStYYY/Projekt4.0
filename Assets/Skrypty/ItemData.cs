using UnityEngine;

[CreateAssetMenu(fileName = "NowyPrzedmiot", menuName = "Sklep/Przedmiot")]
public class ItemData : ScriptableObject
{
    public string itemName;      // Nazwa przedmiotu
    public int price;            // Cena w sklepie
    public GameObject prefab;    // Model 3D, który pojawi siê w œwiecie
    public Sprite icon;          // Ikona do ewentualnego ekwipunku
}