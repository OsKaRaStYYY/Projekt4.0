using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private float interactRange = 3f;      // Jak daleko siêgamy
    [SerializeField] private Transform holdPoint;          // Nasz punkt 'HoldPoint'
    [SerializeField] private InputActionProperty pickupAction; // Przycisk E
    [SerializeField] private InputActionProperty dropAction; // Q?
    [SerializeField] private float throwForce = 1.0f; // Si³a rzutu
    private GameObject heldItem; // Tu zapamiêtamy, co trzymamy

    void OnEnable()
    {
        pickupAction.action.Enable();
        dropAction.action.Enable();
    }
    void OnDisable()
    {
        pickupAction.action.Disable();
        dropAction.action.Disable();
    }
    void Update()
    {
        // Jeœli naciœniemy E
        if (pickupAction.action.triggered)
        {
            if (heldItem == null)
            {
                TryPickUp();
            }
        }

        if (dropAction.action.triggered)
        {
            if (heldItem != null)
            {
                DropItem(); ;
            }
        }
    }
    private void TryPickUp()
    {
        // Strzelamy laserem ze œrodka ekranu
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            // Czy trafiony obiekt ma skrypt ItemWorld?
            if (hit.collider.TryGetComponent(out ItemWorld item))
            {
                heldItem = hit.collider.gameObject;

                // Wy³¹czamy fizykê, ¿eby przedmiot nie ucieka³
                if (heldItem.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
                // Wy³¹czamy kolizje
                if (heldItem.TryGetComponent(out Collider col)) col.enabled = false;

                // "Przyklejamy" do d³oni
                heldItem.transform.SetParent(holdPoint);
                heldItem.transform.localPosition = Vector3.zero;
                heldItem.transform.localRotation = Quaternion.identity;

                Debug.Log("Podniesiono: " + item.data.itemName);
            }
        }
    }
    private void DropItem()
    {
        if (heldItem == null) return;

        bool hasRigidbody = heldItem.TryGetComponent(out Rigidbody rb);
        bool hasCollider = heldItem.TryGetComponent(out Collider col);

        if (hasCollider) col.enabled = true;

        if (hasRigidbody)
        {
            rb.isKinematic = false;
            heldItem.transform.SetParent(null);

            // 1. Resetujemy wszelkie wczeœniejsze obroty, ¿eby przedmiot nie "pamiêta³" krêcenia
            rb.angularVelocity = Vector3.zero;

            // 2. Obliczamy si³ê rzutu
            float finalForce = throwForce / Mathf.Max(rb.mass, 0.1f);

            // 3. Dodajemy tylko si³ê liniow¹ (bez AddTorque)
            rb.AddForce(transform.forward * finalForce, ForceMode.Impulse);
        }
        else
        {
            heldItem.transform.SetParent(null);
        }

        heldItem = null;
    }
    /* private void DropItem()
     {
         if (heldItem == null) return;

         // Odpinamy od d³oni
         if (heldItem.TryGetComponent(out Rigidbody rb))
         {
             // 1. Wy³¹czamy Kinematic, ¿eby fizyka znów dzia³a³a
             rb.isKinematic = false;
             // 2. Odpinamy od d³oni (kamery)
             heldItem.transform.SetParent(null);
             // 3. Dodajemy si³ê rzutu w kierunku, w którym patrzy kamera
             // ForceMode.Impulse jest idealny do nag³ych akcji jak rzut
             rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
             // Opcjonalnie: dodaj lekki obrót, ¿eby rzut wygl¹da³ naturalniej
             // W³¹czamy kolizje
             if (heldItem.TryGetComponent(out Collider col)) col.enabled = true;
             // Zale¿noœæ od masy: im wiêksza masa, tym mniejsza prêdkoœæ koñcowa
             float finalForce = throwForce / rb.mass;
             rb.AddForce(transform.forward * finalForce, ForceMode.Impulse);
         }
         else
         {
             // Jeœli przedmiot nie ma Rigidbody, po prostu go odpinamy
             heldItem.transform.SetParent(null);
         }

         heldItem = null;
     }
    */
}
