/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Leap;

namespace Leap.Unity
{
    /** 
     * HandPool holds a pool of IHandModels and makes HandRepresentations 
     * when given a Leap Hand and a model type of graphics or physics.
     * When a HandRepresentation is created, an IHandModel is removed from the pool.
     * When a HandRepresentation is finished, its IHandModel is returned to the pool.
     */
    public class HandPool : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference for the transform that is a child of the camera rig's root and is a parent to all hand models")]
        private Transform ModelsParent;
        [SerializeField]
        private List<ModelGroup> ModelPool;
        private List<HandRepresentation> activeHandReps = new List<HandRepresentation>();

        private Dictionary<IHandModel, ModelGroup> modelGroupMapping = new Dictionary<IHandModel, ModelGroup>();
        private Dictionary<IHandModel, HandRepresentation> modelToHandRepMapping = new Dictionary<IHandModel, HandRepresentation>();
        /**
         * ModelGroup contains a left/right pair of IHandModel's 
         * @param modelList The IHandModels available for use by HandRepresentations
         * @param modelsCheckedOut The IHandModels currently in use by active HandRepresentations
         * @param IsEnabled determines whether the ModelGroup is active at app Start(), though ModelGroup's are controlled with the EnableGroup() & DisableGroup methods.
         * @param CanDuplicate Allows a IHandModels in the ModelGroup to be cloned at runtime if a suitable IHandModel isn't available.
         */
        [System.Serializable]
        public class ModelGroup
        {
            public string GroupName;
            [HideInInspector]
            public HandPool _handPool;

            public IHandModel LeftModel;
            [HideInInspector]
            public bool IsLeftToBeSpawned;
            public IHandModel RightModel;
            [HideInInspector]
            public bool IsRightToBeSpawned;
            [HideInInspector]
            public List<IHandModel> modelList;
            [HideInInspector]
            public List<IHandModel> modelsCheckedOut;
            public bool IsEnabled = true;
            public bool CanDuplicate;

            public Hands.HandEvent HandPostProcesses;

            /*Looks for suitable IHandModel is the ModelGroup's modelList, if found, it is added to modelsCheckedOut.
             * If not, one can be cloned*/
            public IHandModel TryGetModel(Chirality chirality, ModelType modelType)
            {
                for (int i = 0; i < modelList.Count; i++)
                {
                    if (modelList[i].HandModelType == modelType && modelList[i].Handedness == chirality)
                    {
                        IHandModel model = modelList[i];
                        modelList.RemoveAt(i);
                        modelsCheckedOut.Add(model);
                        return model;
                    }
                }
                if (CanDuplicate)
                {
                    for (int i = 0; i < modelsCheckedOut.Count; i++)
                    {
                        if (modelsCheckedOut[i].HandModelType == modelType && modelsCheckedOut[i].Handedness == chirality)
                        {
                            IHandModel modelToSpawn = modelsCheckedOut[i];
                            IHandModel spawnedModel = UnityEngine.GameObject.Instantiate(modelToSpawn);
                            spawnedModel.transform.parent = _handPool.ModelsParent;
                            _handPool.modelGroupMapping.Add(spawnedModel, this);
                            modelsCheckedOut.Add(spawnedModel);
                            return spawnedModel;
                        }
                    }
                }
                return null;
            }
            public void ReturnToGroup(IHandModel model)
            {
                modelsCheckedOut.Remove(model);
                modelList.Add(model);
                this._handPool.modelToHandRepMapping.Remove(model);
            }
        }
        public void ReturnToPool(IHandModel model)
        {
            ModelGroup modelGroup;
            bool groupFound = modelGroupMapping.TryGetValue(model, out modelGroup);
            Assert.IsTrue(groupFound);
            //First see if there is another active Representation that can use this model
            for (int i = 0; i < activeHandReps.Count; i++)
            {
                HandRepresentation rep = activeHandReps[i];
                if (rep.RepChirality == model.Handedness && rep.RepType == model.HandModelType)
                {
                    bool modelFromGroupFound = false;
                    if (rep.handModels != null)
                    {
                        //And that Represention does not contain a model from this model's modelGroup
                        for (int j = 0; j < modelGroup.modelsCheckedOut.Count; j++)
                        {
                            IHandModel modelToCompare = modelGroup.modelsCheckedOut[j];
                            for (int k = 0; k < rep.handModels.Count; k++)
                            {
                                if (rep.handModels[k] == modelToCompare)
                                {
                                    modelFromGroupFound = true;
                                }
                            }
                        }
                    }
                    if (!modelFromGroupFound)
                    {
                        rep.AddModel(model);
                        modelToHandRepMapping[model] = rep;
                        return;
                    }
                }
            }
            //Otherwise return to pool
            modelGroup.ReturnToGroup(model);
        }
        public void RemoveHandRepresentation(HandRepresentation handRepresentation)
        {
            activeHandReps.Remove(handRepresentation);
        }
        /** Popuates the ModelPool with the contents of the ModelCollection */
        void Start()
        {
            if (ModelsParent == null)
            {
                Debug.LogWarning("HandPool.ModelsParent needs to reference the parent transform of the hand models.  This transform should be a child of the LMHeadMountedRig transform.");
            }

            for (int i = 0; i < ModelPool.Count; i++)
            {
                var collectionGroup = ModelPool[i];
                collectionGroup._handPool = this;
                IHandModel leftModel;
                IHandModel rightModel;
                if (collectionGroup.IsLeftToBeSpawned)
                {
                    IHandModel modelToSpawn = collectionGroup.LeftModel;
                    UnityEngine.GameObject spawnedGO = UnityEngine.GameObject.Instantiate(modelToSpawn.gameObject);
                    leftModel = spawnedGO.GetComponent<IHandModel>();
                    leftModel.transform.parent = ModelsParent;
                }
                else
                {
                    leftModel = collectionGroup.LeftModel;
                }
                if (leftModel != null)
                {
                    collectionGroup.modelList.Add(leftModel);
                    modelGroupMapping.Add(leftModel, collectionGroup);
                }

                if (collectionGroup.IsRightToBeSpawned)
                {
                    IHandModel modelToSpawn = collectionGroup.RightModel;
                    UnityEngine.GameObject spawnedGO = UnityEngine.GameObject.Instantiate(modelToSpawn.gameObject);
                    rightModel = spawnedGO.GetComponent<IHandModel>();
                    rightModel.transform.parent = ModelsParent;
                }
                else
                {
                    rightModel = collectionGroup.RightModel;
                }
                if (rightModel != null)
                {
                    collectionGroup.modelList.Add(rightModel);
                    modelGroupMapping.Add(rightModel, collectionGroup);
                }
            }
        }

