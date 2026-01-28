using UnityEngine;
using PurrNet; // Namespace PurrNet

public class LobbyEconomy : PurrMonoBehaviour
{
    public static LobbyEconomy Instance;

    // SyncVar automatycznie wysy³a now¹ wartoœæ do wszystkich graczy
    // FullAccess = ka¿dy widzi, ServerWrite = tylko serwer zmienia
    public SyncVar<int> teamMoney = new SyncVar<int>(1000);

    protected override void OnAwake() // W PurrNet u¿ywamy OnAwake zamiast Awake dla bezpieczenstwa
    {
        base.OnAwake();
        if (Instance == null) Instance = this;
    }

    // Funkcja wywo³ywana przez Kasê (Clienta), ale wykonywana na Serwerze
    [ServerRpc]
    public void RequestSpendMoney(int amount, bool clearBasket)
    {
        if (teamMoney.Value >= amount)
        {
            teamMoney.Value -= amount;
            // Tutaj mo¿emy dodaæ logikê czyszczenia koszyka lub wys³aæ potwierdzenie
            Debug.Log($"[SERVER] Wydano {amount}. Zosta³o: {teamMoney.Value}");
        }
        else
        {
            Debug.Log("[SERVER] Brak œrodków!");
        }
    }

    // Funkcja pomocnicza do sprawdzania stanu (tylko odczyt)
    public bool CanAfford(int amount)
    {
        return teamMoney.Value >= amount;
    }
}