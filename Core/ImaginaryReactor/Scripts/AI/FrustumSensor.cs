//using System.Collections;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Physics;
//using Unity.Transforms;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.Jobs;


//[BurstCompile]
//namespace ImaginaryReactor { public partial  struct FrustumSensingJob : IJobParallelFor
//{
//    //PhysicsWorld PhysicsWorld;
//    [ReadOnly]
//    [NativeDisableParallelForRestriction]
//    public NativeArray<UnityEngine.Plane> planes;
//    [ReadOnly]
//    [NativeDisableParallelForRestriction]
//    public NativeArray<float3> vertices;
//    [NativeDisableParallelForRestriction]
//    public NativeArray<float> exposureRate;

//    public void Execute(int index)
//    {
//        float3 vertex = vertices[index];
//        exposureRate[0] = IsInside(planes, vertex) ? exposureRate[0] + (1 / vertices.Length) : exposureRate[0];
//    }

//    public bool IsInside(NativeArray<UnityEngine.Plane> planes, Vector3 targetPos)
//    {
//        return IsUp(planes[0], targetPos) && IsUp(planes[1], targetPos) && IsUp(planes[2], targetPos) &&
//            IsUp(planes[3], targetPos) && IsUp(planes[4], targetPos) && IsUp(planes[5], targetPos);
//    }
//    //[BurstCompile]
//    //public bool IsInside(UnityEngine.Plane[] planes, Vector3 targetPos)
//    //{
//    //    return IsUp(planes[0], targetPos) && IsUp(planes[1], targetPos) && IsUp(planes[2], targetPos) &&
//    //        IsUp(planes[3], targetPos) && IsUp(planes[4], targetPos) && IsUp(planes[5], targetPos);
//    //}
//    public bool IsUp(UnityEngine.Plane plane, Vector3 targetPos)
//    {
//        return Vector3.Dot(Vector3.Normalize(targetPos - plane.ClosestPointOnPlane(targetPos)), plane.normal) > 0;
//    }


//}

//[BurstCompile]
//namespace ImaginaryReactor { public partial  struct VerticesTransformPointJob : IJobParallelFor
//{
//    [ReadOnly]
//    public float4x4 localToWorldMatrices;
//    [NativeDisableParallelForRestriction]
//    public NativeArray<float3> vertices;

//    public void Execute(int index)
//    {
//        vertices[index] = localToWorldMatrices.TransformPoint(vertices[index]); //MatrixConversion.TransformPoint(transform.localToWorldMatrix, vertices[index]);// transform.TransformPoint();
//    }
//}

//namespace ImaginaryReactor { public partial  FrustumSensor : MonoBehaviour
//{
//    [SerializeField] private Camera sensorCamera;

//    [SerializeField]
//    private float checkDistance = 0.5f;
//    [SerializeField, Range(0, 1)]
//    private float responsiveness = 0.5f;
//    [SerializeField]
//    private UnityEvent eventWhenFound;

//    //public MeshFilter targetFilter;
//    public Transform target;
//    //public MeshRenderer targetRenderer;

//    private float elapsedTime;
//    public Mesh targetSharedMesh;

//    //NativeArray<UnityEngine.Plane> _planes;
//    //NativeArray<float3> _vertices;
//    //NativeArray<float> _exposureRate;

//    TransformAccessArray tfAccess;

//    private void Awake()
//    {
//        //_planes = new NativeArray<UnityEngine.Plane>(6, Allocator.Persistent);
//        // _vertices = new NativeArray<float3>(targetSharedMesh.vertexCount, Allocator.Persistent);
//        //_exposureRate = new NativeArray<float>(1, Allocator.Persistent);
//        //targetSharedMesh = targetFilter.sharedMesh;

//        tfAccess = new TransformAccessArray(new Transform[1] { target });
//    }

//    private void OnDestroy()
//    {
//        //_planes.Dispose();
//        //_vertices.Dispose();
//        //_exposureRate.Dispose();
//    }

//    private void LateUpdate()
//    {
//        elapsedTime += Time.deltaTime;

//        if(elapsedTime > checkDistance)
//        {
//            JobHandle handle;

