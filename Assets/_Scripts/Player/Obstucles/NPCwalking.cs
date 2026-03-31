using UnityEngine;

public class NPCwalking : MonoBehaviour
{
    public float moveSpeed;

    private Rigidbody2D rigidbody2D;

    public bool isWalking;

    public float walkTime;
    private float walkCounter;
    public float waitTime;
    private float waitCounter;

    private int WalkDirection;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();

        waitCounter = waitTime;
        walkCounter = walkTime;
        
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if(isWalking == true)
        {
            walkCounter -= Time.deltaTime;
           
            switch(WalkDirection)
            {
                case 0:
                    rigidbody2D.velocity = new Vector2(0, moveSpeed);
                    break;
                case 1:
                    rigidbody2D.velocity = new Vector2(moveSpeed, 0);
                    break;
                case 2:
                    rigidbody2D.velocity = new Vector2(0, -moveSpeed);
                    break;
                case 3:
                    rigidbody2D.velocity = new Vector2(-moveSpeed, 0);
                    break;
            }

            if (walkCounter < 0)
            {
                isWalking = false;
                waitCounter = waitTime;
            }
        }
        else
        {
            waitCounter -= Time.deltaTime;

            rigidbody2D.velocity = Vector2.zero;

            if(waitCounter < 0)
            {
                ChooseDirection();
            }
        }
    }

    public void ChooseDirection()
    {
        WalkDirection = Random.Range(0, 4);
        isWalking = true;
        walkCounter = walkTime;
    }
}
