// © 2016 Mario Lelas
using UnityEngine;
using UnityEngine.UI;

namespace MLSpace
{
    /// <summary>
    /// displays character health bar
    /// </summary>
    public class HealthUI : MonoBehaviour
    {
        /// <summary>
        /// health bar tint color
        /// </summary>
        [Tooltip ("Health bar tint color")]
        public Color _color = Color.yellow;

        /// <summary>
        /// health bar image
        /// </summary>
        [Tooltip ("Health bar image")]
        public Image m_Image;

        /// <summary>
        /// health bar image x scaling
        /// </summary>
        [HideInInspector]
        public float scaleX = 1.0f;

        /// <summary>
        /// display text flag
        /// </summary>
        private bool m_display = true;

        /// <summary>
        /// initialized flag
        /// </summary>
        private bool m_Initialized = false; 



        /// <summary>
        /// gets and sets image display flag
        /// </summary>
        public bool display
        {
            get { return m_display; }
            set
            {
#if DEBUG_INFO
                if(!m_Image)
                {
                    Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
                }
                else
                {
                    m_display = value;
                    m_Image.enabled = value;
                    m_Image.gameObject.SetActive(value);
                }
#else
                m_display = value;
                m_Image.enabled = value;
                m_Image.gameObject.SetActive(value);
#endif
            }
        }

        /// <summary>
        /// gets and sets visibility flag
        /// </summary>
        public bool visible { get; set; }



        /// <summary>
        /// initialize component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;

            //GameObject canvas = GameObject.FindGameObjectWithTag("Canvas");
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (!canvas)
            {
                GameObject canvasGO = (GameObject)Instantiate(Resources.Load("Canvas"));
                if (!canvasGO) { Debug.LogError("cannot find object 'Resources/Canvas' - " + " < " + this.ToString() + ">"); return; }
                canvas = canvasGO.GetComponent<Canvas>();
                if (!canvas) { Debug.LogError("Cannot find 'Canvas' component" + " < " + this.ToString() + ">"); return; }
            }


            if (m_Image)
            {
                if (Utils.IsPrefab(m_Image.gameObject))
                {
                    m_Image = (Image)Instantiate(m_Image);
                    if (!m_Image) Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
                    m_Image.gameObject.transform.SetParent(canvas.transform);
                }
            }
            if (!m_Image)
            {
                if (!_createUI()) return;
                m_Image.gameObject.transform.SetParent(canvas.transform);
            }

            m_Image.transform.SetAsFirstSibling();
            m_Image.gameObject.name = this.name + "HealthUI";
            m_Image.color = _color;
            visible = true;

            m_Initialized = true;
        }

        /// <summary>
        /// create health ui from resources
        /// </summary>
        /// <returns>return true if successfull otherwise false</returns>
        private bool _createUI()
        {
            GameObject debugUIObj = (GameObject)Instantiate(Resources.Load("HealthUI"));
            if (!debugUIObj) { Debug.LogError("cannot find object 'Resources/HealthUI' - " + " < " + this.ToString() + ">"); return false; }
            m_Image = debugUIObj.GetComponent<UnityEngine.UI.Image>();
            if (!m_Image) { Debug.LogError("cannot find component 'UnityEngine.UI.Image' - " + " < " + this.ToString() + ">"); return false; }
            return true;
        }

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            initialize();
        }

        /// <summary>
        /// Unity FixedUpdate method
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
        /// </summary>
        void FixedUpdate()
        {
#if DEBUG_INFO
            if (!m_Image) { Debug.LogError("object cannot be null - " + " < " + this.ToString() + ">"); return; }
#endif



            if (m_display)
            {
                scaleX = Mathf.Max(0.0f, scaleX);
                m_Image.transform.localScale = new Vector3(scaleX, 1.0f, 1.0f);
                m_Image.color = Color.Lerp(Color.red, _color,  scaleX);

                m_Image.gameObject.SetActive(visible);
                m_Image.enabled = visible;
                if (visible)
                {
                    Vector3 currentPoint = transform.position + Vector3.up * 2;

#if DEBUG_INFO
                    if(!Camera.main )
                    {
                        Debug.LogError("Cannot find main camera!" + " < " + this.ToString() + ">");
                        return;
                    }
#endif
                    Vector3 toTarget = (Camera.main.transform.position - currentPoint);
                    float angle = Vector3.Angle(Camera.main.transform.forward, toTarget.normalized);
                    if (angle < 90)
                    {
                        m_Image.gameObject.SetActive(false);
                        m_Image.enabled = false;
                        return;
                    }

                    Vector3 screenPos = Camera.main.WorldToScreenPoint(currentPoint);
                    m_Image.transform.position = screenPos;

                    float len = toTarget.magnitude;
                    Ray ray = new Ray(currentPoint, toTarget.normalized);
                    RaycastHit rayhit;
                    m_Image.enabled = true;
                    int layer = 1 << LayerMask.NameToLayer("Default");
                    layer |= 1 << LayerMask.NameToLayer("DefaultSlope");
                    layer |= 1 << LayerMask.NameToLayer("NPCLayer");
                    layer |= 1 << LayerMask.NameToLayer("PlayerLayer");
                    if (Physics.Raycast(ray,
                        out rayhit, len, layer))
                    {
                        m_Image.gameObject.SetActive(false);
                        m_Image.enabled = false;
                    }
                }
            }
        }

    } 
}
