using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float parallaxEffect;
    public float length, startPosition;
    public GameObject cameraObj;
    private float lastCameraY;

    private void Start()
    {
        startPosition = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        lastCameraY = cameraObj.transform.position.y;
    }

    private void LateUpdate()
    {
        float temp = (cameraObj.transform.position.x * (1 - parallaxEffect));
        float dist = (cameraObj.transform.position.x * parallaxEffect);

        float deltaY = cameraObj.transform.position.y - lastCameraY;
        float newY = transform.position.y - deltaY;

        transform.position = new Vector3(startPosition + dist, newY, transform.position.z);

        lastCameraY = cameraObj.transform.position.y;

        if (temp > startPosition + length) startPosition += length;
        else if (temp < startPosition - length) startPosition -= length;
    }
}
