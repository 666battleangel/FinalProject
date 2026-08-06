using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public Camera cameraToLookAt;

	void Update()
	{
		if (cameraToLookAt == null) return;
		transform.LookAt(cameraToLookAt.transform);
	}
}
