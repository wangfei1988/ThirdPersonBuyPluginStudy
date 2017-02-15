// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// class to keep track of all equipment bones
    /// </summary>
    [System.Serializable]
    public class EquipmentBones
    {
        /// <summary>
        /// transform holding one handed weapon not wielding
        /// </summary>
        public Transform weapon1H_rest_bone;

        /// <summary>
        /// transform holding one handed weapon wielding
        /// </summary>
        public Transform weapon1H_wield_bone;

        /// <summary>
        /// transform holding shield not wielding
        /// </summary>
        public Transform shield_rest_bone;

        /// <summary>
        /// transform holding shield wielding
        /// </summary>
        public Transform shield_wield_bone;

        /// <summary>
        /// transform holding two handed weapon not wielding
        /// </summary>
        public Transform weapon2H_rest_bone;

        /// <summary>
        /// transform holding two handed weapon wielding
        /// </summary>
        public Transform weapon2H_wield_bone;

        public Transform quiver_rest_bone;

        public Transform bow_rest_bone;

        public Transform bow_wield_bone;

        public Transform arrow_bone;

        public Transform secondary1H_rest_bone;

        public Transform secondary1H_wield_bone;

    }



    /// <summary>
    /// class for manipulation of equipment
    /// </summary>
    public class EquipmentScript : MonoBehaviour
    {
        public enum EquipmentSlot : int { WeaponOneHanded, Shield, WeaponTwoHanded, Ranged, Quiver, SecondaryOneHanded };

        /// <summary>
        /// helper item info class 
        /// </summary>
        public class ItemInfo
        {
            public InventoryItem item = null;
            public bool wielded = false;

            public ItemInfo (InventoryItem _item, bool _wielded)
            {
                item = _item;
                wielded = _wielded;
            }

            public ItemInfo (InventoryItem _item)
            {
                item = _item;
                wielded = false;
            }
        }

        /// <summary>
        /// holds references to important transforms
        /// </summary>
        [Tooltip("Equipment placement transforms.")]
        public EquipmentBones bones;         

        private Animator m_Animator;              // reference to animator
        private ItemInfo[] m_Items;               // equipment item array for convenience
        private bool m_Initialized = false;       // is class initialized ?

        


        /// <summary>
        /// gets is class initialized
        /// </summary>
        public bool initialized { get { return m_Initialized; } }

        /// <summary>
        /// all equipment array infos
        /// </summary>
        public ItemInfo [] items { get { return m_Items; } }

        /// <summary>
        /// get current weapon1h item
        /// </summary>
        public MeleeWeaponItem currentWeapon1H
        {
            get { return (MeleeWeaponItem)m_Items[(int)EquipmentSlot.WeaponOneHanded].item; } 
            private set { m_Items[(int)EquipmentSlot.WeaponOneHanded].item = value; }
        }

        /// <summary>
        /// gets current weapon2h item
        /// </summary>
        public MeleeWeaponItem currentWeapon2H
        {
            get { return (MeleeWeaponItem)m_Items[(int)EquipmentSlot.WeaponTwoHanded ].item; }
            private set { m_Items[(int)EquipmentSlot.WeaponTwoHanded].item = value; }
        }

        /// <summary>
        /// gets current shield item
        /// </summary>
        public ShieldItem  currentShield
        {
            get { return (ShieldItem )m_Items[(int)EquipmentSlot.Shield].item; }
            private set { m_Items[(int)EquipmentSlot.Shield].item = value; }
        }

        /// <summary>
        /// gets current quiver item
        /// </summary>
        public QuiverItem currentQuiver
        {
            get { return (QuiverItem )m_Items[(int)EquipmentSlot.Quiver].item; }
            private set { m_Items[(int)EquipmentSlot.Quiver].item = value; }
        }

        /// <summary>
        /// gets current bow item
        /// </summary>
        public BowItem currentBow
        {
            get { return (BowItem)m_Items[(int)EquipmentSlot.Ranged ].item; }
            private set { m_Items[(int)EquipmentSlot.Ranged].item = value; }
        }
        /// <summary>
        /// gets current secondary weapon
        /// </summary>
        public MeleeWeaponItem currentSecondary
        {
            get { return (MeleeWeaponItem)m_Items[(int)EquipmentSlot.SecondaryOneHanded ].item; }
            private set { m_Items[(int)EquipmentSlot.SecondaryOneHanded].item = value; }
        }

        /// <summary>
        /// gets and sets is shield in hand flag
        /// </summary>
        public bool shieldInHand
        {
            get { return m_Items[(int)EquipmentSlot.Shield].wielded; }
            private set { m_Items[(int)EquipmentSlot.Shield].wielded = value; }
        }

        /// <summary>
        /// gets and sets is weapon1h in hand
        /// </summary>
        public bool weaponInHand1H
        {
            get { return m_Items[(int)EquipmentSlot.WeaponOneHanded].wielded; }
            private set { m_Items[(int)EquipmentSlot.WeaponOneHanded].wielded = value; }
        } 

        /// <summary>
        /// gets and sets is weapon2h in hand
        /// </summary>
        public bool weaponInHand2H
        {
            get { return m_Items[(int)EquipmentSlot.WeaponTwoHanded ].wielded; }
            private set { m_Items[(int)EquipmentSlot.WeaponTwoHanded].wielded = value; }
        }

        /// <summary>
        /// gets and sets is bow in hand
        /// </summary>
        public bool bowInHand
        {
            get { return m_Items[(int)EquipmentSlot.Ranged].wielded; }
            private set { m_Items[(int)EquipmentSlot.Ranged].wielded = value; }
        }
        
        /// <summary>
        /// gets and sets is secondary weapon1h in hand
        /// </summary>
        public bool secondaryWeaponInHand
        {
            get { return m_Items[(int)EquipmentSlot.SecondaryOneHanded].wielded; }
            private set { m_Items[(int)EquipmentSlot.SecondaryOneHanded].wielded = value; }
        }

        /// <summary>
        /// returns true if any of weapons is wielding
        /// </summary>
        public bool weaponInHand { get { return weaponInHand1H || weaponInHand2H || bowInHand || shieldInHand || secondaryWeaponInHand; } }

        /// <summary>
        /// returns true if weapon 1H and secondary weapon are in hands
        /// </summary>
        public bool dualWeaponsInHand { get { return weaponInHand1H && secondaryWeaponInHand; } }

        /// <summary>
        /// initialize EquipmentScript component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;

            m_Animator = GetComponent<Animator>();
            if (!m_Animator) {  Debug.LogError("Cannot find 'Animator' component <" + this.ToString() + " >"); return; }

            m_Items = new ItemInfo[6]; // new InventoryItem[6];

            m_Items[(int)EquipmentSlot.WeaponOneHanded] = new ItemInfo(null); 
            m_Items[(int)EquipmentSlot.WeaponTwoHanded] = new ItemInfo(null); 
            m_Items[(int)EquipmentSlot.Shield] = new ItemInfo(null); 
            m_Items[(int)EquipmentSlot.Quiver] = new ItemInfo(null); 
            m_Items[(int)EquipmentSlot.Ranged] = new ItemInfo(null);
            m_Items[(int)EquipmentSlot.SecondaryOneHanded] = new ItemInfo(null); 

            m_Initialized = true;
        }

        /// <summary>
        /// set secondary weapon to be primary
        /// </summary>
        public void switchPrimaryWithSecondary()
        {
            MeleeWeaponItem temp = currentWeapon1H;
            bool tempInHand = weaponInHand1H;
            currentWeapon1H = currentSecondary;
            weaponInHand1H = secondaryWeaponInHand;
            currentSecondary = temp;
            secondaryWeaponInHand = tempInHand;
            
        }

        /// <summary>
        /// is object equipped check
        /// </summary>
        /// <param name="obj">object for checking</param>
        /// <returns>returns true is gameobject is equipped</returns>
        public bool isEqupped(GameObject obj)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized." + " < " + this.ToString () + " >");
