using EN;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EN
{
    public class CameraHandler : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform cameraTransform;
        public Transform cameraPivotTransform;
        private Transform myTransform;
        private Vector3 cameraTransformPosition;
        private LayerMask ignoreLayers;
        private Vector3 cameraFollowVelocity = Vector3.zero;
        public static CameraHandler singleton;
        public float lookSpeed = 0.1f;
        public float followSpeed = 0.1f;
        public float pivotSpeed = 0.03f;
        private float defaultPosition;
        private float lookAngle;
        private float pivotAngle;
        public float minimumPivot = -35;
        public float maximumPivot = 35;

        // Дополнительные параметры для сглаживания и ограничений
        public float smoothTime = 0.1f;
        public float distance = 5.0f;
        public float height = 1.5f;
        public float minDistance = 2.0f;
        public float maxDistance = 10.0f;

        private void Awake()
        {
            singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            myTransform.position = targetPosition;
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput)
        {
            lookAngle += (mouseXInput * lookSpeed) / delta;
            pivotAngle -= (mouseYInput * pivotSpeed) / delta;
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            Vector3 rotation = Vector3.zero;
            rotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;
            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;

            Vector3 desiredPosition = cameraPivotTransform.position + cameraPivotTransform.forward * -distance + cameraPivotTransform.up * height;
            Vector3 smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, delta / smoothTime);
            cameraTransform.position = smoothedPosition;

            Vector3 directionToTarget = targetTransform.position - cameraTransform.position;
            float distanceToTarget = directionToTarget.magnitude;
            if (distanceToTarget < minDistance)
            {
                cameraTransform.position = targetTransform.position - cameraTransform.forward * minDistance;
            }
            else if (distanceToTarget > maxDistance)
            {
                cameraTransform.position = targetTransform.position - cameraTransform.forward * maxDistance;
            }
        }
    }
}
