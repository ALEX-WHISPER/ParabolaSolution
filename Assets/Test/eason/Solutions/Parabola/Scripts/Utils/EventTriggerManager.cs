using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventTriggerManager : EventTrigger {

    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate onClick;    //  点击
	public VoidDelegate onSelect;
    public VoidDelegate onPressDown;    //  按下
    public VoidDelegate onPressUp;          //  抬起
    public VoidDelegate onPointerEnter; //  鼠标进入
    public VoidDelegate onPointerExit;      //  鼠标移出

	static public EventTriggerManager Get(GameObject go)
    {
		EventTriggerManager etm = go.GetComponent<EventTriggerManager>();

		if (etm == null)
        {
			etm = go.AddComponent<EventTriggerManager>();
        }

		return etm;
    }

	static public EventTriggerManager Get(Transform transform)     //  重载
    {
		EventTriggerManager etm = transform.GetComponent<EventTriggerManager>();

		if (etm == null)
        {
			etm = transform.gameObject.AddComponent<EventTriggerManager>();
        }
		return etm;
    }

	/// <summary>
	/// Click
	/// </summary>
	/// <param name="eventData">Event data.</param>
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(gameObject);
    }

	/// <summary>
	/// PressDown
	/// </summary>
	/// <param name="eventData">Event data.</param>
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (onPressDown != null)
            onPressDown(gameObject);
    }

	/// <summary>
	/// PressUp
	/// </summary>
	/// <param name="eventData">Event data.</param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onPressUp != null)
            onPressUp(gameObject);
    }

	/// <summary>
	/// Enter
	/// </summary>
	/// <param name="eventData">Event data.</param>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointerEnter != null)
            onPointerEnter(gameObject);
    }

	/// <summary>
	/// Exit
	/// </summary>
	/// <param name="eventData">Event data.</param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (onPointerExit != null)
            onPointerExit(gameObject);
    }
}
