using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingOutlineDetector : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null)
            return;

        FSMMotor collisionObj = collision.GetComponent<FSMMotor>();
        if (collisionObj != null)
        {
            if (BattleManager.Instance.CurrentState == BattleManager.State.Finish)
                return;

            collisionObj.SetToDeadVelocity();
            BattleManager.Instance.AvatarGoal = collisionObj;
            BattleManager.Instance.Stop();
            Debug.Log("Actor out: " + collisionObj.IdView);
        }
    }
}
