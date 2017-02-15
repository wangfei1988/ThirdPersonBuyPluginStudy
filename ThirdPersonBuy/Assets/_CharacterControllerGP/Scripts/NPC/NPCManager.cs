// © 2016 Mario Lelas
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// class that keeps list of all npcs and some usefull methods
    /// </summary>
    public class NPCManager : MonoBehaviour
    {
        private NPCScript[] m_Npcs;     // all npcs array

        private NPCGuardZone[] m_NpcZones;  

        private List<NPCGuardZone> m_CurrentZones = new List<NPCGuardZone>();

        private Player m_Player;     // reference to player 

        /// <summary>
        /// global num,ber of chasers
        /// </summary>
        [HideInInspector]
        public int NUM_CHASERS_GLOBAL = 0; // complete number of chasers 

        /// <summary>
        /// gets all npc array 
        /// </summary>
        public NPCScript [] npcs { get { return m_Npcs; } }

        /// <summary>
        /// gets current guard zones player is residing in
        /// </summary>
        public List<NPCGuardZone> currentZones { get { return m_CurrentZones; } }


        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            NPCScript[] npcs = FindObjectsOfType<NPCScript>();
            m_Npcs = npcs;


            NPCGuardZone[] zones = FindObjectsOfType<NPCGuardZone>();
            m_NpcZones = zones;

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (!playerGO)
            {
                Debug.LogError("Cannot find object with tag 'Player' " + " < " + this.ToString() + ">");
                return;
            }
            m_Player = playerGO.GetComponent<Player>();
            if (!m_Player) { Debug.LogError("Cannot find 'Player' script on " + playerGO.name + " < " +  this.ToString () + ">"); return; }
        }

        /// <summary>
        /// Unity FixedUpdate method
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
        /// </summary>
        void FixedUpdate()
        {
            m_CurrentZones.Clear();
            NUM_CHASERS_GLOBAL = 0;
            foreach (NPCGuardZone zone in m_NpcZones)
            {
                if(zone.playerInZone)
                {
                    m_CurrentZones.Add(zone);
                    chase();
                }
                else
                {
                    zone.npcReturn();
                }
                zone.playerInZone = false;
            }
        }

        /// <summary>
        /// get closest npc
        /// </summary>
        /// <returns>returns closest npc</returns>
        private NPCScript _getClosest()
        {
            NPCScript cur = null;
            float curDist = float.MaxValue;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc.isDead) continue;
                    if (npc.npcState != NPCScript.NPCState.Chase)
                    {

                        float dist = Vector3.Distance(m_Player.transform.position, npc.transform.position);
                        if (dist < curDist)
                        {
                            curDist = dist;
                            cur = npc;
                        }
                    }
                }
            }
            return cur;
        }

        /// <summary>
        /// npc chase state 
        /// </summary>
        private void chase()
        {
            NUM_CHASERS_GLOBAL = 0;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    zone.npc_list[i].npcState = NPCScript.NPCState.Return ;
                }
                zone.numChasers = 0;
                for (int i = zone.numChasers; i < zone.allowedAttackers; i++)
                {
                    NPCScript npc = _getClosest(); 
                    if (npc)
                    {
                        if (npc.isDead) continue;
                        npc.startChase();
                        zone.numChasers++;
                        NUM_CHASERS_GLOBAL++;
                    }
                }
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc)
                    {
                        if (npc.isDead) continue;
                        if (npc.npcState != NPCScript.NPCState.Chase)
                            npc.npcState = NPCScript.NPCState.Wait;
                    }
                }
            }

        }

        /// <summary>
        /// provides info is any npc in combat 
        /// </summary>
        /// <param name="caller">pass caller so it can be skipped</param>
        /// <returns>true or false</returns>
        public bool AnyNpcInCombat(NPCScript caller)
        {
            for (int i = 0; i < npcs.Length; i++)
            {
                NPCScript npc = npcs[i];
                if (npc == caller) continue;
                if (npc.inCombat)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// provides info is any npc in combat in current zone
        /// </summary>
        /// <param name="caller">pass caller so it can be skipped</param>
        /// <returns>true or false</returns>
        public bool AnyNpcInCombatInZone(NPCScript caller)
        {
            if (m_CurrentZones.Count == 0) return false;

            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc == caller) continue;
                    if (npc.inCombat)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// provides info is any npc is attacking in current zone
        /// </summary>
        /// <param name="caller">pass caller so it can be skipped</param>
        /// <returns>true or false</returns>
        public bool AnyNpcAttackingInZone(NPCScript caller)
        {
            if(m_CurrentZones.Count == 0) return false;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc == caller) continue;
                    if (npc.animator.GetCurrentAnimatorStateInfo (0).IsName ("AttackCombo"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// return number of attacker in current zone
        /// </summary>
        /// <returns></returns>
        public int NumNpcAttackingInZone()
        {
            if (m_CurrentZones.Count == 0) return 0;
            int count = 0;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc.animator.GetCurrentAnimatorStateInfo(0).IsName("AttackCombo"))
                    {
                        count++; 
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// provides info of number of npcs in combat in current zone
        /// </summary>
        /// <returns>true or false</returns>
        public int NumNpcInCombatInZone()
        {
            if (m_CurrentZones.Count == 0) return 0;
            int count = 0;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc.inCombat)
                        count++;
                }
            }
            return count;
        }

        /// <summary>
        /// number of npcs in range in current zone/s
        /// </summary>
        /// <returns></returns>
        public int NumInRangeInZone()
        {
            if (m_CurrentZones.Count == 0) return 0;
            int num = 0;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc.inAttackRange)
                        num++;
                }
            }
            return num;
        }

        /// <summary>
        /// number of npcs chasing in current zone/s
        /// </summary>
        /// <returns></returns>
        public int NumChasingInZone()
        {
            if (m_CurrentZones.Count == 0) return 0;

            int num = 0;
            foreach (NPCGuardZone zone in m_CurrentZones)
            {
                for (int i = 0; i < zone.npc_list.Count; i++)
                {
                    NPCScript npc = zone.npc_list[i];
                    if (npc.npcState == NPCScript.NPCState.Chase)
                        num++;
                }
            }
            return num;
        }

        /// <summary>
        /// in current zone/s add all npcs that are in range and under angle of transform.forward to the list.
        /// </summary>
        /// <param name="range">range condition</param>
        /// <param name="angle">angle condition</param>
        /// <param name="xform">transform ( position/ forward )</param>
        /// <param name="npcList">list on which to add</param>
        public void CollectNpcsInRangeAngleInZone(float range, float angle, Transform xform, List<NPCScript > npcList)
        {
            npcList.Clear();
            if (m_CurrentZones.Count == 0) return;
            foreach (NPCGuardZone zone in m_NpcZones)
            {
                if (zone.playerInZone)
                {
                    for (int i = 0; i < zone.npc_list.Count; i++)
                    {
                        NPCScript npc = zone.npc_list[i];
                        if (npc.isDead) continue;
                        if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;
                        Vector3 dir2npc = npc.transform.position - xform.position;
                        float dist = Vector3.Distance(xform.position, npc.transform.position);
                        if (dist < range)
                        {
                            float cur_angle = Vector3.Angle(xform.forward, dir2npc.normalized);
                            if (cur_angle <= angle)
                            {
                                npcList.Add(npc);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// add all npcs that are in range and under angle from direction to the list.
        /// </summary>
        /// <param name="range">range condition</param>
        /// <param name="angle">angle condition</param>
        /// <param name="position">test position</param>
        /// <param name="direction">test direction</param>
        /// <param name="npcList">list on which to add</param>
        public void CollectNpcsInRangeAngle(float range, float angle,Vector3 position, Vector3 direction,List<NPCScript> npcList)
        {
            npcList.Clear();

            for (int i = 0; i < m_Npcs.Length; i++)
            {
                NPCScript npc = m_Npcs[i];
                if (npc.isDead) continue;
                if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;
                Vector3 dir2npc = npc.transform.position - position;
                float dist = Vector3.Distance(position, npc.transform.position);
                if (dist <= range)
                {
                    float cur_angle = Vector3.Angle(direction, dir2npc.normalized);
                    if (cur_angle <= angle)
                        npcList.Add(npc);
                }
            }
        }

        /// <summary>
        /// add all npcs that are in range and under angle from direction to the list.
        /// </summary>
        /// <param name="range">range condition</param>
        /// <param name="angle">angle condition</param>
        /// <param name="position">test position</param>
        /// <param name="direction">test direction</param>
        /// <param name="npcList">list on which to add</param>
        public void CollectNpcsInRangeAngle(float range, float angle, Vector3 position, Vector3 direction, List<IGameCharacter> npcList)
        {
            npcList.Clear();

            for (int i = 0; i < m_Npcs.Length; i++)
            {
                NPCScript npc = m_Npcs[i];
                if (npc.isDead) continue;
                if (npc.ragdollState != RagdollManager.RagdollState.Animated) continue;
                Vector3 dir2npc = npc.transform.position - position;
                float dist = Vector3.Distance(position, npc.transform.position);
                if (dist <= range)
                {
                    float cur_angle = Vector3.Angle(direction, dir2npc.normalized);
                    if (cur_angle <= angle)
                        npcList.Add(npc);
                }
            }
        }

        /// <summary>
        /// revive all npcs with their max health
        /// </summary>
        public void reviveAll()
        {
            for (int i = 0; i < m_Npcs.Length; i++)
            {
                int maxHealth = m_Npcs[i].stats.maxHealth;
                m_Npcs[i].revive(maxHealth);
            }
        }
    } 
}