        /**
         * MakeHandRepresentation receives a Hand and combines that with an IHandModel to create a HandRepresentation
         * @param hand The Leap Hand data to be drive an IHandModel
         * @param modelType Filters for a type of hand model, for example, physics or graphics hands.
         */

        public HandRepresentation MakeHandRepresentation(Hand hand, ModelType modelType)
        {
            Chirality handChirality = hand.IsRight ? Chirality.Right : Chirality.Left;
            HandRepresentation handRep = new HandRepresentation(this, hand, handChirality, modelType);
            for (int i = 0; i < ModelPool.Count; i++)
            {
                ModelGroup group = ModelPool[i];
                if (group.IsEnabled)
                {
                    IHandModel model = group.TryGetModel(handChirality, modelType);
                    if (model != null)
                    {
                        handRep.AddModel(model);
                        if (!modelToHandRepMapping.ContainsKey(model))
                        {
                            model.group = group;
                            modelToHandRepMapping.Add(model, handRep);
                        }
                    }
                }
            }
            activeHandReps.Add(handRep);
            return handRep;
        }
        /**
        * EnableGroup finds suitable HandRepresentations and adds IHandModels from the ModelGroup, returns them to their ModelGroup and sets the groups IsEnabled to true.
         * @param groupName Takes a string that matches the ModelGroup's groupName serialized in the Inspector
        */
        public void EnableGroup(string groupName)
        {
            StartCoroutine(enableGroup(groupName));
        }
        private IEnumerator enableGroup(string groupName)
        {
            yield return new WaitForEndOfFrame();
            ModelGroup group = null;
            for (int i = 0; i < ModelPool.Count; i++)
            {
                if (ModelPool[i].GroupName == groupName)
                {
                    group = ModelPool[i];
                    for (int hp = 0; hp < activeHandReps.Count; hp++)
                    {
                        HandRepresentation handRep = activeHandReps[hp];
                        IHandModel model = group.TryGetModel(handRep.RepChirality, handRep.RepType);
                        if (model != null)
                        {
                            handRep.AddModel(model);
                            modelToHandRepMapping.Add(model, handRep);
                        }
                    }
                    group.IsEnabled = true;
                }
            }
            if (group == null)
            {
                Debug.LogWarning("A group matching that name does not exisit in the modelPool");
            }
        }
        /**
         * DisableGroup finds and removes the ModelGroup's IHandModels from their HandRepresentations, returns them to their ModelGroup and sets the groups IsEnabled to false.
         * @param groupName Takes a string that matches the ModelGroup's groupName serialized in the Inspector
         */
        public void DisableGroup(string groupName)
        {
            StartCoroutine(disableGroup(groupName));
        }
        private IEnumerator disableGroup(string groupName)
        {
            yield return new WaitForEndOfFrame();
            ModelGroup group = null;
            for (int i = 0; i < ModelPool.Count; i++)
            {
                if (ModelPool[i].GroupName == groupName)
                {
                    group = ModelPool[i];
                    for (int m = 0; m < group.modelsCheckedOut.Count; m++)
                    {
                        IHandModel model = group.modelsCheckedOut[m];
                        HandRepresentation handRep;
                        if (modelToHandRepMapping.TryGetValue(model, out handRep))
                        {
                            handRep.RemoveModel(model);
                            group.ReturnToGroup(model);
                            m--;
                        }
                    }
                    Assert.AreEqual(0, group.modelsCheckedOut.Count, group.GroupName + "'s modelsCheckedOut List has not been cleared");
                    group.IsEnabled = false;
                }
            }
            if (group == null)
            {
                Debug.LogWarning("A group matching that name does not exisit in the modelPool");
            }
        }
        public void ToggleGroup(string groupName)
        {
            StartCoroutine(toggleGroup(groupName));
        }
        private IEnumerator toggleGroup(string groupName)
        {
            yield return new WaitForEndOfFrame();
            ModelGroup modelGroup = ModelPool.Find(i => i.GroupName == groupName);
            if (modelGroup != null)
            {
                if (modelGroup.IsEnabled == true)
                {
                    DisableGroup(groupName);
                    modelGroup.IsEnabled = false;
                }
                else
                {
                    EnableGroup(groupName);
                    modelGroup.IsEnabled = true;
                }
            }
            else Debug.LogWarning("A group matching that name does not exisit in the modelPool");
        }
        public void AddNewGroup(string groupName, IHandModel leftModel, IHandModel rightModel)
        {
            ModelGroup newGroup = new ModelGroup();
            newGroup.LeftModel = leftModel;
            newGroup.RightModel = rightModel;
            newGroup.GroupName = groupName;
            newGroup.CanDuplicate = false;
            newGroup.IsEnabled = true;
            ModelPool.Add(newGroup);
        }
        public void RemoveGroup(string groupName)
        {
            while (ModelPool.Find(i => i.GroupName == groupName) != null)
            {
                ModelGroup modelGroup = ModelPool.Find(i => i.GroupName == groupName);
                if (modelGroup != null)
                {
                    ModelPool.Remove(modelGroup);
                }
            }
        }
        public T GetHandModel<T>(int handId) where T : IHandModel
        {
            foreach (ModelGroup group in ModelPool)
            {
                foreach (IHandModel handModel in group.modelsCheckedOut)
                {
                    if (handModel.GetLeapHand().Id == handId && handModel is T)
                    {
                        return handModel as T;
                    }
                }
            }
            return null;
        }

