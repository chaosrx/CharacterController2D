using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlatformController))]
public class FallingPlatform : MonoBehaviour {

    public Vector3 velocity;
    public Vector3 acceleration;

    protected Vector3 currentVelocity;
    protected bool isFalling = false;

    protected PlatformController platformController;

	void Start () {
        platformController = GetComponent<PlatformController>();
        currentVelocity = velocity;
	}
	
	void Update () {
        if (!isFalling) {
            platformController.MovePlatform(Vector3.zero);
            if (platformController.HasPassengers()) {
                isFalling = true;
            }
        } else {
            if (platformController.collisionState.below || platformController.collisionState.above) {
                currentVelocity.y = 0;
            }
            currentVelocity += (acceleration * Time.deltaTime);
            platformController.MovePlatform(currentVelocity * Time.deltaTime);
        }
	}
}
