using UnityEngine;

public class DoorManager : MonoBehaviour
{
    private Animator animator;
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    [ContextMenu(itemName:"Atrigger")]
    public void Open()
    {
        animator.SetTrigger(name:"Atrigger");
    }
}
