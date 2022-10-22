using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyMode : Joystick
{
    [SerializeField] private float _moveThreshold = 1f;
    [SerializeField] private JoyType _joyType = JoyType.Fixed; 

    public float MoveThreshold
    {
        get { return _moveThreshold; }
        set { _moveThreshold = Mathf.Abs(value); }
    }
    private Vector2 _fixedPosition = Vector2.zero;

    public void SetMode(JoyType joyType)
    {
        _joyType = joyType;
        if (joyType == JoyType.Fixed)
        {
            background.anchoredPosition = _fixedPosition;
            background.transform.position = _fixedPosition + new Vector2(25,25);
            background.gameObject.SetActive(true);
        }
        else
        {
            background.gameObject.SetActive(false);
        }
    }
    protected override void Start()
    {
        base.Start();
        _fixedPosition = background.anchoredPosition;
        SetMode(_joyType);
    }
    public override void Update()
    {
        base.Update();
        if (_joyType == JoyType.Fixed && background.gameObject.activeSelf == false)
        {
            SetMode(JoyType.Fixed);
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (_joyType != JoyType.Fixed)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.transform.position = eventData.position;
            background.gameObject.SetActive(true);
        }
        base.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (_joyType != JoyType.Fixed)
        {
            background.gameObject.SetActive(false);
        }
        base.OnPointerUp(eventData);
    }
    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera camera)
    {
        if (_joyType == JoyType.Dynamic && magnitude > MoveThreshold)
        {
            Vector2 difference = normalised * (magnitude - MoveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, camera);
    }
}
public enum JoyType
{
    Fixed, Floating, Dynamic
}
