using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public class NoticeUIManager : MonoBehaviour
    {
        //[System.Serializable]
        public struct HitDirectionInfo
        {
            public Transform uiTransform;
            public Vector3 worldPos;//worldDir;
            public float amount;
            public float remainTime;

            public void SetRemainTime(float _remainTime)
            {
                remainTime = _remainTime;
            }
        }

        public static NoticeUIManager instance;
        //public Transform cameraOnWorld;
        public GameObject hitDirectionPrefab;
        public Canvas canvas;
        Camera mainCam;
        private List<HitDirectionInfo> hitDirectionInfoList;
        public float infoShowDuration = 3;
        public float maxForceFactor = 200;
        public float liftAngle = 7.5f;
        public Vector2 posOffset = new Vector2(0, -0.2f);

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            hitDirectionInfoList = new List<HitDirectionInfo>();
            mainCam = Camera.main;
        }

        public Vector3 GetRotationFromWorldDirection(Vector3 worldDir)
        {
            Vector3 dir = worldDir; // mainCam.transform.InverseTransformDirection(worldDir);
            dir.y += (float)math.sqrt(math.sin(math.abs(dir.z) * math.radians(liftAngle))) * (dir.z > 0 ? 1 : -1);
            return dir;
        }

        public void ShowHitVector(float3 position, float force) //force)
        {
            //Debug.Log("I'm hit! : "+force);
            //RectTransform rectTransform = GetComponent<RectTransform>();
            //rectTransform.position
            float3 worldDir = Vector3.Normalize(mainCam.transform.InverseTransformPoint(position)); //math.normalizesafe(-force);
            float amount = force;//math.length(force);
            GameObject newHitDirGO = Instantiate(hitDirectionPrefab, new Vector3(Screen.width * (0.5f + posOffset.x), Screen.height * (0.5f + posOffset.y), 0), //Vector3.zero, 
                Quaternion.LookRotation(
                    GetRotationFromWorldDirection(worldDir)
                    , Vector3.up), this.transform);

            newHitDirGO.transform.GetChild(0).localScale = Vector3.one * math.clamp(amount / maxForceFactor, 0.25f, 1);

            hitDirectionInfoList.Add(new HitDirectionInfo()
            {
                uiTransform = newHitDirGO.transform,
                worldPos = position,//worldDir = worldDir,
                amount = amount,
                remainTime = infoShowDuration
            });

            //if(RectTransformUtility.ScreenPointToWorldPointInRectangle(newHitDirObj, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), mainCam, out Vector3 worldPos))
            //{
            //    newHitDirObj.position = worldPos;
            //}
        }

        public void LateUpdate()
        {
            int count = hitDirectionInfoList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var hitDirectionInfo = hitDirectionInfoList[i];
                float _remainTime = hitDirectionInfo.remainTime - Time.deltaTime;
                if (_remainTime < 0)
                {
                    Destroy(hitDirectionInfo.uiTransform.gameObject);
                    hitDirectionInfoList.RemoveAt(i);
                }
                else
                {
                    float3 worldDir = Vector3.Normalize(mainCam.transform.InverseTransformPoint(hitDirectionInfo.worldPos));

                    hitDirectionInfo.uiTransform.rotation = Quaternion.LookRotation(
                    GetRotationFromWorldDirection(worldDir), Vector3.up);
                    hitDirectionInfoList[i] = new HitDirectionInfo()
                    {
                        uiTransform = hitDirectionInfo.uiTransform,
                        amount = hitDirectionInfo.amount,
                        remainTime = _remainTime,
                        worldPos = hitDirectionInfo.worldPos
                        //worldDir = hitDirectionInfo.worldDir
                    };//SetRemainTime( remainTime);
                }
            }
        }
    }
}