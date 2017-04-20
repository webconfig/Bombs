using Humper;
using UnityEngine;
using Humper.Responses;
using System.Collections.Generic;
public class TopdownScene : MonoBehaviour
{
    private Box player1, player2;
    public GameObject p1, p2, ground1;
    protected World World { get; set; }
    private float cellSize=1;
    public void Start()
    {
        this.World = new World(30, 20, cellSize);

        this.player2 = CreateObj(0, 0, 2f, 2f, "player1", Tags.Group1, out p2);
    }


    public Box CreateObj(float x, float y, float width, float height, string name, Tags tag, out GameObject obj)
    {
        Box box = this.World.Create(x, y, width, height).AddTags(tag);
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.position = new Vector3(x+width/2.0f, 0, y + height / 2.0f);
        obj.transform.localScale = new Vector3(width,1 , height);
        return box;
    }

    public void Update()
    {
        if (this.player2 != null)
        {
            UpdatePlayer(this.player2, p2);
        }
    }

    private void UpdatePlayer(Box player, GameObject p)
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
        p.transform.position = new Vector3(player.X + player.Width / 2.0f, 0, player.Y + +player.Height / 2.0f);
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
      Draw.DrawRect(new Rect(x, y, w, h), Color.green);
    }

    private void DrawBox(Box box)
    {
        var b = box.Bounds;
        Draw.DrawRect(new Rect(b.X, b.Y, b.Width, b.Height), Color.red);
    }
    private void DrawString(string message, float x, float y, float alpha)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(new Vector3(x, 0, y), message);
#endif
    }
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
        line[0] = new Vector3(rect.x, 0, rect.y);
        line[1] = new Vector3(rect.x + rect.width, 0,rect.y);
        line[2] = new Vector3(rect.x + rect.width, 0, rect.y + rect.height);
        line[3] = new Vector3(rect.x, 0, rect.y + rect.height);
        line[4] = new Vector3(rect.x, 0, rect.y);
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
#if UNITY_EDITOR
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.Label(position, text);
#endif
    }

}
