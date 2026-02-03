using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Interaction;
using UnityEngine;

namespace BonelabHostileNpc
{
    public class WeaponHandler
    {
        private readonly Transform _handTransform;
        private readonly Rigidbody _handRigidbody;
        private readonly float _grabRadius;
        private FixedJoint _handJoint;
        private Gun _currentGun;
        private Rigidbody _gunRigidbody;
        private Grip _grip;

        public WeaponHandler(Transform handTransform, Rigidbody handRigidbody, float grabRadius)
        {
            _handTransform = handTransform;
            _handRigidbody = handRigidbody;
            _grabRadius = grabRadius;
        }

        public bool HasWeapon => _currentGun != null;

        public void ClearWeapon()
        {
            if (_handJoint != null)
            {
                Object.Destroy(_handJoint);
            }

            _handJoint = null;
            _currentGun = null;
            _gunRigidbody = null;
            _grip = null;
        }

        public bool TryFindAndGrabWeapon(Vector3 origin)
        {
            Gun closestGun = null;
            float closestDistance = float.MaxValue;
            foreach (var gun in Object.FindObjectsOfType<Gun>())
            {
                if (gun == null || gun.transform == null)
                {
                    continue;
                }

                var distance = Vector3.Distance(origin, gun.transform.position);
                if (distance > _grabRadius)
                {
                    continue;
                }

                if (distance < closestDistance)
                {
                    closestGun = gun;
                    closestDistance = distance;
                }
            }

            if (closestGun == null)
            {
                return false;
            }

            return AttachWeapon(closestGun);
        }

        public void AimAt(Vector3 targetPosition, float aimSpeed)
        {
            if (_handRigidbody == null || _currentGun == null)
            {
                return;
            }

            var direction = (targetPosition - _handRigidbody.position).normalized;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            var newRotation = Quaternion.Slerp(_handRigidbody.rotation, targetRotation, aimSpeed * Time.deltaTime);
            _handRigidbody.MoveRotation(newRotation);
        }

        public bool TryFire()
        {
            if (_currentGun == null)
            {
                return false;
            }

            return TryInvokeGunMethod(_currentGun, new List<string> { "TryFire", "Fire", "Shoot" });
        }

        public bool TryReload()
        {
            if (_currentGun == null)
            {
                return false;
            }

            return TryInvokeGunMethod(_currentGun, new List<string> { "Reload", "TryReload", "OnReload" });
        }

        public bool IsEmpty()
        {
            if (_currentGun == null)
            {
                return true;
            }

            if (TryGetBool(_currentGun, new List<string> { "IsEmpty", "isEmpty" }, out bool value))
            {
                return value;
            }

            return false;
        }

        private bool AttachWeapon(Gun gun)
        {
            _currentGun = gun;
            _gunRigidbody = gun.GetComponent<Rigidbody>();
            _grip = gun.GetComponentInChildren<Grip>();

            if (_gunRigidbody == null)
            {
                ClearWeapon();
                return false;
            }

            if (_handJoint != null)
            {
                Object.Destroy(_handJoint);
            }

            _handJoint = _handRigidbody.gameObject.AddComponent<FixedJoint>();
            _handJoint.connectedBody = _gunRigidbody;
            _handJoint.breakForce = Mathf.Infinity;
            _handJoint.breakTorque = Mathf.Infinity;

            var gripTransform = _grip != null ? _grip.transform : _gunRigidbody.transform;
            _gunRigidbody.MovePosition(_handRigidbody.position);
            _gunRigidbody.MoveRotation(gripTransform.rotation);

            return true;
        }

        private static bool TryInvokeGunMethod(Gun gun, List<string> methodNames)
        {
            var type = gun.GetIl2CppType();
            foreach (var methodName in methodNames)
            {
                var method = type.GetMethod(methodName, Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic, null, new Il2CppSystem.Type[0], null);
                if (method == null)
                {
                    continue;
                }

                method.Invoke(gun, null);
                return true;
            }

            return false;
        }

        private static bool TryGetBool(Gun gun, List<string> names, out bool value)
        {
            value = false;
            var type = gun.GetIl2CppType();
            foreach (var name in names)
            {
                var property = type.GetProperty(name, Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic);
                if (property != null)
                {
                    var result = property.GetValue(gun, null);
                    if (result != null)
                    {
                        value = (bool)result;
                        return true;
                    }
                }

                var field = type.GetField(name, Il2CppSystem.Reflection.BindingFlags.Instance | Il2CppSystem.Reflection.BindingFlags.Public | Il2CppSystem.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    var result = field.GetValue(gun);
                    if (result != null)
                    {
                        value = (bool)result;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
