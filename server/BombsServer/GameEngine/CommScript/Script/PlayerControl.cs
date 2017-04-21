using Humper.Responses;
using UnityEngine;
using Humper;
using System.Collections.Generic;
using google.protobuf;

namespace GameEngine.Script
{
    public class PlayerControl:Entity
    {
        public float speed = 30;

        public void applyInput(int input)
        {
            var velocity = Vector3.zero;
            switch (input)
            {
                case 1:
                    velocity.x += 0.1f;
                    break;
                case 2:
                    velocity.x -= 0.1f;
                    break;
                case 3:
                    velocity.y += 0.1f;
                    break;
                case 4:
                    velocity.y -= 0.1f;
                    break;
            }
            var move = box.Move(box.X + 0.02f * speed * velocity.x, box.Y + 0.02f * speed * velocity.y, (collision) => CollisionResponses.Touch);
        }
    }
}