        /***
* New Methods added by Tiare
* start here
***/
        public int GetPoolSize()
        {
            return ModelPool.Count;
        }

        public void SetHandOffsetAllHands(Vector3 positionOffset, Vector3 armLookAtPosition, Vector3 axis, float shiftAngle, bool left, bool right)
        {
            for (int i = 0; i < ModelPool.Count; i++)
            {
                ModelGroup group = ModelPool[i];
                IHandModel model = group.LeftModel;
                if (model != null && left)
                {
                    model.UpdateHandOffset(positionOffset, armLookAtPosition, axis, shiftAngle);
                    //Debug.Log("Set left offset for " + group.GroupName);
                }

                model = group.RightModel;
                if (model != null && right)
                {
                    model.UpdateHandOffset(positionOffset, armLookAtPosition, axis,shiftAngle);
                    //Debug.Log("Set right offset for " + group.GroupName);
                }
            }
        }

        public Vector3 GetPalmPosition(int index, bool right)
        {
            Vector3 pos = Vector3.zero;
            if (index < 0 || index > ModelPool.Count - 1)
                return pos;

            ModelGroup group = ModelPool[index];
            if (right)
            {
                IHandModel model = group.RightModel;
                if (model != null)
                    pos = model.GetPosition();
            }
            else
            {
                IHandModel model = group.RightModel;
                if (model != null)
                    pos = model.GetPosition();
            }

            return pos;
        }

        public float  GetConfidence(bool right)
        {
            float confidence = 0;

            for (int i = 0; i < ModelPool.Count; i++)
            {
                ModelGroup group = ModelPool[i];
                if (group != null)
                {
                    IHandModel model = (right) ? group.RightModel : group.LeftModel;
                    if (model != null && model.isActiveAndEnabled)
                    {
                        Hand hand = model.GetLeapHand();
                        if (hand != null)
                            confidence = Mathf.Max(hand.Confidence, confidence);
                    }
                }
            }
            return confidence;
        }

