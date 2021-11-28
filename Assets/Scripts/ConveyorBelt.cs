using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public float speed = 1.0f;

    private Material mat;
    private Vector2 textureOffset = Vector2.zero;
    private void Start()
    {
        mat = transform.GetComponent<MeshRenderer>().material;
    }

    private void OnCollisionStay(Collision collision)
    {
        float conveyorVelocity = speed * Time.deltaTime;
        Rigidbody r;
        if (collision.transform.TryGetComponent<Rigidbody>(out r))
        {
            r.velocity = transform.forward * speed;
        }
    }

    private void Update()
    {
        textureOffset.y += Time.deltaTime * .05f;
        mat.SetTextureOffset("_MainTex", textureOffset);
    }
}
