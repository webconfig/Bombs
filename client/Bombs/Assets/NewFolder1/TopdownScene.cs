using Humper;
using UnityEngine;
using Humper.Responses;
using System.Collections.Generic;
public class TopdownScene : MonoBehaviour
{
    private IBox player1, player2;
    public GameObject p1, p2, ground1;
    protected World World { get; set; }
    private float cellSize=1;
    public void Start()
    {
        this.World = new World(10, 10, cellSize);

        //this.player1 = CreateObj(6.2f, 6.2f, 2.4f, 2.4f, "player1");
        this.player2 = CreateObj(5, 5, 2.5f, 2f, "player1", out p2);// this.World.Create(10, 5, 2.4f, 2.4f).AddTags(Tags.Group1);

        //CreateObj(0, 2.5f, 2f, 2f, "groud1", out ground1);
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


    public IBox CreateObj(float x, float y, float width, float height, string name, out GameObject obj)
    {
        IBox box = this.World.Create(x, y, width, height).AddTags(Tags.Group1);
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = new Vector3(x+width/2.0f, y+height/2.0f, 0);
        obj.transform.localScale = new Vector3(width, height, 1);
        return box;
    }

    public void Update()
    {
        //UpdatePlayer(this.player1, p1);
        if (this.player2 != null)
        {
            UpdatePlayer(this.player2, p2);
        }
    }

    private void UpdatePlayer(IBox player, GameObject p)
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

        if (velocity == Vector2.zero) { return; }
        var move = player.Move(player.X + Time.deltaTime * 3 * velocity.x,
            player.Y + Time.deltaTime * 3 * velocity.y, (collision) => CollisionResponses.Touch);
        p.transform.position = new Vector3(player.X + player.Width / 2.0f, player.Y + +player.Height / 2.0f, 0);
    }

    void OnDrawGizmos()
    {
        if (this.World != null)
        {
            var b = this.World.Bounds;
            this.World.DrawDebug((int)b.X, (int)b.Y, (int)b.Width, (int)b.Height, DrawCell, DrawBox, DrawString);
        }
    }
    private void DrawCell(float x, float y, float w, float h, float alpha)
    {
        //spriteBatch.DrawStroke(new Rectangle(x, y, w, h), new Color(Color.White, alpha));
        //if (UnityEngine.Input.GetKey(KeyCode.Space))
        //{
            Draw.DrawRect(new Rect(x, y, w, h), Color.green);
        //}
    }

    private void DrawBox(IBox box)
    {
        //Color color;

        //if (box.HasTag(Tags.Group1))
        //    color = Color.white;
        //else if (box.HasTag(Tags.Group3))
        //    color = Color.red;
        //else if (box.HasTag(Tags.Group4))
        //    color = Color.gray;
        //else if (box.HasTag(Tags.Group5))
        //    color = Color.yellow;
        //else
        //    color = new Color(165, 155, 250);

        var b = box.Bounds;
        Draw.DrawRect(new Rect(b.X, b.Y, b.Width, b.Height), Color.red);
    }
    private void DrawString(string message, float x, float y, float alpha)
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(new Vector3(x, y, 0), message);
        //Gizmos.draw
        //var size = this.font.MeasureString(message);
        //if (Keyboard.GetState().IsKeyDown(Keys.Space))
        //    spriteBatch.DrawString(this.font, message, new Vector2(x - size.X / 2, y - size.Y / 2), new Color(Color.White, alpha));

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
public class Draw
{
    /// <summary>
    /// When called from an OnDrawGizmos() function it will draw a curved path through the provided array of Vector3s.
    /// </summary>
    /// <param name=""path"">
    /// A <see cref=""Vector3s[]"/">
    /// 
    /// <param name=""color"">
    /// A <see cref=""Color"/">
    ///  
    public static void DrawPath(Vector3[] path, Color color)
    {
        if (path.Length > 0)
        {
            DrawPathHelper(path, color, "gizmos");
        }
    }

    /// <summary>
    /// When called from an OnDrawGizmos() function it will draw a line through the provided array of Vector3s.
    /// </summary>
    /// <param name=""line"">
    /// A <see cref=""Vector3s[]"/">
    /// 
    /// <param name=""color"">
    /// A <see cref=""Color"/">
    ///  
    public static void DrawLine(Vector3[] line, Color color)
    {
        if (line != null && line.Length > 0)
        {
            DrawLineHelper(line, color, "gizmos");
        }
    }

