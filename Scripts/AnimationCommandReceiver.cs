using UnityEngine;

public class AnimationCommandReceiver : MonoBehaviour
{
    public Animator animator;
    public string idleAnimationName = "Idle";
    private string currentAnimation;
    private bool isReturningToIdle = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // Call this method with the animation name (from backend or locally)
    public void PlayAnimation(string animName)
    {
        Debug.Log("[AnimationCommandReceiver] PlayAnimation called with: " + animName);
        animator.Play(animName);
        currentAnimation = animName;
        isReturningToIdle = false;
    }

    void Update()
    {
        // For testing in Editor: Press keys 1-6 to play each animation
        if (Input.GetKeyDown(KeyCode.F1)) PlayAnimation("Idle");
        if (Input.GetKeyDown(KeyCode.F2)) PlayAnimation("Macarena");
        if (Input.GetKeyDown(KeyCode.F3)) PlayAnimation("Waving");
        if (Input.GetKeyDown(KeyCode.F4)) PlayAnimation("Laugh");
        if (Input.GetKeyDown(KeyCode.F5)) PlayAnimation("Salute");
        if (Input.GetKeyDown(KeyCode.F6)) PlayAnimation("Jumping");

        // Automatically return to Idle when current animation ends (and is not already Idle)
        if (!string.IsNullOrEmpty(currentAnimation) && currentAnimation != idleAnimationName && !isReturningToIdle)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(currentAnimation) && state.normalizedTime >= 1.0f)
            {
                Debug.Log("[AnimationCommandReceiver] Animation finished, returning to Idle.");
                isReturningToIdle = true;
                animator.Play(idleAnimationName);
                currentAnimation = idleAnimationName;
            }
        }
    }
}