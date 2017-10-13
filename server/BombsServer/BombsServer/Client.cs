using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BombsServer
{
    public class Client
    {

        public Dictionary<int,Entity> entities = new Dictionary<int, Entity>();

        public bool key_left;
        public bool key_right;

        public int entity_id;
        //===配置项=====
        public bool client_side_prediction = true;
        public bool server_reconciliation = true;
        public bool entity_interpolation = true;
        //=============
        private int input_sequence_number;
        public List<Message> pending_inputs;

        //==================
        private int last_ts;
        public void Update()
        {
            // Listen to the server.
            this.processServerMessages();

            if (this.entity_id == null)
            {
                return;  // Not connected yet.
            }

            // Process inputs.
            this.processInputs();

            // Interpolate other entities.
            if (this.entity_interpolation)
            {
                this.interpolateEntities();
            }

            // Render the World.
            renderWorld(this.canvas, this.entities);

            // Show some info.
            var info = "Non-acknowledged inputs: " + this.pending_inputs.length;
            this.status.textContent = info;
        }


        public void processInputs()
        {
            Message input=new Message();
            if (this.key_right)
            {
                input = new Message();
                input.press_time = (int)(DateTime.Now.Ticks - last_ts);
            }
            else if (this.key_left)
            {
                input = new Message();
                input.press_time = (int)(DateTime.Now.Ticks - last_ts)*-1;
            }
            else
            {
                // Nothing interesting happened.
                return;
            }

            // Send the input to the server.
            input.input_sequence_number = this.input_sequence_number++;
            input.entity_id = this.entity_id;
            this.server.network.send(this.lag, input);

            // Do client-side prediction.
            if (this.client_side_prediction)
            {
                this.entities[this.entity_id].applyInput(input);
            }

            // Save this input for later reconciliation.
            this.pending_inputs.Add(input);
        }

        public void interpolateEntities()
        {
            // Compute render timestamp.
            //var now = +new Date();
            //var render_timestamp = now - (1000.0 / server.update_rate);
            foreach(var entity in entities.Values)
            {
                // No point in interpolating this client's entity.
                if (entity.entity_id == this.entity_id)
                {
                    continue;
                }

                // Find the two authoritative positions surrounding the rendering timestamp.
                var buffer = entity.position_buffer;

                // Drop older positions.
                while (buffer.length >= 2 && buffer[1][0] <= render_timestamp)
                {
                    buffer.shift();
                }

                // Interpolate between the two surrounding authoritative positions.
                if (buffer.length >= 2 && buffer[0][0] <= render_timestamp && render_timestamp <= buffer[1][0])
                {
                    var x0 = buffer[0][1];
                    var x1 = buffer[1][1];
                    var t0 = buffer[0][0];
                    var t1 = buffer[1][0];

                    entity.x = x0 + (x1 - x0) * (render_timestamp - t0) / (t1 - t0);
                }
            }
        }
    }
}
