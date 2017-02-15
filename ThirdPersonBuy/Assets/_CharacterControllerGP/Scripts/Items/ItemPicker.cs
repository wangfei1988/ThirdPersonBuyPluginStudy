// © 2016 Mario Lelas
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// throws ray from camera forwards
    /// and checks if there are any items hit
    /// </summary>
    public class ItemPicker : MonoBehaviour
    {
        /// <summary>
        /// display UI that shows hit object info text
        /// </summary>
        [Tooltip("Display UI that shows hit object info text.")]
        public UnityEngine.UI.Text DisplayUI;

        /// <summary>
        /// distance from character at which ray will accept hits
        /// </summary>
        [Tooltip("Distance from character at which ray will accept hits.")]
        public float pickDistance = 3.0f;

        /// <summary>
        /// outline material
        /// outlines current highlighted object
        /// </summary>
        public Material outlineMaterial;

        /// <summary>
        /// outline color
        /// </summary>
        public Color outlineColor = Color.yellow;

        /// <summary>
        /// outline width
        /// </summary>
        public float outlineWidth = 0.02f;

        public LayerMask layers;

        private PlayerControl m_PlayerCtrl;
        private Collider m_CurrCollider = null;           // current hit collider
        private InventoryItem m_CurrItem = null;          // current hit item
        private bool m_Highlighted = false;                 // is picking ray over an item
        private List<Shader> m_OriginalShaders = new List<Shader>();        // list of original shaders on highlighted object
        private GameObject m_CurrObject = null;                             // current highlighted object 
        private List<Renderer> m_CurrentRenderers = new List<Renderer>();   // original renderers on highlighted object


        /// <summary>
        /// returns true if item is ready for picking
        /// </summary>
        public bool highlighted { get { return m_Highlighted; } }


        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            m_PlayerCtrl = GetComponent<PlayerControl>();
            if(!m_PlayerCtrl)
            {
                Debug.LogError("Cannot find 'PlayerControl' component");
            }
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_Outline", outlineWidth);
        }

        /// <summary>
        /// method fired on item highlight enter
        /// </summary>
        /// <param name="hit">current raycasthit info struct</param>
        void OnItemEnter(RaycastHit hit)
        {
            Rigidbody attachedBody = hit.collider.attachedRigidbody;
            if (attachedBody)
            {
                Renderer[] rs = hit.collider.attachedRigidbody.gameObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < rs.Length; i++)
                {
                    m_OriginalShaders.Add(rs[i].material.shader);
                    m_CurrentRenderers.Add(rs[i]);
                    rs[i].material.shader = outlineMaterial.shader;
                    rs[i].material.SetColor("_OutlineColor", outlineColor);
                    rs[i].material.SetFloat("_Outline", outlineWidth);
                }
            }
            else
            {
                Renderer[] rs = hit.collider.gameObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < rs.Length; i++)
                {
                    m_OriginalShaders.Add(rs[i].material.shader);
                    m_CurrentRenderers.Add(rs[i]);
                    rs[i].material.shader = outlineMaterial.shader;
                    rs[i].material.SetColor("_OutlineColor", outlineColor);
                    rs[i].material.SetFloat("_Outline", outlineWidth);
                }
            }
        }

        /// <summary>
        /// method fired on item highlight exit
        /// </summary>
        void OnItemExit()
        {
            for (int i = 0; i < m_CurrentRenderers.Count; i++)
                m_CurrentRenderers[i].material.shader = m_OriginalShaders[i];
            m_OriginalShaders.Clear();
            m_CurrentRenderers.Clear();
            m_CurrObject = null;
        }

        /// <summary>
        /// method fired on switching highlighted items
        /// </summary>
        void OnItemSwitch()
        {
            for (int i = 0; i < m_CurrentRenderers.Count; i++)
            {
                m_CurrentRenderers[i].material.shader = m_OriginalShaders[i];
            }
            m_OriginalShaders.Clear();
            m_CurrentRenderers.Clear();
        }


        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
#if DEBUG_INFO
            if (!DisplayUI) { Debug.LogError("object cannot be null < " + this.ToString() + " >");return; }
            if (!m_PlayerCtrl) { /*Debug.LogError("object cannot be null < " + this.ToString() + " >");*/return; }
#endif
            DisplayUI.text = "";
            m_Highlighted = false;

            

            if (m_PlayerCtrl.disableInput ||
                m_PlayerCtrl.attackComboUnderway ||
                m_PlayerCtrl.blocking ||
                m_PlayerCtrl.triggerActive ) return;

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            int mask = layers; 
            RaycastHit hit;
            if(Physics.Raycast (ray, out hit, float.MaxValue ,mask))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Item"))
                {
                    float distance = Vector3.Distance(hit.point, m_PlayerCtrl.position);
                    if (distance < pickDistance)
                    {
                        {
                            Rigidbody attachedBody = hit.collider.attachedRigidbody;
                            if (attachedBody)
                            {
                                if (m_CurrObject != attachedBody.gameObject)
                                {
                                    if (m_CurrObject && m_OriginalShaders.Count > 0)
                                    {
                                        OnItemSwitch();
                                    }
                                    {
                                        OnItemEnter(hit);
                                    }
                                    m_CurrObject = attachedBody.gameObject;
                                }
                            }
                            else
                            {
                                if (m_CurrObject != hit.collider.gameObject)
                                {
                                    if (m_CurrObject && m_OriginalShaders.Count > 0)
                                    {
                                        OnItemSwitch();
                                    }

                                    {
                                        OnItemEnter(hit);
                                    }
                                    m_CurrObject = hit.collider.gameObject;
                                }
                            }
                        }



                        m_Highlighted = true;
                        if (m_CurrCollider == hit.collider)
                        {
                            DisplayUI.text = "Pick Up " + m_CurrItem.itemName;
                            if (m_CurrItem is MeleeWeaponItem && (m_CurrItem as MeleeWeaponItem).itemType == InventoryItemType.Weapon1H)
                            {
                                DisplayUI.text = "Press 'E' to pick as primary weapon or 'R' as secondary weapon " + m_CurrItem.name;
                                if (Input.GetButtonDown("Use"))
                                    m_PlayerCtrl.setNewItem(m_CurrItem);
                                else if (Input.GetButtonDown("SecondaryUse"))
                                    m_PlayerCtrl.setSecondaryItem(m_CurrItem as MeleeWeaponItem);
                            }
                            else
                            {
                                if (Input.GetButtonDown("Use"))
                                    m_PlayerCtrl.setNewItem(m_CurrItem);
                            }

                        }
                        else
                        {

                            Rigidbody attachedBody = hit.collider.attachedRigidbody;
                            InventoryItem item = attachedBody.GetComponent<InventoryItem>();
                            if (item)
                            {
                                m_CurrCollider = hit.collider;
                                m_CurrItem = item;
                                DisplayUI.text = "Pick Up " + item.itemName;
                                if (Input.GetButtonDown("Use"))
                                    m_PlayerCtrl.setNewItem(item);
                            }
#if DEBUG_INFO
                        else
                        {
                            Debug.LogWarning("Cannot find InventoryItem component! " + attachedBody.name);
                        }
#endif
                        }
                    }
                }
            }

            if (!m_Highlighted)
            {
                if (m_CurrObject && m_OriginalShaders.Count > 0)
                {
                    OnItemExit();
                }
            }
        }
    } 
}
