using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public sealed class SolarSystemMotion : MonoBehaviour
{
    private readonly List<OrbitBody> bodies = new();
    private Transform sun;
    private Transform earth;
    private Transform moon;
    private float lastRealtime;
    private float moonAngle;

    private readonly Dictionary<string, OrbitSettings> orbitSettings = new()
    {
        { "Mercury", new OrbitSettings(1.8f, 0.68f, 8f, 15f, 0.32f) },
        { "Venus", new OrbitSettings(2.7f, 1.02f, 6.5f, 55f, 0.42f) },
        { "Earth", new OrbitSettings(3.7f, 1.4f, 5.2f, 105f, 0.48f) },
        { "Mars", new OrbitSettings(4.9f, 1.85f, 4.2f, 155f, 0.38f) },
        { "Jupiter", new OrbitSettings(6.3f, 2.38f, 2.7f, 210f, 0.78f) },
        { "Saturn", new OrbitSettings(7.8f, 2.95f, 2.1f, 255f, 0.72f) },
        { "Uranus", new OrbitSettings(9.2f, 3.48f, 1.6f, 300f, 0.58f) },
        { "Neptune", new OrbitSettings(10.6f, 4f, 1.25f, 335f, 0.56f) },
        { "Pluto", new OrbitSettings(11.8f, 4.45f, 0.95f, 25f, 0.28f) },
    };

    private readonly Dictionary<string, float> spinSpeeds = new()
    {
        { "Sun Sphere", 6f },
        { "Mercury", 34f },
        { "Venus", -18f },
        { "Earth", 55f },
        { "Mars", 44f },
        { "Jupiter", 85f },
        { "Saturn", 72f },
        { "Uranus", -45f },
        { "Neptune", 48f },
        { "Pluto", 25f },
    };

    private void OnEnable()
    {
        CacheBodies();
        lastRealtime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (bodies.Count == 0 || sun == null)
        {
            CacheBodies();
        }

        var deltaTime = Application.isPlaying ? Time.deltaTime : GetEditorDeltaTime();
        if (deltaTime <= 0f)
        {
            return;
        }

        foreach (var body in bodies)
        {
            AnimateBody(body, deltaTime);
        }

        AnimateMoon(deltaTime);
    }

    private void CacheBodies()
    {
        bodies.Clear();
        sun = FindInChildren("Sun Sphere");
        earth = FindInChildren("Earth");
        moon = FindInChildren("Moon");

        if (sun != null)
        {
            sun.localPosition = Vector3.zero;
            sun.localScale = Vector3.one * 0.9f;
        }

        foreach (var pair in orbitSettings)
        {
            var body = FindInChildren(pair.Key);
            if (body == null || sun == null)
            {
                continue;
            }

            body.localScale = Vector3.one * pair.Value.scale;
            bodies.Add(new OrbitBody
            {
                transform = body,
                radiusX = pair.Value.radiusX,
                radiusZ = pair.Value.radiusZ,
                orbitAngle = pair.Value.startAngle,
                localY = 0f,
                orbitSpeed = pair.Value.speed,
                spinSpeed = spinSpeeds.TryGetValue(pair.Key, out var spin) ? spin : 30f,
            });

            PositionBody(bodies[bodies.Count - 1]);
        }

        if (sun != null && spinSpeeds.TryGetValue("Sun Sphere", out var sunSpin))
        {
            bodies.Add(new OrbitBody
            {
                transform = sun,
                radiusX = 0f,
                radiusZ = 0f,
                orbitAngle = 0f,
                localY = sun.localPosition.y,
                orbitSpeed = 0f,
                spinSpeed = sunSpin,
            });
        }
    }

    private void AnimateBody(OrbitBody body, float deltaTime)
    {
        if (body.transform == null)
        {
            return;
        }

        body.transform.Rotate(Vector3.up, body.spinSpeed * deltaTime, Space.Self);

        if (body.radiusX <= 0f || body.radiusZ <= 0f || sun == null)
        {
            return;
        }

        body.orbitAngle += body.orbitSpeed * deltaTime;
        PositionBody(body);
    }

    private void PositionBody(OrbitBody body)
    {
        if (body.transform == null || sun == null || body.radiusX <= 0f || body.radiusZ <= 0f)
        {
            return;
        }

        var radians = body.orbitAngle * Mathf.Deg2Rad;
        body.transform.localPosition = sun.localPosition + new Vector3(
            Mathf.Cos(radians) * body.radiusX,
            body.localY,
            Mathf.Sin(radians) * body.radiusZ
        );
    }

    private void AnimateMoon(float deltaTime)
    {
        if (moon == null || earth == null)
        {
            return;
        }

        moon.Rotate(Vector3.up, 35f * deltaTime, Space.Self);
        moonAngle += 65f * deltaTime;
        var radians = moonAngle * Mathf.Deg2Rad;
        moon.localScale = Vector3.one * 0.16f;
        moon.localPosition = earth.localPosition + new Vector3(Mathf.Cos(radians) * 0.55f, 0.08f, Mathf.Sin(radians) * 0.28f);
    }

    private Transform FindInChildren(string objectName)
    {
        var children = GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }

    private float GetEditorDeltaTime()
    {
        var currentRealtime = Time.realtimeSinceStartup;
        var deltaTime = Mathf.Clamp(currentRealtime - lastRealtime, 0f, 0.033f);
        lastRealtime = currentRealtime;
        return deltaTime;
    }

    private sealed class OrbitBody
    {
        public Transform transform;
        public float radiusX;
        public float radiusZ;
        public float orbitAngle;
        public float localY;
        public float orbitSpeed;
        public float spinSpeed;
    }

    private readonly struct OrbitSettings
    {
        public readonly float radiusX;
        public readonly float radiusZ;
        public readonly float speed;
        public readonly float startAngle;
        public readonly float scale;

        public OrbitSettings(float radiusX, float radiusZ, float speed, float startAngle, float scale)
        {
            this.radiusX = radiusX;
            this.radiusZ = radiusZ;
            this.speed = speed;
            this.startAngle = startAngle;
            this.scale = scale;
        }
    }
}
