// WaypointNetwork.cs
// Вешаем на объект PathNetwork (родительский объект для всех путей)
using System.Collections.Generic;
using UnityEngine;

public class WaypointNetwork : MonoBehaviour
{
    [System.Serializable]
    public class PathData
    {
        public string pathName;
        public Transform spawnPoint;      // Точка спавна врагов для этого пути
        public List<Transform> waypoints; // Точки пути (от спавна до цели)
        public Color pathColor = Color.white;
    }

    public List<PathData> paths = new List<PathData>();
    private Dictionary<Transform, PathData> spawnToPath = new Dictionary<Transform, PathData>();

    private void Awake()
    {
        // Создаем словарь для быстрого поиска пути по точке спавна
        foreach (var path in paths)
        {
            if (path.spawnPoint != null && path.waypoints.Count > 0)
            {
                spawnToPath[path.spawnPoint] = path;
            }
        }
    }

    public PathData GetPathForSpawn(Transform spawnPoint)
    {
        if (spawnToPath.TryGetValue(spawnPoint, out PathData path))
            return path;

        Debug.LogWarning($"Не найден путь для точки спавна {spawnPoint.name}");
        return null;
    }

    public Vector3 GetWaypointPosition(PathData path, int index)
    {
        if (path != null && index >= 0 && index < path.waypoints.Count)
            return path.waypoints[index].position;
        return Vector3.zero;
    }

    public int GetNextWaypointIndex(PathData path, int currentIndex)
    {
        if (currentIndex + 1 < path.waypoints.Count)
            return currentIndex + 1;
        return -1; // Достигнут конец пути
    }

    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        foreach (var path in paths)
        {
            if (path.waypoints.Count < 2) continue;

            Gizmos.color = path.pathColor;

            // Рисуем путь
            for (int i = 0; i < path.waypoints.Count - 1; i++)
            {
                if (path.waypoints[i] != null && path.waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(path.waypoints[i].position, path.waypoints[i + 1].position);

                    // Стрелка направления
                    Vector3 dir = (path.waypoints[i + 1].position - path.waypoints[i].position).normalized;
                    Vector3 midPoint = (path.waypoints[i].position + path.waypoints[i + 1].position) / 2;
                }
            }

            // Рисуем точки спавна
            if (path.spawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(path.spawnPoint.position, 0.5f);

                // Соединяем спавн с первой точкой пути
                if (path.waypoints.Count > 0 && path.waypoints[0] != null)
                {
                    Gizmos.color = path.pathColor;
                    Gizmos.DrawLine(path.spawnPoint.position, path.waypoints[0].position);
                }
            }
        }
    }
}