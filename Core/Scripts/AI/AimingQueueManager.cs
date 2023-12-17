using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class AimingQueueManager : MonoBehaviour
{
    public static AimingQueueManager instance;
    // Start is called before the first frame update

    public Dictionary<int, Queue<float3>> queue;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }

        queue = new Dictionary<int, Queue<float3>>();
    }

    public void Enqueue(int id, float3 value, int allowedCount)
    {
        if (queue.ContainsKey(id))
        {
            if (queue[id].Count >= allowedCount)
            {
                queue[id].Dequeue();
            }
                queue[id].Enqueue(value);
        }
        else
        {
            var newQueue = new Queue<float3>();
            newQueue.Enqueue(value);
            queue.Add(id, newQueue);
        }
    }

    public float3 Dequeue(int id, int allowedCount, bool forceDequeue = false)
    {
        float3 result;
        if(queue.ContainsKey(id))
        {
            if (queue[id].Count < allowedCount)
            {
                if (!forceDequeue)
                {
                    return queue[id].TryPeek(out result)?result:new float3(-100, -100, -100);
                }
                else
                {
                    return queue[id].Dequeue();
                }
            }
            else
            {
                return queue[id].Dequeue();
            }
        }

        return queue[id].TryPeek(out result) ? result : new float3(-100, -100, -100);
    }
}
