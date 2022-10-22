using UnityEngine; //Connect to Unity Engine
using UnityEngine.EventSystems; //Use the event systems to use the Canvas events to detect users input

// Use the IPointerDownHandler, IDragHandler & IPointerUpHandler to handle the users input on the joystick
public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Serialized Fields
    [Header("Joystick Options")]
    [Tooltip("Set the range you want the joystick to be able to move. 1f Will allow it to move to the edge of the background")]
    [SerializeField] private float _handlerRange = 1f;
    [Tooltip("Set the deadzone for the joystick to determine how far it can travel before registering a value")]
    [SerializeField] private float _deadZone = 0;
    [Tooltip("Select which axis will be available for input by enabling either vertical, horizontal or both for movement of the joystick")]
    [SerializeField] private AxisOptions _axisOptions = AxisOptions.Both;
    [Tooltip("Select whether you want the horizontal input snapped to the values of -1, 0 or 1. False means the joystick can register any value in between -1 and 1. Doesn't seem to do anything")]
    [SerializeField] private bool _snapX = false;
    [Tooltip("Select whether you want the vertical input snapped to the values of -1, 0 or 1. False means the joystick can register any value in between -1 and 1. Doesn't seem to do anything")]
    [SerializeField] private bool _snapY = false;
    [Header("Object References")]
    [Tooltip("Drag the Background object of the joystick from the canvas in here to store its transform.")]
    [SerializeField] protected RectTransform background;
    [Tooltip("Drag the Handle object of the joystick from the canvas in here to store its transform.")]
    [SerializeField] private RectTransform _handle;
    [Header("Component References")]
    [Tooltip("Drag the Player object in here to capture its PlayerMovement class, or it will be grabbed in start.")]
    [SerializeField] private PlayerMovement _playerMovement;
    [Tooltip("Drag the iWalkr object in here to grab its Animator, or it will be grabbed in start.")]
    [SerializeField] private Animator _playerAnimator;
    #endregion
    #region References
    //
    private RectTransform _baseRect = null;
    //Private canvas reference to store the canvas and allow us to get its render mode
    private Canvas _canvas;
    //Private Camera reference set to empty
    private Camera _cam = null;
    //Vector2 to store the input values from the joystick. Public so we can see the values change in the inspector.
    public Vector2 input = Vector2.zero;
    //Private bool to check that we are moving to run the Move function in the PlayerMovement class
    private bool _isMoving = false;
    #endregion
    #region Properties
    //Bool property to get or set the value for _snapX
    public bool SnapX
    {
        get { return _snapX; }
        set { _snapX = value; }
    }
    //Bool property to get or set the value for _snapy
    public bool SnapY
    {
        get { return _snapY; }
        set { _snapY = value; }
    }
    //Float property to get or set the value of the range the joystick handle can move between. Use Mathf.Abs to turn negative into positive numbers.
    public float HandleRange
    {
        get { return _handlerRange; }
        set { _handlerRange = Mathf.Abs(value); }
    }
    //Float property to get and set the value of the joystick deadzone. Use Mathf.Abs to turn negative into positive numbers.
    public float DeadZone
    {
        get { return _deadZone; }
        set { _deadZone = Mathf.Abs(value); }
    }
    //Property to get or set the selected option of the AxisOptions enum.
    public AxisOptions AxisOption
    {
        get { return _axisOptions; }
        set { _axisOptions = value; }
    }
    //Float property to Get the horizontal input from the snapfloat function or input.x value
    public float Horizontal
    {
        get { return (SnapX) ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x; }
    }
    //Float property to Get the vertical input from the snapfloat function or input.x value
    public float Vertical
    {
        get { return (SnapY) ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; }
    }
    //Vector2 property to get the values from the horizontal and vertical properties and return them as a Vector2 direction. Is not currently referenced anywhere
    public Vector2 Direction
    {
        get { return new Vector2(Horizontal, Vertical); }
    }
    #endregion
    #region Interface
    protected virtual void Start()
    {
        //If PlayerMovement is not attached retrieve it from the Player object
        if (_playerMovement == null)
        {
            _playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        }
        //If Animator is not attached retrieve it from the iWalkr object
        if (_playerAnimator == null)
        {
            _playerAnimator = GameObject.Find("iWalkr").GetComponent<Animator>();
        }
        //Get the rect transform of the object this class is attached to
        _baseRect = GetComponent<RectTransform>();
        //Store the parent object canvas in the canvas variable
        _canvas = GetComponentInParent<Canvas>();
        //If we couldn't store the canvas log an error
        if (_canvas == null)
        {
            Debug.LogError("The Joystick is not placed inside the canvas! or the script is not on the background image! der Scrub!!");
        }
        //Create a centre point that will point to the middle of the joystick background and use it to position the handle in the centre of the background 
        Vector2 _center = new Vector2(0.5f, 0.5f);
        background.pivot = _center;
        _handle.anchorMin = _center;
        _handle.anchorMax = _center;
        _handle.pivot = _center;
        _handle.anchoredPosition = Vector2.zero;
    }
    public virtual void Update()
    {
        //If the moving bool is set to true run the PlayerMovement class's MovePlayer function every update
        if (_isMoving)
        {
            _playerMovement.MovePlayer(input);
        }
    }
    //I believe this function doesn't currently lead anywhere. It is fed into the Direction property which isn't referenced anywhere
    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        //If the value of the input is 0 return the value as we don't need to do anything else
        if (value == 0)
        {
            return value;
        }
        //If we can move the joystick on both Axis
        if (AxisOption == AxisOptions.Both)
        {
            //Store a float representing the angle from the current position of the joystick and the up position
            float _angle = Vector2.Angle(input, Vector2.up);
            //For Horizontal inputs
            if (snapAxis == AxisOptions.Horizontal)
            {
                //If input is outside of the selected range return 0
                if (_angle < 22.5f || _angle > 157.5f)
                {
                    return 0;
                }
                //Else return -1 or 1 based on which side of 0 the joystick input is on
                else
                {
                    return (value > 0) ? 1 : -1;
                }
            }
            //For vertical inputs
            if (snapAxis == AxisOptions.Vertical)
            {
                //If input is outside of the given range return 0
                if (_angle < 67.5f || _angle > 112.5f)
                {
                    return 0;
                }
                //Else return -1 or 1 based on which side of 0 the joystick input is on
                else
                {
                    return (value > 0) ? 1 : -1;
                }
            }
            //A catch return to return the value if we somehow make it past the previous checks. This should never run.
            return value;
        }
        //Else we can only move the joystick on one axis and don't need to measure the angles
        else
        {
            //If input value is above 0 return 1 or if value is below 0 return -1
            if (value > 0)
            {
                return 1;
            }

            if (value < 0)
            {
                return -1;
            }
        }

        //If we somehow make it past all previous checks, return 0. This should never run.
        return 0;
    }
    private void FormatInput()
    {
        //If we are only moving on one axis this function will return a new Vector2 input with the input value in the appropriate axis 
        if (AxisOption == AxisOptions.Horizontal)
        {
            input = new Vector2(input.x, 0f);
        }
        else if (AxisOption == AxisOptions.Vertical)
        {
            input = new Vector2(0f, input.y);
        }
    }
    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera camera)
    {
        //If our input is outside of the deadzone we will check if it is greater than 1 and return it to 1 if it is
        if (magnitude > DeadZone)
        {
            if (magnitude > 1)
            {
                input = normalised;
            }
        }
        //Else we are inside the deadzone and no input will be registered
        else
        {
            input = Vector2.zero;
        }
    }
    //Function currently does not do anything
    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        //Create a Vector2 local point at the zero location
        Vector2 _localPoint = Vector2.zero;
        //If the hit/touch is on the same plane as the _baseRect return the screenpositiontranslated from screen space to Rect space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out _localPoint))
        {
            return _localPoint - (background.anchorMax * _baseRect.sizeDelta);
        }
        //Else return Vector2.zero
        return Vector2.zero;
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        //If our _cam is empty retrieve either the canvas's worldCamera or the scenes main camera based on the RenderMode of the canvas
        if (_cam == null)
        {
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _cam = _canvas.worldCamera;
            }
            else
            {
                _cam = Camera.main;
            }
        }
        //Convert the joystick background's position from world space into the screen space of _cam and store the new value 
        Vector2 _position = RectTransformUtility.WorldToScreenPoint(_cam, background.position);
        //Store a radius of half the distance between the joystick background's anchors
        Vector2 _radius = background.sizeDelta / 2;
        //Input equals (the position passed from the event data - our backgrounds screenspace position) divided by (the radius of the background multplied by the scale amount of the canvas)
        input = (eventData.position - _position) / (_radius * _canvas.scaleFactor);
        //Run the FormatInput function to create the correct input Vector2 for when we are only moving on one axis
        FormatInput();
        //Run the HandleInput method to handle the deadzone and constrain our value to 1 if it is above
        HandleInput(input.magnitude, input.normalized, _radius, _cam);
        //The location of the handle will move with our input multiplied by the radius multiplied within our handle range. A handle range greater than 1 will allow the handle to exceed the boundaries of the background
        _handle.anchoredPosition = input * _radius * HandleRange;
        //Set the is moving bool to true to activate the Update functions movement check so we can consistently move until we release the joystick
        _isMoving = true;
        //Set the walking animation to true until we release the joystick
        _playerAnimator.SetBool("isWalking", true);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //On clicking the joystick, take in the event data and pass it to the OnDrag function
        OnDrag(eventData);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //On release of the joystick reset the input to zero, return the joystick to centred position, and set the moving and animation bools to false
        input = Vector2.zero;
        _handle.anchoredPosition = Vector2.zero;
        _isMoving = false;
        _playerAnimator.SetBool("isWalking", false);
    }
    #endregion
    //Enum to store the options for which Axis will be active on the joystick
    public enum AxisOptions
    {
        Both,
        Horizontal,
        Vertical,
    }
}