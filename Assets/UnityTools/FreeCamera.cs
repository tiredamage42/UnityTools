using UnityEngine;
namespace UnityTools {
    public class FreeCamera : CustomUpdaterMonobehaviour
    {
        [Header("Axes")]
        [Action] public int lookX;
        [Action] public int lookY;
        [Action] public int moveX, moveY, moveZ;

        public override void UpdateLoop (float deltaTime) {
	        UpdateFreeCam();
        }

        [Header("Speeds")]
		public float moveSpeed = 10;
		public float turnSpeed = 5;
        float rotX, rotY;
	
        void Start () {
            rotX = transform.eulerAngles.x;
            rotY = transform.eulerAngles.y;
        }
		void UpdateFreeCam () {	
            transform.position += (transform.right * ActionsInterface.GetAxis(moveX) + transform.up * ActionsInterface.GetAxis(moveY) + transform.forward * ActionsInterface.GetAxis(moveZ)) * moveSpeed;
            rotX += ActionsInterface.GetAxis(lookY) * turnSpeed;
            rotY += ActionsInterface.GetAxis(lookX) * turnSpeed;
            transform.rotation = Quaternion.Euler(rotX, rotY, 0);
		}
	}
}