#endif
                return false;
            }
            if (obj == currentWeapon1H) return true;
            if (obj == currentShield) return true;
            if (obj == currentWeapon2H) return true;
            if (obj == currentQuiver) return true;
            if (obj == currentBow) return true;
            if (obj == currentSecondary) return true;
            return false;
        }


#region UNITY_METHODS

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }

        /// <summary>
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return ;
            }
#endif

            _updateWeapon1H();
            _updateShield();
            _updateWeapon2H();
            _updateBow();
            _updateSecondary1H();
        }
#endregion



        /// <summary>
        /// sets new weapon1h item in equipmant.pass null to remove item.
        /// </summary>
        /// <param name="weaponItem">new weapon item</param>
        public MeleeWeaponItem set1HWeapon(MeleeWeaponItem  weaponItem)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return null;
            }
#endif
            MeleeWeaponItem prev = currentWeapon1H;
            if (weaponItem == null)
            {
                unset1HWeapon();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(weaponItem.gameObject))
                {
                    Debug.LogError("cannot use. item is prefab");
                    return null;
                }
#endif

                currentWeapon1H = weaponItem;

                // For scaling
                // not affecting bones rotation / position
                weaponItem.transform.SetParent(transform, false);

                weaponItem.owner = gameObject;
                weaponItem.initialize();
                weaponItem.equipSetup();

                weaponItem.item.SetActive(true);
                weaponItem.equipped = true;
                if (!weaponItem.keepScaling) weaponItem.transform.localScale = Vector3.one;

                _updateWeapon1H();
            }
            return prev;
        }


        /// <summary>
        /// sets new shield item in equipment.pass null to remove item.
        /// </summary>
        /// <param name="shieldItem">new shield item</param>
        public ShieldItem setShield(ShieldItem shieldItem)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return null ;
            }
