using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : RaycastController {

    public LayerMask passengerMask;

    public Vector3[] localWaypoints;
    protected Vector3[] globalWaypoints;

    public float speed;
    public bool cyclic;
    public float waitTime;
    [Range(0, 2)]
    public float ease;

    protected int fromWaypointIndex;
    protected float percentBetweenWaypoints;
    protected float nextMoveTime;

	protected override void Start () {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++) {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void Update() {
        UpdateRaycastOrigins();

        Vector3 distance = CalculatePlatformMovement();

        if (distance.y > 0) {
            MovePassengers(distance);
            transform.Translate(distance);
        }
        else {
            transform.Translate(distance);
            MovePassengers(distance);
        }
        
    }

    protected float Ease(float x) {
        float a = ease + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    protected Vector3 CalculatePlatformMovement() {
        if (Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1) {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic) {
                if (fromWaypointIndex >= globalWaypoints.Length - 1) {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }

            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
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

    void OnDrawGizmos() {
        if (localWaypoints != null) {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++) {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }

}
