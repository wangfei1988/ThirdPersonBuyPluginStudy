// © 2016 Mario Lelas

#define COLLECT_ON_ENTER

using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{
    /// <summary>
    /// set object under plaform control when standing on it
    /// </summary>
    public class Platform : MonoBehaviour
    {
        // current platformd objects ( transforms )
        private List<Transform> platformed = new List<Transform>();

#if COLLECT_ON_ENTER

        /// <summary>
        /// Unity OnTriggerExit method
        /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
        /// </summary>
        /// <param name="col">collider exiting the trigger</param>
        void OnTriggerExit(Collider col)
        {
            if(platformed .Contains(col.transform ))
            {
                col.transform.SetParent(null, true);
                platformed.Remove(col.transform);
            }
        }


        /// <summary>
        /// Unity OnTriggerEnter method
        /// OnTriggerEnter is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="col">collider entering the trigger</param>
        void OnTriggerEnter(Collider col)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (!rb) return;
            if (!platformed.Contains(col.transform))
            {
                col.transform.SetParent(this.transform, true);
                platformed.Add(col.transform);
            }
        }
#else
        /// <summary>
        /// Unity FixedUpdate method
        /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
        /// </summary>
        void FixedUpdate()
        {
            for (int i = 0; i < platformed.Count; i++)
            {
                platformed[i].SetParent(null, true);
            }
            platformed.Clear();
        }

        /// <summary>
        /// Unity OnTriggerStay method
        /// OnTriggerStay is called once per frame for every Collider other that is touching the trigger
        /// </summary>
        /// <param name="col">collider staying in the trigger</param>
        void OnTriggerStay(Collider col)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (!rb) return;
            if (!platformed.Contains(col.transform))
            {
                col.transform.SetParent(this.transform, true);
                platformed.Add(col.transform);
            }
        }
#endif

    }

}
