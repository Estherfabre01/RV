using Unity.XR.CoreUtils;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    public class DynamicMoveProvider : ContinuousMoveProvider
    {
        public enum MovementDirection
        {
            HeadRelative,
            HandRelative,
        }

        [Space, Header("Movement Direction")]
        [SerializeField]
        Transform m_HeadTransform;

        public Transform headTransform
        {
            get => m_HeadTransform;
            set => m_HeadTransform = value;
        }

        [SerializeField]
        Transform m_LeftControllerTransform;

        public Transform leftControllerTransform
        {
            get => m_LeftControllerTransform;
            set => m_LeftControllerTransform = value;
        }

        [SerializeField]
        Transform m_RightControllerTransform;

        public Transform rightControllerTransform
        {
            get => m_RightControllerTransform;
            set => m_RightControllerTransform = value;
        }

        [SerializeField]
        MovementDirection m_LeftHandMovementDirection;

        public MovementDirection leftHandMovementDirection
        {
            get => m_LeftHandMovementDirection;
            set => m_LeftHandMovementDirection = value;
        }

        [SerializeField]
        MovementDirection m_RightHandMovementDirection;

        public MovementDirection rightHandMovementDirection
        {
            get => m_RightHandMovementDirection;
            set => m_RightHandMovementDirection = value;
        }

        Transform m_CombinedTransform;
        Pose m_LeftMovementPose = Pose.identity;
        Pose m_RightMovementPose = Pose.identity;

        // Variables for limping movement
        float m_LimpCycle = 0f;
        [SerializeField]
        float m_LimpFrequency = 1f; // Frequency of the limp cycle in seconds
        [SerializeField]
        float m_LimpAmplitude = 0.1f; // Amplitude of the limp effect, smaller value for smaller amplitude
        [SerializeField]
        float m_CameraTiltAngle = 5f; // Angle to tilt the camera during the limp

        protected override void Awake()
        {
            base.Awake();

            m_CombinedTransform = new GameObject("[Dynamic Move Provider] Combined Forward Source").transform;
            m_CombinedTransform.SetParent(transform, false);
            m_CombinedTransform.localPosition = Vector3.zero;
            m_CombinedTransform.localRotation = Quaternion.identity;

            forwardSource = m_CombinedTransform;
        }

        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            if (m_HeadTransform == null)
            {
                var xrOrigin = mediator.xrOrigin;
                if (xrOrigin != null)
                {
                    var xrCamera = xrOrigin.Camera;
                    if (xrCamera != null)
                        m_HeadTransform = xrCamera.transform;
                }
            }

            switch (m_LeftHandMovementDirection)
            {
                case MovementDirection.HeadRelative:
                    if (m_HeadTransform != null)
                        m_LeftMovementPose = m_HeadTransform.GetWorldPose();
                    break;

                case MovementDirection.HandRelative:
                    if (m_LeftControllerTransform != null)
                        m_LeftMovementPose = m_LeftControllerTransform.GetWorldPose();
                    break;

                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(MovementDirection)}={m_LeftHandMovementDirection}");
                    break;
            }

            switch (m_RightHandMovementDirection)
            {
                case MovementDirection.HeadRelative:
                    if (m_HeadTransform != null)
                        m_RightMovementPose = m_HeadTransform.GetWorldPose();
                    break;

                case MovementDirection.HandRelative:
                    if (m_RightControllerTransform != null)
                        m_RightMovementPose = m_RightControllerTransform.GetWorldPose();
                    break;

                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(MovementDirection)}={m_RightHandMovementDirection}");
                    break;
            }

            var leftHandValue = leftHandMoveInput.ReadValue();
            var rightHandValue = rightHandMoveInput.ReadValue();

            var totalSqrMagnitude = leftHandValue.sqrMagnitude + rightHandValue.sqrMagnitude;
            var leftHandBlend = 0.5f;
            if (totalSqrMagnitude > Mathf.Epsilon)
                leftHandBlend = leftHandValue.sqrMagnitude / totalSqrMagnitude;

            var combinedPosition = Vector3.Lerp(m_RightMovementPose.position, m_LeftMovementPose.position, leftHandBlend);
            var combinedRotation = Quaternion.Slerp(m_RightMovementPose.rotation, m_LeftMovementPose.rotation, leftHandBlend);
            m_CombinedTransform.SetPositionAndRotation(combinedPosition, combinedRotation);

            // Limping effect: adjust the forward movement
            m_LimpCycle += Time.deltaTime * m_LimpFrequency;
            float limpFactor = AsymmetricLimpCurve(m_LimpCycle) * m_LimpAmplitude;

            var move = base.ComputeDesiredMove(input);
            move.y += limpFactor; // Apply limp factor to the vertical component of the movement

            // Tilt the camera during the descent
            TiltCameraDuringLimp(m_LimpCycle);

            return move;
        }

        float AsymmetricLimpCurve(float cycle)
        {
            cycle = cycle % 1.0f; // Ensure cycle is within [0, 1]
            if (cycle < 0.5f)
            {
                // Slow descent using cos
                return Mathf.Cos(cycle * Mathf.PI) - 1; // Range from -1 to 0
            }
            else
            {
                // Fast ascent using cos
                return Mathf.Cos((cycle - 0.5f) * 2 * Mathf.PI) * 0.5f; // Range from 0 to 0.5
            }
        }

        void TiltCameraDuringLimp(float cycle)
        {
            cycle = cycle % 1.0f; // Ensure cycle is within [0, 1]
            if (m_HeadTransform != null)
            {
                if (cycle < 0.5f)
                {
                    // During descent, tilt the camera to the right
                    m_HeadTransform.localRotation = Quaternion.Euler(m_HeadTransform.localRotation.eulerAngles.x, m_HeadTransform.localRotation.eulerAngles.y, m_CameraTiltAngle);
                }
                else
                {
                    // Reset the camera tilt during ascent
                    m_HeadTransform.localRotation = Quaternion.Euler(m_HeadTransform.localRotation.eulerAngles.x, m_HeadTransform.localRotation.eulerAngles.y, 0);
                }
            }
        }
    }
}
