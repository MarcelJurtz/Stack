using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleRemoveScript : MonoBehaviour {

	private void OnCollisionEnter(Collision col)
    {
        // Destroy rubble when fallen far enough (hitting plane)
        Destroy(col.gameObject);
    }
}
