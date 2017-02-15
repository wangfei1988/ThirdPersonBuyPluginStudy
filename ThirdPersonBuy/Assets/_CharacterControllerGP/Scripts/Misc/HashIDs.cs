// © 2016 Mario Lelas
using UnityEngine;

namespace MLSpace
{
    /// <summary>
    /// holds hashes to  animator parameters
    /// </summary>
    public class HashIDs : MonoBehaviour
    {
        /// <summary>
        /// animator parameter hash
        /// </summary>
        [HideInInspector]
        public static int
            ForwardFloat,
            SideFloat,
            JumpFloat,
            JumpLegFloat,
            CrouchBool,
            OnGroundBool,
            StrafeBool,
            DiveRollBool,
            JumpOnBool,
            ClimbOnBool,
            JumpOverBool,
            MatchStartFloat,
            MatchEndFloat,
            LadderUpBool,
            LadderDownBool,
            LadderSizeInt,
            OnLedgeBool,
            GrabLedgeUpBool,
            GrabLedgeDownBool,
            GrabLedgeLeftBool,
            GrabLedgeRightBool,
            LedgeHangBool,
            Attack1Bool,
            JumpForwardBool,
            JumpDownBool,
            AttackSpeedFloat,
            BlockBool,
            TakeWeapon1HBool,
            TakeShieldBool,
            TakeWeapon2HBool,
            SheatheWeapon2HBool,
            UnarmedBool,
            AimBool,
            DrawBowBool,
            TakeBowBool,
            BowReleaseTrig,
            TakeArrowTrig,
            TakeSecondaryBool,
            TakeWeaponRightWaistBool,
            LeftHandClosedBool,
            RightHandClosedBool
            ;

        /// <summary>
        /// animator state hash
        /// </summary>
        public static int
            UnarmedAttackComboState,
            AttackComboState,
            UnarmedLocomotionState,
            DefaultLocomotionState;

        public static int
            TakeWeaponLeftState,
            TakeWeaponRightState,
            TakeShieldState,
            TakeWeaponShieldState,
            TakeSecondaryWeaponState,
            TakeDualWeaponsState,
            TakeSecWeaponShieldState,
            TakeWeapon2HState,
            SheatheWeapon2HState,
            TakeBowState,
            TakeArrowState;


        /// <summary>
        /// unity Awake method
        /// Awake is called when the script instance is being loaded
        /// </summary>
        void Awake()
        {
            //// parameters
            ForwardFloat = Animator.StringToHash("pForward");
            SideFloat = Animator.StringToHash("pSide");
            JumpFloat = Animator.StringToHash("pJump");
            JumpLegFloat = Animator.StringToHash("pJumpLeg");
            CrouchBool = Animator.StringToHash("pCrouch");
            OnGroundBool = Animator.StringToHash("pOnGround");
            StrafeBool = Animator.StringToHash("pStrafe");
            DiveRollBool = Animator.StringToHash("pDiveRoll");
            JumpOnBool = Animator.StringToHash("pJumpOn");
            ClimbOnBool = Animator.StringToHash("pClimbOn");
            JumpOverBool = Animator.StringToHash("pJumpOver");
            MatchStartFloat = Animator.StringToHash("pMatchStart");
            MatchEndFloat = Animator.StringToHash("pMatchEnd");
            LadderUpBool = Animator.StringToHash("pLadderUp");
            LadderDownBool = Animator.StringToHash("pLadderDown");
            LadderSizeInt = Animator.StringToHash("pLadderSize");
            OnLedgeBool = Animator.StringToHash("pOnLedge");
            GrabLedgeUpBool = Animator.StringToHash("pGrabLedgeUp");
            GrabLedgeDownBool = Animator.StringToHash("pGrabLedgeDown");
            GrabLedgeLeftBool = Animator.StringToHash("pGrabLedgeLeft");
            GrabLedgeRightBool = Animator.StringToHash("pGrabLedgeRight");
            LedgeHangBool = Animator.StringToHash("pLedgeHang");
            Attack1Bool = Animator.StringToHash("pAttack1");
            JumpForwardBool = Animator.StringToHash("pJumpForward");
            JumpDownBool = Animator.StringToHash("pJumpDown");
            AttackSpeedFloat = Animator.StringToHash("pAttackSpeed");
            BlockBool = Animator.StringToHash("pBlock");
            TakeWeapon1HBool = Animator.StringToHash("pTakeWeapon1H");
            TakeShieldBool = Animator.StringToHash("pTakeShield");
            TakeWeapon2HBool = Animator.StringToHash("pTakeWeapon2H");
            SheatheWeapon2HBool = Animator.StringToHash("pSheatheWeapon2H");
            UnarmedBool = Animator.StringToHash("pUnarmed");
            AimBool = Animator.StringToHash("pAim");
            DrawBowBool = Animator.StringToHash("pDrawBow");
            TakeBowBool = Animator.StringToHash("pTakeBow");
            BowReleaseTrig = Animator.StringToHash("pBowRelease");
            TakeArrowTrig = Animator.StringToHash("pTakeArrow");
            TakeSecondaryBool = Animator.StringToHash("pTakingSecondary");
            TakeWeaponRightWaistBool = Animator.StringToHash("pTakingWeaponRightWaist");
            LeftHandClosedBool = Animator.StringToHash("pLeftHandClosed");
            RightHandClosedBool = Animator.StringToHash("pRightHandClosed");

            UnarmedAttackComboState = Animator.StringToHash("UnarmedAttackCombo");
            AttackComboState = Animator.StringToHash("AttackCombo");
            UnarmedLocomotionState = Animator.StringToHash("UnarmedLocomotion");
            DefaultLocomotionState = Animator.StringToHash("DefaultLocomotion");

            TakeWeaponLeftState = Animator.StringToHash("TakeWeaponRightWaist");
            TakeWeaponRightState = Animator.StringToHash("TakeWeaponLeftWaist");
            TakeShieldState = Animator.StringToHash("TakeShield");
            TakeWeaponShieldState = Animator.StringToHash("TakeWeaponShield");
            TakeSecondaryWeaponState = Animator.StringToHash("TakeSecondaryWeapon");
            TakeDualWeaponsState = Animator.StringToHash("TakeDualWeapons");
            TakeSecWeaponShieldState = Animator.StringToHash("TakeSecWeaponShield");
            TakeWeapon2HState = Animator.StringToHash("TakeWeapon2H");
            SheatheWeapon2HState = Animator.StringToHash("SheatheWeapon2H");
            TakeBowState = Animator.StringToHash("TakeBow");
            TakeArrowState = Animator.StringToHash("TakeArrow");
        }

    }
    
}