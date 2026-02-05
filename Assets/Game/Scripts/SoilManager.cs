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

        private RenderTexture _soilTexture;
        private Material _displayMaterial;

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
            FillWithColor(_soilColor);

            // 렌더러에 적용
            ApplyToRenderer();
        }

        private void FillWithColor(Color color)
        {
            // 임시 텍스처 생성 및 즉시 해제
            Texture2D temp = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            temp.SetPixel(0, 0, color);
            temp.Apply();
            Graphics.Blit(temp, _soilTexture);
            Destroy(temp);
        }

        private void ApplyToRenderer()
        {
            if (_soilRenderer == null)
            {
                Debug.LogWarning("[SoilManager] Soil Renderer is not assigned!");
                return;
            }

            // 기존 머티리얼 복제하여 인스턴스화 (공유 머티리얼 오염 방지)
            _displayMaterial = new Material(Shader.Find("Sprites/Default"))
            {
                mainTexture = _soilTexture
            };
            _soilRenderer.material = _displayMaterial;
            _soilRenderer.transform.localScale = new Vector3(_worldSize.x, _worldSize.y, 1f);
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
            RenderTexture.active = _soilTexture;
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

            if (_displayMaterial != null)
            {
                Destroy(_displayMaterial);
            }
        }
    }
}
