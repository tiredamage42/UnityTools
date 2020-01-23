using UnityEngine;
namespace UnityToolsDemo {
    public class LightMover : MonoBehaviour {
        public Transform directional, spot, point;
        Vector3 pointStart;
        void Start() {
            if (point != null)
                pointStart = point.position;
        }
        void Update() {
            if (directional != null) {
                directional.Rotate(Vector3.right * Time.deltaTime * .23f, Space.Self);
                directional.Rotate(Vector3.up * Time.deltaTime, Space.World);
            }
            if (spot != null) {
                spot.Rotate(Vector3.right * Time.deltaTime * 5, Space.Self);
                // spot.Rotate(Vector3.up * Time.deltaTime * .34f, Space.World);
            }
            if (point != null) {
                // point.position = new Vector3(pointStart.x + Mathf.Sin(Time.time) * 5, pointStart.y + Mathf.Cos(Time.time) * 4, pointStart.z);
                point.position = new Vector3(pointStart.x, pointStart.y, pointStart.z + Mathf.Sin(Time.time * .25f) * 5);
            }
        }
    }
}