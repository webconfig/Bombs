using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Assets.Script
{
    public class Client
    {
        public int tickRate = 60;
        public int entityId;  // TODO: extremely inconvenient allowing this to have undefined value
        public GameObject entity; // The player's entity in the world; server provides it.
        public List<GameObject> entities; // awful, contains reference to this.entity as well
        public bool leftKeyDown = false;
        public bool rightKeyDown = false;
        //public LagNetwork network;
        public int lagMs = 250;
        public int lastUpdateTs = -1;
        public int inputSeqNum = 0;
        public List<Input> pendingInputs;
        //public SavedWorldState curWorldState;  // the last state we received from the server
        //public SavedWorldState prevWorldState; // penultimate state from server, used for entity interpolation
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
            //while (true)
            //{
            //    QueuedMessage msg = this.network.receive();
            //    if (msg == null) break;
            //    WorldState incoming = msg.payload as WorldState;

            //    for (int i = 0; i < incoming.entities.Count; ++i)
            //    {
            //        var entity = incoming.entities[i];

            //        //if (this.entityId === undefined) break; // pointless, but tsc unhappy without this

            //        if (entity.id == this.entityId)
            //        {
            //            // entity is the remote state for our local this.entity object

            //            // create an entity for ourself if we haven't yet
            //            //if (!this.entity) {
            //            //    if (typeof this.entityId === 'undefined') {
            //            //        throw new Error(`connected client should always have entityId ${ this}`);
            //            //    }
            //            //    this.entity = new Entity(this.entityId, this.color);
            //            //}

            //            // Set our position to whatever was sent by server
            //            this.entity.x = entity.x;

            //            this.entities[entity.id] = this.entity; // despair

            //            if (this.useReconciliation)
            //            {
            //                // i.e. reapply all inputs not yet ackd by server

            //                var lastProcessed = incoming.lastProcessedInputSeqNums[this.entityId];
            //                if (lastProcessed > 0)
            //                {
            //                    // First, keep inputs that have not yet been taken
            //                    // into account by the last WorldState sent by the
            //                    // server.
            //                    //this.pendingInputs = this.pendingInputs.filter(input => {
            //                    //    return input.seqNum > lastProcessed;
            //                    //});
            //                }

            //                // apply any remaining inputs to our local world state
            //                //this.pendingInputs.forEach(input => {
            //                //    if (this.entity)
            //                //    {
            //                //        this.entity.applyInput(input);
            //                //    }
            //                //});
            //            }

            //        }
            //        else
            //        {
            //            // non-local-player entity
            //            this.entities[entity.id] = entity;
            //        }
            //    }
            //    // update prev and current states for later entity interpolation
            //    this.prevWorldState = this.curWorldState;
            //    //this.curWorldState = new SavedWorldState(Date.now(), incoming);
            //}
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

        public void start()
        {
            //this.updateTimer = setInterval(() => this.update(), 1000 / this.tickRate);
        }
    }
}
