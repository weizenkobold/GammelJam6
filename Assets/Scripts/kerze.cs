using UnityEngine;

public class kerze : MonoBehaviour
{
    private Light pointLight;
    public float flickerAmount;
    public float speed;
    public float baseIntensity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        pointLight.intensity = baseIntensity + Mathf.PerlinNoise(Time.time * speed, 0f) * flickerAmount;
    }
}
