using UnityEngine; //Connect to Unity Engine
using UnityEngine.EventSystems; //Use the event systems so we can detect when the Jump button is pushed

//Use IPointerDownHandler and IPointerUpHandler interfaces so we can perform functions on clicking and releasing the button
public class Jump : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Component References")]
    [Tooltip("Drag the Player object her to capture its PlayerMovement script, otherwise it will be collected during start.")]
    [SerializeField] private PlayerMovement _playerMovement;
    void Start()
    {
        //If PlayerMovement script is not attached, retrieve it from the Player script
        if (_playerMovement == null)
        {
            _playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        }
    }
    #region Clicks
    public void OnPointerDown(PointerEventData eventData)
    {
        //Set isJumping to true on clicking the button
        _playerMovement.isJumping = true;
        //Trigger the jumping animation
        _playerMovement.GetComponentInChildren<Animator>().SetTrigger("isJumping");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Set isJumping to false on releasing the button
        _playerMovement.isJumping = false;
    }
    #endregion
}
