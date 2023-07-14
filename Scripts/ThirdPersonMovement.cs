using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    Animator MyAnimator;
    public float AnimationBlendSpeed = 2f;
    public CharacterController controller;
    public Transform cam;
    public float speed = 10f;
    public float sprintspeed = 11f;
    float mSpeedY = 0;
    float mGravity = -9.81f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float mDesiredAnimationSpeed = 0f;
    public float JumpSpeed = 15f;
    bool mSprinting = false;
    bool mJumping = false;

    // Camera variables
    public float jumpCameraOffset = 2f; // Offset to raise the camera when jumping
    private Vector3 originalCameraOffset; // Original camera offset

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        MyAnimator = GetComponentInChildren<Animator>();

        // Store the original camera offset
        originalCameraOffset = cam.localPosition;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); //horizontal move
        float vertical = Input.GetAxisRaw("Vertical"); //vertical move

        if (Input.GetKey(KeyCode.Space) && !mJumping)
        {
            StartCoroutine(JumpCoroutine());
        }

        if (!controller.isGrounded)
        {
            mSpeedY += mGravity * Time.deltaTime; //jump
        }
        else if (mSpeedY < 0)
        {
            mJumping = false;
            mSpeedY = 0;

            // Reset the camera offset when grounded
            cam.localPosition = originalCameraOffset;
        }

        MyAnimator.SetFloat("SpeedY", mSpeedY / JumpSpeed);

        if (mJumping && mSpeedY < 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, .5f, LayerMask.GetMask("Ground")))
            {
                mJumping = false;
                MyAnimator.SetTrigger("Movement");

                // Reset the camera offset when grounded
                cam.localPosition = originalCameraOffset;
            }
        }

        mSprinting = Input.GetKey(KeyCode.LeftShift); //run keys

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 verticalMovement = Vector3.up * mSpeedY;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            mDesiredAnimationSpeed = mSprinting ? 1 : .5f;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move((verticalMovement + (moveDir.normalized * (mSprinting ? sprintspeed : speed))) * Time.deltaTime);
        }
        else
        {
            mDesiredAnimationSpeed = 0;
        }

        MyAnimator.SetFloat("Speed", Mathf.Lerp(MyAnimator.GetFloat("Speed"), mDesiredAnimationSpeed, AnimationBlendSpeed * Time.deltaTime));

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Attack();
            mJumping = false;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Attack2();
        }
    }

    private void Attack()
    {
        MyAnimator.SetTrigger("Attack");

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            BuildingHealth building = hit.collider.GetComponent<BuildingHealth>();
            if (building != null)
            {
                // Adjust the damage amount as needed
                int damageAmount = 10;
                building.TakeDamage(damageAmount);
            }
        }
    }

    private void Attack2()
    {
        MyAnimator.SetTrigger("Attack2");
    }

    private IEnumerator JumpCoroutine()
    {
        mJumping = true;
        MyAnimator.SetTrigger("Jump");

        float elapsedTime = 0f;
        float totalDuration = 0.5f;
        float startY = transform.position.y;
        float targetY = startY + 25f;

        // Move the character up gradually
        while (elapsedTime < totalDuration)
        {
            float newY = Mathf.Lerp(startY, targetY, elapsedTime / totalDuration);
            Vector3 newPosition = new Vector3(transform.position.x, newY, transform.position.z);
            transform.position = newPosition;

            // Move the camera up gradually
            float newCameraY = Mathf.Lerp(originalCameraOffset.y, originalCameraOffset.y + jumpCameraOffset, elapsedTime / totalDuration);
            Vector3 newCameraPosition = new Vector3(cam.localPosition.x, newCameraY, cam.localPosition.z);
            cam.localPosition = newCameraPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the character reaches the target height exactly
        Vector3 finalPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        transform.position = finalPosition;

        // Reset the camera offset when the character reaches the peak of the jump
        cam.localPosition = originalCameraOffset + new Vector3(0f, jumpCameraOffset, 0f);

        yield return null;
    }
}