#endif
            ShieldItem prev = currentShield;
            if (shieldItem == null)
            {
                unsetShield();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(shieldItem.gameObject))
                {
                    Debug.LogError("cannot use. shield item is prefab");
                    return null;
                }
#endif
                currentShield = shieldItem;
                // For scaling
                // not affecting bones rotation / position
                shieldItem.transform.SetParent(transform, true);

                shieldItem.owner = gameObject;

                shieldItem.initialize();
                shieldItem.setupPhysicsForWearing();
                if (!shieldItem.keepScaling) shieldItem.transform.localScale = Vector3.one;

                shieldItem.item.SetActive(true);
                shieldItem.equipped = true;

                _updateShield();
            }
            return prev;
        }

        
        /// <summary>
        /// sets new weapon2h item in equipment.pass null to remove item.
        /// </summary>
        /// <param name="weaponItem">new weapon item</param>
        public MeleeWeaponItem set2HWeapon(MeleeWeaponItem weaponItem)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return null;
            }
#endif
            MeleeWeaponItem prev = currentWeapon2H;
            if (weaponItem == null)
            {
                unset2HWeapon();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(weaponItem.gameObject))
                {
                    Debug.LogError("cannot use. item is prefab");
                    return null;
                }
#endif
                currentWeapon2H = weaponItem;

                // For scaling
                // not affecting bones rotation / position
                weaponItem.transform.SetParent(transform, true);

                weaponItem.owner = gameObject;
                weaponItem.initialize();
                weaponItem.setupPhysicsForWearing(true);

                weaponItem.item.SetActive(true);
                weaponItem.equipped = true;
                if (!weaponItem.keepScaling) weaponItem.transform.localScale = Vector3.one;
                _updateWeapon2H();
            }
            return prev;
        }

        /// <summary>
        /// sets new quiver with arrow prefab.pass null to remove item.
        /// </summary>
        /// <param name="quiverItem">new quiver item</param>
        public QuiverItem  setQuiver(QuiverItem quiverItem)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return null; 
            }
            QuiverItem prev = currentQuiver;
            if (quiverItem == null)
            {
                unsetQuiver();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(quiverItem.gameObject))
                {
                    Debug.LogError("cannot use. item is prefab");
                    return null;
                }
#endif

                currentQuiver = quiverItem;
                // For scaling
                // not affecting bones rotation / position
                quiverItem.transform.SetParent(transform, true);


                quiverItem.owner = gameObject;

                quiverItem.initialize();
                quiverItem.equipSetup();

                quiverItem.transform.position = bones.quiver_rest_bone.position;
                quiverItem.transform.rotation = bones.quiver_rest_bone.rotation;
                quiverItem.transform.parent = bones.quiver_rest_bone;

#if DEBUG_INFO
                if (quiverItem.arrowPrefab == null)
                    Debug.LogWarning("associated arrow prefab with this quiver is null. Bow will not shoot.");
#endif

                quiverItem.item.SetActive(true);
                quiverItem.equipped = true;
                if (!quiverItem.keepScaling) quiverItem.transform.localScale = Vector3.one;
            }
            return prev;
        }

        /// <summary>
        /// sets new bow item in equipment.pass null to remove item.
        /// </summary>
        /// <param name="bowItem">new bow item</param>
        /// <param name="inHand">put in hand bone or on resting bone</param>
        public BowItem setBow(BowItem bowItem)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return null;
            }
            BowItem prev = currentBow;
            if (bowItem == null)
            {
                unsetBow();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(bowItem.gameObject))
                {
                    Debug.LogError("cannot use. item is prefab");
                    return null;
                }
#endif

                currentBow = bowItem;
                // For scaling
                // not affecting bones rotation / position
                bowItem.transform.SetParent(transform, true);


                bowItem.owner = gameObject;
                bowItem.initialize();
                bowItem.equipSetup ();

                bowItem.item.SetActive(true);
                bowItem.equipped = true;
                if (!bowItem.keepScaling) bowItem.transform.localScale = Vector3.one;
                _updateBow();
            }
            return prev;
        }

        /// <summary>
        /// sets new weapon1h item in equipmant.pass null to remove item.
        /// </summary>
        /// <param name="weaponItem">new weapon item</param>
        public MeleeWeaponItem setSecondary1HWeapon(MeleeWeaponItem weaponItem)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return null;
            }
