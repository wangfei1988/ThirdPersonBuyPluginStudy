// © 2015 Mario Lelas
using UnityEngine;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
using UnityEngine.SceneManagement;
#endif

namespace MLSpace
{
    /// <summary>
    /// Pauses and unpauses game
    /// Toggles controls info text
    /// Sets player hit interval 
    /// </summary>
    public class GameControl : MonoBehaviour
    {
        /// <summary>
        /// Information text UI
        /// </summary>
        public UnityEngine.UI.Text InfoUI;

        /// <summary>
        /// hide cursor on start
        /// </summary>
        public bool hideCursor = true;

        /// <summary>
        /// game time scale
        /// </summary>
        [Range (0.0f,2.0f)]
        public float timeScale = 1.0f;

        /// <summary>
        /// text shown when ckicked F1
        /// </summary>
        [Multiline]
        public string InfoText = "Press F1 to hide controls" +
                        "\nW - Forward" +
                        "\nS - Back" +
                        "\nA - Left" +
                        "\nD - Right"
                        ;

        /// <summary>
        /// black image ( or other ) used for fading
        /// </summary>
        public UnityEngine.UI.Image m_fade_black_image;

        /// <summary>
        /// pause ui text for screen notification
        /// </summary>
        public UnityEngine.UI.Text pauseUI;

        private bool m_Paused = false;                  // is game paused flag
        private bool m_ShowInfo = false;                // show controls info text flag
        private UnityEngine.UI.Image m_startImage;      // current fade start image
        private float m_fadeTimer = 0.0f;               // fade timer this value will change in game ，is a tmp value。 and maxfadetime is 1 
        private VoidFunc fadeFunc = null;               // fade callback
        private float m_fadeSpeed = 0.2f;               // fade speed

        private float restartTimer = 0.0f;              // restart level timer， this value will change in game ，is a tmp value
        private float restartTime = 7.0f;               // restart max time，this value will not change in game

        private bool restartLevel = false;              // is time to restart level
        private NPCManager m_NpcManager;                // npc manager reference

        private PlayerControlBase m_Player;             // player reference. we controll m_Player

        /// <summary>
        /// Unity Start method
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            if (hideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            GameObject pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo)
            {
                if (pgo.activeSelf)
                {
                    PlayerControlBase pc = pgo.GetComponent<PlayerControlBase>();
                    if (pc)
                    {
                        m_Player = pc;
                        if (pc.enabled)
                        {
                            pc.OnDeath = _playerDeathFlag;
                        }
                    }
                    //else
                    //{
                    //    Debug.LogError("Cannot find 'PlayerControlBase' component. < " + pgo.name );
                    //}
                }
            }
            else
            {
                Debug.LogError("Cannot find game object with tag 'Player'. < ");
            }

            if (m_fade_black_image)
            {
                m_startImage = m_fade_black_image;
                fadeFunc = _fadeBlack2Clear;
            }

            if (pauseUI)
                pauseUI.gameObject.SetActive(false);

            m_NpcManager = GetComponent<NPCManager>();
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
            if (fadeFunc != null) fadeFunc(); //fadeFunc run in update function。 actually we suggest run in croutine
            if (restartLevel)
            {
                restartTimer += Time.deltaTime;
                if (restartTimer > restartTime)
                {
                    if (m_fade_black_image)
                    {
                        fadeFunc = _fade2BlackAndRestart;
                    }
                    else
                    {
                        _restartCurrentLevel();
                    }
                }
            }

            if(Input.GetKeyDown (KeyCode .Z))
            {
                if(m_NpcManager)
                {
                    m_NpcManager.reviveAll();
                }
            }

            // lockand center cursor in webgl on left mouse down
#if UNITY_WEBGL
            if(Input.GetMouseButtonDown(0))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
#endif

            if(Input.GetKeyDown (KeyCode .T))
            {
                m_Player.enableJumpToTarget = !m_Player.enableJumpToTarget;
            }

            // quit 
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            // restart
            if (Input.GetButtonDown("Submit"))
                _restartCurrentLevel();

            if (!m_Paused)
            {
                Time.timeScale = timeScale;
            }


            // pause / unpause
            if (Input.GetButtonDown("Pause"))
            {
                if (m_Paused)
                {
                    Time.timeScale = 1.0f;
                    m_Paused = false;
                    if (pauseUI)
                        pauseUI.gameObject.SetActive(false);
                }
                else
                {
                    Time.timeScale = 0.0f;
                    m_Paused = true;
                    if (pauseUI)
                        pauseUI.gameObject.SetActive(true);
                }
            }


            // toggle text info
            if (Input.GetKeyDown(KeyCode.F1))
                m_ShowInfo = !m_ShowInfo;

            if (InfoUI)
            {
                if (m_ShowInfo)
                {
                    InfoUI.text = InfoText;
                }
                else
                {
                    InfoUI.text = "Press F1 to show controls";
                }
            }

            ArrowPool.UpdateArrows();
        }

        /// <summary>
        /// set flag to restart level on player death
        /// </summary>
        private void _playerDeathFlag()
        {
            restartLevel = true;
        }

        /// <summary>
        /// restart current level
        /// </summary>
        private void _restartCurrentLevel()
        {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#else
            Application.LoadLevel(Application.loadedLevel);
#endif
            ArrowPool.ClearArrows();
            restartLevel = false;
            restartTimer = 0.0f;
        }

        /// <summary>
        /// fade to black and restart
        /// </summary>
        private void _fade2BlackAndRestart()
        {
            m_fadeTimer += m_fadeSpeed * Time.deltaTime;
            // Lerp the colour of the texture between itself and transparent.
            Color color = Color.Lerp(m_startImage.color, Color.black, m_fadeTimer * m_fadeTimer);
            m_fade_black_image.color = color;
            if (m_fadeTimer > 1)
            {
                m_fadeTimer = 0.0f;
                fadeFunc = null;
                _restartCurrentLevel();
            }
        }

        /// <summary>
        /// fade from black to clear
        /// </summary>
        private void _fadeBlack2Clear()
        {
            m_fadeTimer += m_fadeSpeed * Time.deltaTime;
            // Lerp the colour of the texture between itself and transparent.
            Color color = Color.Lerp(m_startImage.color, Color.clear, m_fadeTimer * m_fadeTimer);
            m_fade_black_image.color = color;
            if (m_fadeTimer > 1) //
            {
                m_fadeTimer = 0.0f;
                fadeFunc = null;
            }
        }

        /// <summary>
        /// fade from clear to black
        /// </summary>
        /// <param name="restart">restart fading to clear</param>
        private void _fadeClear2Black(bool restart)
        {
            m_fadeTimer += m_fadeSpeed * Time.deltaTime;
            // Lerp the colour of the texture between itself and transparent.
            Color color = Color.Lerp(m_startImage.color, Color.black, m_fadeTimer * m_fadeTimer);
            m_fade_black_image.color = color;
            if (m_fadeTimer > 1)
            {
                m_fadeTimer = 0.0f;
                if (restart) fadeFunc = _fadeBlack2Clear;
                else fadeFunc = null;
            }
        }

    } 
}
