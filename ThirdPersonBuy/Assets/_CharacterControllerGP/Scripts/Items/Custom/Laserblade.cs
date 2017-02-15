// © 2016 Mario Lelas
using UnityEngine;


namespace MLSpace
{
    /// <summary>
    /// custom implementation of MeleeWeaponItem
    /// </summary>
    public class Laserblade : MeleeWeaponItem 
    {
        /// <summary>
        /// laserblade states
        /// </summary>
        private enum LaserbladeState { Draw, Sheathe, None };

        /// <summary>
        /// laserblade blade transform
        /// </summary>
        public Transform blade;

        /// <summary>
        /// original scale of blade transform Y
        /// </summary>
        public float original_scale;

        /// <summary>
        /// scale of blade transform in drawn state
        /// </summary>
        public float drawn_scale;

        /// <summary>
        /// clip played on laserblade draw
        /// </summary>
        public AudioClip drawClip;

        /// <summary>
        /// clip played on laserblade sheathe
        /// </summary>
        public AudioClip sheatheClip;

        /// <summary>
        /// constant humming sound of laserblade when drawn
        /// </summary>
        public AudioClip hummClip;



        private AudioSource m_AudioSource;      // reference to audio source
        private LaserbladeState m_State =       //
            LaserbladeState.None;               // current laserblade action 
        private float m_DrawTimer = 0.0f;       // drawing blade timer 
        private float m_DrawTime = 0.1f;        // maximum blade drawing time
        private bool m_Drawn = false;           // is blade drawn ?

        
        /// <summary>
        /// Unity Start method
        /// is called on the frame when a script is enabled just before any of the Update methods is called the first time
        /// </summary>
        void Start()
        {
            m_AudioSource = GetComponent<AudioSource>();
            if (!m_AudioSource)
                m_AudioSource = this.gameObject.AddComponent<AudioSource>();
            m_AudioSource.clip = hummClip;
            m_AudioSource.loop = true;
            m_AudioSource.spatialBlend = 1.0f;
            m_AudioSource.playOnAwake = false;
            m_AudioSource.Stop();

            this.OnTake = drawBlade;
            this.OnStartSheathing = sheatheBlade;
        }

        /// <summary>
        /// play draw blade clip and start scaling blade transform
        /// </summary>
        public void drawBlade()
        {
            if (m_Drawn) return;

            m_State = LaserbladeState.Draw;
            m_DrawTimer = 0.0f;
            AudioSource.PlayClipAtPoint(drawClip, transform.position);
            m_AudioSource.Play();
            m_Drawn = true;
        }

        /// <summary>
        /// play sheathe blade clip and start scaling blade transform
        /// </summary>
        public void sheatheBlade()
        {
            if (!m_Drawn) return;

            m_State = LaserbladeState.Sheathe;
            m_DrawTimer = 0.0f;
            m_AudioSource.Stop();
            AudioSource.PlayClipAtPoint(sheatheClip, transform.position);
            m_Drawn = false;
        }

        /// <summary>
        /// drop item
        /// </summary>
        /// <param name="pos">drop at position</param>
        /// <param name="rot">drop at rotation</param>
        public override void dropItem(Vector3? pos, Quaternion? rot)
        {
            sheatheBlade();
            base.dropItem(pos, rot);
            
        }

        /// <summary>
        /// reset item at starting position / state
        /// </summary>
        public override void resetItem()
        {
            base.resetItem();
            sheatheBlade();
        }

        /// <summary>
        /// Unity Update method
        /// Update is called every frame, if the MonoBehaviour is enabled
        /// </summary>
        void Update()
        {
            if (m_State == LaserbladeState.Draw)
            {
                m_DrawTimer += Time.deltaTime;
                if(m_DrawTimer >= m_DrawTime)
                {
                    m_State = LaserbladeState.None;
                    blade.localScale = new Vector3(1f, drawn_scale, 1f);
                    return;
                }
                float val = Mathf.Clamp01(m_DrawTimer / m_DrawTime);
                float curScale = Mathf.Lerp(original_scale, drawn_scale, val);
                blade.localScale = new Vector3(1f, curScale, 1f);

            }
            else if (m_State == LaserbladeState.Sheathe)
            {
                m_DrawTimer += Time.deltaTime;
                if (m_DrawTimer >= m_DrawTime)
                {
                    m_State = LaserbladeState.None;
                    blade.localScale = new Vector3(1f, original_scale, 1f);
                    return;
                }
                float val = Mathf.Clamp01(m_DrawTimer / m_DrawTime);
                float curScale = Mathf.Lerp(drawn_scale, original_scale, val);
                blade.localScale = new Vector3(1f, curScale, 1f);
            }
        }


    } 
}
