using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingInlineDetector : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
            return;

        FSMMotor collisionObj = collision.gameObject.GetComponent<FSMMotor>();
        if (collisionObj != null)
        {
            if (BattleManager.Instance.CurrentState == BattleManager.State.Finish)
                return;

            collisionObj.PlayAlmostFall();
        }
    }
}
