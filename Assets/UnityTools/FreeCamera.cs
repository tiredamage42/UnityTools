using UnityEngine;
namespace UnityTools {
    public class FreeCamera : CustomUpdaterMonobehaviour
    {

        [Header("Axes")]
        [Action] public Vector2Int look;
        [Action] public Vector3Int move;
        [Action] public int speedUp;

        [Header("Speeds")]
		public float moveSpeed = 1;
        public float moveSpeedUp = 20;
		public float turnSpeed = 5;
        
        float rotX, rotY;
	
        void Start () {
            rotX = transform.eulerAngles.x;
            rotY = transform.eulerAngles.y;
        }

		public override void UpdateLoop (float deltaTime) {
	        Vector3 side = transform.right * ActionsInterface.GetAxis(move.x);
            Vector3 upDown = transform.up * ActionsInterface.GetAxis(move.y);
            Vector3 fwd = transform.forward * ActionsInterface.GetAxis(move.z);

            transform.position += (side + upDown + fwd) * (ActionsInterface.GetAction(speedUp) ? moveSpeedUp : moveSpeed) * deltaTime;

            float turnSpeed = this.turnSpeed * deltaTime;


            Vector3 angles = transform.rotation.eulerAngles;
            angles.z = 0;
            angles.x += ActionsInterface.GetAxis(look.y) * turnSpeed;
            angles.y += ActionsInterface.GetAxis(look.x) * turnSpeed;
            transform.rotation = Quaternion.Euler(angles);
		}
	}
}