#endif

            MeleeWeaponItem prev = currentSecondary;
            if (weaponItem == null)
            {
                unsetSecondary1HWeapon();
            }
            else
            {
#if DEBUG_INFO
                if (Utils.IsPrefab(weaponItem.gameObject))
                {
                    Debug.LogError("cannot use. item is prefab");
                    return null;
                }
#endif

                currentSecondary = weaponItem;

                // For scaling
                // not affecting bones rotation / position
                weaponItem.transform.SetParent(transform, false);
                weaponItem.owner = gameObject;
                weaponItem.initialize();
                weaponItem.equipSetup();

                weaponItem.item.SetActive(true);
                weaponItem.equipped = true;
                if (!weaponItem.keepScaling) weaponItem.transform.localScale = Vector3.one;

                _updateSecondary1H();
            }
            return prev;
        }

        /// <summary>
        /// equip inventory item
        /// </summary>
        /// <param name="item">new item</param>
        /// <returns>old inventory item if replaced</returns>
        public InventoryItem setItem(InventoryItem item)
        {
            if (!m_Initialized)
            {
                initialize();
            }
            if (item == null) return null;

            InventoryItem prevObj = null;
            InventoryItem currObj = item;
            switch (item.itemType)
            {
                case InventoryItemType.Weapon1H:
                    {
                        MeleeWeaponItem meeleItem = currObj as MeleeWeaponItem;
                        prevObj = set1HWeapon(meeleItem);
                    }
                    break;
                case InventoryItemType.Weapon2H:
                    {
                        MeleeWeaponItem meeleItem = currObj as MeleeWeaponItem;
                        prevObj = set2HWeapon(meeleItem);
                    }
                    break;
                case InventoryItemType.Shield:
                    {
                        ShieldItem shieldItem = currObj as ShieldItem;
                        prevObj = setShield (shieldItem);
                    }
                    break;
                case InventoryItemType.QuiverArrow:
                    {
                        QuiverItem quiverItem = currObj as QuiverItem;
                        prevObj = setQuiver(quiverItem);
                    }
                    break;
                case InventoryItemType.Bow:
                    {
                        BowItem bowItem = currObj as BowItem;
                        prevObj = setBow(bowItem);
                    }
                    break;
            }
            return prevObj;
        }

        /// <summary>
        /// unequip inventory item
        /// </summary>
        /// <param name="item"></param>
        public void unsetItem(InventoryItem item)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return ;
            }
#endif
            if (!item.equipped) return;
            switch (item.itemType)
            {
                case InventoryItemType.Shield: unsetShield(); break;
                case InventoryItemType.Weapon1H:
                    if(item == currentWeapon1H)
                    {
                        unset1HWeapon();
                    }
                    else if(item == currentSecondary)
                    {
                        unsetSecondary1HWeapon();
                    }
                    break;
                case InventoryItemType.Weapon2H: unset2HWeapon(); break;
                case InventoryItemType.QuiverArrow: unsetQuiver(); break;
                case InventoryItemType.Bow: unsetBow(); break;
            }
            item.equipped = false;
        }

        /// <summary>
        /// remove weapon1h from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unset1HWeapon(bool deactivate = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return ;
            }
#endif

            MeleeWeaponItem prev = currentWeapon1H;
            currentWeapon1H = null;
            if (prev)
            {
                prev.transform.SetParent(null, true); 
#if DEBUG_INFO
                if(!prev.item ) { Debug.LogError("object cannot be null");return;}
#endif
                prev.owner = null;
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
            //m_Items[(int)EquipmentSlot.WeaponOneHanded].item = null;
        }

        /// <summary>
        /// remove weapon2h from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unset2HWeapon(bool deactivate = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return ;
            }
#endif

            MeleeWeaponItem prev = currentWeapon2H ;
            currentWeapon2H = null;
            if (prev)
            {
                prev.transform.SetParent(null);
#if DEBUG_INFO
                if (!prev.item) { Debug.LogError("object cannot be null"); return; }
#endif
                prev.owner = null;
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
        }

        /// <summary>
        /// remove shield from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unsetShield(bool deactivate = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return ;
            }
