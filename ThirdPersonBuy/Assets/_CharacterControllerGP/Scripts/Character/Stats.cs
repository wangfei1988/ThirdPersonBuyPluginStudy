// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// Holds player and npc statistics
    /// </summary>
    public class Stats : MonoBehaviour
    {
        /// <summary>
        /// maximum user health
        /// </summary>
        [Tooltip("Maximum user's health.") ]
        public int maxHealth = 100;

        /// <summary>
        /// reach of weapon 默认武器攻击范围（默认武器长度）  
        /// </summary>
        [Tooltip("Default reach of attack.") ]
        public float defaultWeaponReach = 1.5f;

        /// <summary>
        /// user's damage 击中某个人物，对该任务造成的伤害值
        /// </summary>
        [Tooltip ("User's default damage.") ]
        public int defaultDamage = 10;

        /// <summary>
        /// user's attack speed 默认攻击动画播放速度
        /// </summary>
        [Tooltip ("User's default attack speed.") ]
        public float DefaultAttackSpeed = 1.0f;

        /// <summary>
        /// user move speed multiplier。 默认移动速度，这个和移动动画的播放速度无关，指的是对象的的位移变化速度
        /// </summary>
        [Tooltip("User's default move speed multiplier.")]
        public float DefaultMoveSpeed = 1.0f;

        /// <summary>
        /// user attack value 
        /// added to damage given 击中某个人物，对该任务造成的额外伤害值
        /// </summary>
        [Tooltip ("User's attack value. Added to damage given.")]
        public int attack = 0;

        /// <summary>
        /// user defence value
        /// subtracted from damage received
        /// </summary>
        [Tooltip ("User's defence value. Subtracted from received damage.")]
        public int defence = 0;


        private int m_CurrentHealth = 100;              // current health 
        private float m_CurrentWeaponReach = 1.5f;      // current weapon reach
        private int m_CurrentDamage = 10;               // current damage
        private float m_CurrentAttackSpeed = 1f;        // current attack speed


        /// <summary>
        /// gets current user's health
        /// </summary>
        public int currentHealth { get { return m_CurrentHealth; } }

        /// <summary>
        /// getscurrent weapon reach
        /// </summary>
        public float currentWeaponReach { get { return m_CurrentWeaponReach; } }

        /// <summary>
        /// gets current damage
        /// </summary>
        public int currentDamage { get { return m_CurrentDamage; } }

        /// <summary>
        /// gets current attack speed 当前攻击动画播放速度
        /// </summary>
        public float currentAttackSpeed { get { return m_CurrentAttackSpeed; } }


        void Awake()
        {
            m_CurrentHealth = maxHealth;
            m_CurrentWeaponReach = defaultWeaponReach;
            m_CurrentDamage = defaultDamage;
            m_CurrentAttackSpeed = DefaultAttackSpeed ;
        }

        /// <summary>
        /// reset attack values to default
        /// </summary>
        public void resetAttackValues()
        {
            m_CurrentDamage = defaultDamage;
            m_CurrentWeaponReach = defaultWeaponReach;
            m_CurrentAttackSpeed = DefaultAttackSpeed;
        }

        /// <summary>
        /// decrease health by input value
        /// </summary>
        /// <param name="byVal"></param>
        public void decreaseHealth(int byVal)
        {
            m_CurrentHealth -= byVal;
            m_CurrentHealth = Mathf.Max(0, m_CurrentHealth);
        }

        /// <summary>
        /// increase health by input value
        /// </summary>
        /// <param name="byVal"></param>
        public void increaseHealth(int byVal)
        {
            m_CurrentHealth += byVal;
            m_CurrentHealth = Mathf.Min(m_CurrentHealth, maxHealth);
        }

        /// <summary>
        /// set current damage, weapon reach and attack speed
        /// 当切换不同武器时调用。
        /// </summary>
        /// <param name="_damage">new damage</param>
        /// <param name="_reach">new weapon reach</param>
        /// <param name="_attackSpeed">new attack speed</param>
        public void setCurrentAttackValue(int _damage,float _reach,float _attackSpeed)
        {
            m_CurrentDamage = _damage;
            m_CurrentWeaponReach = _reach; //当前武器可攻击范围（长度）
            m_CurrentAttackSpeed = _attackSpeed;
        }

    } 
}
