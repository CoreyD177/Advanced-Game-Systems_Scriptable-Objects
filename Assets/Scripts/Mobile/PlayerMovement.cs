using UnityEngine; //Connect to Unity Engine
[RequireComponent(typeof(CharacterController))] //Require Character Controller for movement
public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Speed & Height")]
    [Tooltip("Set the speed you want the character to move at.")]
    [SerializeField] private float _speed = 5f;
    [Tooltip("Set the strength of gravity for this scene in minus figures so we can use it to drg the character down.")]
    [SerializeField] private float _gravity = -9.81f;
    [Tooltip("Set the height you want the character to jump to.")]
    [SerializeField] private float _jumpHeight = 3f;
    //Private variable to store and use our calculated jump velocity
    private float _jumpVelocity;
    [Header("Object References")]
    [Tooltip("Add the main camera for the scene here, or it will be collected during start.")]
    [SerializeField] private Camera _camera;
    //Reference the character controller of the players object
    private CharacterController _charCon;
    //A bool check to see if we are currently trying to jump that will be changed via the Jump class attached to the Jump Button
    public bool isJumping = false;
    #endregion
    #region Setup
    private void Start()
    {
        //Retrieve the Character Controller from the player object
        _charCon = GetComponent<CharacterController>();
        //If our Camera reference is empty add the scenes main camera to the reference
        if (_camera == null)
        {
            _camera = GameObject.Find("MainCamera").GetComponent<Camera>();
        }
    }
    private void FixedUpdate()
    {
        //Run the jump function every fixed update to handle jumping and gravity. Running in standard update was causing some instances of jumping too high
        Jump(isJumping);        
    }
    #endregion
    #region Movement
    //MovePlayer is called by the Joystick class attached to the joystick background
    public void MovePlayer(Vector2 input)
    {
        //Set a Vector3 value to store the movement inputs with a y value of 0
        Vector3 move = new Vector3(input.x, 0, input.y);
        //Multiply the movement data by the speed and delta time
        move *= _speed * Time.deltaTime;
        //Use the character controller to move the character according to the movement value
        _charCon.Move(move);
        //Rotate the character to face the direction it is moving in
        transform.rotation = Quaternion.LookRotation(move);
        //Adjust the camera's position so it follows the character without rotating
        _camera.transform.position = new Vector3(transform.position.x, 1f, transform.position.z - 5f);
    }
    public void Jump(bool jump)
    {
        //Calculate the gravity by multiplying gravity twice by delta time
        float _gravityAcceleration = _gravity * Time.deltaTime * Time.deltaTime;
        //Adjust the jump speed using the height multiplied by -2f and by our calculated gravity
        float _jumpSpeed = Mathf.Sqrt(_jumpHeight * -2f * _gravityAcceleration);
        //A new Vector3 variable set to 0 to store our jump movement
        Vector3 _moveDir = Vector3.zero;
        //If character is on the ground
        if (_charCon.isGrounded)
        {
            //Reset jump velocity to 0 so we are not jumping when we don't want to be
            _jumpVelocity = 0f;
            //If we push the jump button make the jump velocity equal the jump speed so we can apply the jump movement
            if (jump)
            {
                _jumpVelocity = _jumpSpeed;
            }
        }
        //Add gravity to the jump velocity to help control the jump height and allow object to come back down
        _jumpVelocity += _gravityAcceleration;
        //our y direction is the new y velocity clamped below the original jump height we set
        _moveDir.y = Mathf.Clamp(_jumpVelocity, -1f, 3f);
        //Move in the direction we have now calculated
        _charCon.Move(_moveDir);        
    }
    #endregion
}
