using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement
{
    [AddComponentMenu("XR/Locomotion/Continuous Move Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_ContinuousMoveProvider)]
    public class ContinuousMoveProvider : LocomotionProvider
    {
        [SerializeField]
        float m_MoveSpeed = 1f;
        public float moveSpeed
        {
            get => m_MoveSpeed;
            set => m_MoveSpeed = value;
        }

        [SerializeField]
        bool m_EnableStrafe = true;
        public bool enableStrafe
        {
            get => m_EnableStrafe;
            set => m_EnableStrafe = value;
        }

        [SerializeField]
        bool m_EnableFly;
        public bool enableFly
        {
            get => m_EnableFly;
            set => m_EnableFly = value;
        }

        [SerializeField]
        bool m_UseGravity = true;
        public bool useGravity
        {
            get => m_UseGravity;
            set => m_UseGravity = value;
        }

        [SerializeField]
        Transform m_ForwardSource;
        public Transform forwardSource
        {
            get => m_ForwardSource;
            set => m_ForwardSource = value;
        }

        public XRInputValueReader<Vector2> leftHandMoveInput { get; set; }
        public XRInputValueReader<Vector2> rightHandMoveInput { get; set; }

        CharacterController m_CharacterController;
        bool m_AttemptedGetCharacterController;
        bool m_IsMovingXROrigin;
        Vector3 m_VerticalVelocity;

        // Ajout de variables pour la pause
        [SerializeField]
        float m_PauseDuration = 1f; // Durée de la pause en secondes

        float m_PauseTimer; // Timer pour la pause

        protected override void Awake()
        {
            base.Awake();
            m_PauseTimer = 0f;
        }

        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            var xrOrigin = mediator.xrOrigin;
            if (xrOrigin == null)
                return Vector3.zero;

            var inputMove = Vector3.ClampMagnitude(new Vector3(m_EnableStrafe ? input.x : 0f, 0f, input.y), 1f);
            var forwardSourceTransform = m_ForwardSource == null ? xrOrigin.Camera.transform : m_ForwardSource;
            var inputForwardInWorldSpace = forwardSourceTransform.forward;
            var originTransform = xrOrigin.Origin.transform;
            var speedFactor = m_MoveSpeed * Time.deltaTime * originTransform.localScale.x;

            if (m_PauseTimer <= 0f)
            {
                var translationInRigSpace = Quaternion.FromToRotation(originTransform.forward, inputForwardInWorldSpace) * inputMove * speedFactor;
                var translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);
                m_PauseTimer = m_PauseDuration; // Commence la pause après avoir calculé le mouvement
                return translationInWorldSpace;
            }
            else
            {
                m_PauseTimer -= Time.deltaTime;
                return Vector3.zero; // Retourne aucun mouvement pendant la pause
            }
        }

        protected override void MoveRig(Vector3 translationInWorldSpace)
        {
            var xrOrigin = mediator.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            FindCharacterController();

            var motion = translationInWorldSpace;

            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                if (m_CharacterController.isGrounded || !m_UseGravity || m_EnableFly)
                {
                    m_VerticalVelocity = Vector3.zero;
                }
                else
                {
                    m_VerticalVelocity += Physics.gravity * Time.deltaTime;
                }

                motion += m_VerticalVelocity * Time.deltaTime;
            }

            TryStartLocomotionImmediately();

            if (locomotionState != LocomotionState.Moving)
                return;

            m_IsMovingXROrigin = true;
            transformation.motion = motion;
            TryQueueTransformation(transformation);
        }

        void FindCharacterController()
        {
            var xrOrigin = mediator.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                if (!xrOrigin.TryGetComponent(out m_CharacterController) && xrOrigin != mediator.xrOrigin.gameObject)
                    mediator.xrOrigin.TryGetComponent(out m_CharacterController);

                m_AttemptedGetCharacterController = true;
            }
        }
    }
}
