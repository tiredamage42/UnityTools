using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityTools.InteractionSystem {

    public class InteractionPoint : MonoBehaviour
    {
        static LayerMask interactTriggerMask { get { return InteractionSettings.instance.checkLayerMask; } }
        public Transform referenceTransform;
        public int associatedController = 0;
        public string tagSuffix;
        [HideInInspector] public Interactor baseInteractor;
        public bool findInteractables = true;
        
        public Action<InteractionPoint, Interactable> onInspectUpdate, onInspectStart, onInspectEnd;
        [HideInInspector] public Vector3 lastInteractionPoint;
        // public bool hoverLocked { get; private set; }
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
            
            // if (hoverLocked)
            //     return;

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
            SetInteractable(closestInteractable);
        }


        static Interactable GetInteractableFromCollider (Collider c) {
            Interactable interactable = null;
            InteractableTrigger trigger = c.GetComponent<InteractableTrigger>();
            if (trigger != null) 
                interactable = trigger.interactable;
            if (interactable == null) 
                interactable = c.GetComponent<Interactable>();
            return interactable;
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
                Interactable contacting = GetInteractableFromCollider(overlappingColliders[colliderIndex]);
                
                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode, associatedController))
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
                Interactable contacting = GetInteractableFromCollider(hit.collider);

                if (contacting == null || !contacting.IsAvailable(baseInteractor.interactionMode, associatedController))
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
                SetInteractable(null);
                // hoverLocked = false;
                usingRay = false;
            }

            if (interactable != null)
            {  
                interactable.OnInspectedUpdate(this, baseInteractor.interactionMode);

                if (onInspectUpdate != null)
                    onInspectUpdate(this, interactable);
                
                if (interactable.useType != Interactable.UseType.Scripted) {
                    for (int i = 0; i < currentActionOptions.Count; i++) {
                        int action = currentActionOptions[i];
                        if (ActionsInterface.GetActionDown(action, controller: associatedController)) 
                            interactable.BroadcastActionChange(this, baseInteractor.interactionMode, action, StateChange.Start);
                        if (ActionsInterface.GetAction(action, controller: associatedController)) 
                            interactable.BroadcastActionChange(this, baseInteractor.interactionMode, action, StateChange.Update);
                        if (ActionsInterface.GetActionUp(action, controller: associatedController)) 
                            interactable.BroadcastActionChange(this, baseInteractor.interactionMode, action, StateChange.End);   
                    }
                }
            }
        }

        bool hasInteractable;
        Interactable interactable;

        void UnInspectInteractable (Interactable i) {
            if (i != null)
                i.OnInspectedEnd(this, baseInteractor.interactionMode);
            
            if (onInspectEnd != null) 
                onInspectEnd(this, i);
                
            UIEvents.HideActionPrompt(associatedController);
            if (currentActionOptions != null)
                currentActionOptions.Clear();
        }
        List<int> currentActionOptions;
        
        void SetInteractable (Interactable newInteractable) {
            if (interactable != newInteractable || (hasInteractable && (interactable == null || !interactable.gameObject.activeInHierarchy))) {

                UnInspectInteractable(interactable);

                interactable = newInteractable;

                if (interactable != null)
                {
                    interactable.OnInspectedStart(this, baseInteractor.interactionMode, out string message, out currentActionOptions, out List<string> hints);
                    UIEvents.ShowActionPrompt(associatedController, message, currentActionOptions, hints);
                    
                    if (onInspectStart != null) 
                        onInspectStart(this, interactable);
                    
                }
            }
            hasInteractable = newInteractable != null;
        }

        // public void ForceHoverUnlock()
        // {
        //     hoverLocked = false;
        // }

        // Continue to hover over this object indefinitely, whether or not the interaction point moves out of its interaction trigger volume.
        // public void HoverLock(Interactable interactable)
        // {
        //     hoverLocked = true;
        //     hoveringInteractable = interactable;
        // }

        // Stop hovering over this object indefinitely.
        // public void HoverUnlock(Interactable interactable)
        // {
        //     if (hoveringInteractable == interactable)
        //         hoverLocked = false;
        // }

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