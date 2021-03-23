using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionContact : MonoBehaviour
{
	void OnCollisionEnter(Collision collision)
	{
		//onGround = true;
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		//onGround = true;
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		//print(collision.contactCount);
		for (int i = 0; i < collision.contactCount; i++)
		{		
			ContactPoint cc = collision.GetContact(i);
			Vector3 normal = cc.normal;
			Debug.DrawRay(cc.point, normal * 1.5f);
			//Debug.Log($"{cc.otherCollider.gameObject.name} on point {i} penetrates with {cc.separation}");
		}
	}

    
}
