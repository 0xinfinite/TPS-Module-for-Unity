using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.CharacterController;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
//using UnityEngine;
namespace ImaginaryReactor
{
    public struct ThirdPersonCharacterUpdateContext
    {
        // Here, you may add additional global data for your character updates, such as ComponentLookups, Singletons, NativeCollections, etc...
        // The data you add here will be accessible in your character updates and all of your character "callbacks".

        public void OnSystemCreate(ref SystemState state)
        {
            // Get lookups
        }

        public void OnSystemUpdate(ref SystemState state)
        {
            // Update lookups
        }
    }

    public readonly partial struct ThirdPersonCharacterAspect : IAspect, IKinematicCharacterProcessor<ThirdPersonCharacterUpdateContext>
    {
        public readonly KinematicCharacterAspect CharacterAspect;
        public readonly RefRW<ThirdPersonCharacterComponent> CharacterComponent;
        public readonly RefRW<ThirdPersonCharacterControl> CharacterControl;

        public void PhysicsUpdate(ref ThirdPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
            ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

            // First phase of default character update
            CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
            CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
            CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);

            // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
            HandleVelocityControl(ref context, ref baseContext);

            // Second phase of default character update
            CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
            CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, characterComponent.Gravity);
            CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
            CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
            CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
            CharacterAspect.Update_ProcessStatefulCharacterHits();
        }

        public void PhysicsUpdate(ref ThirdPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in RootMotionComponent rootMotion)
        {
            ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
            ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

            // First phase of default character update
            CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
            CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
            CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);

            // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
            characterBody.RelativeVelocity = rootMotion.Velocity + new float3(0, characterBody.RelativeVelocity.y, 0);
            characterBody.IsGrounded = false;
            characterBody.GroundHit = default;
            CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, characterComponent.Gravity, baseContext.Time.DeltaTime);
            //HandleVelocityControl(ref context, ref baseContext);

            // Second phase of default character update
            CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
            CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, characterComponent.Gravity);
            CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
            CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
            CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
            CharacterAspect.Update_ProcessStatefulCharacterHits();
        }

        private void Jumping(bool ignoreInertia, ref KinematicCharacterBody body, float jumpSpeed, float3 desireToMove, float3 groundingUp, float maxAirSpeed = 0)
        {
            float3 velocityOnPlane = MathUtilities.ProjectOnPlane(body.RelativeVelocity, groundingUp);
            if (ignoreInertia && math.dot(math.normalizesafe(velocityOnPlane), math.normalizesafe(desireToMove)) < 0)
            {
                body.RelativeVelocity = MathUtilities.ProjectOnPlane(desireToMove, groundingUp) + (groundingUp * jumpSpeed);
            }
            else
            {
                CharacterControlUtilities.StandardJump(ref body, groundingUp * jumpSpeed, true, groundingUp);
            }
        }

        private void HandleVelocityControl(ref ThirdPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            float deltaTime = baseContext.Time.DeltaTime;
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
            ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
            ref ThirdPersonCharacterControl characterControl = ref CharacterControl.ValueRW;

            characterComponent.GroundedTime = characterBody.IsGrounded ? characterComponent.GroundedTime + deltaTime : 0;

            // Rotate move input and velocity to take into account parent rotation
            if (characterBody.ParentEntity != Entity.Null)
            {
                characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
                characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
            }
            //Debug.Log(characterControl.LastJumpPressedTime);
            //Debug.Log(characterComponent.ContactCount);

            //Debug.Log(characterBody.GroundHit.Normal.y + "/"+math.lengthsq(characterBody.GroundHit.Normal));

            if (characterBody.IsGrounded)
            {
                // Move on ground
                float3 targetVelocity = characterControl.MoveVector * characterComponent.GroundMaxSpeed;

                if (characterComponent.GroundedTime < 0.2f && math.lengthsq(characterControl.MoveVector) > 0.01f)
                {
                    CharacterControlUtilities.StandardGroundMove_Accelerated(ref characterBody.RelativeVelocity, characterControl.MoveVector * characterComponent.GroundedMovementSharpness,
                        characterComponent.AirMaxSpeed, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal, false);
                }
                else
                {
                    CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity, characterComponent.GroundedMovementSharpness, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal);
                }

                characterControl.FloatingFuel = characterComponent.MaxClimbFuel;

                // Jump
                if (/*characterControl.LastJumpPressedTime < 10 && */characterControl.Jump)//characterControl.Jump)
                {
                    //CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * characterComponent.JumpSpeed, true, characterBody.GroundingUp);
                    Jumping(characterComponent.IgnoreInertiaWhenJump, ref characterBody, characterComponent.JumpSpeed / math.max(characterComponent.ContactCount, 1)
                        , characterControl.MoveVector, characterBody.GroundingUp, characterComponent.AirMaxSpeed);
                    characterControl.Jump = false;
                }
                characterComponent.LastWallNormal = float3.zero;
                characterComponent.WallNormal = float3.zero;
                characterComponent.AirJumpedCount = 0;
                characterComponent.ContactCount = 0;
                characterComponent.WallSlideTime = 0;
            }
            else
            {
                //Debug.Log(math.lengthsq(characterComponent.WallNormal));
                if (characterComponent.AbleToWallRun && math.lengthsq(characterComponent.WallNormal) > 0.1 && math.distance(characterComponent.WallNormal,
                    characterComponent.LastWallNormal) > 0.1f && characterComponent.WallSlideTime < characterComponent.WallSlideDuration)//&& characterComponent.WallNormal.y < 0.3f)
                {
                    //UnityEngine.Debug.Log("Wallrunning");
                    float3 tmpVelocity = characterBody.RelativeVelocity;
                    float3 verticalVelocity = math.projectsafe(tmpVelocity, characterBody.GroundingUp);

                    float3 offsetAppliedWallNormal = math.normalizesafe(characterComponent.WallNormal + characterBody.GroundingUp * 0.1f);

                    characterControl.MoveVector = math.lengthsq(characterControl.MoveVector) > 0.1f ?
                            (math.dot(characterControl.MoveVector, characterComponent.WallNormal) > 0.707f || characterControl.Jump ? characterControl.MoveVector :
                            math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, offsetAppliedWallNormal) - characterComponent.WallNormal * 0.5f))
                        : -characterComponent.WallNormal;
                    float3 targetVelocity = characterControl.MoveVector * characterComponent.WallRunMaxSpeed;
                    //targetVelocity.y = 0;

                    CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity,
                        characterComponent.WallRunMovementSharpness, deltaTime, characterComponent.WallNormal,//characterBody.GroundingUp,
                        offsetAppliedWallNormal);//StandardGroundMove_Accelerated(ref characterBody.RelativeVelocity,
                                                 //    targetVelocity, characterComponent.WallRunMaxSpeed, deltaTime, characterBody.GroundingUp, offsetAppliedWallNormal, false);

                    CharacterControlUtilities.AccelerateVelocity(ref verticalVelocity,
                        characterComponent.Gravity * characterComponent.GravityMultiplierWhenWallSlide , deltaTime);

                    float3 verticalDirection = math.normalizesafe(verticalVelocity);
                    if (math.dot(verticalDirection, characterComponent.Gravity) > 0 && math.length(verticalVelocity) > characterComponent.MaxWallSlideSpeed)
                    {
                        verticalVelocity = verticalDirection * characterComponent.MaxWallSlideSpeed;
                    }

                    characterBody.RelativeVelocity = MathUtilities.ProjectOnPlane(characterBody.RelativeVelocity, characterBody.GroundingUp)
                        + verticalVelocity; //.y = 0;

                    

                    characterControl.FloatingFuel = characterComponent.MaxClimbFuel;
                    characterComponent.AirJumpedCount = 0;
                    characterComponent.WallSlideTime += deltaTime;

                    if (/*characterControl.LastJumpPressedTime < 10 &&*/ characterControl.Jump)//characterControl.Jump)
                    {
                        //CharacterControlUtilities.StandardJump(ref characterBody, 
                        //   math.normalizesafe(characterComponent.WallNormal / math.max(characterComponent.ContactCount, 1)) * characterComponent.WallJumpSpeed ,
                        //    true, characterComponent.WallNormal //characterBody.GroundingUp
                        //    );
                        Jumping(true, ref characterBody,  characterComponent.WallJumpSpeed / math.max(characterComponent.ContactCount, 1), /*characterControl.MoveVector*/
                           math.lengthsq(characterControl.MoveVector) > 0.1f ?
                           math.normalizesafe(math.reflect(characterControl.MoveVector, characterComponent.WallNormal) + characterBody.GroundingUp) :
                           math.normalizesafe( characterComponent.WallNormal + characterBody.GroundingUp) , characterComponent.WallNormal
                            , characterComponent.AirMaxSpeed);
                        characterControl.Jump = false;
                        characterComponent.LastWallNormal = characterComponent.WallNormal;
                        characterComponent.WallNormal = 
                        characterComponent.WallSlideTime = 0;
                    }
                    characterComponent.ContactCount = 0;
                }
                else
                {
                    float climbSpeed = characterBody.RelativeVelocity.y;
                    // Move in air
                    float verticalAcceleration = characterControl.Floating *
                        (climbSpeed > 0 ? characterComponent.AirVerticalAcceleration : characterComponent.AirVerticalAccelerationWhenDescend);

                    if (climbSpeed < characterComponent.MaxClimbSpeed && characterControl.FloatingFuel > 0)
                    {
                        climbSpeed += verticalAcceleration * deltaTime;
                        characterBody.RelativeVelocity.y = climbSpeed;
                        characterControl.FloatingFuel -= characterControl.Floating * deltaTime;
                    }

                    float3 airAcceleration = characterControl.MoveVector * characterComponent.AirAcceleration;

                    if (math.lengthsq(airAcceleration) > 0f)
                    {
                        float3 tmpVelocity = characterBody.RelativeVelocity;
                        //float3 projectedVelocityOnPlane = MathUtilities.ProjectOnPlane(tmpVelocity, characterBody.GroundingUp);
                        //float currentSpeed = math.length(projectedVelocityOnPlane);
                        //Debug.Log("current speed : "+currentSpeed);
                        //if (currentSpeed > characterComponent.AirMaxSpeed*1.1f && 
                        //    math.dot(math.normalizesafe(tmpVelocity),characterControl.MoveVector)>0)
                        //{
                        //    Debug.Log("Strafing");
                        //    characterBody.RelativeVelocity = currentSpeed * deltaTime * math.normalizesafe(characterControl.MoveVector)
                        //        + characterBody.RelativeVelocity.y;
                        //}
                        //else
                        float3 projectedCurrentDirectionOnPlane = math.normalizesafe(MathUtilities.ProjectOnPlane(tmpVelocity, characterBody.GroundingUp));

                        if (math.dot(projectedCurrentDirectionOnPlane, math.normalizesafe(characterControl.MoveVector)) < 0.3f)
                        {
                            CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime, characterComponent.AirBreak);
                        }
                        //else
                        {
                            CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, airAcceleration, characterComponent.AirMaxSpeed, characterBody.GroundingUp, deltaTime, false);
                        }


                        // Cancel air acceleration from input if we would hit a non-grounded surface (prevents air-climbing slopes at high air accelerations)
                        if (characterComponent.PreventAirAccelerationAgainstUngroundedHits && CharacterAspect.MovementWouldHitNonGroundedObstruction(in this, ref context, ref baseContext, characterBody.RelativeVelocity * deltaTime, out ColliderCastHit hit))
                        {
                            characterBody.RelativeVelocity = tmpVelocity;
                        }
                    }

                    if (characterControl.Jump && characterComponent.AirJump > characterComponent.AirJumpedCount)
                    {
                        Jumping(characterComponent.IgnoreInertiaWhenJump, ref characterBody, characterComponent.JumpSpeed, characterControl.MoveVector
                            , characterBody.GroundingUp, characterComponent.AirMaxSpeed);
                        characterControl.Jump = false;
                        characterComponent.AirJumpedCount++;
                    }

                    // Gravity
                    CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, characterComponent.Gravity, deltaTime);

                    // Drag
                    CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime, characterComponent.AirDrag);
                }

                characterComponent.WallNormal = float3.zero;
            }
        }

        public void VariableUpdate(ref ThirdPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
            ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;
            ref ThirdPersonCharacterControl characterControl = ref CharacterControl.ValueRW;
            ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;

            // Add rotation from parent body to the character rotation
            // (this is for allowing a rotating moving platform to rotate your character as well, and handle interpolation properly)

            if (characterComponent.AlwaysLookForwardOfCamera)
            {
                //KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation, characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);
                //CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, baseContext.Time.DeltaTime, math.normalizesafe(characterControl.CameraForwardVector), MathUtilities.GetUpFromRotation(characterRotation), characterComponent.RotationSharpness);
                characterRotation = quaternion.LookRotationSafe(characterControl.CameraForwardVector, characterBody.GroundingUp);
            }
            else
            {
                KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation, characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);

                // Rotate towards move direction
                if (math.lengthsq(characterControl.MoveVector) > 0f)
                {
                    CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, baseContext.Time.DeltaTime, math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation), characterComponent.RotationSharpness);
                }
            }
        }

        #region Character Processor Callbacks
        public void UpdateGroundingUp(
            ref ThirdPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext)
        {
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;

            CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
        }

        public bool CanCollideWithHit(
            ref ThirdPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit)
        {
            //ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;//ValueRO;

            //if (hit.Normal.y < 0.3f && hit.Normal.y > -0.001f)
            //{
            //    characterComponent.WallNormal = hit.Normal;
            //    //characterComponent.WallContactCount += 1;
            //}

            return PhysicsUtilities.IsCollidable(hit.Material);
        }

        public bool IsGroundedOnHit(
            ref ThirdPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit,
            int groundingEvaluationType)
        {
            //ThirdPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;
            ref ThirdPersonCharacterComponent characterComponent = ref CharacterComponent.ValueRW;//ValueRO;

            if (hit.Normal.y < 0.3f && hit.Normal.y > -0.001f)
            {
                float3 wallNormal = hit.Normal;
                wallNormal.y = 0;
                characterComponent.WallNormal = math.normalizesafe(wallNormal);

            }
            characterComponent.ContactCount += 1;

            return CharacterAspect.Default_IsGroundedOnHit(
                in this,
                ref context,
                ref baseContext,
                in hit,
                in characterComponent.StepAndSlopeHandling,
                groundingEvaluationType);
        }

        public void OnMovementHit(
                ref ThirdPersonCharacterUpdateContext context,
                ref KinematicCharacterUpdateContext baseContext,
                ref KinematicCharacterHit hit,
                ref float3 remainingMovementDirection,
                ref float remainingMovementLength,
                float3 originalVelocityDirection,
                float hitDistance)
        {
            ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
            ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
            ThirdPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;

            CharacterAspect.Default_OnMovementHit(
                in this,
                ref context,
                ref baseContext,
                ref characterBody,
                ref characterPosition,
                ref hit,
                ref remainingMovementDirection,
                ref remainingMovementLength,
                originalVelocityDirection,
                hitDistance,
                characterComponent.StepAndSlopeHandling.StepHandling,
                characterComponent.StepAndSlopeHandling.MaxStepHeight,
                characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);

        }

        public void OverrideDynamicHitMasses(
            ref ThirdPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref PhysicsMass characterMass,
            ref PhysicsMass otherMass,
            BasicHit hit)
        {
            // Custom mass overrides
        }

        public void ProjectVelocityOnHits(
            ref ThirdPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref float3 velocity,
            ref bool characterIsGrounded,
            ref BasicHit characterGroundHit,
            in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
            float3 originalVelocityDirection)
        {
            ThirdPersonCharacterComponent characterComponent = CharacterComponent.ValueRO;

            CharacterAspect.Default_ProjectVelocityOnHits(
                ref velocity,
                ref characterIsGrounded,
                ref characterGroundHit,
                in velocityProjectionHits,
                originalVelocityDirection,
                characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);

        }
        #endregion
    }
}