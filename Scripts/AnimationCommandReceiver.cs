using UnityEngine;
using System.Collections;

public class AnimationCommandReceiver : MonoBehaviour
{
    public Animator animator;
    public string idleAnimationName = "Idle";
    private Coroutine returnToIdleCoroutine;
    public float crossfadeDuration = 0.3f; // Use a longer blend for Idle

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animName)
    {
        Debug.Log("[AnimationCommandReceiver] PlayAnimation called with: " + animName);

        // For non-idle, use Play for immediate switch
        if (animName != idleAnimationName)
            animator.Play(animName);
        else
            animator.CrossFade(idleAnimationName, crossfadeDuration);

        // Stop any previous return-to-idle coroutine
        if (returnToIdleCoroutine != null)
        {
            StopCoroutine(returnToIdleCoroutine);
            returnToIdleCoroutine = null;
        }

        // Only start coroutine for gesture animations
        if (animName != idleAnimationName)
        {
            float duration = GetAnimationClipLength(animName);
            if (duration <= 0f) duration = 2f; // fallback
            duration += 0.1f; // Add small buffer
            returnToIdleCoroutine = StartCoroutine(ReturnToIdleAfterDelay(duration));
        }
    }

    private float GetAnimationClipLength(string animName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animName)
                return clip.length;
        }
        Debug.LogWarning("[AnimationCommandReceiver] AnimationClip not found: " + animName);
        return 0f;
    }

    private IEnumerator ReturnToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("[AnimationCommandReceiver] Coroutine - returning to Idle (with crossfade).");
        animator.CrossFade(idleAnimationName, crossfadeDuration);
        returnToIdleCoroutine = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) PlayAnimation("Idle");
        if (Input.GetKeyDown(KeyCode.F2)) PlayAnimation("Macarena");
        if (Input.GetKeyDown(KeyCode.F3)) PlayAnimation("Waving");
        if (Input.GetKeyDown(KeyCode.F4)) PlayAnimation("Laugh");
        if (Input.GetKeyDown(KeyCode.F5)) PlayAnimation("Salute");
        if (Input.GetKeyDown(KeyCode.F6)) PlayAnimation("Jumping");
        if (Input.GetKeyDown(KeyCode.F7)) PlayAnimation("Singing");
        if (Input.GetKeyDown(KeyCode.F8)) PlayAnimation("Thinking");
        if (Input.GetKeyDown(KeyCode.F9)) PlayAnimation("Kiss");      // Added Kiss
        if (Input.GetKeyDown(KeyCode.F10)) PlayAnimation("Dance");    // Added Dance
    }
}
