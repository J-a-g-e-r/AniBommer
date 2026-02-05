using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Player playerInput;
    private CharacterController controller;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private GameObject bombPrefab;

    private void Awake()
    {
        playerInput = new Player();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }





    void Update()
    {
        Vector2 movementInput = playerInput.PlayerController.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        controller.Move(move * Time.deltaTime * playerSpeed);


        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // MPlace bomb
        if (playerInput.PlayerController.PlaceBomb.triggered)
        {
            //Instantiate(bombPrefab, new Vector3(Mathf.RoundToInt(transform.position.x),bombPrefab.transform.position.y, Mathf.RoundToInt(transform.position.z)),bombPrefab.transform.rotation);
            float tileSize = 2f;

            float snapX = Mathf.Round(transform.position.x / tileSize) * tileSize;
            float snapZ = Mathf.Round(transform.position.z / tileSize) * tileSize;

            Instantiate(
                bombPrefab,
                new Vector3(
                    snapX,
                    bombPrefab.transform.position.y,
                    snapZ
                ),
                bombPrefab.transform.rotation
            );
        }

    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = 0.5f;
        pos.x = Mathf.Clamp(pos.x, -18f, 18f);
        pos.z = Mathf.Clamp(pos.z, -12f, 12f);
        transform.position = pos;
    }
}
