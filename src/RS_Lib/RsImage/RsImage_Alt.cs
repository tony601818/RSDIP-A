﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RS_Lib
{
    /// <summary>
    /// 通用-遥感图像类
    /// </summary>
    public partial class RsImage
    {

        /// <summary>
        /// 构造BSQ-由byte产生新的遥感图像数据
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="data"></param>
        /// <param name="isNeedDeepCopy"></param>
        public RsImage(string fn, byte[,,] data, bool isNeedDeepCopy = true)
        {
            this.FileName = fn;
            this._dataFilePath = "Memory";
            this._headerFilePath = "Memory";

            if (isNeedDeepCopy)
                DeepCopyData(data);
            else
                this._picData = data;
            
            ReadMetaData(data);
        }

        /// <summary>
        /// 由三维byte数组生成图像文件
        /// </summary>
        /// <param name="data"></param>
        private void ReadMetaData(byte[,,] data)
        {
            this.Interleave = "bsq";
            this.BandsCount = data.GetLength(0);
            this.Samples = data.GetLength(2);
            this.Lines = data.GetLength(1);
            this.Description = "{ Image Generated By Gp.A @ " +
                               DateTime.Now.ToString(CultureInfo.CurrentCulture) + "}";
            this.DataType = 1;
            this.XStart = 0;
            this.YStart = 0;
        }

        /// <summary>
        /// 深拷贝三维数组
        /// </summary>
        /// <param name="data"></param>
        private void DeepCopyData(byte[,,] data)
        {
            this._picData = new byte[
                data.GetLength(0),
                data.GetLength(1),
                data.GetLength(2)];

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    for (int k = 0; k < data.GetLength(2); k++)
                    {
                        this._picData[i, j, k] = data[i, j, k];
                    }
                }
            }

        }

        /// <summary>
        /// 根据图片读取元信息
        /// </summary>
        private void ReadMetaPic(int height, int width)
        {
            this.Interleave = "bsq";
            this.BandsCount = 3; // RGB
            this.Samples = width;
            this.Lines = height;
            this.Description = "{ Photo Generated By Gp.A @ " +
                               DateTime.Now.ToString(CultureInfo.CurrentCulture) + "}";
            this.DataType = 1;
            this.XStart = 0;
            this.YStart = 0;
        }

        private void ReadPictureRGB(byte[] value)
        {
            this._picData = new byte[3, this.Lines, this.Samples];

            for (int i = 0; i < this.Lines; i++)
            {
                for (int j = 0; j < this.Samples; j++)
                {
                    _picData[2, i, j] = value[0 + i * this.Samples * 3 + j * 3];
                    _picData[1, i, j] = value[1 + i * this.Samples * 3 + j * 3];
                    _picData[0, i, j] = value[2 + i * this.Samples * 3 + j * 3];
                }
            }
        }

        /// <summary>
        /// 由图片创建图像
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="info"></param>
        public RsImage(string imgPath, string info)
        {
            this._dataFilePath = imgPath;
            FileInfo fi = new FileInfo(imgPath);
            this.FileName = fi.Name;

            Bitmap bmp = new Bitmap(imgPath);

            // 高速读取图像信息
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int imgSize = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[imgSize];
            
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, imgSize);
            bmp.UnlockBits(bmpData);

            ReadMetaPic(bmp.Height, bmp.Width);
            ReadPictureRGB(rgbValues);

            this.Description += info;
        }

    }
}
