using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class CharacterController2D : MonoBehaviour {

    [Range (2, 20)]
    public int horizontalRayCount = 4;
    [Range(2, 20)]
    public int verticalRayCount = 4;
    public LayerMask collisionMask;
    public float maxSlopeAngle = 70.0f;

    private float _skinWidth = 0.015f;
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;

    private BoxCollider2D _collider;
    private RaycastOrigins _raycastOrigins;
    public CollisionState collisionState;

    void Start () {
        _collider = GetComponent<BoxCollider2D>();
	}

    public void Move(Vector3 distance) {
        UpdateRaycastOrigins();
        CalculateRaySpacing();
        collisionState.Reset();

        if(distance.y < 0) {
            HandleSlopeDescending(ref distance);
        }
        if (distance.x != 0) {
            HandleHorizontalCollisions(ref distance);
        }
        if (distance.y != 0) {
            HandleVerticalCollisions(ref distance);
        }

        transform.Translate(distance);
    }

    void HandleHorizontalCollisions(ref Vector3 distance) {
        float directionX = Mathf.Sign(distance.x);
        float rayLength = Mathf.Abs(distance.x) + _skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxSlopeAngle) {
                    if (slopeAngle != collisionState.oldSlopeAngle) {
                        float distanceToSlope = hit.distance - _skinWidth;
                        distance.x -= distanceToSlope * directionX;
                    }
                    HandleSlopeClimbing(ref distance, slopeAngle);
                }

                if (!collisionState.climbingSlope || slopeAngle > maxSlopeAngle) {
                    distance.x = (hit.distance - _skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisionState.climbingSlope) {
                        distance.y = Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(distance.x);
                    }

                    collisionState.left = directionX == -1;
                    collisionState.right = directionX == 1;
                }
            }
        }
    }

    void HandleVerticalCollisions(ref Vector3 distance) {
        float directionY = Mathf.Sign(distance.y);
        float rayLength = Mathf.Abs(distance.y) + _skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + distance.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit) {
                distance.y = (hit.distance - _skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisionState.climbingSlope) {
                    distance.x = distance.y / Mathf.Tan(collisionState.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(distance.x);
                }

                collisionState.below = directionY == -1;
                collisionState.above = directionY == 1;
            }
        }
    }

    void HandleSlopeClimbing(ref Vector3 distance, float slopeAngle) {
        if (distance.y <= 0) {
            float moveDistance = Mathf.Abs(distance.x);
            distance.y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            distance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(distance.x);

            collisionState.climbingSlope = true;
            collisionState.slopeAngle = slopeAngle;
            collisionState.below = true;
        }
    }

    void HandleSlopeDescending(ref Vector3 distance) {
        float directionX = Mathf.Sign(distance.x);
        Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit) {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
                if (Mathf.Sign(hit.normal.x) == directionX) {
                    float moveDistance = Mathf.Abs(distance.x);
                    if (hit.distance - _skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * moveDistance) {
                        distance.y -= Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        distance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(distance.x);

                        collisionState.descendingSlope = true;
                        collisionState.slopeAngle = slopeAngle;
                        collisionState.below = true;
                    }
                }
            }
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = _collider.bounds;
        bounds.Expand(_skinWidth * -2);

        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    void CalculateRaySpacing() {
        Bounds bounds = _collider.bounds;
        bounds.Expand(_skinWidth * -2);

        _horizontalRaySpacing = bounds.size.x / (horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public struct CollisionState {
        public bool above, below, left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle;
        public float oldSlopeAngle;

        public void Reset() {
            above = below = left = right = false;

            climbingSlope = false;
            descendingSlope = false;
            oldSlopeAngle = slopeAngle;
            slopeAngle = 0;
        }
    }
}
