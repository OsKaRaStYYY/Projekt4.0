using UnityEngine;
using UnityEngine.UIElements; // KLUCZOWE: Biblioteka UI Toolkit
using PurrNet;

public class ShopUI_Toolkit : PurrMonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private UIDocument document; // Przypisz tutaj obiekt z komponentem UI Document
    [SerializeField] private ShoppingBasket basket;

    // Zmienne na znalezione etykiety
    private Label _walletLabel;
    private Label _basketLabel;

    private void OnEnable()
    {
        // 1. Pobieramy "korzeñ" drzewa UI
        var root = document.rootVisualElement;

        // 2. Szukamy etykiet po nazwach, które nada³eœ w UI Builderze
        // Upewnij siê, ¿e w UXML nazwa³eœ je dok³adnie "WalletLabel" i "BasketLabel"
        _walletLabel = root.Q<Label>("WalletLabel");
        _basketLabel = root.Q<Label>("BasketLabel");
    }

    private void Start()
    {
        // Startujemy logikê sieciow¹
        StartCoroutine(SubscribeToEconomy());
    }

    private System.Collections.IEnumerator SubscribeToEconomy()
    {
        // Czekamy na LobbyEconomy
        while (LobbyEconomy.Instance == null) yield return null;

        // Subskrypcja (taka sama jak wczeœniej)
        LobbyEconomy.Instance.teamMoney.onChanged += UpdateWalletDisplay;

        // Pierwsze odœwie¿enie
        UpdateWalletDisplay(LobbyEconomy.Instance.teamMoney.Value);
    }

    private void UpdateWalletDisplay(int amount)
    {
        // W UI Toolkit po prostu zmieniamy property .text
        if (_walletLabel != null)
        {
            _walletLabel.text = $"Portfel: {amount} z³";
        }
    }

    void Update()
    {
        // Aktualizacja koszyka w ka¿dej klatce
        if (_basketLabel != null && basket != null)
        {
            _basketLabel.text = $"W koszyku: {basket.GetTotalCost()} z³";
        }
    }
}