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
    private bool isGrabbing;
    
    private CancellationTokenSource _cts;
    public bool isClawActivated;

    void Start () {
        clawAnimator = claw.gameObject.GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        canControl = true;
        isReleasing = false;
        isAutoDescending = false;
        isGrabbing = false;
        isClawActivated = false;
        _cts = new CancellationTokenSource();
        
        // Initialize basket flags
        reachedBasket = new bool[3];
        for (int i = 0; i < reachedBasket.Length; i++)
        {
            reachedBasket[i] = false;
        }
	}

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    void Update()
    {
        if (canControl && Input.GetKeyDown(KeyCode.Space))
        {
            isAutoDescending = true;
            canControl = false;
            isClawActivated = true;
        }
    }

    void FixedUpdate () {
        motor.transform.position = new Vector3(transform.position.x, motor.transform.position.y, transform.position.z);
        tubes.transform.position = new Vector3(transform.position.x, tubes.transform.position.y, transform.position.z + tubeOffset);

        // 맨 처음 Spacebar 누를 때 또는 타이머가 0이 되었을 때
        if (isAutoDescending && !isGrabbing)
        {
            float descendSpeed = clawSpeed;
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
                isAutoDescending = false;
                isGrabbing = true;
                CloseClawAsync().Forget();
            }
        }
        // 물건을 바구니로 옮기는 중
        else if (isReleasing)
        {
            // 목표 위치 도달 여부 체크
            bool reachedHeight = clawHeight.position.y >= maxHeight;
            bool reachedX = transform.position.x <= leftLimit.transform.position.x + 0.5f;
            bool reachedZ = transform.position.z <= frontLimit.transform.position.z + 0.5f;

            // x 이동
            if (!reachedX)
            {
                transform.Translate(speed * -1 * Time.deltaTime, 0, 0);
            }

            // z 이동
            if (!reachedZ)
            {
                transform.Translate(0, 0, speed * -1 * Time.deltaTime);
            }

            // y 이동
            if (!reachedHeight)
            {
                transform.Translate(0, clawSpeed * Time.deltaTime, 0);
            }

            // x, y, z 모두 도달하면
            if (reachedX && reachedZ && reachedHeight)
            {
                isInBasket = true;
                isReleasing = false;

                if (isInBasket)
                {
                    ReleasePrizeInBasketAsync().Forget();
                    isInBasket = false;
                }
            }
        }
        // 바구니에서 물건을 놓기 위해 내려가는 중
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
        // 바구니에서 물건을 놓은 후 위로 올라가는 중
        else if (isRisingFromBasket)
        {
            if (clawHeight.position.y <= maxHeight)
            {
                transform.Translate(0, clawSpeed * Time.deltaTime, 0);
            }
            else
            {
                isRisingFromBasket = false;
            }
        }
        else if (canControl)
        {
            HandlePlayerMovement();
        }
    }

    private void HandlePlayerMovement()
    {
        if (transform.position.x > leftLimit.transform.position.x && Input.GetKey(KeyCode.A))
        {
            transform.Translate(speed * -1 * Time.fixedDeltaTime, 0, 0);
        }
        if (transform.position.x < rightLimit.transform.position.x && Input.GetKey(KeyCode.D))
        {
            transform.Translate(speed * Time.fixedDeltaTime, 0, 0);
        }
        
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
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            CloseClawAnimation();
            await UniTask.Delay(2000, cancellationToken: _cts.Token);
            isGrabbing = false;
            isReleasing = true;
        }
        catch (System.OperationCanceledException)
        {
            // Handle cancellation
        }
    }

    private async UniTaskVoid ReleasePrizeInBasketAsync()
    {
        try
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            isLoweringToRelease = true;
            isReleasing = false;
            await UniTask.Delay(500, cancellationToken: _cts.Token);
        }
        catch (System.OperationCanceledException)
        {

        }
    }

    private async UniTaskVoid OpenClawAtBasketAsync()
    {
        try
        {
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            OpenClawAnimation();
            await UniTask.Delay(500, cancellationToken: _cts.Token);
            isRisingFromBasket = true;
            reachedBasket[0] = false;
            reachedBasket[1] = false;
            reachedBasket[2] = false;
        }
        catch (System.OperationCanceledException)
        {

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

    public void AutoGrabWhenTimerZero()
    {
        if (!isClawActivated && canControl)
        {
            isAutoDescending = true;
            canControl = false;
            isClawActivated = true;
        }
    }
    
}
