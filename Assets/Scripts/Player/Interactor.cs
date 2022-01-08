using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Interactor : MonoBehaviour
{
    public GameObject pressEToInteract;
    public float interactionRange = 4;
    private GameObject _img;
    public Camera cam;

    private Interactable _interactable;
    private void Start()
    {
        _img = Instantiate(pressEToInteract, FindObjectOfType<Canvas>().transform);
    }

    private void Update()
    {
        bool hitAnything = Physics.Raycast(transform.position, transform.forward, out var hit, interactionRange);
        Debug.DrawLine(transform.position, transform.position+transform.forward*4, Color.blue);
        if (hitAnything && hit.transform != null)
        {
            Debug.DrawLine(transform.position, hit.transform.position, Color.red); 
            _interactable = FindInteractable(hit.transform);
            if (_interactable != null)
            {
                _img.SetActive(true);
                _img.transform.position = cam.WorldToScreenPoint(hit.transform.position + Vector3.up*2);
                _img.transform.localScale = new Vector3(1-0.5f*hit.distance/interactionRange, 1-0.5f*hit.distance / interactionRange, 1-0.5f*hit.distance / interactionRange);
            }
            else
            {
                _img.SetActive(false);
            }
        }
        else
        {
            _img.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E) && _interactable != null)
        {
            _interactable.Interact(transform.parent.gameObject);
        }

    }
        
    public static Interactable FindInteractable(Transform other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable == null && other.parent != null)
        {
            interactable = FindInteractable(other.parent);
        }

        return interactable != null ? interactable : null;
    }
}
