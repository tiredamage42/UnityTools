using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using UnityTools.Internal;
namespace UnityTools {
    public class PhysicsTools
    {
        const float navmeshCheckDistance = 5;
        const float groundCheckDistance = 500;
        public static LayerMask environmentMask { get { return GameManagerSettings.instance.environmentMask; } }

        public static Vector3 GroundPosition (Vector3 pos, bool stickToGround, bool stickToNavMesh, out Vector3 up) {
            up = Vector3.up;
            if (stickToGround) {
                RaycastHit hit;
                if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, environmentMask, QueryTriggerInteraction.Ignore)) {
                    pos = hit.point;
                    up = hit.normal;
                }
            }
            if (stickToNavMesh) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(pos, out hit, navmeshCheckDistance, NavMesh.AllAreas)) 
                    pos = hit.position;
            }
            return pos;
        }

        public static void UnIntersectTransform (Transform transform, Vector3 nudgeDir) {
            transform.WarpTo( UnIntersectColliderGroup(transform.GetComponentsInChildren<Collider>(), transform.position, nudgeDir), transform.rotation);
        }

        public static Vector3 UnIntersectColliderGroup (Collider[] group, Vector3 originalRoot, Vector3 nudgeDir) {
            int tries = 0;

            Vector3 offset = Vector3.zero;
            while (ColliderGroupIntersects(group, offset)){
                offset += nudgeDir * unIntersectNudge;
                tries++;


                if (tries >= maxUnIntersectTries) {
                    Debug.LogWarning("Max Intersection Tries Reached, giving up");
                    return originalRoot;
                }
            }
            return originalRoot + offset;
        }

        const int maxUnIntersectTries = 500;
        const float unIntersectNudge = .01f;        
        const int physicsCheckOverlapCount = 10;
        static Collider[] hits = new Collider[physicsCheckOverlapCount];
        static bool ColliderInIgnores (Collider c, Collider[] ignores) {
            for (int x = 0; x < ignores.Length; x++) 
                if (c == ignores[x]) 
                    return true;
            return false;
        }
        static bool CollidersAreHit (Collider[] ignores, int length) {
            for (int i = 0; i < length; i++) 
                if (hits[i] != null && !ColliderInIgnores(hits[i], ignores)) 
                    return true;
            return false;
        }
        static bool ColliderGroupIntersects(Collider[] group, Vector3 offset) {
            for (int i = 0; i < group.Length; i++) {
                Collider c = group[i];

                float scale = c.transform.lossyScale.x;
                Vector3 pos = c.transform.position + offset;
                Quaternion rot = c.transform.rotation;

                SphereCollider sphere = c as SphereCollider;
                CapsuleCollider capsule = c as CapsuleCollider;
                BoxCollider box = c as BoxCollider;

                if (sphere != null) {
                    if (CollidersAreHit(group, Physics.OverlapSphereNonAlloc(pos + sphere.center * scale, sphere.radius * scale, hits, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else if (capsule != null) {
                    // x y z
                    Vector3 dir = capsule.direction == 0 ? c.transform.right : (capsule.direction == 1 ? c.transform.up : c.transform.forward);
                    
                    
                    Vector3 center = pos + capsule.center * scale;
                    // Vector3 mod1 = dir * ((capsule.height * .5f - capsule.radius * .5f) * scale);
                    Vector3 mod1 = dir * ((capsule.height * .5f - capsule.radius) * scale);
                    
                    if (CollidersAreHit(group, Physics.OverlapCapsuleNonAlloc(center + mod1, center - mod1, capsule.radius * scale, hits, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else if (box != null) {
                    if (CollidersAreHit(group, Physics.OverlapBoxNonAlloc(pos, box.size * .5f * scale, hits, rot, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else {
                    if (CollidersAreHit(group, Physics.OverlapBoxNonAlloc(pos, c.bounds.size * .5f * scale, hits, rot, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }   
            }
            return false;
        }
    }
}