    public static void DrawRect(Rect rect, Color color)
    {
        Vector3[] line = new Vector3[5];
        line[0] = new Vector3(rect.x, rect.y, 0);
        line[1] = new Vector3(rect.x + rect.width, rect.y, 0);
        line[2] = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
        line[3] = new Vector3(rect.x, rect.y + rect.height, 0);
        line[4] = new Vector3(rect.x, rect.y, 0);
        if (line != null && line.Length > 0)
        {
            DrawLineHelper(line, color, "gizmos");
        }
    }

    private static void DrawLineHelper(Vector3[] line, Color color, string method)
    {
        Gizmos.color = color;
        for (int i = 0; i < line.Length - 1; i++)
        {
            if (method == "gizmos")
            {
                Gizmos.DrawLine(line[i], line[i + 1]); ;
            }
            else if (method == "handles")
            {
                Debug.LogError("iTween Error: Drawing a line with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
                //UnityEditor.Handles.DrawLine(line[i], line[i+1]);
            }
        }
    }

    private static void DrawPathHelper(Vector3[] path, Color color, string method)
    {
        Vector3[] vector3s = PathControlPointGenerator(path);

        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        Gizmos.color = color;
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            if (method == "gizmos")
            {
                Gizmos.DrawLine(currPt, prevPt);
            }
            else if (method == "handles")
            {
                Debug.LogError("iTween Error: Drawing a path with Handles is temporarily disabled because of compatability issues with Unity 2.6!");
                //UnityEditor.Handles.DrawLine(currPt, prevPt);
            }
            prevPt = currPt;
        }
    }
    /// <summary>
    /// 三点计算抛物线.
    /// </summary>
    /// <returns>组成抛物线的点.</returns>
    /// <param name=""path"">确定抛物线的三个点或者更多的点的数组.
    public static List<Vector3> DrawPathHelper(Vector3[] path)
    {
        List<Vector3> array = new List<Vector3>(177);
        Vector3[] vector3s = PathControlPointGenerator(path);
        //Line Draw:
        Vector3 prevPt = Interp(vector3s, 0);
        int SmoothAmount = path.Length * 20;
        for (int i = 1; i <= SmoothAmount; i++)
        {
            float pm = (float)i / SmoothAmount;
            Vector3 currPt = Interp(vector3s, pm);
            array.Add(currPt);
            prevPt = currPt;
        }
        return array;
    }
    private static Vector3[] PathControlPointGenerator(Vector3[] path)
    {
        Vector3[] suppliedPath;
        Vector3[] vector3s;
        //create and store path points:
        suppliedPath = path;
        //populate calculate path;
        int offset = 2;
        vector3s = new Vector3[suppliedPath.Length + offset];
        System.Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);
        //populate start and end control points:
        //vector3s[0] = vector3s[1] - vector3s[2];
        vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
        vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);
        //is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
        if (vector3s[1] == vector3s[vector3s.Length - 2])
        {
            Vector3[] tmpLoopSpline = new Vector3[vector3s.Length];
            System.Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
            tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
            tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
            vector3s = new Vector3[tmpLoopSpline.Length];
            System.Array.Copy(tmpLoopSpline, vector3s, tmpLoopSpline.Length);
        }
        return (vector3s);
    }
    //andeeee from the Unity forum's steller Catmull-Rom class ( http://forum.unity3d.com/viewtopic.php?p=218400#218400 ):
    private static Vector3 Interp(Vector3[] pts, float t)
    {
        int numSections = pts.Length - 3;
        int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
        float u = t * (float)numSections - (float)currPt;
        Vector3 a = pts[currPt];
        Vector3 b = pts[currPt + 1];
        Vector3 c = pts[currPt + 2];
        Vector3 d = pts[currPt + 3];
        return .5f * (
            (-a + 3f * b - 3f * c + d) * (u * u * u)
            + (2f * a - 5f * b + 4f * c - d) * (u * u)
            + (-a + c) * u
            + 2f * b
            );
    }
}
public static class GizmosExtentions
{
    public static void DrawText(this Gizmos gizmos, Vector3 position, string text)
    {
        DrawText(gizmos, position, text, Color.gray);
    }
    public static void DrawText(this Gizmos gizmos, Vector3 position, string text, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(position, text);
    }

}
