using UnityEngine;

public class MenuCameraFly : MonoBehaviour
{
    [System.Serializable]
    public class CameraPoint
    {
        public Vector3 position;
        public Vector3 rotation;
        public bool teleportHere;
        public bool teleportWithoutInstantRotation;
        public float waitTime;

        public CameraPoint(
            Vector3 position,
            Vector3 rotation,
            bool teleportHere = false,
            bool teleportWithoutInstantRotation = false,
            float waitTime = 0.4f)
        {
            this.position = position;
            this.rotation = rotation;
            this.teleportHere = teleportHere;
            this.teleportWithoutInstantRotation = teleportWithoutInstantRotation;
            this.waitTime = waitTime;
        }
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 2.2f;
    [SerializeField] private float positionThreshold = 0.08f;
    [SerializeField] private float rotationThreshold = 0.8f;

    [Header("Loop")]
    [SerializeField] private bool loop = true;

    private CameraPoint[] points;
    private int currentIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private void Awake()
    {
        points = new CameraPoint[]
        {
            

            // ПЕРВЫЙ ТЕЛЕПОРТ: только позиция, без мгновенного поворота
            new CameraPoint(new Vector3(56.2999992f,  3.38000011f, -33.7999992f), new Vector3(0f, 270f,        0f), true, true),
            new CameraPoint(new Vector3(-46.7999992f, 3.38000011f, -33.7999992f), new Vector3(0f, 270f,        0f)),

            new CameraPoint(new Vector3(-46.7999992f, 3.38000011f, -33.7999992f), new Vector3(0f, 358.309662f, 0f)),
            new CameraPoint(new Vector3(-52f,         3.38000011f, 142f),         new Vector3(0f, 358.309662f, 0f)),

            new CameraPoint(new Vector3(8.80000019f,  3.70000005f, 8.30000019f),  new Vector3(0f, 344.647705f, 0f), true),
            new CameraPoint(new Vector3(5.5f,         3.70000005f, 20.3999996f),  new Vector3(0f, 344.647705f, 0f)),

            new CameraPoint(new Vector3(9.60000038f,  3.38000011f, 119.300003f),  new Vector3(0f, 358.309662f, 0f), true),
            new CameraPoint(new Vector3(9.10000038f,  3.38000011f, 136.300003f),  new Vector3(0f, 290.876343f, 0f)),
            new CameraPoint(new Vector3(4.4000001f,   3.38000011f, 138.100006f),  new Vector3(0f, 189.954697f, 0f)),
            new CameraPoint(new Vector3(2.5999999f,   3.38000011f, 128f),         new Vector3(0f, 189.954697f, 0f)),

            new CameraPoint(new Vector3(-1.35000002f, 3.38000011f, -13.8199997f), new Vector3(0f, 342.304779f, 0f), true, true),
            new CameraPoint(new Vector3(-7.30000019f, 3.38000011f, 4.80999994f),  new Vector3(0f, 342.304779f, 0f)),
            new CameraPoint(new Vector3(-17.9599991f, 3.38000011f, 6.82000017f), new Vector3(0f, 280.645325f, 0f)),
            new CameraPoint(new Vector3(-18.5f,       3.38000011f, 46.7000008f), new Vector3(0f, 359.899902f, 0f)),
            new CameraPoint(new Vector3(-9.30000019f, 3.38000011f, 52.0999985f), new Vector3(0f, 59.6002235f, 0f)),
            new CameraPoint(new Vector3(-9.69999981f, 3.38000011f, 133.899994f), new Vector3(0f, 359.135742f, 0f)),

           

            

           

            new CameraPoint(new Vector3(26.2999992f,  3.70000005f, -0.400000006f), new Vector3(0f, 0f, 0f), true),
            new CameraPoint(new Vector3(26.2999992f,  3.70000005f, 117.699997f),   new Vector3(0f, 0f, 0f)),
        };
    }

    private void Start()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("MenuCameraFly: точки не заданы.");
            enabled = false;
            return;
        }

        transform.position = points[0].position;
        transform.rotation = Quaternion.Euler(points[0].rotation);
        currentIndex = 1;
    }

    private void Update()
    {
        if (points == null || points.Length == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                AdvanceToNextPoint();
            }
            return;
        }

        CameraPoint target = points[currentIndex];

        if (target.teleportHere)
        {
            transform.position = target.position;

            if (!target.teleportWithoutInstantRotation)
            {
                transform.rotation = Quaternion.Euler(target.rotation);
            }

            isWaiting = true;
            waitTimer = target.waitTime;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        Quaternion targetRotation = Quaternion.Euler(target.rotation);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * 100f * Time.deltaTime
        );

        bool reachedPosition = Vector3.Distance(transform.position, target.position) <= positionThreshold;
        bool reachedRotation = Quaternion.Angle(transform.rotation, targetRotation) <= rotationThreshold;

        if (reachedPosition && reachedRotation)
        {
            transform.position = target.position;
            transform.rotation = targetRotation;

            isWaiting = true;
            waitTimer = target.waitTime;
        }
    }

    private void AdvanceToNextPoint()
    {
        currentIndex++;

        if (currentIndex >= points.Length)
        {
            if (loop)
                currentIndex = 0;
            else
                enabled = false;
        }
    }
}