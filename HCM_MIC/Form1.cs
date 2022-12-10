using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace HCM_MIC
{
    public partial class Form1 : Form
    {
        const int HISTO_WIDTH = 256;
        const int HISTO_HEIGHT = 256;
        Image img;
        Color color;
        double[,] v;
        int cluster = 4, coord = 3;
        double lowval;
        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1400, 920);

            pictureBox1.Location = new Point(20, 20);
            pictureBox1.Size = new Size(256, 256);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox2.Location = new Point(20, 300);
            pictureBox2.Size = new Size(256, 256);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            pictureBox3.Location = new Point(20, 600);
            pictureBox3.Size = new Size(256, 256);
            pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            label1.Location = new Point(120, 280);
            label2.Location = new Point(110, 580);
            label3.Location = new Point(110, 860);
        }
        private void 열기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "영상 파일 열기";
            openFileDialog1.Filter = "All Files(*.*) |*.*|Bitmap File(*.bmp) |*.bmp |Jpeg File(*.jpg) | *.jpg";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string strFilename = openFileDialog1.FileName;
                img = Image.FromFile(strFilename);
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);

                Bitmap bitmap = (Bitmap)pictureBox1.Image;
                byte[,] tempArray1 =BitmapToByteArray2D(bitmap);
                viewHistogram2(1, tempArray1); //r g b
                viewHistogram3(1, tempArray1);
            }
            
        }
        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public byte[,] BitmapToByteArray2D(Bitmap bmp)
        {
            byte[,] bmpArray = new byte[bmp.Width * bmp.Height, 3];
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    bmpArray[y * bmp.Width + x, 0] = color.R;
                    bmpArray[y * bmp.Width + x, 1] = color.G;
                    bmpArray[y * bmp.Width + x, 2] = color.B;
                }
            }
            return bmpArray;
        }

        private void Kmeans()
        {
            Bitmap origin = (Bitmap)pictureBox1.Image;
            int height = origin.Height;
            int width = origin.Width;
            int data = height * width;
            int sumr, sumg, sumb, count;
            byte[,] imgArray = BitmapToByteArray2D(origin);
            byte[,] x;

            x = imgArray;

            Console.WriteLine("[영상크기] h : " + height + ", w : " + width + ", wh : " + data);
            double[,] d = new double[data, cluster];    //데이터 배열
            v = new double[cluster, coord];   //각 클러스터별 벡터 값 받을 배열
            double[,] v_OLD = new double[cluster, coord]; //이전 클러스터 벡터 값 저장 배열
            double[,] U = new double[data, cluster];
            
            #region
         
            for (int i = 0; i < data; i++)
            {
                for (int j = 0; j < cluster; j++)
                {
                    U[i, j] = 0;
                }
            }

            int c = 0;
            do
            {
                for (int j = 0; j < cluster; j++)
                {
                    U[c, j] = 1;
                    if (c < data - 1)
                    {
                        c++;
                    }
                }
            } while (c < data - 1);

            for (int i = 0; i < cluster; i++)
            {
                sumr = 0;
                sumg = 0;
                sumb = 0;
                count = 0;
                for (int j = 0; j < data; j++)
                {
                    if (U[j, i] == 1)
                    {
                        sumr += x[j, 0];
                        sumg += x[j, 1];
                        sumb += x[j, 2];
                        count++;
                    }
                }
                v[i, 0] = sumr / count;
                v[i, 1] = sumg / count;
                v[i, 2] = sumb / count;
            }
            #endregion
            double min, num;
            int v_count;
            do
            {
                for (int i = 0; i < data; i++)/*Initialize U*/  //소속행렬 초기화
                {
                    for (int j = 0; j < cluster; j++)
                    {
                        U[i, j] = 0;
                    }
                }
                v_count = 0;

                for (int i = 0; i < data; i++)
                {
                    min = 0;
                    count = 0;
                    for (int j = 0; j < cluster; j++)   //유클리디안 거리 계산
                    {
                        d[i, j] = Math.Sqrt(Math.Pow((v[j, 0] - x[i, 0]), 2) + Math.Pow((v[j, 1] - x[i, 1]), 2) + Math.Pow((v[j, 2] - x[i, 2]), 2));
                        if (j == 0)
                        {
                            //여기서 min은 뭐임?
                            //첫번째 거리계싼한 애를 min으로 두고
                            min = d[i, j];
                        }
                        else if (j > 0 && (d[i, j] < min))  //그 다음부터 min갱신
                        {
                            min = d[i, j];
                            count = j;
                        }
                    }
                    U[i, count] = 1;
                }
                //무게중심의 변화체크
                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        v_OLD[i, j] = v[i, j];
                    }
                }

                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        count = 0;
                        num = 0;
                        for (int k = 0; k < data; k++)
                        {
                            num = num + (U[k, i] * x[k, j]);//소속행렬*데이터
                            if (U[k, i] == 1)
                            {
                                count++;
                            }
                        }
                        if ((num / count).Equals(double.NaN)) //double이상의 상수(예외처리)
                        {
                            v[i, j] = 0;
                        }
                        else //새로운 중심벡터 계산
                        {
                            v[i, j] = num / count;
                        }
                    }
                }

                for (int i = 0; i < cluster; i++)
                {
                    for (int j = 0; j < coord; j++)
                    {
                        if (v_OLD[i, j] != v[i, j])
                            v_count++;
                    }
                }
            } while (v_count != 0);

            //제일 명암도 낮은 클러스터
            double[] TempArray = new double[cluster];
            for (int m = 0; m < cluster; m++)
            {
                for (int n = 0; n < coord; n++)
                {
                    
                    TempArray[m] += v[m, n];
                }
                Console.WriteLine(m + "클러스터 중심벡터 합 : " + TempArray[m]);
            }
            
            lowval = 255*3;
            for (int k = 0; k < cluster; k++)
            {
                if (TempArray[k] < lowval)
                {
                    lowval = TempArray[k];
                }
            }
            Console.WriteLine("제일 작은 클러스터 : " + lowval);
           
        }
        public Bitmap byteArray2DToBitmap(byte[,] byteArray, int width, int height)
        {
            Bitmap newbmp = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color newColor = Color.FromArgb(byteArray[y * width + x, 0], byteArray[y * width + x, 1], byteArray[y * width + x, 2]);
                    newbmp.SetPixel(x, y, newColor);
                }
            }
            return newbmp;
        }

        private void 일반스트레칭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int Xmin = 255, Xmax = 0, x, y;

            int alpha, beta;    //최소, 최대
            int[] LUT = new int[256];

            Bitmap bitmap = (Bitmap)pictureBox1.Image;
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color color;
            byte[,] tempArray1 = new byte[width * height, 3];
            byte[,] newArray = new byte[width * height, 3];

            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    color = bitmap.GetPixel(w, h);
                    tempArray1[h * bitmap.Width + w, 0] = color.R;
                    tempArray1[h * bitmap.Width + w, 1] = color.G;
                    tempArray1[h * bitmap.Width + w, 2] = color.B;

                    if (Xmin > (color.R + color.G + color.B) / 3)
                        Xmin = (color.R + color.G + color.B) / 3;
                    if (Xmax < (color.R + color.G + color.B) / 3)
                        Xmax = (color.R + color.G + color.B) / 3;
                }
            }
            alpha = Xmin;
            beta = Xmax;

            for (x = 0; x < alpha; x++) LUT[x] = 0;
            for (x = 255; x > beta; x--) LUT[x] = 255;
            for (x = alpha; x <= beta; x++)
                LUT[x] = (int)((x - alpha) * 255.0 / (beta - alpha));

            for (y = 0; y < bitmap.Height; y++)
                for (x = 0; x < bitmap.Width; x++)
                {
                    newArray[y * bitmap.Width + x, 0] = (byte)LUT[tempArray1[y * bitmap.Width + x, 0]];
                    newArray[y * bitmap.Width + x, 1] = (byte)LUT[tempArray1[y * bitmap.Width + x, 1]];
                    newArray[y * bitmap.Width + x, 2] = (byte)LUT[tempArray1[y * bitmap.Width + x, 2]];
                }

            pictureBox2.Image = byteArray2DToBitmap(newArray, width, height);

            viewHistogram2(15, newArray); //r g b
            viewHistogram3(15, newArray);
        }
        void viewHistogram2(int letfTopy, byte[,] histoArray)
        {
            Graphics gr = CreateGraphics();
            int x, y;
            Color color;
            int[] histogram = new int[256];
            Bitmap histoBitmap = new Bitmap(HISTO_WIDTH, HISTO_HEIGHT);
            
            for (int rgb = 0; rgb < 3; rgb++)
            {
                histogram.Initialize();
                for (y = 0; y < img.Height; y++)
                    for (x = 0; x < img.Width; x++)
                        histogram[histoArray[y * img.Width + x, rgb]]++;

                int max_cnt = 0;
                for (x = 0; x < HISTO_WIDTH; x++)
                    if (histogram[x] > max_cnt)
                        max_cnt = histogram[x];

                for (x = 0; x < HISTO_WIDTH; x++)
                    for (y = 0; y < HISTO_HEIGHT; y++)
                    {
                        color = Color.FromArgb(125, 125, 125);
                        histoBitmap.SetPixel(x, y, color);
                    }
                for (x = 0; x < HISTO_WIDTH; x++)
                {
                    double dHeight = (double)histogram[x] * (HISTO_HEIGHT - 1) / (double)max_cnt;

                    for (y = 0; y < dHeight; y++)
                    {
                        if (rgb == 0)
                        {
                            color = Color.FromArgb(255, 0, 0);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                        else if (rgb == 1)
                        {
                            color = Color.FromArgb(0, 255, 0);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                        else
                        {
                            color = Color.FromArgb(0, 0, 255);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                    }
                    gr.DrawImage(histoBitmap, 296+ rgb *(256+10), 20* letfTopy);
                }
            }
        }
        void viewHistogram3(int letfTopy, byte[,] histoArray)
        {
            Graphics gr = CreateGraphics();
            int x, y;
            Color color;
            int[] histogram = new int[256];
            Bitmap histoBitmap = new Bitmap(HISTO_WIDTH, HISTO_HEIGHT);
            for (x = 0; x < HISTO_WIDTH; x++)
                for (y = 0; y < HISTO_HEIGHT; y++)
                {
                    color = Color.FromArgb(125, 125, 125);
                    histoBitmap.SetPixel(x, y, color);
                }
            for (int rgb = 0; rgb < 3; rgb++)
            {
                histogram.Initialize();
                for (y = 0; y < img.Height; y++)
                    for (x = 0; x < img.Width; x++)
                        histogram[histoArray[y * img.Width + x, rgb]]++;

                int max_cnt = 0;
                for (x = 0; x < HISTO_WIDTH; x++)
                    if (histogram[x] > max_cnt)
                        max_cnt = histogram[x];
                for (x = 0; x < HISTO_WIDTH; x++)
                {
                    double dHeight = (double)histogram[x] * (HISTO_HEIGHT - 1) / (double)max_cnt;

                    for (y = 0; y < dHeight; y++)
                    {
                        if (rgb == 0)
                        {
                            color = Color.FromArgb(0, 0, 0);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                        else if (rgb == 1)
                        {
                            color = Color.FromArgb(0, 0, 0);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                        else
                        {
                            color = Color.FromArgb(0, 0, 0);
                            histoBitmap.SetPixel(x, (HISTO_HEIGHT - 1) - y, color);
                        }
                    }
                    gr.DrawImage(histoBitmap, 296 + 3 * (256 + 10), 20* letfTopy);
                }

            }
            
        }
        private void 퍼지스트레칭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int x, y, r, g, b;

            int Xmean, adjustment, Imax, Imin, Imid;
            double cut, l_value, r_value;

            int alpha , beta;
            Bitmap origin = (Bitmap)pictureBox1.Image;
            int height = origin.Height;
            int width = origin.Width;
            int[] LUT = new int[256];   //히스토그램 표시할 배열
            byte[,] tempArray = new byte[height*width,3];
            tempArray.Initialize();
            tempArray = BitmapToByteArray2D(origin);
            for (int rgb = 0; rgb < 3; rgb++)   //영상 전체 채널별 히스토 평균
            {
                r = g = b = 0;
                for(x = 0; x < width; x++) 
                {
                    for(y = 0; y < height; y++)
                    {
                        color = origin.GetPixel(x, y);
                        if (rgb == 0)
                            r += color.R;
                        else if (rgb == 1)
                            g += color.G;
                        else
                            b += color.B;
                    }
                }
                r = r / (height * width);
                g = g / (height * width);
                b = b / (height * width);
                //rgb별 히스토그램 평균 설정
                if (rgb == 0)
                    Xmean = r; 
                else if (rgb == 1)
                    Xmean = g;
                else
                    Xmean = b;

                //소속함수
                if (Xmean > 128)    //밝기 조정률
                {
                    adjustment = 255 - Xmean;
                }
                else
                {
                    adjustment = Xmean;
                }
                Imax = Xmean + adjustment;
                Imin = Xmean - adjustment;
                Imid = (Imax + Imin) / 2;
                //****
                Kmeans();
                //a-cut
                //(double)(Imin / Imax);
                cut = lowval / 3;
                cut /= 255;

                Console.WriteLine("Imin : " + Imin + ", Imax : " + Imax + ", a-cut : " + cut + ", 밝기 조정률 : " + adjustment);

                l_value = (Imid - Imin) * cut + Imin;   //a-cut -> 상한, 하한 계산
                r_value = -(Imax - Imid) * cut + Imax;
                alpha = (int)l_value;
                beta = (int)r_value;

                Console.WriteLine("alpha : " + alpha + " || beta : " + beta);

                for (x = 0; x < alpha; x++) LUT[x] = 0;
                for (x = 255; x > beta; x--) LUT[x] = 255;
                //스트레칭 -> alpha => 0, beta => 255
                for (x = alpha; x <= beta; x++)
                    LUT[x] = (int)((x - alpha) * 255.0 / (beta - alpha));
                for (y = 0; y < height; y++)
                    for (x = 0; x < width; x++)
                        tempArray[y * width + x, rgb] = (byte)LUT[tempArray[y * width + x, rgb]];
            }
            pictureBox3.Image = byteArray2DToBitmap(tempArray, width, height);
            viewHistogram2(30, tempArray);
            viewHistogram3(30, tempArray);
        }
    }
}
