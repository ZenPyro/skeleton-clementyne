using Godot;
using System;

public class HardPlayer : KinematicBody
{
    Spatial pivoteNode;
    //Velocity we'll use to move the `HardPlayer` instance
    //NOTE: Here the 3D vector `targetVelocity` is a property (instance variable) because ->
    //-> we want to update and reuse its value across frames
    private Vector3 targetVelocity = Vector3.Zero;
    //A `Vector3` instance to point out of the `HardPlayer` instance to show the path of ->
    //-> its patrol
    private Vector3 patrolVector = Vector3.Forward;
    //`Timer` instance to regulate how often a new random angle is generated for the patroling ->
    //-> `Vector3` instance `patrolVector`
    private Timer patrolTimer;
    private float rotateAngle = 0f;
    int starter = 0;
    String nameWall = "";
    bool hasMob = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {   
        pivoteNode = GetNode<Spatial>("Pivot");
        patrolTimer = GetNode<Timer>("patrolTimer");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
         
    //  }

    public override void _PhysicsProcess(float delta)
    {   
        if(GetSlideCount() != 0){
            for(int i = 0; i < GetSlideCount(); i++){
                KinematicCollision collision = GetSlideCollision(i);

                if(collision.Collider is StaticBody wall){
                    if(wall.Name == "WallN" || wall.Name == "WallS" || wall.Name == "WallE" || wall.Name == "WallW"){
                        patrolTimer.Paused = true;
                        patrolVector = -patrolVector;
                        targetVelocity = patrolVector * 5;
                    }
                    else if(hasMob == false){
                        targetVelocity = patrolVector * 5;
                    }
                }
            }
        }
        else if(hasMob == false){
            targetVelocity = patrolVector * 5;
            Quat qRotation = new Quat(pivoteNode.Rotation);
            pivoteNode.LookAt(Translation + targetVelocity, Vector3.Up);
            Quat qTargetRot = new Quat(pivoteNode.Rotation);

            pivoteNode.Rotation = qRotation.Slerp(qTargetRot, 0.2f).GetEuler();
        }
        
        

        
        PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;


        //Godot.Collections.Dictionary dictRayInter2 = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, (pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20)).Rotated(Vector3.Up, Mathf.Pi/3), new Godot.Collections.Array(this), CollisionMask);
        //Godot.Collections.Dictionary dictRayInter3 = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, (pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20)).Rotated(Vector3.Up, -Mathf.Pi/3), new Godot.Collections.Array(this), CollisionMask);
        
        Godot.Collections.Dictionary dictRayInterHit = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*5), new Godot.Collections.Array(this), CollisionMask);
        
        //Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20), new Godot.Collections.Array(this), CollisionMask);
        //NOTE: If you manipulate the `Transform` instance variable you must use the ->
        //-> `Orthonormalized()` instance function of the `Transform` Class ->
        //-> to stop the deformation of the `Transform` instance over time (from ->
        //-> manipulating it every frame)
        pivoteNode.Transform.Orthonormalized();

