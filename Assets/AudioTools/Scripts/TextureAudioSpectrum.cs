using UnityEngine;
using UnityEngine.UI;

namespace FredericRP.AudioTools
{
  public class TextureAudioSpectrum : MonoBehaviour
  {
    [Header("Source")]
    [SerializeField]
    [Tooltip("If none, will use the global AudioListener source")]
    AudioSource source;
    [Header("Audio wave treatment")]
    [SerializeField]
    float lerpSpeed = 100;
    [SerializeField]
    FFTWindow windowType = FFTWindow.Rectangular;
    // For fast high moving waves, ratio/offset should be around 4/160, for smaller less moving waves: 10/90
    [SerializeField]
    float ratio = 4f;
    [SerializeField]
    float offset = 200f;
    [SerializeField]
    bool logScale = true;
    [Header("Visuals")]
    [SerializeField]
    Vector2Int targetTextureSize = new Vector2Int(512, 256);
    [SerializeField]
    Texture2D rampTexture = null;
    [SerializeField]
    Image[] targetImageList = null;
    [Header("Change before entering play mode")]
    [SerializeField]
    [Tooltip("When smooth is on, target width is multiplied by 4 and a interpolation is made from one sample to the next one.")]
    bool smooth = false;
    [SerializeField]
    [Tooltip("Artefacts can occur if not point.")]
    FilterMode filterMode = FilterMode.Point;

    int sampleCount;
    float[] samples;
    Texture2D spectrumTex;
    int min, max, step;

    // Start is called before the first frame update
    void Start()
    {
      if (targetImageList == null || targetImageList.Length < 1)
      {
        Debug.LogWarning("AudioSpectrum disabled as no target image has been selected.");
        enabled = false;
        return;
      }
      sampleCount = targetTextureSize.x;
      min = Mathf.RoundToInt(Mathf.NegativeInfinity);
      max = Mathf.RoundToInt(Mathf.Infinity);
      // Multiplies by 4 the texture size if smooth is required
      if (smooth)
      {
        targetTextureSize.x *= 4;
        step = 4;
      }
      else
      {
        step = 1;
      }
      samples = new float[sampleCount];
      spectrumTex = new Texture2D(targetTextureSize.x, targetTextureSize.y, TextureFormat.RGBA32, false);
      spectrumTex.filterMode = filterMode;
      // create sprite on first target image
      targetImageList[0].sprite = Sprite.Create(spectrumTex, new Rect(0, 0, spectrumTex.width, spectrumTex.height), Vector2.zero);
      // Assign same sprite on other targets
      for (int i = 1; i < targetImageList.Length; i++)
        targetImageList[i].sprite = targetImageList[0].sprite;
    }

    // Update is called once per frame
    void Update()
    {
      if (source != null)
        source.GetSpectrumData(samples, 0, windowType);
      else
        AudioListener.GetSpectrumData(samples, 0, windowType);

      // Treatment: log, multiplier, offset
      for (int i = 0; i < sampleCount - 1; i++)
      {
        // Set ratio from texture size ?!
        samples[i] = Mathf.Lerp(samples[i], (logScale ? Mathf.Log(samples[i]) : samples[i]) * ratio + offset, lerpSpeed * Time.deltaTime);
        // Enlarge spectrum interval
        if (samples[i] < min)
          min = Mathf.RoundToInt(samples[i]);
        else if (samples[i] > max)
          max = Mathf.RoundToInt(samples[i]);
      }
      // Visual: draw pixels from source texture
      for (int i = 0; i < sampleCount; i++)
      {
        // Force at least 1 pixel (beware that if you chose anything else than Point for filter mode, 1 pixel could become invisible
        SetBar(i, 0, 2, 2);
        for (int j = 1; j < spectrumTex.height; j++)
        {
          if (i < sampleCount - 1)
            SetBar(i, j, samples[i], samples[i + 1]);
          else
            SetBar(i, j, samples[i], samples[i]);
        }
      }
      // Visual: apply changed pixels to texture
      spectrumTex.Apply();
    }

    void SetBar(int i, int j, float sample, float nextSample)
    {
      spectrumTex.SetPixel(i * step, j, j <= sample ? rampTexture.GetPixel(0, j) : Color.clear);
      if (smooth)
      {
        float target = smooth ? Mathf.Lerp(sample, nextSample, 0.25f) : sample;
        spectrumTex.SetPixel(i * step + 1, j, j <= target ? rampTexture.GetPixel(0, j) : Color.clear);
        target = smooth ? Mathf.Lerp(sample, nextSample, 0.5f) : sample;
        spectrumTex.SetPixel(i * step + 2, j, j <= target ? rampTexture.GetPixel(0, j) : Color.clear);
        target = smooth ? Mathf.Lerp(sample, nextSample, 0.75f) : sample;
        spectrumTex.SetPixel(i * step + 3, j, j <= target ? rampTexture.GetPixel(0, j) : Color.clear);
      }
    }
  }
}