using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class VuzixInputModule : MonoBehaviour
{
    List<RaycastResult> m_raycastResults = new List<RaycastResult>();

    public GameObject[] ShowWhenInteractible;
    public GameObject[] ShowWhenNotInteractible;

    void Update()
    {
        PointerEventData pointerEvent = new PointerEventData(EventSystem.current);
        
        Vector2 pos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        pointerEvent.Reset();
        pointerEvent.delta = Vector2.zero;
        pointerEvent.position = pos;
        pointerEvent.scrollDelta = Vector2.zero;
        pointerEvent.button = PointerEventData.InputButton.Left;

        EventSystem.current.RaycastAll(pointerEvent, m_raycastResults);
        var raycast = m_raycastResults.FirstOrDefault();
        pointerEvent.pointerCurrentRaycast = raycast;

        bool interactible = false;

        if (raycast.gameObject != null)
        {
            //interactible = raycast.gameObject.GetComponent<IPointerClickHandler>() != null;
            interactible = ExecuteEvents.GetEventHandler<IPointerClickHandler>(raycast.gameObject) != null;
            //Debug.Log("got one=" + raycast.gameObject);
        }

        ObjectHelper.SetObjectsActive(ShowWhenInteractible, interactible);
        ObjectHelper.SetObjectsActive(ShowWhenNotInteractible, !interactible);

        if (Input.GetKeyDown((KeyCode)10) || Input.GetKeyDown(KeyCode.Return))
        {
            pointerEvent.eligibleForClick = true;
            pointerEvent.delta = Vector2.zero;
            pointerEvent.dragging = false;
            pointerEvent.useDragThreshold = true;
            pointerEvent.pressPosition = pointerEvent.position;
            pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

            /*
            DeselectIfSelectionChanged(currentOverGo, pointerEvent);

            if (pointerEvent.pointerEnter != currentOverGo)
            {
                HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                pointerEvent.pointerEnter = currentOverGo;
            }
            */

            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerClickHandler);

            //ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            /*
            var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

            if (newPressed == null)
            {
                newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
            }*/
        }
    }
}
