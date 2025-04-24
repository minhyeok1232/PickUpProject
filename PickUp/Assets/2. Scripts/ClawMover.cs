using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ClawMover : MonoBehaviour 
{

    public float maxHeight;
    public float minHeight;
    public float tubeOffset;

    public GameObject tubes;
    public GameObject motor;
    public GameObject claw;
    public Animator clawAnimator;
    
    public float speed;
    public float clawSpeed;
    Rigidbody rigidBody;

    public Transform clawHeight;

    // These are simple cubes used to compare movement limits
    public Transform leftLimit;
    public Transform rightLimit;
    public Transform frontLimit;
    public Transform backLimit;

    public bool canControl;
    private bool isReleasing;

    public bool[] reachedBasket;

    private bool isInBasket;
    private bool isLoweringToRelease;
    private bool isRisingFromBasket;
    private bool isAutoDescending;

    private CancellationTokenSource _cts;

    // Use this for initialization
    void Start () {
        clawAnimator = claw.gameObject.GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        canControl = true;
        isReleasing = false;
        _cts = new CancellationTokenSource();
	}

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle input in Update instead of FixedUpdate for better responsiveness
        if (canControl && Input.GetKeyDown(KeyCode.Space))
        {
            isAutoDescending = true;
            canControl = false;
        }
    }

    void FixedUpdate () {
        // Fix motor to claw's X/Z movement
        motor.transform.position = new Vector3(transform.position.x, motor.transform.position.y, transform.position.z);
        tubes.transform.position = new Vector3(tubes.transform.position.x, tubes.transform.position.y, motor.transform.position.z + tubeOffset);

        // Release prize sequence
        if (isReleasing)
        {
            if (clawHeight.position.y <= maxHeight)
            {
                transform.Translate(0, clawSpeed * Time.deltaTime, 0);
            }
            else
            {
                reachedBasket[0] = true;
            }

            if (transform.position.x >= leftLimit.transform.position.x + 0.5f)
            {
                transform.Translate(speed * -1 * Time.deltaTime, 0, 0);
            }
            else
            {
                reachedBasket[1] = true;
            }

            if (transform.position.z >= frontLimit.transform.position.z + 0.5f)
            {
                transform.Translate(0, 0, speed * -1 * Time.deltaTime);
            }
            else
            {
                reachedBasket[2] = true;
            }

            // Check if claw is inside the basket
            if (reachedBasket[0] && reachedBasket[1] && reachedBasket[2])
            {
                isInBasket = true;

                // Start coroutine to lower claw and release prize
                if (isInBasket) {
                    ReleasePrizeInBasketAsync().Forget();
                    isInBasket = false;
                }
            }
        }
        else if (isLoweringToRelease)
        {
            if (clawHeight.position.y > minHeight)
            {
                transform.Translate(0, clawSpeed * -1 * Time.deltaTime, 0);
            }
            else {
                OpenClawAtBasketAsync().Forget();
                isLoweringToRelease = false;
            }
        }
        else if (isRisingFromBasket)
        {
            if (clawHeight.position.y <= maxHeight)
            {
                transform.Translate(0, clawSpeed * Time.deltaTime, 0);
            }
            else
            {
                canControl = true;
                isRisingFromBasket = false;
            }
        }
        else if (canControl)
        {
            HandlePlayerMovement();
        }

        // Handle descent separately from player control
        if (isAutoDescending)
        {
            float descendSpeed = clawSpeed; // Increased speed for more responsive descent
            if (clawHeight.position.y > minHeight)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.MoveTowards(transform.position.y, minHeight, descendSpeed * Time.fixedDeltaTime),
                    transform.position.z
                );
            }
            else
            {
                CloseClawAsync().Forget();
                isAutoDescending = false;
            }
        }
    }

    private void HandlePlayerMovement()
    {
        // Lateral Movement
        if (transform.position.x > leftLimit.transform.position.x && Input.GetKey(KeyCode.A))
        {
            transform.Translate(speed * -1 * Time.fixedDeltaTime, 0, 0);
        }
        if (transform.position.x < rightLimit.transform.position.x && Input.GetKey(KeyCode.D))
        {
            transform.Translate(speed * Time.fixedDeltaTime, 0, 0);
        }

        // Forward Movement
        if (transform.position.z < backLimit.transform.position.z && Input.GetKey(KeyCode.W))
        {
            transform.Translate(0, 0, speed * Time.fixedDeltaTime);
        }
        if (transform.position.z > frontLimit.transform.position.z && Input.GetKey(KeyCode.S))
        {
            transform.Translate(0, 0, speed * -1 * Time.fixedDeltaTime);
        }
    }

    private async UniTaskVoid CloseClawAsync()
    {
        try
        {
            await UniTask.Delay(2000, cancellationToken: _cts.Token); // 2 seconds
            CloseClawAnimation();
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            isReleasing = true;
        }
        catch (System.OperationCanceledException)
        {
            // Task was cancelled, cleanup if needed
        }
    }

    private async UniTaskVoid ReleasePrizeInBasketAsync()
    {
        try
        {
            await UniTask.Delay(1500, cancellationToken: _cts.Token); // 1.5 seconds
            isLoweringToRelease = true;
            isReleasing = false;
            await UniTask.Delay(1500, cancellationToken: _cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            // Task was cancelled, cleanup if needed
        }
    }

    private async UniTaskVoid OpenClawAtBasketAsync()
    {
        try
        {
            await UniTask.Delay(1000, cancellationToken: _cts.Token); // 1 second
            OpenClawAnimation();
            await UniTask.Delay(1000, cancellationToken: _cts.Token);
            isRisingFromBasket = true;
            reachedBasket[0] = false;
            reachedBasket[1] = false;
            reachedBasket[2] = false;
        }
        catch (System.OperationCanceledException)
        {
            // Task was cancelled, cleanup if needed
        }
    }

    public void OpenClawAnimation() {
        if (clawAnimator) {
            clawAnimator.SetBool("Open", true);
            clawAnimator.SetBool("Close", false);
        }
    }

    public void CloseClawAnimation()
    {
        if (clawAnimator) {
            clawAnimator.SetBool("Open", false);
            clawAnimator.SetBool("Close", true);
        }
    }
}
