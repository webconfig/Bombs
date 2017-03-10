using System.Collections.Generic;

namespace GameEngine.Demo
{
    public class Message
    {
        public int seqNum;
    }
    public class Input : Message
    {
        public float pressTime;
        public int entityId;
        public Input(int seqNum, float pressTime, int entityId)
        {
            this.seqNum = seqNum;
            this.pressTime = pressTime;
            this.entityId = entityId;
        }
    }
    public class Entity
    {
        public int id;
        //color: string;
        public float x = 0;
        public float speed = 2;

        public Entity(int id)
        {
            this.id = id;
        }

        public void applyInput(Input input)
        {
            this.x += input.pressTime * this.speed;
        }

        /** Return a copy of this entity. */
        public Entity copy()
        {
            Entity e = new Entity(this.id);
            e.x = this.x;
            e.speed = this.speed;
            return e;
        }
    }

    public class WorldState : Message
    {
        public List<Entity> entities;

        /** Last input the server has processed from the client to which the
         * WorldState message is sent. */
       public List<int> lastProcessedInputSeqNums;

        public WorldState(int seqNum, List<Entity> entities, List<int> lastProcessedInputSeqNums)
        {
            this.seqNum = seqNum;
            this.entities = entities;
            this.lastProcessedInputSeqNums = lastProcessedInputSeqNums;
        }
    }

    public class SavedWorldState
    {
        /**
         * The time at which the client processed this WorldState message.
         */
        public int processedTs;
        public WorldState value;
        public SavedWorldState(int processedTs, WorldState value)
        {
            this.processedTs = processedTs;
            this.value = value;
        }
    }

    public class QueuedMessage
    {
        public int recvTs;
        public Message payload;
    }

    public class LagNetwork
    {
        public List<QueuedMessage> messages;

        /** Returns next message "received" from the network, if any. */
        public QueuedMessage receive()
        {
            //let now = Date.now();
            //for (int i = 0; i < this.messages.Count; ++i)
            //{
            //    var qm = this.messages[i];
            //    if (qm.recvTs <= now)
            //    {
            //        //this.messages.splice(i, 1);
            //        return qm;
            //    }
            //}
            return null;
        }

        public void send(int lagMs, Message message)
        {
            //var m = new QueuedMessage();
            //m.recvTs = Date.now() + lagMs;
            //m.payload = message;
            //this.messages.Add(m);
        }
    }

    public class Client
    {
        public string cssId; // id of the div containing this client (kind of a hack)
        //color: string;
        //canvas: HTMLCanvasElement;
        //nonAckdInputsElement: Element;
        public Server server;
        public int tickRate = 60;
        // Why is there `entityId` and also `entity.id`?  Good question.
        // `entityId` is assigned by the server when the connection is made.
        // It is later used to retreive state data for this client from
        // WorldState messages.
        public int entityId;  // TODO: extremely inconvenient allowing this to have undefined value
        public Entity entity; // The player's entity in the world; server provides it.
        public List<Entity> entities; // awful, contains reference to this.entity as well
        public bool leftKeyDown = false;
        public bool rightKeyDown = false;
        public LagNetwork network;
        public int lagMs = 250;
        public int lastUpdateTs = -1;
        public int inputSeqNum = 0;
        public List<Input> pendingInputs;
        public SavedWorldState curWorldState;  // the last state we received from the server
        public SavedWorldState prevWorldState; // penultimate state from server, used for entity interpolation
        public bool usePrediction = false;
        public bool useReconciliation = false;
        public bool useEntityInterpolation = false;
        private int updateTimer;

        public Client()
        {
            //this.cssId = cssId;
            //this.color = color;
            //this.canvas = canvas;
            //this.nonAckdInputsElement = nonAckdInputsElement;
        }

        public void processServerMessages()
        {
            while (true)
            {
                QueuedMessage msg = this.network.receive();
                if (msg == null) break;
                WorldState incoming = msg.payload as WorldState;

                for (int i = 0; i < incoming.entities.Count; ++i)
                {
                    var entity = incoming.entities[i];

                    //if (this.entityId === undefined) break; // pointless, but tsc unhappy without this

                    if (entity.id == this.entityId)
                    {
                        // entity is the remote state for our local this.entity object

                        // create an entity for ourself if we haven't yet
                        //if (!this.entity) {
                        //    if (typeof this.entityId === 'undefined') {
                        //        throw new Error(`connected client should always have entityId ${ this}`);
                        //    }
                        //    this.entity = new Entity(this.entityId, this.color);
                        //}

                        // Set our position to whatever was sent by server
                        this.entity.x = entity.x;

                        this.entities[entity.id] = this.entity; // despair

                        if (this.useReconciliation)
                        {
                            // i.e. reapply all inputs not yet ackd by server

                            var lastProcessed = incoming.lastProcessedInputSeqNums[this.entityId];
                            if (lastProcessed > 0)
                            {
                                // First, keep inputs that have not yet been taken
                                // into account by the last WorldState sent by the
                                // server.
                                //this.pendingInputs = this.pendingInputs.filter(input => {
                                //    return input.seqNum > lastProcessed;
                                //});
                            }

                            // apply any remaining inputs to our local world state
                            //this.pendingInputs.forEach(input => {
                            //    if (this.entity)
                            //    {
                            //        this.entity.applyInput(input);
                            //    }
                            //});
                        }

                    }
                    else
                    {
                        // non-local-player entity
                        this.entities[entity.id] = entity;
                    }
                }
                // update prev and current states for later entity interpolation
                this.prevWorldState = this.curWorldState;
                //this.curWorldState = new SavedWorldState(Date.now(), incoming);
            }
        }


