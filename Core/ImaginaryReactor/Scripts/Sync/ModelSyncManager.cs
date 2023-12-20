using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ImaginaryReactor
{
    public class ModelSyncManager : MonoBehaviour
    {
        public static ModelSyncManager instance;

        public Dictionary<int, ModelSyncObject> modelDict;

        public Queue<int> clearBodyQueue;

        public void SetPosition(int id, float3 _position)
        {
            if (modelDict.ContainsKey(id))
            {
                modelDict[id].transform.position = _position;
            }
        }
        public void SetRotation(int id, quaternion _rotation)
        {
            if (modelDict.ContainsKey(id))
            {
                modelDict[id].transform.rotation = _rotation;
            }
        }

        public void SetBool(int id, string paramName, bool value)
        {
            if (modelDict.ContainsKey(id))
            {
                if (modelDict[id].animator)
                { modelDict[id].animator.SetBool(paramName, value); }
            }
        }
        public void SetTrigger(int id, string paramName)
        {
            if (modelDict.ContainsKey(id))
            {
                if (modelDict[id].animator)
                { modelDict[id].animator.SetTrigger(paramName); }
            }
        }

        public void SetFloat(int id, string paramName, float value)
        {
            if (modelDict.ContainsKey(id))
            {
                if (modelDict[id].animator)
                { modelDict[id].animator.SetFloat(paramName, value); }
            }
        }


        public void PushLine(int id, float3 startPos, float3 endPos)
        {
            if (modelDict.ContainsKey(id))
            {
                modelDict[id].transform.GetComponent<ISync>().Line(startPos, endPos);
            }
        }


        public void PushLine(float3 startPos, float3 endPos)
        {
            //if (modelDict.ContainsKey(id))
            float minDistance = float.MaxValue;
            ModelSyncObject selectedObj = null;
            foreach (var obj in modelDict.Values)
            {
                float tempDistance = Vector3.Distance(startPos, obj.transform.position);
                if (minDistance > tempDistance)
                {
                    minDistance = tempDistance;
                    selectedObj = obj;
                }
            }

            if(selectedObj != null)
            {
                selectedObj.transform.GetComponent<ISync>().Line(startPos, endPos);
            }
        }


        public void AddModel(int id, ModelSyncObject obj)
        {
            if (modelDict.ContainsKey(id))
            {
                if (modelDict[id] != obj)
                {
                    modelDict[id] = obj;
                }
            }
            else
            {
                modelDict.Add(id, obj);
            }
        }

        public void RemoveModel(int id, ModelSyncObject obj)
        {
            if (modelDict.ContainsKey(id))
            {
                if (modelDict[id] == obj)
                {
                    modelDict.Remove(id);
                }
            }
        }

        public void ClearingCharacterBody(int id)
        {
            clearBodyQueue.Enqueue(id);
        }

        private void Awake()
        {
            if (instance == null)
            { instance = this; }
            else
            {
                Destroy(gameObject);
            }
            clearBodyQueue = new Queue<int>();
            modelDict = new Dictionary<int, ModelSyncObject>();
        }

    }
}