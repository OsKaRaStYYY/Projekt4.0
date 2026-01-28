using UnityEngine;
using UnityEngine.InputSystem;
using PurrNet; // Biblioteka sieciowa

public class Interactor : PurrMonoBehaviour
{
    [Header("Ustawienia")]
    [SerializeField] private float interactRange = 3f;          // Jak daleko siêgamy
    [SerializeField] private float throwForce = 5.0f;           // Si³a rzutu
    [SerializeField] private Transform holdPoint;               // Pusty obiekt przed kamer¹

    [Header("Sterowanie")]
    [SerializeField] private InputActionProperty pickupAction;  // Klawisz E
    [SerializeField] private InputActionProperty dropAction;    // Klawisz Q

    // Zmienna prywatna - co aktualnie trzymamy
    private GameObject heldItem;

    // --- SETUP SIECIOWY (PurrNet) ---

    protected override void OnStart()
    {
        base.OnStart();

        // Jeœli ten skrypt nale¿y do innego gracza, wy³¹czamy mu kamerê i nas³uch audio.
        // Dziêki temu widzisz tylko swoj¹ perspektywê.
        if (!isOwner)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam) cam.enabled = false;

            var listener = GetComponentInChildren<AudioListener>();
            if (listener) listener.enabled = false;
        }
    }

    void OnEnable()
    {
        // W³¹czamy sterowanie TYLKO jeœli to nasza postaæ
        if (isOwner)
        {
            pickupAction.action.Enable();
            dropAction.action.Enable();
        }
    }

    void OnDisable()
    {
        pickupAction.action.Disable();
        dropAction.action.Disable();
    }

    // --- PÊTLA G£ÓWNA ---

    void Update()
    {
        // WA¯NE: Jeœli to nie moja postaæ, ignorujê input
        if (!isOwner) return;

        // 1. Obs³uga E (Podnoszenie / Interakcja)
        if (pickupAction.action.triggered)
        {
            if (heldItem == null)
            {
                AttemptInteraction();
            }
            // Opcjonalnie: Jeœli chcesz, ¿eby E te¿ upuszcza³o, dopisz tu 'else DropItem();'
        }

        // 2. Obs³uga Q (Wyrzucanie)
        if (dropAction.action.triggered)
        {
            if (heldItem != null)
            {
                DropItem();
            }
        }
    }

    // --- LOGIKA ---

    private void AttemptInteraction()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        // Strzelamy promieniem
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            // PRIORYTET 1: Czy to coœ interaktywnego (np. Kasa, Przycisk)?
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
                return; // Skoro kliknêliœmy przycisk, to nie szukamy dalej
            }

            // PRIORYTET 2: Czy to przedmiot do podniesienia?
            if (hit.collider.TryGetComponent(out ItemWorld item))
            {
                PickUp(item.gameObject);
            }
        }
    }

    private void PickUp(GameObject obj)
    {
        heldItem = obj;

        // Wy³¹czamy fizykê (Rigidbody), ¿eby przedmiot nie spad³
        if (heldItem.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        // Wy³¹czamy kolizje, ¿eby przedmiot nie uderza³ w gracza
        if (heldItem.TryGetComponent(out Collider col))
            col.enabled = false;

        // Przyczepiamy do punktu przed kamer¹
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }

    private void DropItem()
    {
        if (heldItem == null) return;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        Collider col = heldItem.GetComponent<Collider>();

        // 1. Najpierw w³¹czamy kolizje (wa¿ne dla fizyki)
        if (col != null) col.enabled = true;

        // 2. Odpinamy od rodzica (HoldPoint)
        heldItem.transform.SetParent(null);

        // 3. W³¹czamy fizykê i rzucamy
        if (rb != null)
        {
            rb.isKinematic = false;

            // Zerujemy rotacjê, ¿eby przedmiot nie "pamiêta³" krêcenia sprzed podniesienia
            rb.angularVelocity = Vector3.zero;

            // Obliczamy si³ê uwzglêdniaj¹c masê (ciê¿sze lec¹ wolniej)
            // Mathf.Max zabezpiecza przed dzieleniem przez zero
            float finalForce = throwForce / Mathf.Max(rb.mass, 0.1f);

            rb.AddForce(transform.forward * finalForce, ForceMode.Impulse);
        }

        // Czyœcimy referencjê
        heldItem = null;
    }
}
}