using UnityEngine;

public static class CameraExtentions
{
    public static Bounds OrthographicBounds(this Camera camera)
    {
        if(camera.orthographic)
        {
             float screenAspect = (float)UnityEngine.Screen.width / (float)UnityEngine.Screen.height;
            float cameraHeight = camera.orthographicSize * 2;
            Bounds bounds = new Bounds(
                camera.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
        return new Bounds();
    }
}
