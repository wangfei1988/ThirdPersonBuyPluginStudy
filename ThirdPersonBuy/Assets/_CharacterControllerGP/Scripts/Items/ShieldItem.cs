// © 2015 Mario Lelas
using System.Collections.Generic;
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// shield item class
    /// </summary>
    public class ShieldItem : WeaponItem
    {
        /// <summary>
        /// returns shield item type
        /// </summary>
        public override InventoryItemType itemType { get { return InventoryItemType.Shield; } }

        /// <summary>
        /// replacement clips list
        /// </summary>
        public List<AnimationClipReplacementInfo> replacement_clips;

        /// <summary>
        /// attack combo clip
        /// </summary>
        public AnimationClipReplacementInfo attackClip;

        /// <summary>
        /// blocking stance clip
        /// </summary>
        public AnimationClipReplacementInfo blockClip;

        /// <summary>
        ///  block hit clips
        /// </summary>
        public List<AnimationClipReplacementInfo> blockHitClips;

        /// <summary>
        /// locomotion clips
        /// </summary>
        public List<AnimationClipReplacementInfo> locomotionClips;



        /// <summary>
        /// initializes melee weapon component
        /// </summary>
        public override bool initialize()
        {
            if (m_Initialized) return true;
            base.initialize();

            // collecting all hashes in awake func

            attackClip.orig_hash = Animator.StringToHash(attackClip.original_name);
            blockClip.orig_hash = Animator.StringToHash(blockClip.original_name);

            for (int i = 0; i < blockHitClips.Count; i++)
            {
                blockHitClips[i].orig_hash =
                    Animator.StringToHash(blockHitClips[i].original_name);
            }
            for (int i = 0; i < replacement_clips.Count; i++)
            {
                replacement_clips[i].orig_hash =
                    Animator.StringToHash(replacement_clips[i].original_name);
            }
            for (int i = 0; i < locomotionClips.Count; i++)
            {
                locomotionClips[i].orig_hash =
                    Animator.StringToHash(locomotionClips[i].original_name);
            }
            m_ItemType = InventoryItemType.Shield;

            return m_Initialized;
        }
    }
}
