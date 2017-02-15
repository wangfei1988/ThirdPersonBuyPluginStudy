// © 2016 Mario Lelas
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MLSpace
{
    /// <summary>
    /// floating text info holder class
    /// </summary>
    public class FloatTextInfo
    {
        /// <summary>
        /// UI.Text component reference
        /// </summary>
        public Text textui;

        /// <summary>
        /// current position of text
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// lifetime duration
        /// </summary>
        public float time;

        /// <summary>
        /// life timer
        /// </summary>
        public float timer;
    }

    /// <summary>
    /// script for debug info 
    /// has two modes 
    /// 1. simple text display
    /// 2. instances of texts that floats upwards and disappear
    /// NOTE: its not using pool - its instantiating text on every 'setText()'
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        public enum TextType { Normal, Float };

        /// <summary>
        /// current text type
        /// </summary>
        public TextType textType = TextType.Normal;

        /// <summary>
        /// text color
        /// </summary>
        public Color _color = Color.yellow;

        /// <summary>
        /// UI Text component ( normal mode )
        /// </summary>
        public Text m_debugText;

        /// <summary>
        /// speed of text float up in float mode
        /// </summary>
        public float floatSpeed = 1.0f;

        /// <summary>
        /// list of UI.Text components ( float mode )
        /// </summary>
        public List<FloatTextInfo> m_debugTexts = new List<FloatTextInfo>();

        /// <summary>
        /// display text flag
        /// </summary>
        private bool m_display = true;

        

        /// <summary>
        /// initialized flag
        /// </summary>
        private bool m_Initialized = false;

        /// <summary>
        /// gets and sets text display flag
        /// </summary>
        public bool display
        {
            get { return m_display; }
            set
            {
#if DEBUG_INFO
                if (!m_debugText)
                {
                    Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
                }
                else
                {
                    m_display = value;
                    m_debugText.enabled = value;
                    m_debugText.gameObject.SetActive(value);
                }
#else
                m_display = value;
                m_debugText.enabled = value;
                m_debugText.gameObject.SetActive(value);
#endif
            }
        }

        /// <summary>
        /// gets and sets visibility flag
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// canvas component
        /// </summary>
        private Canvas canvas;


        /// <summary>
        /// create UI.Text component
        /// </summary>
        /// <returns></returns>
        private GameObject createUI()
        {
            GameObject debugUIObj = (GameObject)Instantiate(Resources.Load("DebugUIText"));
            if (!debugUIObj) { Debug.LogError("cannot find object 'Resources/DebugUIText' - " + " < " + this.ToString() + ">" + this.name); return null; }
            m_debugText = debugUIObj.GetComponent<UnityEngine.UI.Text>();
            if (!m_debugText) { Debug.LogError("cannot find component 'UnityEngine.UI.Text' - " + " < " + this.ToString() + ">"); return null; }

            return debugUIObj;
        }

        /// <summary>
        /// initIalize component
        /// </summary>
        public void initialize()
        {
            if (m_Initialized) return;
            canvas = GameObject.FindObjectOfType<Canvas>();
            if (!canvas)
            {
                GameObject canvasGO = (GameObject)Instantiate(Resources.Load("Canvas"));
                if (!canvasGO) Debug.LogError("cannot find object 'Resources/Canvas' - " + " < " + this.ToString() + ">");
                canvas = canvasGO.GetComponent<Canvas>();
                if (!canvas) { Debug.LogError("Cannot find 'Canvas' component" + " < " + this.ToString() + ">"); return; }
            }


            if (m_debugText)
            {
                if (Utils.IsPrefab(m_debugText.gameObject))
                {
                    m_debugText = (Text)Instantiate(m_debugText);
                    if (!m_debugText) Debug.LogError("object cannot be null" + " < " + this.ToString() + ">");
                    m_debugText.gameObject.transform.SetParent(canvas.transform);
                }
            }
            if (!m_debugText)
            {
                createUI();
                m_debugText.gameObject.transform.SetParent(canvas.transform);
            }

            m_debugText.transform.SetAsFirstSibling();
            m_debugText.gameObject.name = this.name + "DebugUIText";
            m_debugText.text = "";
            m_debugText.color = _color;
            Visible = true;
            m_Initialized = true;
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
        /// Unity LateUpdate method
        /// LateUpdate is called every frame, if the Behaviour is enabled
        /// </summary>
        void LateUpdate()
        {
#if DEBUG_INFO
            if (!m_debugText) { Debug.LogError("object cannot be null - " + " < " + this.ToString() + ">"); return; }
#endif

            if (!m_display) return;


            switch (textType)
            {
                case TextType.Normal: _updateNormal(); break;
                case TextType.Float: _updateFloat(); break;
            }
        }

        /// <summary>
        /// set text to display
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="time">display tiome</param>
        /// <param name="color">display color</param>
        public void setText(string text, float time = 1.0f, Color? color = null )
        {
#if DEBUG_INFO
            if (!m_debugText) { Debug.LogError("object cannot be null: " + this.name + ", " + this.ToString ());return; }
#endif
            switch (textType)
            {
                case TextType.Normal:
                    {
                        if (!m_debugText) return;
                        m_debugText.text = text;
                        if (m_debugText.text == "")
                            m_debugText.enabled = false;
                        //else m_debugText.enabled = true;
                    }
                    break;
                case TextType.Float:
                    {
                        FloatTextInfo fti = new FloatTextInfo();
                        fti.time = time;
                        fti.timer = 0.0f;
                        fti.position = transform.position + Vector3.up * 2;
                        fti.textui = (Text)Instantiate(m_debugText);
                        fti.textui.transform.SetParent(canvas.transform);
                        fti.textui.text = text;
                        if (color.HasValue) fti.textui.color = color.Value;
                        m_debugTexts.Add(fti);
                    }
                    break;
            }

        }

        /// <summary>
        /// update text normal mode
        /// </summary>
        private void _updateNormal()
        {

            m_debugText.gameObject.SetActive(Visible);
            m_debugText.enabled = Visible;
            if (Visible)
            {
                Vector3 currentPoint = transform.position + Vector3.up * 2;

                Vector3 toTarget = (Camera.main.transform.position - currentPoint);
                float angle = Vector3.Angle(Camera.main.transform.forward, toTarget.normalized);
                if (angle < 90)
                {
                    m_debugText.gameObject.SetActive(false);
                    m_debugText.enabled = false;
                    return;
                }

                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentPoint);
                m_debugText.transform.position = screenPos;

                float len = toTarget.magnitude;
                Ray ray = new Ray(currentPoint, toTarget.normalized);
                RaycastHit rayhit;
                m_debugText.enabled = true;
                int layer = 1 << LayerMask.NameToLayer("Default");
                layer |= 1 << LayerMask.NameToLayer("NPCLayer");
                layer |= 1 << LayerMask.NameToLayer("PlayerLayer");
                if (Physics.Raycast(ray,
                    out rayhit, len, layer))
                {
                    m_debugText.gameObject.SetActive(false);
                    m_debugText.enabled = false;
                }
            }
        }

        /// <summary>
        /// update text float mode
        /// </summary>
        private void _updateFloat()
        {
            for (int i = 0; i < m_debugTexts.Count; i++)
            {
                FloatTextInfo fti = m_debugTexts[i];
                Text txt = fti.textui;
                fti.timer += Time.deltaTime * floatSpeed;
                if (fti.time < fti.timer)
                {
                    m_debugTexts.Remove(fti);
                    Destroy(txt.gameObject );
                    continue;
                }


                txt.gameObject.SetActive(Visible);
                txt.enabled = Visible;
                Vector3 currentPoint = transform.position + Vector3.up * 2;
                currentPoint.y += fti.timer;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentPoint);
                txt.transform.position = screenPos;



            }

        }
    }
}
