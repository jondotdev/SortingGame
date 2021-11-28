using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public enum SortingType
    {
        ALL,
        RED,
        BLUE,
        GREEN,
        YELLOW
    }
    
    public bool canInteract = true;
    public bool canPickUp = false;
    public Vector3 scaleOnPickup = Vector3.one;
    public bool throwable = false;
    public SortingType sortID = SortingType.ALL;

    public string hintText = "An Item";

    public bool dieOnNegativeY = true;

    private bool hasRB = false;
    private Rigidbody rb;
    private RigidbodyConstraints rbConstraintDefaults;
    private Vector3 defaultScale;

    private void Start() {
        if (transform.TryGetComponent<Rigidbody>(out rb)) 
        { 
            hasRB = true;
            rbConstraintDefaults = rb.constraints;
        }
        defaultScale = transform.localScale;
    }

    private void Update()
    {
        if(dieOnNegativeY && transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    public void PickUp(Transform Hand)
    {
        if (!canPickUp) {
            Debug.LogWarning("Object: '" + transform.name + "' cannot be picked up!");
            return;
        }

        canInteract = false;

        if (hasRB) 
        { 
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        transform.GetComponent<Collider>().enabled = false;
        transform.localScale = scaleOnPickup;
        transform.SetPositionAndRotation(Hand.position, Hand.rotation);
        transform.SetParent(Hand);
    }

    public void Throw(Vector3 dir, float force) {
        transform.parent = null;
        transform.GetComponent<Collider>().enabled = true;
        transform.localScale = defaultScale;
        rb.constraints = rbConstraintDefaults;
        rb.AddForce(dir * force, ForceMode.VelocityChange);
        canInteract = true;
    }
}
