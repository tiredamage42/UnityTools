using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityTools.InteractionSystem {

    public class InteractionPoint : MonoBehaviour
    {
        public static int interactTriggerMask { get { 
            int interactableLayer;
            if (Layers.LayerExists(Interactable.interactableLayerName, out interactableLayer))
                return 1 << interactableLayer;
            return 0;
        }}

        public Transform referenceTransform;
        public int associatedController = 0;
        public string tagSuffix;
        [HideInInspector] public Interactor baseInteractor;
        public bool findInteractables = true;
        
        public Action<InteractionPoint, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        [HideInInspector] public Vector3 lastInteractionPoint;
        public bool hoverLocked { get; private set; }
        
        Interactable _hoveringInteractable;
        const int ColliderArraySize = 16;
        Collider[] overlappingColliders;
        [HideInInspector] public bool usingRay;
        [HideInInspector] public Vector3 rayHitPoint;

        void Awake () {
            overlappingColliders = new Collider[ColliderArraySize];
        }
        public void SetBaseInteractor (Interactor baseInteractor) {
            this.baseInteractor = baseInteractor;
        }
        

        void FindInteractables () {
            usingRay = false;
            
            if (hoverLocked)
                return;

            if (referenceTransform == null) {
                Debug.LogError("no interactor reference transform supplied for " + name);
                return;
            }

            Interactable closestInteractable = null;

            float closestDistance = float.MaxValue;
            
            // use check around the interactors position
            if (InteractionSettings.instance.useProximityCheck ) {
                UpdateHovering (referenceTransform.position, InteractionSettings.instance.proximityRadius, ref closestDistance, ref closestInteractable);

                if (closestInteractable != null) {
                    lastInteractionPoint = referenceTransform.position;
                }
            }

            usingRay = closestInteractable == null && InteractionSettings.instance.useRayCheck;
            
            // Vector3 hitPos = Vector3.zero;
            // rayHitPoint = referenceTransform.position + referenceTransform.forward * baseInteractor.rayCheckDistance;
            if (usingRay) {
            
                rayHitPoint = referenceTransform.position + referenceTransform.forward * baseInteractor.rayCheckDistance;
                UpdateRayCheck(referenceTransform.position, referenceTransform.forward, baseInteractor.rayCheckDistance, ref rayHitPoint, ref closestInteractable);
                
                if (closestInteractable != null) {
                    lastInteractionPoint = rayHitPoint;
                }
            }
            
         
            // Hover on this one
            hoveringInteractable = closestInteractable;
        }
        
        void UpdateHovering(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref Interactable closestInteractable)
        {
           
            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i) overlappingColliders[i] = null;
            
            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, interactTriggerMask, QueryTriggerInteraction.Collide);

            if (numColliding == ColliderArraySize)
                Debug.LogWarning("overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on InteractionPoint.cs");

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < numColliding; colliderIndex++)
            {
                InteractableTrigger c = overlappingColliders[colliderIndex].GetComponent<InteractableTrigger>();
                if (c == null)
                    continue;

                Interactable contacting = c.interactable;
                
                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode))
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                }
            }
        }
        void UpdateRayCheck(Vector3 origin, Vector3 direction, float distance, ref Vector3 hitPos, ref Interactable closestInteractable) {
            RaycastHit hit;
            //maybe raycast all
            // if (Physics.Raycast(new Ray(origin, direction), out hit, distance, interactTriggerMask, QueryTriggerInteraction.Collide)){
            if (Physics.Raycast(origin, direction, out hit, distance, interactTriggerMask, QueryTriggerInteraction.Collide)){
                
                hitPos = hit.point;
                InteractableTrigger c = hit.collider.GetComponent<InteractableTrigger>();
                if (c == null)
                    return;

                Interactable contacting = c.interactable;
                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode))
                    return;

                if (contacting.onlyProximityHover && !InteractionSettings.instance.ignoreOnlyProximity)
                    return;
                
                closestInteractable = contacting;
            }
        }
        void Update()
        {         
            // rayHitPoint = referenceTransform.position;
            if (findInteractables) {
                FindInteractables();
            }   
            else {
                hoveringInteractable = null;
                hoverLocked = false;
                usingRay = false;
            }
            
            if (hoveringInteractable != null)
            {  
                hoveringInteractable.OnInspectedUpdate(this, baseInteractor.interactionMode);

                if (onInspectUpdate != null){
                    onInspectUpdate(this, hoveringInteractable);
                }
            }
        }

        public Interactable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                Interactable oldInteractable = _hoveringInteractable;
                Interactable newInteractable = value;
                if (oldInteractable != value)
                {
                    if (oldInteractable != null)
                    {
                        oldInteractable.OnInspectedEnd(this, baseInteractor.interactionMode);
                        if (onInspectEnd != null) {
                            onInspectEnd(this, oldInteractable);
                        }
                    }

                    _hoveringInteractable = newInteractable;

                    if (newInteractable != null)
                    {
                        newInteractable.OnInspectedStart(this, baseInteractor.interactionMode);
                        if (onInspectStart != null) {
                            onInspectStart(this, newInteractable);
                        }
                    }
                }
            }
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        // Continue to hover over this object indefinitely, whether or not the interaction point moves out of its interaction trigger volume.
        public void HoverLock(Interactable interactable)
        {
            hoverLocked = true;
            hoveringInteractable = interactable;
        }

        // Stop hovering over this object indefinitely.
        public void HoverUnlock(Interactable interactable)
        {
            if (hoveringInteractable == interactable)
                hoverLocked = false;
        }

        List<string> BuildSuffixedTags(List<string> tags) {
            if (string.IsNullOrEmpty(tagSuffix))
                return tags;
            
            List<string> forBase = new List<string>();
            for (int i = 0; i < tags.Count; i++) {
                forBase.Add(tags[i] + tagSuffix);
            }
            return forBase;
        }
        public void AddInteractionTags (List<string> tags) {
            baseInteractor.dynamicObject.AddTags(BuildSuffixedTags(tags));   
        }
        public void RemoveInteractionTags (List<string> tags) {
            baseInteractor.dynamicObject.RemoveTags(BuildSuffixedTags(tags));      
        }   
    }
}