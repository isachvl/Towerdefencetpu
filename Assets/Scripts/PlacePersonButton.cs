using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacePersonButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private const string PlacementTag90 = "Point_people";
    private const string PlacementTag0 = "Point_people1";
    private const string PlacementTag270 = "Point_people2";

    [Header("Placement Object")]
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private Camera targetCamera;

    [Header("Cost")]
    [SerializeField] private int placementCost = 100;
    [SerializeField] private TMP_Text moneyTextFallback;

    [Header("Transform")]
    [SerializeField] private Vector3 placementOffset = Vector3.zero;

    [Header("Raycast")]
    [SerializeField] private float maxRayDistance = 500f;

    private GameObject previewInstance;
    private Collider[] previewColliders;
    private bool isHoldingButton;
    private bool isPlacing;
    private bool moneySpent;
    private bool canPlaceHere;
    private Vector3 currentPlacementPoint;
    private float currentYRotation;
    private Transform currentPlacementParent;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHoldingButton = true;
        StartPlacement();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHoldingButton = false;

        if (!isPlacing)
        {
            return;
        }

        if (canPlaceHere)
        {
            ConfirmPlacement();
            return;
        }

        CancelPlacement();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        isHoldingButton = true;
    }

    private void Update()
    {
        if (!isPlacing || previewInstance == null)
        {
            return;
        }

        UpdatePreviewPosition();

        if (isHoldingButton && !Input.GetMouseButton(0))
        {
            isHoldingButton = false;

            if (canPlaceHere)
            {
                ConfirmPlacement();
            }
            else
            {
                CancelPlacement();
            }
        }
    }

    private void StartPlacement()
    {
        if (isPlacing)
        {
            return;
        }

        if (personPrefab == null)
        {
            Debug.LogError("PlacePersonButton: personPrefab is not assigned.");
            return;
        }

        Camera activeCamera = GetCamera();
        if (activeCamera == null)
        {
            Debug.LogError("PlacePersonButton: no camera found for placement.");
            return;
        }

        if (!TrySpendMoney(placementCost))
        {
            Debug.Log($"Not enough money to place object. Need {placementCost}.");
            return;
        }

        previewInstance = Instantiate(personPrefab);
        previewColliders = previewInstance.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < previewColliders.Length; i++)
        {
            previewColliders[i].enabled = false;
        }

        previewInstance.transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
        isPlacing = true;
        moneySpent = true;
        canPlaceHere = false;
        currentPlacementPoint = previewInstance.transform.position;
        currentYRotation = 0f;
        currentPlacementParent = null;

        UpdatePreviewPosition();
    }

    private void UpdatePreviewPosition()
    {
        Camera activeCamera = GetCamera();
        if (activeCamera == null)
        {
            canPlaceHere = false;
            return;
        }

        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            currentPlacementPoint = hit.point + placementOffset;
            previewInstance.transform.position = currentPlacementPoint;
            canPlaceHere = TryGetRotationForTag(hit.collider.tag, out currentYRotation);
            currentPlacementParent = canPlaceHere ? hit.collider.transform : null;
            previewInstance.transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);
            return;
        }

        canPlaceHere = false;
        currentPlacementParent = null;
    }

    private void ConfirmPlacement()
    {
        if (currentPlacementParent != null)
        {
            previewInstance.transform.SetParent(currentPlacementParent, true);
        }

        previewInstance.transform.position = currentPlacementPoint;
        previewInstance.transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        for (int i = 0; i < previewColliders.Length; i++)
        {
            previewColliders[i].enabled = true;
        }

        PersonRangedAttacker rangedAttacker = previewInstance.GetComponent<PersonRangedAttacker>();
        if (rangedAttacker != null)
        {
            rangedAttacker.StartPlacementTimer();
        }

        previewInstance = null;
        previewColliders = null;
        isPlacing = false;
        moneySpent = false;
        canPlaceHere = false;
        currentPlacementParent = null;
    }

    private void CancelPlacement()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        RefundMoneyIfNeeded();

        previewInstance = null;
        previewColliders = null;
        isPlacing = false;
        canPlaceHere = false;
        currentPlacementParent = null;
    }

    private void RefundMoneyIfNeeded()
    {
        if (!moneySpent)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(placementCost);
        }
        else if (moneyTextFallback != null && TryGetMoneyFromText(out int currentMoney))
        {
            moneyTextFallback.text = (currentMoney + placementCost).ToString();
        }

        moneySpent = false;
    }

    private bool TrySpendMoney(int amount)
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.money < amount)
            {
                return false;
            }

            GameManager.Instance.SpendMoney(amount);
            return true;
        }

        if (moneyTextFallback == null)
        {
            Debug.LogWarning("PlacePersonButton: assign GameManager or moneyTextFallback.");
            return false;
        }

        if (!TryGetMoneyFromText(out int currentMoney) || currentMoney < amount)
        {
            return false;
        }

        moneyTextFallback.text = (currentMoney - amount).ToString();
        return true;
    }

    private bool TryGetMoneyFromText(out int currentMoney)
    {
        currentMoney = 0;

        if (moneyTextFallback == null)
        {
            return false;
        }

        string digitsOnly = string.Empty;
        string source = moneyTextFallback.text;

        for (int i = 0; i < source.Length; i++)
        {
            if (char.IsDigit(source[i]))
            {
                digitsOnly += source[i];
            }
        }

        return int.TryParse(digitsOnly, out currentMoney);
    }

    private Camera GetCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        return FindFirstObjectByType<Camera>();
    }

    private bool TryGetRotationForTag(string hitTag, out float rotationY)
    {
        rotationY = 0f;

        if (hitTag == PlacementTag90)
        {
            rotationY = 90f;
            return true;
        }

        if (hitTag == PlacementTag0)
        {
            rotationY = 0f;
            return true;
        }

        if (hitTag == PlacementTag270)
        {
            rotationY = 270f;
            return true;
        }

        return false;
    }
}
