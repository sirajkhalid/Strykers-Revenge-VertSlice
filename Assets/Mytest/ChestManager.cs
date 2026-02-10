using UnityEngine;

public class ChestManager : MonoBehaviour
{

    private Animator animator;
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [ContextMenu(itemName: "Atrigger")]
    public void Open()
    {
        animator.SetTrigger(name: "Atrigger");
    }
}
