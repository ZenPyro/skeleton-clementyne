using Godot;
using System;

public class Main : Node
{
    [Export]
    public PackedScene PlayerScene {get; set;}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Clementyne <3");
        Player p1 = PlayerScene.Instance<Player>();
        p1.Translation = new Vector3(-51,-4,-3);
        AddChild(p1);
        
        Player p2 = PlayerScene.Instance<Player>();
        p2.Translation = new Vector3(34,-4,0);
        AddChild(p2);

        Player p3 = PlayerScene.Instance<Player>();
        p3.Translation = new Vector3(0,-4,0);
        AddChild(p3);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
