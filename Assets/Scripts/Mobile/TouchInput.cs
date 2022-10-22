using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    //The layer that we can interact with
    public LayerMask touchMask;
    //List of current touch inputs
    private List<GameObject> _touchList = new List<GameObject>();
    //Array of old touch inputs
    private GameObject[] _touchOld;
    //Raycast to see if we are touching an object that will be in the correct layer mask
    private RaycastHit _hitInfo;
    
    private void Update()
    {
        //This is the developer version
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            //Create array at size of new touch list
            _touchOld = new GameObject[_touchList.Count];
            //Sending the list info from current touches to the array
            _touchList.CopyTo(_touchOld);
            //Clear the current list
            _touchList.Clear();
            //Create a ray from screen point of the mouse
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //If the ray hits the touched mask
            if (Physics.Raycast(_ray, out _hitInfo, touchMask))
            {
                //The object we hit store a temp
                GameObject _touchedObj = _hitInfo.collider.gameObject;
                //Add the temp to the list
                _touchList.Add(_touchedObj);
                //if mouse pressed
                if (Input.GetMouseButtonDown(0))
                {
                    //Send message to the touched object OnTouchDown
                    _touchedObj.SendMessage("OnTouchDown", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                    Debug.Log("OnTouchDown");
                }
                //If mouse is any interaction
                if (Input.GetMouseButton(0))
                {
                    //Send message to the touched object OnTouchStay
                    _touchedObj.SendMessage("OnTouchStay", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                    Debug.Log("OnTouchStay");
                }
                //If mouse button is released
                if (Input.GetMouseButtonUp(0))
                {
                    //Send message to the touched object OnTouchUp
                    _touchedObj.SendMessage("OnTouchUp", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                    Debug.Log("OnTouchUp");
                }
            }
            //OnTouchExit
            //Check each item in the old touch list
            foreach (GameObject item in _touchOld)
            {
                if (!_touchList.Contains(item))
                {
                    item.SendMessage("OnTouchExit", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                    Debug.Log("OnTouchExit");
                }
            }
        }
#endif
        #region User
        if (Input.touchCount > 0)
        {
            //Create array at size of new touch list
            _touchOld = new GameObject[_touchList.Count];
            //Sending the list info from current touches to the array
            _touchList.CopyTo(_touchOld);
            //Clear the current list
            _touchList.Clear();
            //check each touch in inputs touch
            foreach(Touch touch in Input.touches)
            {
                //Create a ray from the screen point of the touch
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                //If the ray hits the touched mask
                if (Physics.Raycast(ray, out _hitInfo, touchMask))
                {
                    //The object we hit stores a temp
                    GameObject _touchedObject = _hitInfo.transform.gameObject;
                    //Add the temp to the list
                    _touchList.Add(_touchedObject);
                    //If the touch began
                    if (touch.phase == TouchPhase.Began)
                    {
                        //Send message to the touched object OnTouchDown
                        _touchedObject.SendMessage("OnTouchDown", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                        Debug.Log("OnTouchDown");
                    }
                    //If the touch ended
                    if (touch.phase == TouchPhase.Ended)
                    {
                        //Send message to the touched object OnTouchDown
                        _touchedObject.SendMessage("OnTouchUp", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                        Debug.Log("OnTouchUp");
                    }
                    //If touch is stationary
                    if (touch.phase == TouchPhase.Stationary)
                    {
                        //Send message to the touched object OnTouchDown
                        _touchedObject.SendMessage("OnTouchStay", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                        Debug.Log("OnTouchStay");
                    }
                    //If touch is cancelled
                    if (touch.phase == TouchPhase.Canceled)
                    {
                        //Send message to the touched object OnTouchDown
                        _touchedObject.SendMessage("OnTouchExit", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                        Debug.Log("OnTouchExit");
                    }
                }
            }
            //Check each item in old touches
            foreach (GameObject item in _touchOld)
            {
                if (!_touchList.Contains(item))
                {
                    item.SendMessage("OnTouchExit", _hitInfo.point, SendMessageOptions.DontRequireReceiver);
                    Debug.Log("OnTouchExit");
                }
            }
        }
        #endregion
    }

}
