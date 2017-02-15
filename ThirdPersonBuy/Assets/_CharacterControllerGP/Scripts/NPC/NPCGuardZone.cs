// © 2016 Mario Lelas
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// alerts assigned npc to players presence
    /// </summary>
    public class NPCGuardZone : MonoBehaviour
    {
        /// <summary>
        /// attackers allowed to attack player. Other wait in fight area
        /// </summary>
        [Tooltip ("Attackers allowed to attack player. Other wait in fight area.")]
        public int allowedAttackers = 4;

        /// <summary>
        /// area of fight. Npc will reside in this area if not attacking
        /// </summary>
        [Tooltip ("Area of fight. Npc will reside in this area if not attacking.")]
        public float fightArea = 6.0f;

        /// <summary>
        /// list of npcs in zone
        /// </summary>
        [Tooltip ("List of npcs in zone.")]
        public  List<NPCScript> npc_list = new List<NPCScript>();

        private bool m_PlayerInZone = false;            // is player in zone flag
        private bool m_CollectingNPCs = true;           // time to collect npcs in zone
        private float m_CollectNPCsTime = 2.0f;         // collect npcs max time
        private float m_CollectNPCsTimer = 0.0f;        // collect npcs timer
        private int m_NumChasers = 0;                     // number of chasers

        /// <summary>
        /// gets and sets player in zone flag
        /// </summary>
        public bool playerInZone { get { return m_PlayerInZone; } set { m_PlayerInZone = value; } }

        /// <summary>
        /// number of chasers 
        /// </summary>
        public int numChasers { get { return m_NumChasers; } set { m_NumChasers = value; } }
        
        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            // check if guard zone is assigned on all
            for (int i = 0; i < npc_list.Count; i++)
            {
                NPCScript npc = npc_list[i];
                npc.guardZone = this;
            }
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
            m_CollectNPCsTimer += Time.deltaTime;
            if (m_CollectNPCsTimer > m_CollectNPCsTime)
            {
                m_CollectingNPCs = false;

            }
        }

        /// <summary>
        /// return npcs to guard position
        /// </summary>
        public void npcReturn()
        {
            foreach (NPCScript npc in npc_list)
            {
                if (npc.isDead) continue;
                npc.breakAttack();
                npc.return2Post();
            }
        }

        /// <summary>
        /// Unity OnTriggerStay method
        /// OnTriggerStay is called once per frame for every Collider other that is touching the trigger
        /// </summary>
        /// <param name="col">collider staying in the trigger</param>
        void OnTriggerStay(Collider col)
        {
            if (m_CollectingNPCs)
            {
                if (col.tag == "NPC")
                {
                    NPCScript npc = col.GetComponent<NPCScript>();
                    if(npc)
                    {
                        if (!npc_list.Contains(npc))
                        {
                            npc.guardZone = this;
                            npc_list.Add(npc);
                        }
                    }
                }
            }

            // if player is inside, flag as true
            if (col.gameObject.tag == "Player")
            {
                m_PlayerInZone = true;
            }
        }
    }
}
