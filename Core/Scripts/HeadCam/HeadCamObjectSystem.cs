using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(HeadCamSystem))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class HeadCamObjectSystem: SystemBase
{
    protected override void OnUpdate()
    {
        //foreach( var headCam in HeadCamObjectManager.instance.headCamList)//headCamDict.Keys)
        //{
        //    HeadCamObject target = headCam;

        //    Entities.ForEach((HeadCam HeadCamComponent) => { if (HeadCamComponent.ID == target.id) {

        //            Entity cameraEntity = //SystemAPI.GetSingletonEntity<HeadCam>();
        //            LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(cameraEntity);
        //            target.transform.SetPositionAndRotation(targetLocalToWorld.Position, targetLocalToWorld.Rotation);

        //        } }).Schedule();

        //    //if (target != null && SystemAPI.HasSingleton<HeadCam>())
        //    //{
        //    //    Entity cameraEntity = SystemAPI.GetSingletonEntity<HeadCam>();
        //    //    LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(cameraEntity);
        //    //    target.transform.SetPositionAndRotation(targetLocalToWorld.Position, targetLocalToWorld.Rotation);
        //    //}
        //}


        //if (HeadCamObject.Target != null && SystemAPI.HasSingleton<HeadCam>())
        //{
        //    Entity cameraEntity = SystemAPI.GetSingletonEntity<HeadCam>();
        //    LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(cameraEntity);
        //    HeadCamObject.Target.transform.SetPositionAndRotation(targetLocalToWorld.Position, targetLocalToWorld.Rotation);
        //}

        foreach (var (headCam, ltw )in SystemAPI.Query<HeadCam, LocalToWorld>())
        {
            HeadCamObjectManager.instance.headCamDict[headCam.ID].transform.SetPositionAndRotation(ltw.Position, ltw.Rotation);
        }

        //HeadCamJob job = new HeadCamJob()
        //{

            //}.Schedule();
    }

    //[BurstCompile]
    //public partial struct HeadCamJob : IJobEntity
    //{
    //    public HeadCamObject HeadCamObj;

    //    void Execute(
    //        Entity entity,
    //        in LocalToWorld ltw,
    //        in HeadCam headCam)
    //    {
    //        HeadCamObj.transform.SetPositionAndRotation(ltw.Position, ltw.Rotation);
    //    }
    //}
}