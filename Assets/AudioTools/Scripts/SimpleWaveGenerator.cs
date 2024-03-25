using UnityEngine;


namespace FredericRP.AudioTools
{
  public class SimpleWaveGenerator : MonoBehaviour
  {
    [Header("Sound generation")]
    [SerializeField]
    [Range(1, 20000)]
    float channel1Frequency = 440;
    [SerializeField]
    [Range(1, 20000)]
    float channel2Frequency = 440;
    [SerializeField]
    float sampleRate = 44100;
    [SerializeField]
    float waveLengthInSeconds = 2.0f;

    AudioSource audioSource;
    int timeIndex = 0;

    private void Start()
    {
      audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.playOnAwake = false;
      audioSource.spatialBlend = 0; //force 2D sound
      audioSource.Stop(); //avoids audiosource from starting to play automatically
    }

    private void Update()
    {
      if (!audioSource.isPlaying)
      {
        timeIndex = 0;  //resets timer before playing sound
        audioSource.Play();
      }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
      for (int i = 0; i < data.Length; i += channels)
      {
        if (channels == 1)
          data[i] = CreateSine(timeIndex, channel1Frequency, sampleRate);
        if (channels == 2)
          data[i + 1] = CreateSine(timeIndex, channel2Frequency, sampleRate);
        timeIndex++;

        //if timeIndex gets too big, reset it to 0
        if (timeIndex >= (sampleRate * waveLengthInSeconds))
        {
          timeIndex = 0;
        }
      }
    }

    //Creates a sinewave
    public float CreateSine(int timeIndex, float frequency, float sampleRate)
    {
      return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
    }

  }
}