﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RS_Lib
{
    /// <summary>
    /// RGB转HSI
    /// </summary>
    public class RGBHSI
    {
        private struct Pixel
        {
            public double H, S, I;
        }
        
        public double[,,] HSIData { get; private set; }

        private readonly byte[] _band;
        private readonly byte[,,] _oriData;

        public byte[,,] GetLinearStretch()
        {
            int h = HSIData.GetLength(1),
                l = HSIData.GetLength(2);
            byte[,,] res = new byte[3, h, l];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int k = 0; k < l; k++)
                    {
                        var tmp = HSIData[i, j, k]*255;

                        if (tmp < 0)
                        {
                            res[i, j, k] = 0;
                            continue;
                        }
                        if (tmp > 254.5)
                        {
                            res[i, j, k] = 255;
                            continue;
                        }

                        res[i, j, k] = (byte) (tmp + 0.5);
                    }
                }
            }

            return res;
        }

        public RGBHSI(byte[,,] d, byte[] b)
        {
            if (b.Length != 3) throw new ArgumentException("选中波段必须为3个！");

            _oriData = d;
            _band = b;
            HSIData = new double[3, d.GetLength(1), d.GetLength(2)];

            TransRGB();
        }

        private void TransRGB()
        {
            for (int i = 0; i < _oriData.GetLength(1); i++)
            {
                for (int j = 0; j < _oriData.GetLength(2); j++)
                {
                    var a = RGB2HSI(
                        _oriData[_band[0], i, j],
                        _oriData[_band[1], i, j],
                        _oriData[_band[2], i, j]);

                    HSIData[0, i, j] = a.H;
                    HSIData[1, i, j] = a.S;
                    HSIData[2, i, j] = a.I;
                }
            }
        }

        private Pixel RGB2HSI(double R, double G, double B)
        {
            R = R / 255;
            G = G / 255;
            B = B / 255;
            
            double min = Math.Min(Math.Min(R, G), B),
                max = Math.Max(Math.Max(R, G), B),
                dt = max - min;

            Pixel px = new Pixel {I = max};

            // max!=0
            if (Math.Abs(max) > 1e-9)
            {
                px.S = dt/max;
            }
            else
            {
                // I无法计算
                px.S = 0;
                px.H = -1;
                return px;
            }

            if (Math.Abs(R - max) < 1e-9)
            {
                px.H = (G - B)/dt;
            }
            else if(Math.Abs(G - max) < 1e-9)
            {
                px.H = 2 + (B - R)/dt;
            }
            else
            {
                px.H = 4 + (R - G)/dt;
            }

            px.H *= 60;
            if (px.H < 0)
                px.H += 360;

            return px;
        }


    }
}
