using Humper;
using UnityEngine;
using Humper.Responses;
public class TopdownScene: MonoBehaviour
{
    private IBox player1, player2;
    public GameObject p1, p2,ground1;
    protected World World { get; set; }
    public  void Start()
    {
        this.World = new World(1024, 700);

        //this.player1 = CreateObj(6.2f, 6.2f, 2.4f, 2.4f, "player1");
        this.player2 = CreateObj(0, 0, 2f, 2f, "player1",out p2);// this.World.Create(10, 5, 2.4f, 2.4f).AddTags(Tags.Group1);

        CreateObj(0, 2.5f,2f, 2f, "groud1",out ground1);
        //CreateObj(28f, 24f, 20f, 20f, "groud2");
        //CreateObj(23f, 22f, 8f, 40f, "groud3");
        //// Map
        //this.World.Create(10, 10, 15, 2).AddTags(Tags.Group2);
        //this.World.Create(18, 14, 20, 20).AddTags(Tags.Group2);
        //this.World.Create(19, 2, 8, 40).AddTags(Tags.Group2);

        //GameObject g1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //g1.name = "g1";
        //g1.transform.position = new Vector3(10,10,0);
        //g1.transform.localScale = new Vector3(15, 2, 1);
        //GameObject g2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //g2.name = "g2";
        //g2.transform.position = new Vector3(18,14, 0);
        //g2.transform.localScale = new Vector3(20, 20, 1);
        //GameObject g3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //g3.name = "g3";
        //g3.transform.position = new Vector3(19, 2,0);
        //g3.transform.localScale = new Vector3(8, 40, 1);
    }


    public IBox CreateObj(float x,float y,float width,float height,string name,out GameObject obj)
    {
        IBox box = this.World.Create(x, y, width, height).AddTags(Tags.Group1);
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = new Vector3(x, y, 0);
        obj.transform.localScale = new Vector3(width, height, 1);
        return box;
    }

    public void Update()
    {
        //UpdatePlayer(this.player1, p1);
        UpdatePlayer(this.player2, p2);
    }

    private void UpdatePlayer(IBox player,GameObject p)
    {
        var velocity = Vector2.zero;
        if (UnityEngine.Input.GetKey(KeyCode.D))
        {
            velocity.x += 0.1f;
        }
        if (UnityEngine.Input.GetKey(KeyCode.A))
        {
            velocity.x -= 0.1f;
        }
        if (UnityEngine.Input.GetKey(KeyCode.W))
        {
            velocity.y += 0.1f;
        }
        if (UnityEngine.Input.GetKey(KeyCode.S))
        {
            velocity.y -= 0.1f;
        }

        var move = player.Move(player.X + Time.deltaTime*3 * velocity.x, 
            player.Y + Time.deltaTime * 3 * velocity.y, (collision) => CollisionResponses.Touch);
        p.transform.position = new Vector3(player.X, player.Y, 0);
    }
}
public enum Tags
{
    Group1 = 1 << 0,
    Group2 = 1 << 1,
    Group3 = 1 << 2,
    Group4 = 1 << 3,
    Group5 = 1 << 4,
}
