// © 2015 Mario Lelas
using UnityEngine;
using System.Collections.Generic;

namespace MLSpace
{


    /// <summary>
    /// melee weapon item class
    /// </summary>
    public class MeleeWeaponItem : WeaponItem
    {
        /// <summary>
        /// melee weapon types
        /// </summary>
        public enum MeleeWeaponEnum { Weapon1H, Weapon2H };                

        /// <summary>
        /// current melee weapon type
        /// </summary>
        [HideInInspector]
        public MeleeWeaponEnum meleeWeaponType = MeleeWeaponEnum.Weapon1H;

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

            attackClip.orig_hash = Animator.StringToHash(attackClip.original_name);
            blockClip.orig_hash = Animator.StringToHash(blockClip.original_name);
            for (int i = 0;i<blockHitClips .Count;i++)
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

            if (meleeWeaponType == MeleeWeaponEnum.Weapon1H) m_ItemType = InventoryItemType.Weapon1H;
            else if (meleeWeaponType == MeleeWeaponEnum.Weapon2H) m_ItemType = InventoryItemType.Weapon2H;

            return m_Initialized;
        }

    }
}
