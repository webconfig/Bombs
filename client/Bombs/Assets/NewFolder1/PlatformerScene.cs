using Humper;
using UnityEngine;
using Humper.Responses;
using System.Collections.Generic;
using System.Linq;
public class PlatformerScene : MonoBehaviour
{
    //public class Crate
    //{
    //    public Crate(Box box)
    //    {
    //        this.box = box.AddTags(Tags.Group5);
    //        this.box.Data = this;
    //    }

    //    private Box box;

    //    public Vector2 velocity;

    //    private bool inWater;

    //    public void Update(float delta)
    //    {
    //        velocity.y += delta * 0.001f;


    //        if (inWater)
    //            velocity.y *= 0.5f;

    //        var move = box.Move(box.X + delta * velocity.x, box.Y + delta * velocity.y, (collision) =>
    //        {
    //            if (collision.Other.HasTag(Tags.Group3))
    //            {
    //                return CollisionResponses.Cross;
    //            }

    //            return CollisionResponses.Slide;
    //        });

    //        inWater = (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)));


    //        velocity.x *= 0.85f;

    //        // Testing if on ground
    //        if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2) && (c.Normal.Y < 0)))
    //        {
    //            velocity.y = 0;
    //            velocity.x *= 0.85f;
    //        }

    //    }
    //}
    private Box player1, platform;
    public GameObject p1, pf;
    private Crate[] crates;
    protected World World { get; set; }
    private Vector2 platformVelocity = Vector2.one * 0.05f;

    public void Start()
    {
        this.World = new World(1024, 700);

        this.player1 = CreateObj(50, 430, 10, 24, "player1", Tags.Group1, out p1);
        this.platform = CreateObj(0, 400, 100, 20, "platform", Tags.Group4, out pf);

        GameObject p,c1p,c2p;
        Box c1 = CreateObj(150, 360, 40, 40, "c1", Tags.Group5, out c1p);
        Box c2 = CreateObj(210, 360, 40, 40, "c2", Tags.Group5, out c2p);
        this.crates = new[]
        {
                new Crate(c1,c1p),
                new Crate(c2,c2p),
        };

        // Map
        CreateObj(0, 300, 400, 20,"map1", Tags.Group2,out p);
        CreateObj(380, 220, 20, 80, "map2", Tags.Group2, out p);
        CreateObj(380, 200, 300, 20, "map3", Tags.Group2, out p);
        CreateObj(420, 400, 200, 20, "map4", Tags.Group2, out p);
        CreateObj(680, 200, 20, 200, "map5", Tags.Group2, out p);
        CreateObj(680, 400, 200, 20, "map6", Tags.Group2, out p);
        CreateObj(400, 220, 280, 100, "map7", Tags.Group2, out p);
    }


    private Vector2 velocity = Vector2.zero;
    private float timeInRed;
    private bool onPlatform;
    public void Update()
    {

        UpdatePlatform(this.platform, Time.deltaTime * 1000);

        foreach (var crate in crates)
        {
            crate.Update(Time.deltaTime * 1000);
        }

        UpdatePlayer(this.player1, Time.deltaTime * 1000);
    }

    private void UpdatePlatform(Box platform, float delta)
    {
        if ((platform.X < 50 && platformVelocity.x < 0) || (platform.X > 300 && platformVelocity.x > 0))
        {
            this.platformVelocity.x *= -1;
        }

        platform.Move(platform.X + this.platformVelocity.x * delta, platform.Y, (collistion) => CollisionResponses.None);
        pf.transform.position = new Vector3(platform.X + platform.Width / 2.0f, platform.Y + +platform.Height / 2.0f, 0);
    }

    private void UpdatePlayer(Box player, float delta)
    {
        velocity.y -=delta * 0.001f;
        velocity.x = 0;

        if (UnityEngine.Input.GetKey(KeyCode.D))
            velocity.x += 0.1f;
        if (UnityEngine.Input.GetKey(KeyCode.A))
            velocity.x -= 0.1f;
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            velocity.y += 0.5f;

        if (onPlatform)
            velocity += platformVelocity;

        if (timeInRed > 0)
            velocity.y *= 0.75f;

        // Moving player
        var move = player.Move(player.X + delta * velocity.x, player.Y + delta * velocity.y, (collision) =>
        {
            if (collision.Other.HasTag(Tags.Group3))
            {
                return CollisionResponses.Cross;
            }

            return CollisionResponses.Slide;
        });

        // Testing if on moving platform
        onPlatform = move.Hits.Any((c) => c.Box.HasTag(Tags.Group4));


        // Testing if on ground
        for (int i = 0; i < move.Hits.Count; i++)
        {
            Hit c = move.Hits[i];
            if(c.Box.HasTag(Tags.Group4, Tags.Group2, Tags.Group5) && (c.Normal.Y < 0))
            {
                velocity.y = 0;
                break;
            }
        }
        //if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group4, Tags.Group2, Tags.Group5) && (c.Normal.Y < 0)))
        //{
        //    velocity.y = 0;
        //}

        var pushedCrateCollision = move.Hits.FirstOrDefault((c) => c.Box.HasTag(Tags.Group5));
        if (pushedCrateCollision != null)
        {
            var pushedCrate = pushedCrateCollision.Box.Data as Crate;
            var n = pushedCrateCollision.Normal;
            pushedCrate.velocity = new Vector2(n.X * n.X* velocity.x, n.Y * n.Y* velocity.y);
        }

        // Testing if in red water
        if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)))
        {
            timeInRed += delta;
            if (timeInRed > 3000)
            {
                Debug.Log("Game Oveer");
            }
        }
        else
        {
            timeInRed = 0;
        }

        player.Data = velocity;
        p1.transform.position = new Vector3(player.X + player.Width / 2.0f, player.Y + +player.Height / 2.0f, 0);
    }


    public Box CreateObj(float x, float y, float width, float height, string name, Tags tag, out GameObject obj)
    {
        Box box = this.World.Create(x, y, width, height).AddTags(tag);
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = new Vector3(x + width / 2.0f, y + height / 2.0f, 0);
        obj.transform.localScale = new Vector3(width, height, 1);
        obj.transform.SetParent(transform);
        return box;
    }
}
public class Crate
{
    public GameObject _obj;

    public Crate(Box box,GameObject obj)
    {
        _obj = obj;
        this.box = box;
        this.box.Data = this;
    }

    private Box box;

    public Vector2 velocity;

    private bool inWater;

    public void Update(float delta)
    {
        velocity.y -= delta * 0.001f;


        if (inWater)
            velocity.y *= 0.5f;

        var move = box.Move(box.X + delta * velocity.x, box.Y + delta * velocity.y, (collision) =>
        {
            if (collision.Other.HasTag(Tags.Group3))
            {
                return CollisionResponses.Cross;
            }

            return CollisionResponses.Slide;
        });

        inWater = (move.Hits.Any((c) => c.Box.HasTag(Tags.Group3)));


        velocity.x *= 0.85f;

        // Testing if on ground
        if (move.Hits.Any((c) => c.Box.HasTag(Tags.Group2) && (c.Normal.Y < 0)))
        {
            velocity.y = 0;
            velocity.x *= 0.85f;
        }
        _obj.transform.position = new Vector3(box.X + box.Width / 2.0f, box.Y + +box.Height / 2.0f, 0);
    }
}

