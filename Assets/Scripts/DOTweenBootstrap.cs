using DG.Tweening;
using UnityEngine;

public class DOTweenBootstrap : MonoBehaviour
{
    void Awake()
    {
        DOTween.SetTweensCapacity(500, 100); 
    }
}
