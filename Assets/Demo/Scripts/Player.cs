using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float moveSpeed = 5;
    public float jumpHeight = 2;
    public float jumpTime = 0.4f;

    private float _jumpVelocity;
    private float _gravity;
    private Vector3 _velocity;

    private Controller2D characterController;

	void Start () {
        characterController = GetComponent<Controller2D>();

        _gravity = -(2 * jumpHeight) / Mathf.Pow(jumpTime, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * jumpTime;
    }

    void Update() {
        if (characterController.collisionState.above || characterController.collisionState.below) {
            _velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && characterController.collisionState.below) {
            _velocity.y = _jumpVelocity;
        }

        _velocity.x = input.x * moveSpeed;
        _velocity.y += _gravity * Time.deltaTime;

        characterController.Move(_velocity * Time.deltaTime);
    }
	
}
