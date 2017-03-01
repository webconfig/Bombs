using System;
using System.Collections.Generic;
using System.Reflection;
public static class Op
{
    public static class Client
    {
        public const int Login = 0x10000001;
        public const int CreateRoom = 0x20000001;
        public const int QueryRoom = 0x20000002;
        public const int JoinRoom = 0x20000003;
        public const int LeaveRoom = 0x20000004;
    }


    public static string GetName(int op)
    {
        foreach (var field in typeof(Op).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if ((int)field.GetValue(null) == op)
                return field.Name;
        }
        return "?";
    }
}
