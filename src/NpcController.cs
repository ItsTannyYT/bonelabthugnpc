using System.Collections.Generic;
using UnityEngine;

namespace BonelabHostileNpc
{
    public class NpcController : MonoBehaviour
    {
        public float ViewDistance = 25f;
        public float ViewAngle = 70f;
        public float AimSpeed = 6f;
        public float CombatFireRateMin = 0.25f;
        public float CombatFireRateMax = 0.33f;
        public float ReactionDelay = 0.4f;
        public float GrabRadius = 3f;
        public LayerMask ObstructionMask = ~0;

        private DetectionSystem _detection;
        private WeaponHandler _weaponHandler;
        private HealthComponent _health;
        private Rigidbody _rightHandRigidbody;
        private Transform _head;
        private float _nextFireTime;
        private float _alertTime;
        private NpcState _state = NpcState.Idle;
        private Animator _animator;
        private List<Rigidbody> _ragdollBodies = new List<Rigidbody>();

        public void Awake()
        {
            _head = FindHead();
            _rightHandRigidbody = FindRightHand();
            _animator = GetComponentInChildren<Animator>();
            _health = gameObject.AddComponent<HealthComponent>();
            _health.OnDeath += HandleDeath;

            if (_head != null)
            {
                _detection = new DetectionSystem(_head, ViewDistance, ViewAngle, ObstructionMask);
            }

            if (_rightHandRigidbody != null)
            {
                _weaponHandler = new WeaponHandler(_rightHandRigidbody.transform, _rightHandRigidbody, GrabRadius);
            }

            CacheRagdollBodies();
        }

        public void Update()
        {
            if (_state == NpcState.Dead)
            {
                return;
            }

            if (_detection == null || _weaponHandler == null)
            {
                return;
            }

            switch (_state)
            {
                case NpcState.Idle:
                    HandleIdle();
                    break;
                case NpcState.Alert:
                    HandleAlert();
                    break;
                case NpcState.Combat:
                    HandleCombat();
                    break;
                case NpcState.Reload:
                    HandleReload();
                    break;
            }
        }

        private void HandleIdle()
        {
            if (!_weaponHandler.HasWeapon)
            {
                _weaponHandler.TryFindAndGrabWeapon(transform.position);
            }

            if (_detection.CanSeePlayer(out _))
            {
                _alertTime = Time.time;
                _state = NpcState.Alert;
            }
        }

        private void HandleAlert()
        {
            if (_detection.CanSeePlayer(out Vector3 playerPosition))
            {
                RotateToward(playerPosition);
            }

            if (Time.time - _alertTime >= ReactionDelay)
            {
                _state = NpcState.Combat;
                ScheduleNextShot();
            }
        }

        private void HandleCombat()
        {
            if (!_weaponHandler.HasWeapon)
            {
                _state = NpcState.Idle;
                return;
            }

            if (_detection.CanSeePlayer(out Vector3 playerPosition))
            {
                _weaponHandler.AimAt(playerPosition, AimSpeed);
            }
            else
            {
                _state = NpcState.Alert;
                _alertTime = Time.time;
                return;
            }

            if (_weaponHandler.IsEmpty())
            {
                _state = NpcState.Reload;
                return;
            }

            if (Time.time >= _nextFireTime)
            {
                _weaponHandler.TryFire();
                ScheduleNextShot();
            }
        }

        private void HandleReload()
        {
            if (_weaponHandler.TryReload())
            {
                _state = NpcState.Combat;
                ScheduleNextShot();
                return;
            }

            _state = NpcState.Combat;
        }

        private void ScheduleNextShot()
        {
            var interval = Random.Range(CombatFireRateMin, CombatFireRateMax);
            _nextFireTime = Time.time + interval;
        }

        private void RotateToward(Vector3 target)
        {
            var flatDirection = target - transform.position;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
        }

        private void HandleDeath()
        {
            _state = NpcState.Dead;
            _weaponHandler?.ClearWeapon();
            EnableRagdoll(true);
        }

        private void CacheRagdollBodies()
        {
            _ragdollBodies.Clear();
            foreach (var body in GetComponentsInChildren<Rigidbody>())
            {
                if (body == null || body == _rightHandRigidbody)
                {
                    continue;
                }

                _ragdollBodies.Add(body);
            }
        }

        private void EnableRagdoll(bool enabled)
        {
            if (_animator != null)
            {
                _animator.enabled = !enabled;
            }

            foreach (var body in _ragdollBodies)
            {
                if (body == null)
                {
                    continue;
                }

                body.isKinematic = !enabled;
                body.detectCollisions = enabled;
            }
        }

        private Transform FindHead()
        {
            foreach (var transformItem in GetComponentsInChildren<Transform>())
            {
                if (transformItem.name.ToLower().Contains("head"))
                {
                    return transformItem;
                }
            }

            return transform;
        }

        private Rigidbody FindRightHand()
        {
            foreach (var body in GetComponentsInChildren<Rigidbody>())
            {
                var lowerName = body.name.ToLower();
                if (lowerName.Contains("righthand") || lowerName.Contains("right_hand") || lowerName.Contains("hand_r"))
                {
                    return body;
                }
            }

            return GetComponent<Rigidbody>();
        }
    }
}