        public void processInputs()
        {
            //if (this.server === undefined) return;
            //if (this.entity === undefined) return;
            //if (this.entityId === undefined) return;

            //const nowTs: number = Date.now();
            //const lastUpdateTs: number = this.lastUpdateTs >= 0 ? this.lastUpdateTs : nowTs;
            //const delta: number = (nowTs - lastUpdateTs) / 1000;
            //this.lastUpdateTs = nowTs;

            //// package up the player's current input
            //let input: Input;
            //if (this.rightKeyDown)
            //{
            //    input = new Input(this.inputSeqNum++, delta, this.entityId);
            //}
            //else if (this.leftKeyDown)
            //{
            //    input = new Input(this.inputSeqNum++, -delta, this.entityId);
            //}
            //else
            //{
            //    // nothing interesting happenend
            //    return;
            //}

            //this.server.network.send(this.lagMs, input);

            //if (this.usePrediction)
            //{
            //    // optimistically apply our input (assume server will accept it)
            //    this.entity.applyInput(input);
            //}

            //if (this.useReconciliation)
            //{
            //    // Save input for later reconciliation. We'll need to re-apply
            //    // some of our optimistically applied inputs after we next
            //    // hear from the server.
            //    this.pendingInputs.push(input);
            //}
        }

        private void interpolateEntities()
        {
            //    if (this.prevWorldState === undefined) return;
            //    if (this.curWorldState === undefined) return;

            //    // Recall: "each player sees itself in the present but sees the
            //    // other entities in the past"
            //    // (http://www.gabrielgambetta.com/fpm3.html) so figure out how
            //    // far beyond the most recent server state we are right now, then
            //    // interpolate everyone else that amount of time between prev and
            //    // cur server states (i.e. one update behind).
            //    const now = Date.now();
            //const delta = now - this.curWorldState.processedTs;
            //const statesDelta = this.curWorldState.processedTs - this.prevWorldState.processedTs;
            //let interpFactor = delta / statesDelta;
            //    if (interpFactor === Infinity) interpFactor = 1; // If it'll let us div 0, why not

            //    const prev = Util.cast(this.prevWorldState.value, WorldState);
            //const cur = Util.cast(this.curWorldState.value, WorldState);

            //    for (let i = 0; i<cur.entities.length; ++i) {
            //        const curEntity = cur.entities[i];
            //        if (curEntity.id === this.entityId) continue; // don't interpolate self
            //        const prevEntity = prev.entities[i]; // assumes the set of entities never changes
            //const newEntity = curEntity.copy();
            //newEntity.x = prevEntity.x + (interpFactor* (curEntity.x - prevEntity.x));
            //        newEntity.speed = prevEntity.speed + (interpFactor* (curEntity.speed - prevEntity.speed));
            //        this.entities[i] = newEntity;
            //    }
        }

        public void update()
        {
            this.processServerMessages();
            //if (!this.entity) return; // not connected yet
            if (this.useEntityInterpolation)
            {
                this.interpolateEntities();
            }
            this.processInputs();
            //this.render();
            //this.nonAckdInputsElement.textContent = this.pendingInputs.length.toString();
        }

        public void  start()
        {
            //this.updateTimer = setInterval(() => this.update(), 1000 / this.tickRate);
        }
}

    public class Server
    {
        public List<Client> clients;           // nth client also has entityId == n
        public List<Entity> entities;          // nth entry has entityId n
        public List<int> lastProcessedInputSeqNums; // last processed input's seq num, by entityId
        public LagNetwork network;  // server's network (where it receives inputs from clients)
        private int tickRate = 5;
        private int updateTimer;
        private int worldStateSeq = 0;

        /// <summary>
        /// 客户端加入
        /// </summary>
        /// <param name="client"></param>
        public void connect(Client client)
        {
            client.server = this;
            int entityId = this.clients.Count;
            client.entityId = entityId; // give the client its entity id so it can identify future state messages
            this.clients.Add(client);

            Entity entity = new Entity(entityId);
            entity.x = 5; // spawn point
            this.entities.Add(entity);
        }
        /** Look for cheaters here. */
        private static bool validInput(Input input) {
            // Not exactly sure where 1/40 comes from.  I got it from the
            // original code.  The longest possible valid "press" should be
            // 1/client.tickRate (1/60).  But the JS timers are not reliable,
            // so if you use 1/60 below you end up throwing out a lot of
            // inputs that are slighly too long... so maybe that's where 1/40
            // comes from?
            //return Math.abs(input.pressTime) <= 1 / 40;
            return true;
        }

        public void processInputs()
        {
            while (true)
            {
                var msg = this.network.receive();
                //if (!msg) break;
                //const input = Util.cast(msg.payload, Input);
                //if (!input) break;
                //if (Server.validInput(input)) {
                //    const id = input.entityId;
                //    this.entities[id].applyInput(input);
                //    this.lastProcessedInputSeqNums[id] = input.seqNum;
                //} else {
                //    console.log('throwing out input!', input);
                //}
            }
        }

        public void sendWorldState()
        {
            // Make sure to send copies of our state, and not just references.
            // i.e. simulate serializing the data like we'd do if we were
            // using a real network.
            //var msg = new WorldState(
            //    this.worldStateSeq++,
            //    this.entities.map(e => e.copy()),
            //    this.lastProcessedInputSeqNums.slice()
            //);
            //this.clients.forEach(client => {
            //    client.network.send(client.lagMs, msg);
            //});
        }

        public void update()
        {
            this.processInputs();
            this.sendWorldState();
           // this.render();
        }
}

}
