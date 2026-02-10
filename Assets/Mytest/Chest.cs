using NUnit.Framework;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

public class Chest : MonoBehaviour
{
    DoorManager doorManager;

    [SerializeField]
    private string _colliderScript;

    [SerializeField]
    private UnityEvent _collisionEntered;

    [SerializeField] 
    private UnityEvent _collisionUse;

    [SerializeField]
    private UnityEvent _collisionExit;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent(_colliderScript))
        {
            _collisionEntered?.Invoke();
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.GetComponent(_colliderScript))
        {
            _collisionExit?.Invoke();
        }
    }

    public void OnCollisionUse2D(Collision2D col)
    {
        if (col.gameObject.GetComponent(_colliderScript))
        {
            _collisionUse?.Invoke();
           
        }
    }
}