#endif

            ShieldItem prev = currentShield;
            currentShield = null;
            if(prev)
            {
                prev.transform.SetParent(null);
#if DEBUG_INFO
                if (!prev.item) { Debug.LogError("object cannot be null"); return; }
#endif
                prev.owner = null;
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
        }

        /// <summary>
        /// remove quiver from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unsetQuiver(bool deactivate = true)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return;
            }

            QuiverItem prev = currentQuiver;
            currentQuiver = null;
            if (prev)
            {
                prev.transform.SetParent(null);
#if DEBUG_INFO
                if (!prev.item) { Debug.LogError("object cannot be null"); return; }
#endif
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
        }

        /// <summary>
        /// remove bow from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unsetBow(bool deactivate = true)
        {
            if (!m_Initialized)
            {
#if DEBUG_INFO
                Debug.LogError("component not initialized.");
#endif
                return;
            }

            BowItem prev = currentBow;
            currentBow = null;
            if (prev)
            {
                prev.transform.SetParent(null);
#if DEBUG_INFO
                if (!prev.item) { Debug.LogError("object cannot be null"); return; }
#endif
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
        }

        /// <summary>
        /// remove weapon1h from equipment
        /// </summary>
        /// <param name="deactivate">deactivate object on unset or not</param>
        public void unsetSecondary1HWeapon(bool deactivate = true)
        {
#if DEBUG_INFO
            if (!m_Initialized)
            {
                Debug.LogError("Component not initialized! " + " < " + this.ToString() + ">");
                return;
            }
#endif

            MeleeWeaponItem prev = currentSecondary;
            currentSecondary = null;
            if (prev)
            {
                prev.transform.SetParent(null, true);
#if DEBUG_INFO
                if (!prev.item) { Debug.LogError("object cannot be null"); return; }
#endif
                prev.owner = null;
                prev.item.SetActive(!deactivate);
                prev.equipped = false;
            }
        }


        /// <summary>
        /// updare current one handed weapon transform
        /// </summary>
        private void _updateWeapon1H()
        {
            if (!currentWeapon1H) return;
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
            if (!bones.weapon1H_rest_bone) { Debug.LogError("cannot rest weapon1H.No resting bone assigned."); return; }
            if (!bones.weapon1H_wield_bone) { Debug.LogError("cannot wield weapon1H.No wielding bone assigned."); return; }
#endif
            if (currentWeapon1H.switchFlag )
            {
                currentWeapon1H.switchTimer += Time.deltaTime;
                if (currentWeapon1H.weaponSwitchTime > 0)
                {
                    float lValue = Mathf.Clamp01(currentWeapon1H.switchTimer / currentWeapon1H.weaponSwitchTime);
                    if (weaponInHand1H)
                    {
                        Vector3 start = bones.weapon1H_rest_bone.position;
                        Vector3 end = bones.weapon1H_wield_bone.position;
                        Quaternion s = bones.weapon1H_rest_bone.rotation;
                        Quaternion e = bones.weapon1H_wield_bone.rotation;

                        currentWeapon1H.transform.position = Vector3.Lerp(start, end, lValue);
                        currentWeapon1H.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentWeapon1H.switchTimer >= currentWeapon1H.weaponSwitchTime)
                        {
                            m_Animator.SetBool(/*"pRightHandClosed"*/HashIDs .RightHandClosedBool , true);
                            if (currentWeapon1H.OnTake != null) currentWeapon1H.OnTake();
                            currentWeapon1H.switchFlag = false;
                        }
                    }
                    else
                    {
                        Vector3 start = bones.weapon1H_wield_bone.position;
                        Vector3 end = bones.weapon1H_rest_bone.position;
                        Quaternion e = bones.weapon1H_rest_bone.rotation;
                        Quaternion s = bones.weapon1H_wield_bone.rotation;

                        currentWeapon1H.transform.position = Vector3.Lerp(start, end, lValue);
                        currentWeapon1H.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentWeapon1H.switchTimer >= currentWeapon1H.weaponSwitchTime)
                        {
                            currentWeapon1H.switchFlag = false;
                        }
                    }

                    return;
                }
                else
                {
                    currentWeapon1H.switchFlag = false;
                }
            }
            if (weaponInHand1H)
            {
                currentWeapon1H.transform.position = bones.weapon1H_wield_bone.position;
                currentWeapon1H.transform.rotation = bones.weapon1H_wield_bone.rotation;
            }
            else
            {
                currentWeapon1H.transform.position = bones.weapon1H_rest_bone.position;
                currentWeapon1H.transform.rotation =
                    bones.weapon1H_rest_bone.rotation;
            }

        }

        /// <summary>
        /// update current shield transform
        /// </summary>
        private void _updateShield()
        {
            if (!currentShield) return;

#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
            if (!bones.shield_rest_bone) { Debug.LogError("cannot rest shield.No resting bone assigned."); return; }
            if (!bones.shield_wield_bone) { Debug.LogError("cannot wield shield.No wielding bone assigned."); return; }
#endif

            if (currentShield.switchFlag)
            {
                currentShield.switchTimer += Time.deltaTime;
                if (currentShield.weaponSwitchTime > 0)
                {
                    float lValue = Mathf.Clamp01(currentShield.switchTimer / currentShield.weaponSwitchTime);
                    if (shieldInHand)
                    {
                        Vector3 start = bones.shield_rest_bone .position;
                        Vector3 end = bones.shield_wield_bone .position;
                        Quaternion s = bones.shield_rest_bone.rotation;
                        Quaternion e = bones.shield_wield_bone.rotation;

                        currentShield.transform.position = Vector3.Lerp(start, end, lValue);
                        currentShield.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentShield.switchTimer >= currentShield.weaponSwitchTime)
                        {
                            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                            if (currentShield.OnTake != null) currentShield.OnTake();
                            currentShield.switchFlag = false;
                        }
                    }
                    else
                    {
                        Vector3 start = bones.shield_wield_bone.position;
                        Vector3 end = bones.shield_rest_bone.position;
                        Quaternion e = bones.shield_rest_bone.rotation;
                        Quaternion s = bones.shield_wield_bone.rotation;

                        currentShield.transform.position = Vector3.Lerp(start, end, lValue);
                        currentShield.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentShield.switchTimer >= currentShield.weaponSwitchTime)
                        {
                            currentShield.switchFlag = false;
                        }
                    }

                    return;
                }
                else
                {
                    currentShield.switchFlag = false;
                }
            }

            if (shieldInHand)
            {
                currentShield.transform.position = bones.shield_wield_bone.position;
                currentShield.transform.rotation = bones.shield_wield_bone.rotation;
            }
            else
            {
                currentShield.transform.position = bones.shield_rest_bone.position;
                currentShield.transform.rotation =
                    bones.shield_rest_bone.rotation;
            }
        }

        /// <summary>
        /// update current two handed weapon transform
        /// </summary>
        private void _updateWeapon2H()
        {
            if (!currentWeapon2H) return;
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
            if (!bones.weapon2H_rest_bone) { Debug.LogError("cannot rest weapon2H.No resting bone assigned."); return; }
            if (!bones.weapon2H_wield_bone) { Debug.LogError("cannot wield weapon2H.No wielding bone assigned."); return; }
#endif

            if (currentWeapon2H.switchFlag)
            {
                currentWeapon2H.switchTimer += Time.deltaTime;
                if (currentWeapon2H.weaponSwitchTime > 0)
                {
                    float lValue = Mathf.Clamp01(currentWeapon2H.switchTimer / currentWeapon2H.weaponSwitchTime);
                    if (weaponInHand2H)
                    {
                        Vector3 start = bones.weapon2H_rest_bone.position;
                        Vector3 end = bones.weapon2H_wield_bone.position;
                        Quaternion s = bones.weapon2H_rest_bone.rotation;
                        Quaternion e = bones.weapon2H_wield_bone.rotation;

                        currentWeapon2H.transform.position = Vector3.Lerp(start, end, lValue);
                        currentWeapon2H.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentWeapon2H.switchTimer >= currentWeapon2H.weaponSwitchTime)
                        {
                            m_Animator.SetBool(/*"pRightHandClosed"*/HashIDs.RightHandClosedBool, true);
                            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                            if (currentWeapon2H.OnTake != null) currentWeapon2H.OnTake();
                            currentWeapon2H.switchFlag = false;
                        }
                    }
                    else
                    {
                        Vector3 start = bones.weapon2H_wield_bone.position;
                        Vector3 end = bones.weapon2H_rest_bone.position;
                        Quaternion e = bones.weapon2H_rest_bone.rotation;
                        Quaternion s = bones.weapon2H_wield_bone.rotation;

                        currentWeapon2H.transform.position = Vector3.Lerp(start, end, lValue);
                        currentWeapon2H.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentWeapon2H.switchTimer >= currentWeapon2H.weaponSwitchTime)
                        {
                            currentWeapon2H.switchFlag = false;
                        }
                    }

                    return;
                }
                else
                {
                    currentWeapon2H.switchFlag = false;
                }
            }


            if (weaponInHand2H)
            {
                currentWeapon2H.transform.position = bones.weapon2H_wield_bone.position;
                currentWeapon2H.transform.rotation = bones.weapon2H_wield_bone.rotation;
            }
            else
            {
                currentWeapon2H.transform.position = bones.weapon2H_rest_bone.position;
                currentWeapon2H.transform.rotation = bones.weapon2H_rest_bone.rotation;
            }

        }

        /// <summary>
        /// update current bow transform
        /// </summary>
        private void _updateBow()
        {
            if (!currentBow) return;
#if DEBUG_INFO
            if (!bones.bow_rest_bone) { Debug.LogError("cannot rest bow.No resting bone assigned."); return; }
            if (!bones.bow_wield_bone ) { Debug.LogError("cannot wield bow.No wielding bone assigned."); return; }
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (currentBow.switchFlag)
            {
                currentBow.switchTimer += Time.deltaTime;
                if (currentBow.weaponSwitchTime > 0)
                {
                    float lValue = Mathf.Clamp01(currentBow.switchTimer / currentBow.weaponSwitchTime);
                    if (bowInHand)
                    {
                        Vector3 start = bones.bow_rest_bone.position;
                        Vector3 end = bones.bow_wield_bone.position;
                        Quaternion s = bones.bow_rest_bone.rotation;
                        Quaternion e = bones.bow_wield_bone.rotation;

                        currentBow.transform.position = Vector3.Lerp(start, end, lValue);
                        currentBow.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentBow.switchTimer >= currentBow.weaponSwitchTime)
                        {
                            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                            if (currentBow.OnTake != null) currentBow.OnTake();
                            currentBow.switchFlag = false;
                        }
                    }
                    else
                    {
                        Vector3 start = bones.bow_wield_bone.position;
                        Vector3 end = bones.bow_rest_bone.position;
                        Quaternion e = bones.bow_rest_bone.rotation;
                        Quaternion s = bones.bow_wield_bone .rotation;

                        currentBow.transform.position = Vector3.Lerp(start, end, lValue);
                        currentBow.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentBow.switchTimer >= currentBow.weaponSwitchTime)
                        {
                            currentBow.switchFlag = false;
                        }
                    }

                    return;
                }
                else
                {
                    currentBow.switchFlag = false;
                }
            }

            if (bowInHand)
            {
                currentBow.transform.position = bones.bow_wield_bone.position;
                currentBow.transform.rotation = bones.bow_wield_bone.rotation;
            }
            else
            {
                currentBow.transform.position = bones.bow_rest_bone.position;
                currentBow.transform.rotation = bones.bow_rest_bone.rotation;
            }
        }

        /// <summary>
        /// update current secondary weapon transform
        /// </summary>
        private void _updateSecondary1H()
        {
            if (!currentSecondary) return;

#if DEBUG_INFO
            if (!bones.secondary1H_rest_bone) { Debug.LogError("Cannot rest secondary weapon. No resting bone assigned."); return; }
            if (!bones.secondary1H_wield_bone) { Debug.LogError("Cannot wield secondary weapon. No wield bone assigned."); return; }
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (currentSecondary.switchFlag)
            {
                currentSecondary.switchTimer += Time.deltaTime;
                if (currentSecondary.weaponSwitchTime > 0)
                {
                    float lValue = Mathf.Clamp01(currentSecondary.switchTimer / currentSecondary.weaponSwitchTime);
                    if (secondaryWeaponInHand)
                    {
                        Vector3 start = bones.secondary1H_rest_bone.position;
                        Vector3 end = bones.secondary1H_wield_bone.position;
                        Quaternion s = bones.secondary1H_rest_bone.rotation;
                        Quaternion e = bones.secondary1H_wield_bone.rotation;

                        currentSecondary.transform.position = Vector3.Lerp(start, end, lValue);
                        currentSecondary.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentSecondary.switchTimer >= currentSecondary.weaponSwitchTime)
                        {
                            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                            if (currentSecondary.OnTake != null) currentSecondary.OnTake();
                            currentSecondary.switchFlag = false;
                        }
                    }
                    else
                    {
                        Vector3 start = bones.secondary1H_wield_bone.position;
                        Vector3 end = bones.secondary1H_rest_bone.position;
                        Quaternion e = bones.secondary1H_rest_bone.rotation;
                        Quaternion s = bones.secondary1H_wield_bone.rotation;

                        currentSecondary.transform.position = Vector3.Lerp(start, end, lValue);
                        currentSecondary.transform.rotation = Quaternion.Slerp(s, e, lValue);

                        if (currentSecondary.switchTimer >= currentSecondary.weaponSwitchTime)
                        {
                            currentSecondary.switchFlag = false;
                        }
                    }

                    return;
                }
                else
                {
                    currentSecondary.switchFlag = false;
                }
            }

            if (secondaryWeaponInHand)
            {
                currentSecondary.transform.position = bones.secondary1H_wield_bone.position;
                currentSecondary.transform.rotation = bones.secondary1H_wield_bone.rotation;
            }
            else
            {
                currentSecondary.transform.position = bones.secondary1H_rest_bone.position;
                currentSecondary.transform.rotation = bones.secondary1H_rest_bone.rotation;
            }

        }


        /// <summary>
        /// wield current one handed weapon
        /// transfer onto wielding bones
        /// </summary>
        public void wieldWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">");return; }
