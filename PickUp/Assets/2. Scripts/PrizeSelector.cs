using UnityEditor.Rendering;
using UnityEditor.TerrainTools;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PrizeSelector : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;
    private VisualElement root;
    private Label prizeNameLabel;
    private Label exitLabel;
    private VisualElement frame;
    private VisualElement giftBox;
    
    private VisualElement prizeArea;

    private bool isDropping = false;
    private bool isMouseInside = false;

    void Start()
    {
        if (uiDocument == null) return;
        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        if (root == null) return;

        exitLabel = root.Q<Label>("exitLabel");
        frame = root.Q<VisualElement>("frame");
        giftBox = root.Q<VisualElement>("giftBox");
        prizeNameLabel = root.Q<Label>("prizeNameLabel");
        prizeArea = root.Q<VisualElement>("prizeArea");

        if (prizeArea != null)
        {
            // 마우스 이벤트 등록
            prizeArea.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            prizeArea.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            prizeArea.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        // 시작할 때는 UI를 숨김
        HideUI();
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        isMouseInside = true;
    }

    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        isMouseInside = false;
        if (!isDropping)
        {
            HideUI();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!isMouseInside || isDropping) return;

        // 프레임의 높이를 기준으로 마우스 위치 계산
        float frameHeight = frame.resolvedStyle.height;
        float mouseY = evt.localMousePosition.y;
        float threshold = frameHeight * 0.7f; // 70% 지점을 기준으로

        // 마우스가 아래에서 위로 올라올 때 선물 표시
        if (mouseY < threshold)
        {
            if (!root.style.display.value.Equals(DisplayStyle.Flex))
            {
                ShowUI("Prize");
            }
        }
        else
        {
            if (!isDropping)
            {
                HideUI();
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (isDropping) return;
        
        if (other.gameObject.CompareTag("Prize"))
        {
            ShowUI(other.gameObject.name);
            isDropping = true;
        }
    }

    private void ShowUI(string prizeName)
    {
        if (root == null) return;

        root.style.display = DisplayStyle.Flex;
        if (prizeNameLabel != null) prizeNameLabel.text = prizeName;

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
        float targetY = 40f;
        
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

    private void HideUI()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            isDropping = false;
        }
    }

    private void OnDisable()
    {
        HideUI();
    }
}
