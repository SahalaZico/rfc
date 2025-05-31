using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMDamageCollider : MonoBehaviour
{
    [SerializeField] protected FSMMotor owner = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == null)
            return;

        FSMMotor collisionMotor = collision.gameObject.GetComponent<FSMMotor>();
        if (collisionMotor != null)
            collisionMotor.DoDamage(5f, owner);
    }
}
