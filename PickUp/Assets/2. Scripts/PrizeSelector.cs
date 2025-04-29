using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Threading;
using NUnit.Framework;

public class PrizeSelector : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;
    
    [SerializeField]
    private GameObject[] effectPrefab;
    
    [SerializeField]
    private Transform[] effectSpawnPoint;
    
    private VisualElement root;
    private VisualElement frame;
    private VisualElement giftBox;
    private VisualElement prizeArea;
    private VisualElement present;
    
    private Label prizeNameLabel;
    private Label priceLabel;
    
    private Button exitLabel;
    private Button exitButton;
    private Button retryButton;

    private bool isDropping = false;
    private bool isMouseInside = false;
    private bool isDragging = false;
    
    private float dragStartY;
    private float initialPrizeAreaHeight;
    private const float minPrizeAreaHeight = 0f;
    
     private GameObject currentPrizeObject;
     private GameObject spawnedEffectInstance;
    // private StyleBackground originalBackground;

    void Start()
    {
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        if (root == null) return;
        
        present   = root.Q<VisualElement>("present");
        frame     = root.Q<VisualElement>("frame");
        giftBox   = root.Q<VisualElement>("giftBox");
        prizeArea = root.Q<VisualElement>("prizeArea");
        
        prizeNameLabel = root.Q<Label>("prizeNameLabel");
        priceLabel     = root.Q<Label>("priceLabel");
        
        exitLabel = root.Q<Button>("exitLabel");
        exitButton  = root.Q<Button>("exitButton");
        retryButton = root.Q<Button>("retryButton");
        
        retryButton.style.display = DisplayStyle.None;
        exitButton.style.display  = DisplayStyle.None;
        
        prizeArea.RegisterCallback<PointerDownEvent>(OnPointerDown);
        prizeArea.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        prizeArea.RegisterCallback<PointerUpEvent>(OnPointerUp);


        exitLabel.clicked += () => OnExitClicked();
        
        // 시작할 때는 UI를 숨김
        HideUI();
    }

    void OnPointerDown(PointerDownEvent evt)
    {
        isDragging = true;
        dragStartY = evt.position.y;
        initialPrizeAreaHeight = prizeArea.resolvedStyle.height;
    }

    void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging) return;

        float deltaY = dragStartY - evt.position.y; // 위로 올릴수록 양수
        float newHeight = Mathf.Max(initialPrizeAreaHeight - deltaY, minPrizeAreaHeight);

        prizeArea.style.height = new Length(newHeight, LengthUnit.Pixel);
    }
    
    void OnPointerUp(PointerUpEvent evt)
    {
        isDragging = false;

        float frameHeight = frame.resolvedStyle.height;
        float currentHeight = prizeArea.resolvedStyle.height;

        if (currentPrizeObject.gameObject.layer == 11)
        {
            present.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("8. Images/Airpot"));
        }
        else if (currentPrizeObject.gameObject.layer == 13)
        {
            present.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("8. Images/Mouse"));
        }
        else if (currentPrizeObject.gameObject.layer == 14)
        {
            present.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("8. Images/clock"));
        }
        else if (currentPrizeObject.gameObject.layer == 10)
        {
            present.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("8. Images/iPhone"));
        }
        else if (currentPrizeObject.gameObject.layer == 9)
        {
            present.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("8. Images/wallet"));
        }
        
        
        if (currentHeight <= frameHeight / 2.0f) // 절반 이상 열었으면
        {
            StartCoroutine(AnimateLiftUpDown(true));
            PlayEffect();
            // 밀어올려서 상품확인하기에서
            // 모든 상품의 Tag는 Prize이며, 각 상품마다 Layer을 지정해서, 구분을 시켜준다.
            
            exitLabel.style.display = DisplayStyle.None;
            
            if (currentPrizeObject.gameObject.layer == 11)
            {
                prizeNameLabel.text = "AirPods Pro Gen 3";
                prizeNameLabel.style.fontSize = 28;

                priceLabel.text = "51,000원";
            }
            else if (currentPrizeObject.gameObject.layer == 13)
            {
                prizeNameLabel.text = "G502 Wired Gaming Mouse";
                prizeNameLabel.style.fontSize = 28;

                priceLabel.text = "109,000원";
            }
            else if (currentPrizeObject.gameObject.layer == 14)
            {
                prizeNameLabel.text = "Omega Speedmaster Moonwatch";
                prizeNameLabel.style.fontSize = 28;

                priceLabel.text = "2,380,000원";
            }
            else if (currentPrizeObject.gameObject.layer == 10)
            {
                prizeNameLabel.text = "Iphone 16 pro max";
                prizeNameLabel.style.fontSize = 28;

                priceLabel.text = "1,580,000원";
            }
            else if (currentPrizeObject.gameObject.layer == 9)
            {
                prizeNameLabel.text = "Prada Leather Card Wallet Black";
                prizeNameLabel.style.fontSize = 28;

                priceLabel.text = "780,000원";
            }
            
            // root 기본 이미지를 없애고, RenderTexture로 바꾸기
            if (root != null)
            {
                if (effectPrefab[1] == null) return;
                if (effectSpawnPoint[1] == null) return;
                
                spawnedEffectInstance = Instantiate(effectPrefab[1], effectSpawnPoint[1].position, Quaternion.identity);
            }
            
            // 이후, 뽑기에서 exitIcon이나, retryIcon을 누르면 effectInstnace를 Destroy하고,
            // 원래  배경색상도변경
            // present.style.backgroundImage도변경
            // 1. retryButton, exitButton -> Enable
            retryButton.style.display = DisplayStyle.Flex;
            exitButton.style.display  = DisplayStyle.Flex;
            
            // 2. Button Event Add
            retryButton.clicked -= OnRetryClicked; 
            retryButton.clicked += OnRetryClicked;

            exitButton.clicked -= OnExitClicked;
            exitButton.clicked += OnExitClicked;
        }
        else 
        {
            StartCoroutine(AnimateLiftUpDown(false));
        }
    }
    
    private void PlayEffect()
    {
        if (effectPrefab[0]     == null) return;
        if (effectSpawnPoint[0] == null) return;

        GameObject effectInstance = Instantiate(effectPrefab[0], effectSpawnPoint[0].position, Quaternion.identity);
        Destroy(effectInstance, 2.0f);
    }

    IEnumerator AnimateLiftUpDown(bool bUp)
    {
        float startHeight = prizeArea.resolvedStyle.height;
        float endHeight = 0.0f;
        if (!bUp) endHeight = frame.resolvedStyle.height;  
        
        float duration = 0.4f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = 1 - Mathf.Pow(1 - t, 3); // EaseOutCubic
            float currentHeight = Mathf.Lerp(startHeight, endHeight, eased);
            prizeArea.style.height = new Length(currentHeight, LengthUnit.Pixel);
            yield return null;
        }

        prizeArea.style.height = new Length(endHeight, LengthUnit.Pixel);
    }

    void OnCollisionEnter(Collision other)
    {
        if (isDropping) return;
        
        if (other.gameObject.CompareTag("Prize"))
        {
            currentPrizeObject = other.gameObject;
            ShowUI(other.gameObject.name);
            isDropping = true;
        }
    }

    private void ShowUI(string prizeName)
    {
        if (root == null) return;

        root.style.display = DisplayStyle.Flex;

        if (giftBox != null && frame != null)
        {
            float frameHeight = frame.resolvedStyle.height;
            float giftBoxHeight = giftBox.resolvedStyle.height;
            
            float startY = -(giftBoxHeight);
            float targetY = (frameHeight - giftBoxHeight) / 2;
            
            DropGiftAnimationAsync(giftBox, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    async UniTaskVoid DropGiftAnimationAsync(VisualElement element, CancellationToken cancellationToken)
    {
        // Translate
        float startY = -60f;
        float targetY = 30f;
        
        // Duration
        float duration = 1.0f;
        float Timer = 0.0f;
        float fallDurationRatio = 0.3f; // 전체 시간 중 떨어지는 시간 비율
        
        float shakeAmount = 60f; 
        float shakeSpeed = 12f; 
        
        bool hasBounced = false;
        float bounceHeight = -50f; // 튀어오르는 높이


        element.style.translate = new StyleTranslate(new Translate(0, startY, 0));

        while (Timer < duration)
        {
            Timer += Time.deltaTime;
            float t = Timer / duration;
            
            if (!hasBounced)
            {
                // 1. 가속도
                float fallT = Mathf.Min(t / fallDurationRatio, 1f);
                float eased = fallT * fallT;
                
                // 2. Y축 이동
                float currentY = Mathf.Lerp(startY, targetY, eased);

                float shake = Mathf.Sin(Timer * shakeSpeed) * shakeAmount * (1 - eased);

                element.style.rotate = new StyleRotate(new Rotate(shake));

                element.style.translate = new StyleTranslate(new Translate(0, currentY, 0));
                
                if (Mathf.Abs(currentY - targetY) < 0.1f)
                {
                    hasBounced = true;
                }
            }
            else
            {
                // 1. 시작 위치
                float bounceStartY = targetY;
                // 2. 정규화
                float bounceT = t * 2f - 1f; // Normalize
                // 3. 포물선
                float bounceY = -bounceHeight * (bounceT * bounceT - 1f);
              
                float eased = 1 - Mathf.Pow(1 - t, 3);
                float currentY = Mathf.Lerp(0, bounceStartY + bounceY, eased);
  
                float shake = Mathf.Sin(Timer * shakeSpeed) * shakeAmount * (1 - eased);
   
                element.style.rotate = new StyleRotate(new Rotate(shake));
  
                element.style.translate = new StyleTranslate(new Translate(0, currentY, 0));
            }
            
            await UniTask.Yield(cancellationToken);
        }
        hasBounced = false;
    }

    private void OnRetryClicked()
    {
        Destroy(currentPrizeObject);
        
        if (spawnedEffectInstance != null)
        {
            Destroy(spawnedEffectInstance);
            spawnedEffectInstance = null;
        }
        
        isDropping = false;
        isMouseInside = false;
        isDragging = false;
        dragStartY = 0f;
        initialPrizeAreaHeight = 0f;

        // UI 요소 초기화
        if (giftBox != null)
        {
            giftBox.style.top = 0;
            giftBox.style.height = new Length(240, LengthUnit.Pixel);
            giftBox.style.rotate = new StyleRotate(new Rotate(0));
            giftBox.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }

        if (present != null)
        {
            present.style.backgroundImage = new StyleBackground();
        }

        if (prizeArea != null)
        {
            prizeArea.style.height = new Length(240, LengthUnit.Pixel);
            prizeArea.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }
        
        if (prizeNameLabel != null)
        {
            prizeNameLabel.text = "▲ 밀어올려서 상품 확인하기 ▲";
            prizeNameLabel.style.fontSize = 24;
        }

        if (priceLabel != null)
        {
            priceLabel.text = "";
        }
        
        retryButton.style.display = DisplayStyle.None;
        exitButton.style.display = DisplayStyle.None;
        exitLabel.style.display = DisplayStyle.Flex;

        HideUI();
    }

    private void OnExitClicked()
    {
        Application.Quit();

        // Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HideUI()
    {
        if (root != null) root.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        HideUI();
    }
}
