using System.Collections;
using UnityEngine;

namespace ImaginaryReactor
{
    public class HeadCamObject : MonoBehaviour
    {
        public static Transform Target;
        public Camera camera;
        public int id = -1;

        private void Awake()
        {
            Target = this.transform;
            camera = GetComponent<Camera>();
        }

        IEnumerator Start()
        {
            while (!HeadCamObjectManager.instance)
            {
                yield return new WaitForEndOfFrame();
            }

            if (id == -1)
            {
                HeadCamObjectManager.instance.headCamDict.Add(HeadCamObjectManager.instance.headCamDict.Count + 1, this);
            }
            else
            {
                HeadCamObjectManager.instance.headCamDict.Add(id, this);
            }
        }
    }
}