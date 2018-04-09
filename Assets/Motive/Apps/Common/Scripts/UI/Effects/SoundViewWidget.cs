// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays the waveform of an IAudioPlayer.
    /// </summary>
    public class SoundViewWidget : MonoBehaviour
    {
        public RawImage AudioWaveForm;
        public float updateAudioTextureTime = 0.033f;
        public int WaveformSegmentSize = 256;
        public Color WaveformBackgroundColor = Color.clear;
        public Color WaveformForegroundColor = Color.cyan;

        int m_height;
        int m_width;

        //Logger m_logger;

        float m_timer;
        Texture2D m_waveformTexture;
        int m_waveformTextureWidth;
        int m_waveformTextureHeight;

        double[] m_waveformSamples;
        Color[] m_blank;

        bool m_audioStarted;
        IAudioPlayer m_player;

        void Awake()
        {
            //m_logger = new Logger(this);

            m_waveformTextureWidth = (int)AudioWaveForm.rectTransform.rect.width;
            m_waveformTextureHeight = (int)AudioWaveForm.rectTransform.rect.height;

            m_waveformSamples = new double[WaveformSegmentSize];

            m_waveformTexture = new Texture2D(m_waveformTextureWidth, m_waveformTextureHeight);
            AudioWaveForm.texture = m_waveformTexture;

            int size = m_waveformTextureWidth * m_waveformTextureHeight;

            m_blank = new Color[size];

            for (var i = 0; i < size; ++i)
            {
                m_blank[i] = WaveformBackgroundColor;
            }

            //clear the texture in case no audio ever triggers
            m_waveformTexture.SetPixels(m_blank, 0);
            m_waveformTexture.Apply();

            //transform.localPosition = Vector3.zero;
        }

        public void Play(IAudioPlayer player)
        {
            m_player = player;
        }

        public void Stop()
        {
            m_player = null;
        }

        void Update()
        {
            if (m_player != null)
            {
                m_timer += Time.deltaTime;

                if (m_timer > updateAudioTextureTime)
                {
                    UpdateAudioWaveForm();
                    m_timer = 0.0f;
                }
            }
        }

        void UpdateAudioWaveForm()
        {
            if (m_player.IsPlaying)
            {
                // clear the texture
                m_waveformTexture.SetPixels(m_blank, 0);
                // get samples from channel 0 (left)
                m_player.GetOutputSamples(0, m_waveformSamples);

                // draw the waveform
                int lastReading = m_waveformTextureHeight / 2;
                for (int i = 0, x = 0; i < WaveformSegmentSize; ++i)
                {
                    var spanx = i * m_waveformTextureWidth / WaveformSegmentSize;
                    var dx = spanx - x;
                    //Single pixel at current reading
                    //m_waveformTexture.SetPixel((int)(m_waveformTextureWidth * i /WaveformSegmentSize), (int)(m_waveformTextureHeight * (m_waveformSamples[i]+1f)/2), WaveformForegroundColor );

                    //Line of pixels from last pixel to new reading

                    int reading = (int)(m_waveformTextureHeight * (m_waveformSamples[i] + 1f) / 2);

                    for (var _dx = 0; _dx < dx; _dx++, x++)
                    {
                        if (reading > lastReading)
                        {
                            for (var y = lastReading; y < reading; ++y)
                            {
                                m_waveformTexture.SetPixel(x, y, WaveformForegroundColor);
                            }
                        }
                        else
                        {
                            for (var y = lastReading; y > reading; --y)
                            {
                                m_waveformTexture.SetPixel(x, y, WaveformForegroundColor);
                            }
                        }
                    }

                    lastReading = reading;
                }

                // upload to the graphics card
                m_waveformTexture.Apply();
            }
        }
    }

}