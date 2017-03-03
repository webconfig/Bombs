﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Comm.Network.Iocp
{
    // 与每个客户Socket相关联，进行Send和Receive投递时所需要的参数
    public sealed class Pools<T> where T : class
    {
        // just for assigning an ID so we can watch our objects while testing.
        private Int32 nextTokenId = 0;
        Stack<T> pool;

        // The number of SocketAsyncEventArgs instances in the pool.         
        internal Int32 Count
        {
            get { return this.pool.Count; }
        }

        internal Pools(Int32 capacity)
        {
            this.pool = new Stack<T>(capacity);
        }

        internal Int32 AssignTokenId()
        {
            Int32 tokenId = Interlocked.Increment(ref nextTokenId);
            return tokenId;
        }

        // Removes a SocketAsyncEventArgs instance from the pool.
        // returns SocketAsyncEventArgs removed from the pool.
        internal T Pop()
        {
            //Console.WriteLine("SocketAsyncEventArgsPool:pop");
            lock (this.pool)
            {
                try
                {
                    T e = this.pool.Count > 0 ? this.pool.Pop() : null;
                    Console.WriteLine("EventArgs is poped {0}", e.GetHashCode());
                    return e;
                }
                catch
                {
                    return null;
                }
            }
        }

        // Add a SocketAsyncEventArg instance to the pool. 
        // "item" = SocketAsyncEventArgs instance to add to the pool.
        internal void Push(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null");
            }
            lock (this.pool)
            {
                //Console.WriteLine("SocketAsyncEventArgsPool:push");
                this.pool.Push(item);
            }
        }
    }
}