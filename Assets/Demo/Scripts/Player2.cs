using UnityEngine;
using System.Collections;

public class Player2 : MonoBehaviour {

    public float moveSpeed = 5;
    public float jumpHeight = 2;
    public float jumpTime = 0.4f;

    private float _jumpVelocity;
    private float _gravity;
    private Vector3 _velocity;

    private CharacterController2D characterController;

	void Start () {
        characterController = GetComponent<CharacterController2D>();

        _gravity = -(2 * jumpHeight) / Mathf.Pow(jumpTime, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * jumpTime;
    }

    void Update() {
        if (characterController.collisionState.above || characterController.collisionState.below) {
            _velocity.y = 0;
        }

        float inputX = Input.GetKey(KeyCode.RightArrow) ? 1f : 0f;
        inputX = Input.GetKey(KeyCode.LeftArrow) ? -1f : inputX;

        Vector2 input = new Vector2(inputX, 0);

        if (Input.GetKeyDown(KeyCode.UpArrow) && characterController.collisionState.below) {
            _velocity.y = _jumpVelocity;
        }

        _velocity.x = input.x * moveSpeed;
        _velocity.y += _gravity * Time.deltaTime;

        characterController.Move(_velocity * Time.deltaTime);
    }
	
}
