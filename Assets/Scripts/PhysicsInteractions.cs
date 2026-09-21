using UnityEngine;

public class PhysicsInteractions : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Фізичне зіткнення з: " + collision.gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Увійшли в зону-тригер: " + other.gameObject.name);
    }
}