        Godot.Collections.Array hitArray = new Godot.Collections.Array(pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20), (pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20)).Rotated(Vector3.Up, Mathf.Pi/3), (pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*20)).Rotated(Vector3.Up, -Mathf.Pi/3));
        for(int i = 0; i < hitArray.Count; i++){
            Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, (Vector3) hitArray[i], new Godot.Collections.Array(this), CollisionMask);

            if(dictRayInter.Count != 0){
                if(dictRayInter["collider"] is MediumPlayer mob){
                    hasMob = true;
                    patrolTimer.Paused = true;
                    GD.Print("FOLLOW SOMETHING: " + mob.Name + " " + dictRayInter["position"]);

                    //Below commented out line of code points the `HardPlayer` instance ->
                    //-> in the direction of which of the three raycast `Vector3` ->
                    //-> instances hit the `MediumPlayer` instance (`collider`)
                    //targetVelocity = ((Vector3) dictRayInter["position"] - Translation).Normalized()*7;
                    targetVelocity = (mob.Translation - Translation).Normalized()*7;
                    //pivoteNode.LookAt((Vector3) dictRayInter["position"], Vector3.Up);
                    //GD.Print("Ray: " + i);
                    break;
                }
            }
            // //This `else-statement` below will never run in its position
            // else{
            //     hasMob = false;
            // }
        }
        //IMPORTANT NOTE: The reason all of the rotation processes had to be outside of the ->
        //-> `for-loop` is because interpolation would cause the raycast `Vector3` instances ->
        //-> to rotate slightly (like interpolation does) on the first iteration but this ->
        //-> slight rotation would make the raycasts lose sight of the `MediumPlayer` instance ->
        //-> inbetween them, causing the subsequent rotation (interpolation) iterations not to ->
        //-> run because they are in the `for-loop` which require the raycasts to have sight ->
        //-> of the `MediumPlayer` instance to run the `if-statement` where the ->
        //-> rotation interpolation is inside of and since there is no sight of the ->
        //-> `MediumPlayer` instance the interpolation never continues and is at a stand ->
        //-> still
        //TO FIX THIS: Put all of the rotation interpolation code outside of the `for-loop` ->
        //-> so even when the rotation interpolation causes the raycasts `Vector3` instances ->
        //-> to lose sight of the `MediumPlayer` instance, the rotation interpolation will ->
        //-> still run below because the `hasMob` instance variable `boolean` is true ->
        //-> and so the rotation interpolation will continue to run until the raycasts regain ->
        //-> sight of the `MediumPlayer` instance, this can be seen in the `Print()` function ->
        //-> statements below which show theat even when a current raycast `Vector3` instance ->
        //-> is not printing out that it sees a `MediumPlayer` instance, the below ->
        //-> rotation interpolation print statements still run until the `HardPlayer` instance ->
        //-> is fully rotated (causing more of the below print statements to run and less ->
        //-> of the above print statements to run)
        //NOTE: Because the rotation interpolation is not using the `targetVelocity` instance ->
        //-> variable assignment from inside the `for-loop` the `HardPlayer` instance will ->
        //-> be moving with a velocity equal to the `targetVelocity` that was calculated ->
        //-> last time the `MediumPlayer` instance was collided with by the raycast `Vector3` ->
        //-> instances that means the `HardPlayer` instance will not be moving towards the ->
        //-> `MediumPlayer` instance directly until the end of the rotation interpolation ->
        //-> iterations where the raycasts will collide with the `MediumPlayer` instance again ->
        //-> and start to calculate `targetVelocity` in the direction of the `MediumPlayer` ->
        //-> instance again
        if(hasMob == true){
            //GD.Print("Rotation Start: " + pivoteNode.Rotation);
            Quat qRotationSighted = new Quat(pivoteNode.Rotation);
            pivoteNode.LookAt(Translation + targetVelocity, Vector3.Up);
            Quat qTargetRotSighted = new Quat(pivoteNode.Rotation);
            pivoteNode.Rotation = qRotationSighted.Slerp(qTargetRotSighted, 0.2f).GetEuler();
            //GD.Print("Rotation End: " + pivoteNode.Rotation);
        }
        
        if(dictRayInterHit.Count != 0){
            if(dictRayInterHit["collider"] is MediumPlayer mob4){
                mob4.QueueFree();
                hasMob = false;
            }
        }
        
        //BELOW IS THE OLD-WAY I STARTED THE RAYCASTING (Kept to show my previous thinking):
        // if(dictRayInter.Count != 0){
        //     if(dictRayInter["collider"] is MediumPlayer mob){
        //         GD.Print("FOLLOW SOMETHING: " + mob.Name + " " + dictRayInter["position"]);
                
        //         //pivoteNode.Rotation = qRotationSighted.Slerp(qTargetRotSighted, 0.4f).GetEuler();
        //         targetVelocity = ((Vector3) dictRayInter["position"] - Translation).Normalized()*7;
        //         //targetVelocity = (mob.Translation - Translation).Normalized() * 7;
        //         //targetVelocity = new Vector3(Transform.basis.x.x, Transform.basis.y.y, (mob.Transform.origin.z - Transform.origin.z)*7);
        //         //Quat qRotationSighted = new Quat(pivoteNode.Rotation);
        //         Quat qRotationSighted = new Quat(pivoteNode.Rotation);
        //         pivoteNode.LookAt((Vector3) dictRayInter["position"], Vector3.Up);
        //         Quat qTargetRotSighted = new Quat(pivoteNode.Rotation);
        //         pivoteNode.Rotation = qRotationSighted.Slerp(qTargetRotSighted, 0.2f).GetEuler();
        //         //Quat qTargetRotSighted = new Quat(pivoteNode.Rotation);
        //         //Rotate(Vector3.Up, pivoteNode.Rotation.y);
        //         hasMob = true;
        //         patrolTimer.Paused = true;
        //     }
            // else if(dictRayInter2["collider"] is MediumPlayer mob2){
            //     GD.Print("FOLLOW SOMETHING 2: " + mob2.Name + " " + dictRayInter2["position"]);
                
            //     targetVelocity = ((Vector3) dictRayInter2["position"] - Translation).Normalized()*7;
            //     //Quat qRotationSighted = new Quat(pivoteNode.Rotation);
            //     pivoteNode.LookAt((Vector3) dictRayInter2["position"], Vector3.Up);
            //     //Quat qTargetRotSighted = new Quat(pivoteNode.Rotation);
            //     //pivoteNode.Rotation = qRotationSighted.Slerp(qTargetRotSighted, 0.2f).GetEuler();

            //     hasMob = true;
            //     patrolTimer.Paused = true;
            // }
            // else if(dictRayInter3["collider"] is MediumPlayer mob3){
            //     GD.Print("FOLLOW SOMETHING 3: " + mob3.Name + " " + dictRayInter3["position"]);
                
            //     targetVelocity = ((Vector3) dictRayInter3["position"] - Translation).Normalized()*10;
            //     //Quat qRotationSighted = new Quat(pivoteNode.Rotation);
            //     pivoteNode.LookAt((Vector3) dictRayInter3["position"], Vector3.Up);
            //     //Quat qTargetRotSighted = new Quat(pivoteNode.Rotation);
            //     //pivoteNode.Rotation = qRotationSighted.Slerp(qTargetRotSighted, 0.2f).GetEuler();

            //     hasMob = true;
            //     patrolTimer.Paused = true;
            // }
        

        MoveAndSlide(targetVelocity, Vector3.Up);
        if(patrolTimer.Paused == true && hasMob == false){
            patrolTimer.Paused = false;
        }
    }

    public void _on_patrolTimer_timeout(){
        rotateAngle = (float) GD.RandRange(-Mathf.Pi/3, Mathf.Pi/3);
        patrolVector = patrolVector.Rotated(Vector3.Up, rotateAngle);
    }
}
