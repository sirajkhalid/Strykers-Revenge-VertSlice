using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator anim;
    private float time;

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (anim == null || anim.runtimeAnimatorController == null)
        {
            Destroy(gameObject, time); // backup destroy
            return;
        }

        // Get length of first (or only) clip
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        if (clips.Length > 0)
            time = clips[0].length;
        else
            time = 1f;

        Destroy(gameObject, time);
    }
}
