using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        //Debug.Log(GetType() + ".OnTriggerEnter: " + collider);
        AppManager.Instance.AddPlayerPoints(Const.PlayerPointsForAsteroid);
        Destroy(gameObject);
    }
}
