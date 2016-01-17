using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : RaycastController {

    public LayerMask passengerMask;

    public Vector3 velocity;

	protected override void Start () {
        base.Start();
	}

    void Update() {
        UpdateRaycastOrigins();

        Vector3 distance = velocity * Time.deltaTime;

        if (distance.y > 0) {
            MovePassengers(distance);
            transform.Translate(distance);
        }
        else {
            transform.Translate(distance);
            MovePassengers(distance);
        }
        
    }

    protected void MovePassengers(Vector3 distance) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();

        float directionX = Mathf.Sign(distance.x);
        float directionY = Mathf.Sign(distance.y);

        if (distance.y != 0) {
            float rayLength = Mathf.Abs(distance.y) + _skinWidth;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float moveX = (directionY == 1) ? distance.x : 0;
                        float moveY = distance.y - (hit.distance - _skinWidth) * directionY;

                        hit.transform.GetComponent<CharacterController2D>().Move(new Vector3(moveX, moveY), true);
                    }
                }
            }
        }

        if (distance.x != 0) {
            float rayLength = Mathf.Abs(distance.x) + _skinWidth;

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float moveX = distance.x - (hit.distance - _skinWidth) * directionX;
                        float moveY = 0;

                        hit.transform.GetComponent<CharacterController2D>().Move(new Vector3(moveX, moveY), true);
                    }
                }
            }
        }

        if (directionY == -1 || distance.y == 0 && distance.x != 0) {
            float rayLength = _skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = _raycastOrigins.topLeft + Vector2.right * (_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float moveX = distance.x;
                        float moveY = distance.y;

                        hit.transform.GetComponent<CharacterController2D>().Move(new Vector3(moveX, moveY), true);
                    }
                }
            }
        }
    }

}
