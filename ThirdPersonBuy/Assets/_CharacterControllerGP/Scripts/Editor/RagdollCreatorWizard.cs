using UnityEngine;
using UnityEditor;

namespace MLSpace
{
    /// <summary>
    /// Ragdoll creator scriptable wizard
    /// </summary>
    public class RagdollCreatorWizard : ScriptableWizard
    {
        public RagdollCreator ragdollCreator;

        public static RagdollCreatorWizard DisplayWizard()
        {
            RagdollCreatorWizard builder = (RagdollCreatorWizard)DisplayWizard("Create Ragdoll", typeof(RagdollCreatorWizard));
            builder.ragdollCreator = new RagdollCreator();
            return builder;
        }

        void OnWizardCreate()
        {
            ragdollCreator.CheckConsistency();
            ragdollCreator.CalculateAxes();
            ragdollCreator.Create();
        }
    } 
}
