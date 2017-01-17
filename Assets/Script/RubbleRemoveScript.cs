using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleRemoveScript : MonoBehaviour {

	private void OnCollisionEnter(Collision col)
    {
        Debug.Log("Hit");
        Destroy(col.gameObject);
    }
}