        public Hand GetActiveHandModel(Chirality handedness)
        {
            for (int i = 0; i < activeHandReps.Count; i++)
            {
                HandRepresentation rep = activeHandReps[i];
                if (rep.RepChirality == handedness)
                    return rep.MostRecentHand;
            }
            return null;
        }

        public void FreezeFingers(bool freeze, bool right)
        {
            for (int i = 0; i < ModelPool.Count; i++)
            {
                ModelGroup group = ModelPool[i];
                if (group != null)
                {
                    IHandModel model = (right) ? group.RightModel : group.LeftModel;
                    if (model != null && model.isActiveAndEnabled)
                    {
                        model.FreezeFingers(freeze);
                    }
                }
            }
        }


        /**
        * RenderGroup finds suitable HandRepresentations and activates or deactivates the renderer of any found mesh.
         * @param groupName Takes a string that matches the ModelGroup's groupName serialized in the Inspector
        */
        public void RenderGroup(string groupName, bool render)
        {
            StartCoroutine(renderGroup(groupName, render));
        }

        private IEnumerator renderGroup(string groupName, bool enable)
        {
            yield return new WaitForEndOfFrame();
            ModelGroup group = null;
            for (int i = 0; i < ModelPool.Count; i++)
            {
                if (ModelPool[i].GroupName == groupName)
                {
                    group = ModelPool[i];

                    EnableRenderers(group.LeftModel, enable);
                    EnableColliders(group.LeftModel, enable);

                    EnableRenderers(group.RightModel, enable);
                    EnableColliders(group.RightModel, enable);
                }
            }
            if (group == null)
            {
                Debug.LogWarning("A group matching that name does not exisit in the modelPool");
            }
        }

        private void EnableRenderers (IHandModel model, bool render)
        {
            if (model != null)
            {
                Renderer[] renderers = model.gameObject.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                    r.enabled = render;
            }
        }

        private void EnableColliders(IHandModel model, bool collide)
        {
            if (model != null)
            {
                Collider[] colliders = model.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider c in colliders)
                    c.enabled = collide;
            }
        }


        //public void ApplyHandRotationOffset(string groupName, Quaternion rotationOffset)
        //{
        //    StartCoroutine(applyHandRotationOffset(groupName, rotationOffset));
        //}

        //private IEnumerator applyHandRotationOffset(string groupName, Quaternion rotationOffset)
        //{
        //    yield return new WaitForEndOfFrame();
        //    ModelGroup group = null;
        //    for (int i = 0; i < ModelPool.Count; i++)
        //    {
        //        if (ModelPool[i].GroupName == groupName)
        //        {
        //            group = ModelPool[i];
        //            ApplyRotations(group.LeftModel, rotationOffset);
        //            ApplyRotations(group.RightModel, rotationOffset);
        //        }
        //    }
        //    if (group == null)
        //    {
        //        Debug.LogWarning("A group matching that name does not exisit in the modelPool");
        //    }
        //}

        //private void ApplyRotations(IHandModel model, Quaternion rotationOffset)
        //{
        //    Quaternion tempRot = Quaternion.identity;
        //    if (model != null)
        //    {
        //        Transform[] bones = model.transform.GetComponentsInChildren<Transform>();
        //        foreach (Transform b in bones)
        //        {
        //            tempRot = b.rotation;
        //            // tempRot = b.rotation * rotationOffset;
        //            b.rotation = rotationOffset * tempRot;//tempRot;
        //        }
        //    }
        //}

        /***
        * New Methods added by Tiare
        * end here
        ***/

#if UNITY_EDITOR
        /**In the Unity Editor, Validate that the IHandModel is an instance of a prefab from the scene vs. a prefab from the project. */
        void OnValidate()
        {
            for (int i = 0; i < ModelPool.Count; i++)
            {
                if (ModelPool[i] != null)
                {
                    if (ModelPool[i].LeftModel)
                    {
                        ModelPool[i].IsLeftToBeSpawned = shouldBeSpawned(ModelPool[i].LeftModel);
                    }
                    if (ModelPool[i].RightModel)
                    {
                        ModelPool[i].IsRightToBeSpawned = shouldBeSpawned(ModelPool[i].RightModel);
                    }
                }
            }
        }

        private bool shouldBeSpawned(Object model)
        {
            var prefabType = PrefabUtility.GetPrefabType(model);
            if (PrefabUtility.GetPrefabType(this) != PrefabType.Prefab)
            {
                return prefabType == PrefabType.Prefab;
            }
            else
            {
                return PrefabUtility.GetPrefabObject(model) != PrefabUtility.GetPrefabObject(this);
            }
        }

#endif
    }
}

