using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Script
{
    public class EntityControl : ScriptBase
    {
        public void applyInput(Input input)
        {
            switch ((KeyCode)input.keycode)
            {
                case KeyCode.D:
                    this.entity.x += input.lagMs * this.entity.speed;
                    break;
                case KeyCode.A:
                    this.entity.x -= input.lagMs * this.entity.speed;
                    break;
                case KeyCode.W:
                    this.entity.z += input.lagMs * this.entity.speed;
                    break;
                case KeyCode.S:
                    this.entity.z -= input.lagMs * this.entity.speed;
                    break;
            }
        }

        public  void ShowInfo()
        {
            entity.Show();
        }
    }
}
