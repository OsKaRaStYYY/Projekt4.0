using UnityEngine;
using PurrNet;
using System;

public class CashRegister : PurrMonoBehaviour, IInteractable
{
    [SerializeField] private ShoppingBasket basket;

    public void Interact()
    {
        // Wa¿ne: W PurrNet musisz sprawdziæ, czy masz prawo klikaæ
        // Ale w przypadku kasy, ka¿dy gracz mo¿e wys³aæ RPC do serwera.

        int total = basket.GetTotalCost();

        if (total <= 0) return;

        // Sprawdzamy lokalnie czy nas staæ (¿eby nie spamiæ serwera)
        if (LobbyEconomy.Instance.CanAfford(total))
        {
            // Wysy³amy proœbê do serwera: "Halo, chcê wydaæ X kasy!"
            LobbyEconomy.Instance.RequestSpendMoney(total, true);

            // Lokalne czyszczenie koszyka (wizualne)
            // W pe³nym multi powinieneœ u¿yæ NetworkDestroy na przedmiotach
            ClearBasket();
        }
        else
        {
            Debug.Log("Lokalnie widzê, ¿e nas nie staæ.");
        }
    }

    private void ClearBasket()
    {
        foreach (var item in basket.itemsInBasket)
        {
            // WA¯NE: W PurrNet niszczymy obiekty sieciowe inaczej:
            if (item.TryGetComponent<PurrNet.NetworkObject>(out var netObj))
            {
                // Jeœli jesteœmy serwerem - niszczymy od razu
                if (isServer) netObj.Despawn();
                // Jeœli jesteœmy klientem - musielibyœmy poprosiæ serwer o zniszczenie (RPC)
                // Na razie dla uproszczenia ukrywamy obiekt:
                else item.gameObject.SetActive(false);
            }
            else
            {
                Destroy(item.gameObject);
            }
        }
        basket.itemsInBasket.Clear();
    }
}