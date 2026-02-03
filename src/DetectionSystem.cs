using BoneLib;
using UnityEngine;

namespace BonelabHostileNpc
{
    public class DetectionSystem
    {
        private readonly Transform _origin;
        private readonly float _viewDistance;
        private readonly float _viewAngle;
        private readonly LayerMask _obstructionMask;

        public DetectionSystem(Transform origin, float viewDistance, float viewAngle, LayerMask obstructionMask)
        {
            _origin = origin;
            _viewDistance = viewDistance;
            _viewAngle = viewAngle;
            _obstructionMask = obstructionMask;
        }

        public bool CanSeePlayer(out Vector3 playerPosition)
        {
            playerPosition = Vector3.zero;
            var playerReferences = PlayerReferences.Instance;
            if (playerReferences == null || playerReferences.Head == null)
            {
                return false;
            }

            playerPosition = playerReferences.Head.position;
            var direction = playerPosition - _origin.position;
            if (direction.sqrMagnitude > _viewDistance * _viewDistance)
            {
                return false;
            }

            var angle = Vector3.Angle(_origin.forward, direction);
            if (angle > _viewAngle * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(_origin.position, direction.normalized, out RaycastHit hit, _viewDistance, _obstructionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != playerReferences.Head)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
