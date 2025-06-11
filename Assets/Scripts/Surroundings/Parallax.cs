using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    public float length, startPosition;
    public GameObject cameraObj;

    private void Start()
    {
        startPosition = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void FixedUpdate()
    {
        float temp = (cameraObj.transform.position.x * (1 - parallaxEffect));
        float dist = (cameraObj.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startPosition + dist, transform.position.y, transform.position.z);

        if (temp > startPosition + length) startPosition += length;
        else if (temp < startPosition - length) startPosition -= length;
    }
}
