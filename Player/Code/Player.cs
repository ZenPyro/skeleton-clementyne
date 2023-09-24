using Godot;
using System;

public class Player : KinematicBody
{
    //Velocity we'll use to move the `Player` instance
    //NOTE: Here the 3D vector `targetVelocity` is a property (instance variable) because ->
    //-> we want to update and reuse its value across frames
    Vector3 targetVelocity = Vector3.Zero;

    Spatial pivotNode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        pivotNode = GetNode<Spatial>("Pivot");

        //Use the below line of code if you are not instancing multiple `Player` instances ->
        //-> as children of the `Main` instance (Scene)
        //pivotNode = GetTree().Root.GetChild(0).GetChild(2).GetChild<Spatial>(0);
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public override void _PhysicsProcess(float delta){
        //Iterate through all collisions that happened this frame
        //NOTE: This will always run because the `Player` instance is always colliding with ->
        //-> the `Ground` `StaticBody` instance (node)
        for(int i = 0; i < GetSlideCount(); i++){
            KinematicCollision collision = GetSlideCollision(i);

            //IMPORTANT NOTE: Just for testing purposes I am using the hardcoded `ColliderId` ->
            //-> instance variable for the `KinematicCollision` instance variable `collision` ->
            //-> this is because it was the only way I could find to differentiate between the ->
            //-> "Wall" `StaticBody` instances (nodes) and the "Ground" `StaticBody` instance ->
            //-> (node)
            //if(collision.ColliderId != 1290){
            if(collision.Collider is StaticBody wall){
                if(wall.Name == "WallN" || wall.Name == "WallS" || wall.Name == "WallW" || wall.Name == "WallE"){
                    //If the collision is a "Wall" `StaticBody` instance (node), then we want ->
                    //-> the to use the `Pivot` instance (node) of the `Spatial` Class and use ->
                    //-> its instance function `LookAt()` which rotates the instance (node) ->
                    //-> towards a specified `Vector3` instance, in this case this `Vector3` ->
                    //-> instance is the `Normal` instance variable `Vector3` from the ->
                    //-> `collision` instance we collided with (think of this a ->
                    //-> `Vector3` instance pointing straight back at the `Player` instance) ->
                    //-> rotated around the `y` axis by `Mathf.pi/2` radians, now it is ->
                    //-> important to note that this is not enough because a normal is just ->
                    //-> a unit vector, so its axes are only of magnitude `0` or `1` which ->
                    //-> if we only used, would be pointing our `Player` instance in the ->
                    //-> wrong direction (this can be thought of as a problem because the ->
                    //-> `Normal` instance variable is local to its parent `collision` ->
                    //-> `StaticBody` instance (node)) so we have to add the `Translation` ->
                    //-> instance variable (`Vector3` instance) of our `Player` instance ->
                    //-> to the `Normal` instance variable which makes the vector point to ->
                    //-> the "right" from our `Player` instance
                    //GD.Print(wall.Name);
                    pivotNode.LookAt(Translation + collision.Normal.Rotated(Vector3.Up, Mathf.Pi/2), Vector3.Up);
                    //pivotNode.Rotate(Vector3.Up, Mathf.Pi/2);
                    
                    //Above we made the model turn based on which "Wall" `StaticBody` ->
                    //-> instance (node) the `Player` instance collided with, now we have ->
                    //-> to make the `Player` instance turn as well, this is done in a ->
                    //-> similar fashion to above, we take the `Normal` instance variable ->
                    //-> `Vector3` from the `collision` instance we collided with (Remember ->
                    //-> it is a unit vector point back towards the `Player` instance) and ->
                    //-> we rotated it by `Mathf.Pi/2` which seems like it would go left but ->
                    //-> since the `Normal` `Vector3` instance variable is pointing toward ->
                    //-> the `Player` instance, this actually points the vector towards the ->
                    //-> right, we then multiply it by `10` to give it the correct magnitude ->
                    //-> (velocity) and then put this value into the `targetVelocity` `Vector3` ->
                    //-> instance variable for the `Player` instance so it can be used in the ->
                    //-> calculation for the `MoveAndSlide` instance function of the ->
                    //-> `KinematicBody` Class
                    targetVelocity = collision.Normal.Rotated(Vector3.Up, Mathf.Pi/2) * 10;
                }
                //If the `Player` instance is not colliding with a "Wall", just move forward
                else{
                    targetVelocity = Vector3.Forward * 10;
                }
            }
        }

        MoveAndSlide(targetVelocity, Vector3.Up);
    }
}