#endif

            if (!currentWeapon1H) return;

            weaponInHand1H = true;
            if (currentWeapon1H.weaponSwitchTime > 0)
            {
                currentWeapon1H.switchFlag = true;
                currentWeapon1H.switchTimer = 0.0f;
            }
            else
            {
                m_Animator.SetBool(HashIDs.RightHandClosedBool, true);
                if (currentWeapon1H.OnTake != null) currentWeapon1H.OnTake();
            }
        }

        /// <summary>
        /// rest current one handed weapon
        /// transfer onto rest bones
        /// </summary>
        public void restWeapon1H()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif
            if (!currentWeapon1H) return;

            weaponInHand1H = false;
            if (currentWeapon1H.weaponSwitchTime > 0)
            {
                currentWeapon1H.switchFlag = true;
                currentWeapon1H.switchTimer = 0.0f;
            }
            if(currentWeapon1H.OnSheathe != null)
                currentWeapon1H.OnSheathe();
            m_Animator.SetBool(/*"pRightHandClosed"*/HashIDs.RightHandClosedBool, false);
        }

        /// <summary>
        /// wield current shield
        /// transfer onto wielding bones
        /// </summary>
        public void wieldShield()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentShield) return;

            shieldInHand = true;
            if (currentShield.weaponSwitchTime > 0)
            {
                currentShield.switchFlag = true;
                currentShield.switchTimer = 0.0f;
            }
            else
            {
                m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, false);
                if (currentShield.OnTake != null) currentShield.OnTake();
            }
        }

        /// <summary>
        /// rest current shield
        /// transfer onto rest bones
        /// </summary>
        public void restShield()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentShield) return;

            if (currentShield.weaponSwitchTime > 0)
            {
                currentShield.switchFlag = true;
                currentShield.switchTimer = 0.0f;
            }

            shieldInHand = false;
            if (currentShield.OnSheathe != null)
                currentShield.OnSheathe();
            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, false);
        }

        /// <summary>
        /// wield current two handed weapon
        /// transfer onto wielding bones
        /// </summary>
        public void wieldWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentWeapon2H) return;

            weaponInHand2H = true;
            if (currentWeapon2H.weaponSwitchTime > 0)
            {
                currentWeapon2H.switchFlag = true;
                currentWeapon2H.switchTimer = 0.0f;

            }
            else
            {
                m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                m_Animator.SetBool(/*"pRightHandClosed"*/HashIDs.RightHandClosedBool, true);
                if (currentWeapon2H.OnTake != null)
                    currentWeapon2H.OnTake();
            }
        }

        /// <summary>
        /// rest current two handed weapon
        /// transfer onto rest bones
        /// </summary>
        public void restWeapon2H()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentWeapon2H) return;

            if (currentWeapon2H.weaponSwitchTime > 0)
            {
                currentWeapon2H.switchFlag = true;
                currentWeapon2H.switchTimer = 0.0f;
            }
            weaponInHand2H = false;
            m_Animator.SetBool(/*"pRightHandClosed"*/HashIDs.RightHandClosedBool, false);
            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, false);
            if (currentWeapon2H.OnSheathe != null)
                currentWeapon2H.OnSheathe();
        }

        /// <summary>
        /// wield current bow
        /// transfer onto wielding bones
        /// </summary>
        public void wieldBow()
        {
#if DEBUG_INFO
            if (!m_Animator ) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentBow) return;

            bowInHand = true;
            if (currentBow.weaponSwitchTime > 0)
            {
                currentBow.switchFlag = true;
                currentBow.switchTimer = 0.0f;
            }
            else
            {
                m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool , true);
                if (currentBow.OnTake != null) currentBow.OnTake();
            }

        }

        /// <summary>
        /// rest current bow
        /// transfer onto rest bones
        /// </summary>
        public void restBow()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentBow) return;

            bowInHand = false;
            if (currentBow.weaponSwitchTime > 0)
            {
                currentBow.switchFlag = true;
                currentBow.switchTimer = 0.0f;
            }
            if (currentBow.OnSheathe != null)
                currentBow.OnSheathe();
            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, false);
        }

        /// <summary>
        /// wield current secondary one handed weapon
        /// transfer onto wielding bones
        /// </summary>
        public void wieldSecondary()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentSecondary) return;

            secondaryWeaponInHand = true;
            if (currentSecondary.weaponSwitchTime > 0)
            {
                currentSecondary.switchFlag = true;
                currentSecondary.switchTimer = 0.0f;
            }
            else
            {
                m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, true);
                if (currentSecondary.OnTake != null) currentSecondary.OnTake();
            }
        }

        /// <summary>
        /// rest current secondary one handed weapon
        /// transfer onto rest bones
        /// </summary>
        public void restSecondary()
        {
#if DEBUG_INFO
            if (!m_Animator) { Debug.LogError("object cannot be null <" + this.ToString() + ">"); return; }
#endif

            if (!currentSecondary) return;

            secondaryWeaponInHand = false;
            if (currentSecondary.weaponSwitchTime > 0)
            {
                currentSecondary.switchFlag = true;
                currentSecondary.switchTimer = 0.0f;
            }
            if (currentSecondary.OnSheathe != null)
                currentSecondary.OnSheathe();
            m_Animator.SetBool(/*"pLeftHandClosed"*/HashIDs.LeftHandClosedBool, false);
        }
    } 
}
