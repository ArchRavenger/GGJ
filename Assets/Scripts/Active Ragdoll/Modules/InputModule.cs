using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    /// <summary> Tells the ActiveRagdoll what it should do. Input can be external (like the
    /// one from the player or from another script) and internal (kind of like sensors, such as
    /// detecting if it's on floor). </summary>
    public class InputModule : Module 
    {
        private AudioSource _audioSource;
        public AudioClip jumpingClip;
        public  float jumpStrength = 10f;
        // ---------- EXTERNAL INPUT ----------

        public delegate void onMoveDelegate(Vector2 movement);
        public onMoveDelegate OnMoveDelegates { get; set; }
        public void OnMove(InputValue value) {
            OnMoveDelegates?.Invoke(value.Get<Vector2>());
        }

        public delegate void onLeftArmDelegate(float armWeight);
        public onLeftArmDelegate OnLeftArmDelegates { get; set; }
        public void OnLeftArm(InputValue value) {
            OnLeftArmDelegates?.Invoke(value.Get<float>());
        }

        public delegate void onRightArmDelegate(float armWeight);
        public onRightArmDelegate OnRightArmDelegates { get; set; }
        public void OnRightArm(InputValue value) {
            OnRightArmDelegates?.Invoke(value.Get<float>());
        }

        // ---------- INTERNAL INPUT ----------

        [Header("--- FLOOR ---")]
        public float floorDetectionDistance = 0.3f;
        public float maxFloorSlope = 60;

        private bool _isOnFloor = true;
        public bool IsOnFloor { get { return _isOnFloor; } }

        Rigidbody _rightFoot, _leftFoot;


        void Start() {
            _rightFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightFoot).GetComponent<Rigidbody>();
            _leftFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftFoot).GetComponent<Rigidbody>();
            _audioSource = GetComponent<AudioSource>(); 
        }

        void Update() {


            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJump();
            }

            UpdateOnFloor();
        }

        public void OnJump(InputValue value)
        {
            // Check if the ragdoll is on the floor before jumping
            if (_activeRagdoll.Input.IsOnFloor)
                _audioSource.PlayOneShot(jumpingClip);
            {
                var jumpForce = new Vector3(0, jumpStrength, 0);
                _activeRagdoll.PhysicalTorso.AddForce(jumpForce, ForceMode.Impulse);
            }
        }

        public delegate void onFloorChangedDelegate(bool onFloor);
        public onFloorChangedDelegate OnFloorChangedDelegates { get; set; }
        private void UpdateOnFloor() {
            bool lastIsOnFloor = _isOnFloor;

            _isOnFloor = CheckRigidbodyOnFloor(_rightFoot, out Vector3 foo)
                         || CheckRigidbodyOnFloor(_leftFoot, out foo);

            if (_isOnFloor != lastIsOnFloor)
                OnFloorChangedDelegates(_isOnFloor);
        }

        /// <summary>
        /// Checks whether the given rigidbody is on floor
        /// </summary>
        /// <param name="bodyPart">Part of the body to check</param
        /// <returns> True if the Rigidbody is on floor </returns>
        public bool CheckRigidbodyOnFloor(Rigidbody bodyPart, out Vector3 normal) {
            // Raycast
            Ray ray = new Ray(bodyPart.position, Vector3.down);
            bool onFloor = Physics.Raycast(ray, out RaycastHit info, floorDetectionDistance, ~(1 << bodyPart.gameObject.layer));

            // Additional checks
            onFloor = onFloor && Vector3.Angle(info.normal, Vector3.up) <= maxFloorSlope;

            if (onFloor && info.collider.gameObject.TryGetComponent<Floor>(out Floor floor))
                    onFloor = floor.isFloor;

            normal = info.normal;
            return onFloor;
        }
        private void OnJump()
        {
            if (_activeRagdoll.Input.IsOnFloor)
            {
                _audioSource.PlayOneShot(jumpingClip);  // Play jumping sound
                var jumpForce = new Vector3(0, jumpStrength, 0);
                _activeRagdoll.PhysicalTorso.AddForce(jumpForce, ForceMode.Impulse);
            }
        }
    }
} // namespace ActiveRagdoll