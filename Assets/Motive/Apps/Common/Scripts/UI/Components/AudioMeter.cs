// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using System;
using Motive.Core.Media;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays an audio waveform meter.
    /// </summary>
    public class AudioMeter : MonoBehaviour
    {
        public int ColumnCount;
        public int ElementCount;
        public float LowCutoff = 0.0001f;
        public float HighCutoff = 0.01f;

        public GameObject MeterContainer;

        public GameObject MeterColumnPrefab;
        public GameObject MeterElementPrefab;

        public Color ElementColor;

        double[] m_playerSamples;
        GameObject[] m_columns;

        //static double g_log2 = Math.Log10(2) * 20;

        double maxInput;
        double minInput = 1;

        public IAudioPlayer Player;

        void Awake()
        {
            if (!MeterContainer)
            {
                MeterContainer = gameObject;
            }

            m_playerSamples = new double[16];

            m_columns = new GameObject[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
            {
                var col = Instantiate(MeterColumnPrefab);

                col.transform.SetParent(MeterContainer.transform);

                m_columns[i] = col;

                for (int e = 0; e < ElementCount; e++)
                {
                    var elem = Instantiate(MeterElementPrefab);
                    elem.transform.SetParent(col.transform);

                    var layout = elem.GetComponent<LayoutElement>();

                    if (layout)
                    {
                        layout.preferredHeight = ((RectTransform)MeterContainer.transform).rect.height / ElementCount;
                    }
                }
            }

        }

        void Update()
        {
            var H = (HighCutoff - 10 * LowCutoff) / 9.0;
            var S = 1.0 / (H + HighCutoff);

            if (Player != null && Player.IsPlaying)
            {
                Player.GetOutputSamples(0, m_playerSamples);

                for (int c = 0; c < ColumnCount; c++)
                {
                    var col = m_columns[c];

                    var s = m_playerSamples[c];

                    var input = Math.Abs(s); // (((Math.Log10(Math.Abs(s) + 1)) / g_log2) + 1);

                    if (input > maxInput) maxInput = input;
                    if (input < minInput) minInput = input;

                    //input = Math.Log10(input * sensitivity);
                    input = Math.Log10((input + H) * S);

                    var h = ElementCount + (input * ElementCount);

                    //Debug.Log("max=" + maxInput + ", min=" + minInput + ", H=" + H + ", sensitivity=" + S + ", s=" + s + ", i=" + input + ", h=" + h);

                    for (int e = 0; e < col.transform.childCount; e++)
                    {
                        var child = col.transform.GetChild(e);

                        if (e < h)
                        {
                            child.gameObject.SetActive(true);
                            var img = child.GetComponentInChildren<Image>();

                            if (img)
                            {
                                img.color = ElementColor;
                            }
                        }
                        else
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int c = 0; c < ColumnCount; c++)
                {
                    var col = m_columns[c];

                    for (int e = 0; e < col.transform.childCount; e++)
                    {
                        var child = col.transform.GetChild(e);

                        if (e == 0)
                        {
                            child.gameObject.SetActive(true);

                            var img = child.GetComponentInChildren<Image>();

                            if (img)
                            {
                                img.color = ElementColor;
                            }
                        }
                        else
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}