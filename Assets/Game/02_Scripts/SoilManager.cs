using UnityEngine;

namespace Game
{
    /// <summary>
    /// 땅(Soil) 텍스처를 관리하고 굴착(Dig) 기능을 제공
    /// RenderTexture를 사용하여 실시간으로 구멍을 뚫는 효과를 구현
    /// </summary>
    public class SoilManager : MonoBehaviour
    {
        public static SoilManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Vector2 _worldSize = new Vector2(50f, 50f);
        [SerializeField] private int _resolution = 2048;
        [SerializeField] private Color _soilColor = new Color(0.5f, 0.3f, 0.1f, 1f);
        [SerializeField] private Texture _brushTexture;
        [SerializeField] private Material _digMaterial;

        [Header("References")]
        [SerializeField] private Renderer _soilRenderer;
        [SerializeField] private Renderer _shadowRenderer; // 그림자 레이어
        [SerializeField] private Renderer _backgroundRenderer; // 바닥 배경 레이어

        private RenderTexture _soilTexture;
        private RenderTexture _shadowTexture;
        
        private Material _soilMaterial;
        private Material _shadowMaterial;

        // 캐싱: 매 프레임 new Rect 생성을 방지
        private Rect _brushRect;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeSoil();
        }

        private void InitializeSoil()
        {
            // RenderTexture 생성
            _soilTexture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGB32)
            {
                name = "SoilRenderTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _soilTexture.Create();

            // 초기 색상으로 채우기
            FillWithColor(_soilTexture, _soilColor);

            // 렌더러에 적용
            ApplyToRenderer(_soilRenderer, ref _soilMaterial, _soilTexture);
            
            // 그림자 초기화
            InitializeShadow();
            
            // 배경 초기화
            InitializeBackground();
        }

        private void InitializeShadow()
        {
            if (_shadowRenderer == null) return;

            // 그림자용 텍스처 생성 (Soil과 동일 해상도)
            _shadowTexture = new RenderTexture(_resolution, _resolution, 0, RenderTextureFormat.ARGB32)
            {
                name = "ShadowRenderTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _shadowTexture.Create();

            // 그림자는 검은색(0,0,0, 0.5f)으로 채움
            FillWithColor(_shadowTexture, new Color(0, 0, 0, 0.5f));

            // 렌더러 적용
            ApplyToRenderer(_shadowRenderer, ref _shadowMaterial, _shadowTexture);
        }

        private void InitializeBackground()
        {
            if (_backgroundRenderer == null) return;
            // 배경은 별도의 RenderTexture 없이 텍스처만 타일링되면 됨 (이미 세팅되었다고 가정)
            // 필요하다면 여기서 메테리얼 인스턴싱 등을 수행
        }

        private void FillWithColor(RenderTexture targetRT, Color color)
        {
            // 임시 텍스처 생성 및 즉시 해제
            Texture2D temp = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            temp.SetPixel(0, 0, color);
            temp.Apply();
            Graphics.Blit(temp, targetRT);
            Destroy(temp);
        }

        private void ApplyToRenderer(Renderer renderer, ref Material materialInstance, Texture texture)
        {
            if (renderer == null) return;

            // URP Unlit 사용
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            materialInstance = new Material(shader)
            {
                mainTexture = texture
            };
            
            // Alpha Clipping 등을 방지하기 위해 Transparent 설정 (URP 표준)
            materialInstance.SetFloat("_Surface", 1.0f); // 1 = Transparent
            materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materialInstance.SetInt("_ZWrite", 0);
            materialInstance.renderQueue = 3000;

            renderer.material = materialInstance;
            renderer.transform.localScale = new Vector3(_worldSize.x, _worldSize.y, 1f);
        }

        /// <summary>
        /// 지정된 월드 좌표에 구멍을 뚫습니다.
        /// </summary>
        /// <param name="worldPos">월드 좌표</param>
        /// <param name="radius">굴착 반경</param>
        public void Dig(Vector2 worldPos, float radius)
        {
            if (_digMaterial == null || _brushTexture == null) return;

            // 월드 좌표 -> UV 좌표 변환
            float uvX = (worldPos.x + _worldSize.x * 0.5f) / _worldSize.x;
            float uvY = (worldPos.y + _worldSize.y * 0.5f) / _worldSize.y;

            // 범위 체크 (Early Exit)
            if (uvX < 0f || uvX > 1f || uvY < 0f || uvY > 1f) return;

            // 픽셀 좌표 계산
            float pixelX = uvX * _resolution;
            float pixelY = uvY * _resolution;
            float pixelRadius = (radius / _worldSize.x) * _resolution;

            // Rect 재사용 (GC 최적화)
            _brushRect.Set(
                pixelX - pixelRadius,
                pixelY - pixelRadius,
                pixelRadius * 2f,
                pixelRadius * 2f
            );

            // GL 컨텍스트 설정 및 그리기
            DigTexture(_soilTexture);
            
            // 그림자도 똑같이 파줌 (동기화)
            if (_shadowTexture != null)
            {
                DigTexture(_shadowTexture);
            }
        }

        private void DigTexture(RenderTexture targetRT)
        {
            RenderTexture.active = targetRT;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _resolution, 0, _resolution);

            Graphics.DrawTexture(_brushRect, _brushTexture, _digMaterial);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        private void OnDestroy()
        {
            if (_soilTexture != null)
            {
                _soilTexture.Release();
                _soilTexture = null;
            }

            if (_soilMaterial != null)
            {
                Destroy(_soilMaterial);
            }
            if (_shadowMaterial != null)
            {
                Destroy(_shadowMaterial);
            }
        }
    }
}
