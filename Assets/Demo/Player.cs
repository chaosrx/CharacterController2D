using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float moveSpeed = 5;
    public float jumpVelocity = 5;
    public float gravity = -10;

    private Vector3 velocity;

    CharacterController2D characterController;

	void Start () {
        characterController = GetComponent<CharacterController2D>();	
	}

    void Update() {
        if (characterController.collisionState.above || characterController.collisionState.below) {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && characterController.collisionState.below) {
            velocity.y = jumpVelocity;
        }

        velocity.x = input.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }
	
}