//            NativeArray<UnityEngine.Plane> _planes = new NativeArray<UnityEngine.Plane>(6, Allocator.TempJob);
//            NativeArray<float3> _vertices = new NativeArray<float3>(targetSharedMesh.vertexCount, Allocator.TempJob);
//            NativeArray<float> _exposureRate = new NativeArray<float>(1, Allocator.TempJob);

//            UnityEngine.Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(sensorCamera);
//            _planes[0] = camPlanes[0];
//            _planes[1] = camPlanes[1];
//            _planes[2] = camPlanes[2];
//            _planes[3] = camPlanes[3];
//            _planes[4] = camPlanes[4];
//            _planes[5] = camPlanes[5];

//            for (int i = 0; i < targetSharedMesh.vertexCount; i++)
//            {
//                _vertices[i] = targetSharedMesh.vertices[i];
//            }

//            VerticesTransformPointJob tpJob = new VerticesTransformPointJob()
//            {
//                localToWorldMatrices = float4x4.TRS(target.position, target.rotation, target.localScale),
//                vertices = _vertices
//            };

//            handle = tpJob.Schedule(_vertices.Length, 1);
//            //handle.Complete();
//            //JobHandle handle2;

//            _exposureRate[0] = 0;
//            FrustumSensingJob sensingJob = new FrustumSensingJob()
//            {
//                planes = _planes,
//                vertices = _vertices,
//                exposureRate = _exposureRate,
//            };

//            JobHandle handle2 = sensingJob.Schedule(_vertices.Length, 1, handle);

//            handle2.Complete();
//            Debug.Log(_exposureRate[0]);
//            if (_exposureRate[0] > responsiveness)
//            {
//                eventWhenFound?.Invoke();
//                Debug.Log("Found Target");
//            }
//            _planes.Dispose();
//            _exposureRate.Dispose();
//            _vertices.Dispose();

//            elapsedTime = 0;

//        }
//    }
//}

////namespace ImaginaryReactor { public partial  FrustumSensingManager : MonoBehaviour
////{
////    //PhysicsWorld PhysicsWorld;

////    public List<HeadCamObject> camList;
////    NativeArray<UnityEngine.Plane> _planeArr;
////    NativeArray<float3> _cameraPos;
////    //NativeArray<float3> _targetPos;
////    //NativeArray<float3> _boundSize;
////    NativeArray<bool> _results;

////    public Transform target;
////    public Vector3 _boundSize;
////    public int index;
////    public bool result;

////    // Start is called before the first frame update
////    void Start()
////    {
////        //PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

////        _planeArr = new NativeArray<UnityEngine.Plane>(camList.Count*6, Allocator.Persistent);
////        _cameraPos = new NativeArray<float3>(camList.Count, Allocator.Persistent);
////        //_targetPos= new NativeArray<float3>(camList.Count, Allocator.Persistent);
////        //_boundSize= new NativeArray<float3>(camList.Count, Allocator.Persistent);
////        _results= new NativeArray<bool>(camList.Count, Allocator.Persistent);
////    }

////    private void OnDestroy()
////    {
////        _planeArr.Dispose();
////        _cameraPos.Dispose();
////        //_targetPos.Dispose();
////        //_boundSize.Dispose();
////        _results.Dispose();
////    }

////    // Update is called once per frame
////    void LateUpdate()
////    {
////        JobHandle handle;

////        for(int i = 0; i<camList.Count; i++)
////        {
////            UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camList[i].camera);

////            _planeArr[i * 6] = planes[0];
////            _planeArr[i * 6+1] = planes[1];
////            _planeArr[i * 6+2] = planes[2];
////            _planeArr[i * 6+3] = planes[3];
////            _planeArr[i * 6+4] = planes[4];
////            _planeArr[i * 6+5] = planes[5];
////        }

////        FrustumSensingJob frustumSensingJob = new FrustumSensingJob()
////        {
////            planeArr = _planeArr,
////            cameraPos = _cameraPos,
////            targetPos = target.position,
////            boundSize = _boundSize,
////            results = _results
////        };

////        handle = frustumSensingJob.Schedule(camList.Count, 1);

////        handle.Complete();

////        result = _results[Mathf.Min(index, _results.Length - 1)];
////    }
////}
