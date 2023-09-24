using Godot;
using System;

public class MediumPlayer : KinematicBody
{
    Spatial pivoteNode;
    //Velocity we'll use to move the `MediumPlayer` instance
    //NOTE: Here the 3D vector `targetVelocity` is a property (instance variable) because ->
    //-> we want to update and reuse its value across frames
    private Vector3 targetVelocity = Vector3.Zero;
    //A `Vector3` instance to point out of the `MediumPlayer` instance to show the path of ->
    //-> its patrol
    private Vector3 patrolVector = Vector3.Forward;
    //`Timer` instance to regulate how often a new random angle is generated for the patroling ->
    //-> `Vector3` instance `patrolVector`
    private Timer patrolTimer;
    private float rotateAngle = 0f;
    int starter = 0;
    String nameWall = "";

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
                        targetVelocity = patrolVector * 10;
                    }
                    else{
                        targetVelocity = patrolVector * 10;
                    }
                }
            }
        }
        else{
            targetVelocity = patrolVector * 10;
        }
        //pivoteNode.LookAt(Translation + targetVelocity, Vector3.Up);
        //GD.Print("1: " + pivoteNode.Rotation);
        //1.) Create a `Quat` instance, storing (caching) the current rotation of the ->
        //-> `MediumPlayer` instance
        Quat qRotation = new Quat(pivoteNode.Rotation);
        //QUESTION:
        //I was confused because how does the `Pivot` instance (node) rotate to the final ->
        //-> wanted rotation but still appear to be smoothly rotating to it in game
        //ANSWER:
        //Rendering is only updated once per frame, at the end of the frame, that means ->
        //-> you can think of everything inside of the `_PhysicsProcess(float delta)` ->
        //-> instance function as being inside a frame, and since rendering is only ->
        //-> updated once per frame, at the end of the frame, that means whatever the ->
        //-> values are at the instance function, are the ones used for rendering
        //SAID IN ANOTHER WAY: Changing the instance variable `Rotation` will not update ->
        //-> the rendered graphic immediately, that will be done later, outisde of our ->
        //-> code, at the end of the frame, so the code (outside of our code)
        //SAID IN AN EVEN BETTER WAY: You might think that the line of code below would ->
        //-> cause the `MediumPlayer` instance to snap immediately, but that is not the case ->
        //-> rendering is only updated once per frame, at the end of the frame, so only the ->
        //-> final value set will actually be read by the rendering engine, in our case, that ->
        //-> would be line 98
        //NEXT: I was confused how the `t` `interpolet` kept progressing the rotation forward ->
        //-> even though it is a static variable inside the function, but now I see that it is ->
        //-> because we re-evaluate the `Rotation` instance variable of the `Pivot` instance ->
        //-> (node) to the new `Rotation` instance variable which is then used on the next pass ->
        //-> of the `_PhysicsProcess(float delta)` instance function as the new starting rotation
        //2.) Use the `LookAt(Vector3 target, Vector3 up)` instance function (from the ->
        //-> `Spatial` Class) to then rotate the `MediumPlayer` instance to look at the ->
        //-> "target final rotation"
        pivoteNode.LookAt(Translation + targetVelocity, Vector3.Up);
        //GD.Print("2: " + pivoteNode.Rotation);
        //3.) Store (cache) this new rotation (the target final rotation) as a new `Quat` instance
        Quat qTargetRot = new Quat(pivoteNode.Rotation);
        //pivoteNode.LookAt(Translation + Vector3.Back, Vector3.Up);
        //pivoteNode.LookAt(Translation + Vector3.Forward, Vector3.Up);
        
        //GD.Print("3: " + pivoteNode.Rotation);
        //4.) Set the `MediumPlayer` instance's `Rotation` instance variable (`Vector3`) to ->
        //-> the result of using the `Quat.Slerp(Quat to, float weight)` instance function ->
        //-> this instance function returns the result of the spherical linear interpolation ->
        //-> between the invoking `Quat` instance (current rotation (`qRotation`)) and the ->
        //-> `to` `Quat` instance (target final rotation (`qTargetRot`)) by a specified ->
        //-> amount using the `weight` argument (in our case `0.2f`)
        pivoteNode.Rotation = qRotation.Slerp(qTargetRot, 0.2f).GetEuler();

        //GD.Print("4: " + pivoteNode.Rotation);
        
        //GD.Print(targetVelocity);
        MoveAndSlide(targetVelocity, Vector3.Up);
        if(patrolTimer.Paused == true){
            patrolTimer.Paused = false;
        }

        //Below is the start to using raycasting off of the `MediumPlayer` instance
        //NOTE: In the physics world, Godot stores all the low level collision and physics ->
        //-> information (data) in a `space`. There are multiple ways of obtaining this ->
        //-> `3D space` but the way I chose is listed step by step below:
        //1.) I use the `GetWorld()` `Spatial` instance function which returns the `World` ->
        //-> instance (`resource`) that this `Spatial` instance (node) (in this case it is ->
        //-> the `MediumPlayer` instance) is registered to (think of this as going up the ->
        //-> hierarchy of parents of this specific instance until you find the "top" where ->
        //-> the data of what `World` this instance belongs to is at)
        //2.) Using this `World` instance (`resource`) we get its instance variable which ->
        //-> is a `PhysicsDirectSpaceState` instance, this instance gives direct access ->
        //-> to the world's physics 3D space state, this is used for querying current and ->
        //-> potential collisions
        //3.) This `PhysicsDirectSpaceState` instance we obtained is now initialized to ->
        //-> our `PhysicsDirectSpaceState` instance called `spaceState`, this will be ->
        //-> used to perform queires into physics space (used for raycasting (shown later))
        //IMPORTANT NOTE: Godot physics run by default in the same thread as game logic ->
        //-> but may be set to run on a seperate thread to work more efficiently. Due ->
        //-> to this, the only time accessing `space` is safe is during the ->
        //-> `Node._PhysicsProcess(float delta)` callback. Accessing it from outside this ->
        //-> instance function may result in an error due to the `space` being `locked`
        PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;

        //Below might be some helpful printouts for the future (most result in the same value):
        // GD.Print("Mob: " + GlobalTransform.origin);
        // GD.Print(pivoteNode.GlobalTransform.origin);
        // GD.Print("Global: " + -pivoteNode.GlobalTransform.basis.z);
        // GD.Print(pivoteNode.Transform.origin + Translation);
        // GD.Print("Global: " + pivoteNode.GlobalTransform.origin);
        // GD.Print("Translation" + pivoteNode.GlobalTranslation);

        //1.) Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(Translation, Translation + Vector3.Forward.Rotated(Vector3.Up, pivoteNode.Rotation.y)*1000, new Godot.Collections.Array(this), CollisionMask);
        //2.) Rotation = pivoteNode.Rotation; ->
        //-> Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(Transform.basis.z, Transform.basis.z*1000, new Godot.Collections.Array(this), CollisionMask);
        //3.) Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(Translation, Translation + -pivoteNode.Transform.basis.z*1000, new Godot.Collections.Array(this), CollisionMask);
        //4.) Below is the 4th way I found to do this:
        //Using the `PhysicsDirectsSpaceState` instance (`spaceState`) I invoke its ->
        //-> instance function `IntersectRay(Vector3 from, Vector3 to)` (has more arguments ->
        //-> but I left them out here) which intersects a ray given in space and returns ->
        //-> a dictionary instance back (`Godot.Collections.Dictionary` instance) ->
        //-> that contains key value pairs for all the information about the collision the ->
        //-> ray had. ->
        //HOW I CAST THE RAY USING THE `IntersectRay()` INSTANCE FUNCTION:
        //1.) I use the `Vector3` instance variable of the `pivoteNode` `Spatial` instance ->
        //-> called `origin` from the `GlobalTransform` instance variable (`Transform`) ->
        //-> this gives me the global "position" `Vector3` instance of the `pivoteNode` ->
        //-> instance, from the `orgin` portion of the `Transform`, this `origin` ->
        //-> is then passed as the `Vector3` `from` argument to say where the raycast ->
        //-> should start from
        //2.) I then use the `pivoteNode` instance to get its `GlobalTransform` instance ->
        //-> variable like before, but this time I use this instance variable to get ->
        //-> the `basis` instance variable (`Basis`) (which is the `basis` portion of ->
        //-> the `Transform`) I then use the `basis` instance variable to get the ->
        //-> `Vector3` `z` instance variable which is a vector point towards where the ->
        //-> `pivoteNode` instance is pointing (the `z` vector is rotated toward that direction ->
        //-> around the `y` axis), I then make it negative so that it points "forward" ->
        //-> instead of "backward", then I multiply it by 10 to give the vector some magnitude ->
        //-> I then add the same `Vector3` from the last argument to it, to translate ->
        //-> the raycast vector to project out of the actual `pivoteNode` instance and not ->
        //-> `(0,0,0)` of the `world`s `origin`, this `Vector3` instance is then used ->
        //-> as the `Vector3` `to` argument to say where the raycast end point is
        //READ IMPORTANT NOTE BELOW FOR STEP 2.)
        //3.) The next argument passed is used to tell the raycast vector what instances ->
        //-> to ignore, this is important because the `HardPlayer` instance's own ->
        //-> `CollisionShape` instance will not be ignored by default, and would cause the ->
        //-> raycast vector to collide immediately and not go anywhere, we fix this by ->
        //-> passing a new `Godot.Collections.Array` array instance that is full of all ->
        //-> instances that are exceptions (this means the raycast vector will not ->
        //-> collide with these instance), is this case we initialize this new `Array` instance ->
        //-> to just hold the `HardPlayer` instance itself (passing the `this` operator to the ->
        //-> `Godot.Collections.Array` constructor)
        //4.) The final argument we pass is what collision layers the raycast vector ->
        //-> should scan, in this case pass the `HardPlayer` instance's own `CollisionMask` ->
        //-> instance variable which basically tells the raycast vector `CollisionObject` ->
        //-> instance to only scan the same layers as the `HardPlayer` instance scans
        //5.) We store this new `Godot.Collections.Dictionary` instance in a ->
        //-> new `Godot.Collections.Dictionary` instance called `dictRayInter` to be used ->
        //-> to interact with later
        //IMPORTANT NOTE: If you are doing it this 4th way, you must realize that ->
        //-> although the `pivoteNode.GlobalTransform.basis.z` instance variable (`Vector3` ->
        //-> instance) is global, it still needs to have the ->
        //-> `pivoteNode.GlobalTransform.orgin` instance variable (`Vector3` instance) ->
        //-> added to it, so that this "new" vector (raycast) we are casting (projecting) ->
        //-> comes off from the `origin` of the actual `MediumPlayer` instance ->
        //-> (that being wherever the `MediumPlayer` instance position is) and not ->
        //-> the `space` or `world`'s `origin` (that being `(0,0,0)`)
        Godot.Collections.Dictionary dictRayInter = spaceState.IntersectRay(pivoteNode.GlobalTransform.origin, pivoteNode.GlobalTransform.origin + (-pivoteNode.GlobalTransform.basis.z*10), new Godot.Collections.Array(this), CollisionMask);
        //EASIER WAY TO EXPLAIN LINE OF CODE ABOVE:
        //We use the `IntersectRay` instance function (of the `PhysicsDirectSpaceState` ->
        //-> Class) to create and cast a ray, by giving a start, end, collisions exceptions ->
        //-> (`CollisionObject` instances to ignore), and what the rays `CollisionMask` is ->
        //-> (to tell the ray what physics layers to scan)
        
        //NOTE: If you manipulate the `Transform` instance variable you must use the ->
        //-> `Orthonormalized()` instance function of the `Transform` Class ->
        //-> to stop the deformation of the `Transform` instance over time (from ->
        //-> manipulating it every frame)
        pivoteNode.Transform.Orthonormalized();

        if(dictRayInter.Count != 0){
            if(dictRayInter["collider"] is StaticBody wall){
                if(nameWall != wall.Name){
                    nameWall = wall.Name;
                    GD.Print("HIT SOMETHING: " + wall.Name + " " + dictRayInter["position"]);
                }
            }
            else if(dictRayInter["collider"] is Player mob){
                GD.Print("HIT SOMETHING: " + mob.Name + " " + dictRayInter["position"]);
                mob.QueueFree();
            }
        }
    }

    public void _on_patrolTimer_timeout(){
        rotateAngle = (float) GD.RandRange(-Mathf.Pi/3, Mathf.Pi/3);
        patrolVector = patrolVector.Rotated(Vector3.Up, rotateAngle);
        //GD.Print(patrolVector);
    }
}
