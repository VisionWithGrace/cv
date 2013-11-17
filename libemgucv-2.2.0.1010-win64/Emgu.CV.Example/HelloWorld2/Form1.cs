using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Kinect;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace HelloWorld
{
    public partial class Form1 : Form
    {
        public static byte CalculateIntensityFromDistance(short distance)
        {
            // This will map a distance value to a 0 - 255 range
            // for the purposes of applying the resulting value
            // to RGB pixels.
            const int MaxDepthDistance = 4000;
            const int MinDepthDistance = 800;
            const int MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

            int newDistance = distance - MinDepthDistance;
            if (newDistance >= 0)
                return (byte)(255 - (255 * newDistance
                / (MaxDepthDistanceOffset)));
            else
                return (byte)255;
        }
        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Intermediate storage for the depth data received from the sensor
        /// </summary>
        private DepthImagePixel[] depthPixels;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;

        /// <summary>
        /// Intermediate storage for the player opacity mask
        /// </summary>
        private int[] playerPixelData;

        /// <summary>
        /// Intermediate storage for the depth to color mapping
        /// </summary>
        private ColorImagePoint[] colorCoordinates;

        /// <summary>
        /// Inverse scaling factor between color and depth
        /// </summary>
        private int colorToDepthDivisor;

        /// <summary>
        /// Width of the depth image
        /// </summary>
        private int depthWidth;

        /// <summary>
        /// Height of the depth image
        /// </summary>
        private int depthHeight;

        /// <summary>
        /// Width of the color image
        /// </summary>
        private int colorWidth;

        /// <summary>
        /// Height of the color image
        /// </summary>
        private int colorHeight;


        private Image<Bgra, Byte> emguOverlayedDepth;
        private Image<Gray, Byte> emguOverlayedGrayDepth;
        private Image<Gray, Byte> emguProcessedGrayDepth;
        private Image<Gray, Byte> emguDepthWithBoxes;
        private Image<Bgra, Byte> emguRawColor;
        private Image<Bgra, Byte> emguProcessedColor;
        private Bitmap colorBitmap;
        private bool firstDepthFrame = true;
        private bool firstColorFrame = true;
        //private List<PointF>[] pointList;

        private HashSet<int> GoodColors;

        private int frameLimit = 60;
        private int frameCounter = 0;
        /// <summary>
        /// Open form
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Form1_Load_1(object sender, EventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthFormat);
                this.depthWidth = this.sensor.DepthStream.FrameWidth;
                this.depthHeight = this.sensor.DepthStream.FrameHeight;

                this.sensor.ColorStream.Enable(ColorFormat);
                this.colorWidth = this.sensor.ColorStream.FrameWidth;
                this.colorHeight = this.sensor.ColorStream.FrameHeight;

                this.colorToDepthDivisor = this.colorWidth / this.depthWidth;

                // Turn on to get player masks
                this.sensor.SkeletonStream.Enable();

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];

                // Allocate space to put the color pixels we'll create
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                this.playerPixelData = new int[this.sensor.DepthStream.FramePixelDataLength];

                this.colorCoordinates = new ColorImagePoint[this.sensor.DepthStream.FramePixelDataLength];

                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.AllFramesReady += this.SensorAllFramesReady;

                this.GoodColors = new HashSet<int>();

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                
            }
        }



        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, so nothing to do
            if (null == this.sensor)
            {
                return;
            }

            bool depthReceived = false;
            bool colorReceived = false;


            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (null != depthFrame && (this.frameCounter++ >= this.frameLimit) )
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                    this.firstDepthFrame = false;
                    depthReceived = true;
                    this.frameCounter = 0;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame && depthReceived)
                {
                    // Copy the pixel data from the image to a temporary array
                    // Pigeons
                    //colorFrame.CopyPixelDataTo(this.colorPixels);
                    //this.emguRawColor = new Image<Bgra, Byte>(colorFrame.Width, colorFrame.Height, new Bgra(255,0,255,255));
                    
                    this.colorBitmap = new Bitmap(ColorImageFrameToBitmap(colorFrame));
                    this.emguRawColor = new Image<Bgra, byte>(colorBitmap);
                    this.emguProcessedColor = new Image<Bgra, byte>(colorBitmap);
                    this.GoodColors.Clear();
                    colorReceived = true;
                }
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (true == depthReceived)
            {
                this.sensor.CoordinateMapper.MapDepthFrameToColorFrame(
                    DepthFormat,
                    this.depthPixels,
                    ColorFormat,
                    this.colorCoordinates);

                //Array.Clear(this.playerPixelData, 0, this.playerPixelData.Length);
                this.emguOverlayedDepth = new Image<Bgra, byte>(this.colorWidth, this.colorHeight, new Bgra(0, 0, 0, 255));
                this.emguOverlayedDepth.Resize(2, INTER.CV_INTER_NN);
                //Loop through colorCoordinates
                for(int i=0; i<this.colorCoordinates.Length; i++)
                {
                    if(this.depthPixels[i].IsKnownDepth)
                    {
                        // scale color coordinates to depth resolution
                        int colorInDepthX = this.colorCoordinates[i].X / this.colorToDepthDivisor;
                        int colorInDepthY = this.colorCoordinates[i].Y / this.colorToDepthDivisor;

                        if (colorInDepthX > 0 && colorInDepthX < this.depthWidth && colorInDepthY >= 0 && colorInDepthY < this.depthHeight)
                        {
                            if(this.depthPixels[i].Depth < 4000)
                            {
                                var intensity = CalculateIntensityFromDistance(this.depthPixels[i].Depth);
                                emguOverlayedDepth[colorInDepthY, colorInDepthX] = new Bgra(intensity, intensity, intensity, 255);
                            }
                            /*else
                            {
                                emguOverlayedDepth[colorInDepthY, colorInDepthX] = new Bgra(0, 0, 0, 255);
                            }*/
                        }
                    }
                }
                //right one
                emguDepthImageBox.Image = this.emguOverlayedDepth;
                
                this.emguOverlayedGrayDepth = new Image<Gray, byte>(this.colorWidth, this.colorHeight, new Gray(0));
                this.emguOverlayedGrayDepth.Resize(2,INTER.CV_INTER_NN);
                this.emguOverlayedGrayDepth = this.emguOverlayedDepth.Convert<Gray, Byte>();
                this.emguProcessedGrayDepth = new Image<Gray, byte>(this.colorWidth, this.colorHeight, new Gray(0));
                this.emguProcessedGrayDepth.Resize(2, INTER.CV_INTER_NN);
                CvInvoke.cvSmooth(this.emguOverlayedGrayDepth, this.emguOverlayedGrayDepth, Emgu.CV.CvEnum.SMOOTH_TYPE.CV_MEDIAN, 5, 5, 9, 9);
                //CvInvoke.cvSobel(this.emguOverlayedGrayDepth, this.emguProcessedGrayDepth, 1, 0, 3);
                CvInvoke.cvCanny(this.emguOverlayedGrayDepth, this.emguProcessedGrayDepth, 100, 50, 3);

                Point seedPoint;
                this.emguOverlayedGrayDepth = this.emguProcessedGrayDepth.Copy();
                for (int fillY = this.colorHeight - 1; fillY > 0; fillY--)
                {
                    for (int fillX = this.colorWidth - 1; fillX > 0; fillX--)
                    {
                        seedPoint = new Point(fillX, fillY);
                        if (this.emguOverlayedGrayDepth[fillY,fillX].Intensity >= 200)
                        {
                            if (fillX < this.colorWidth - 1)
                            {
                                this.emguProcessedGrayDepth[fillY, fillX + 1] = new Emgu.CV.Structure.Gray(255);
                            }
                            if (fillX > 0)
                            {
                                this.emguProcessedGrayDepth[fillY, fillX - 1] = new Emgu.CV.Structure.Gray(255);
                            }
                            if (fillY < this.colorHeight - 1)
                            {
                                this.emguProcessedGrayDepth[fillY + 1, fillX] = new Emgu.CV.Structure.Gray(255);
                            }
                            if (fillY > 0)
                            {
                                this.emguProcessedGrayDepth[fillY - 1, fillX] = new Emgu.CV.Structure.Gray(255);
                            }
                        }
                    }
                }

                // Flood-fill method for boxes
                IntPtr src = this.emguProcessedGrayDepth;
	            MCvScalar newVal;
	            MCvScalar loDiff = new MCvScalar(0);
	            MCvScalar upDiff = new MCvScalar(0);
                MCvConnectedComp comp = new MCvConnectedComp();

                List<PointF>[] pointList = new List<PointF>[256];
                for (int i = 0; i < 256; i++)
                {
                    pointList[i] = new List<PointF>();
                }

	            int flags = 4;
	            IntPtr mask = new Image<Gray, byte>(this.emguProcessedGrayDepth.Width+2, this.emguProcessedGrayDepth.Height + 2, new Gray (0));
                int whatColor = 0;
                for (int fillY = this.emguProcessedGrayDepth.Height - 1; fillY > 0; fillY -= this.emguProcessedGrayDepth.Height / 256)
                {
                    for (int fillX = this.emguProcessedGrayDepth.Width - 1; fillX > 0; fillX -= this.emguProcessedGrayDepth.Width / 256)
                    {
                        seedPoint = new Point(fillX, fillY);
                        if (this.emguProcessedGrayDepth[seedPoint].Intensity == 0)
                        {
                            whatColor++;
                            newVal = new MCvScalar(whatColor);
                            CvInvoke.cvFloodFill(src, seedPoint, newVal, loDiff, upDiff, out comp, flags, mask);
                            pointList[whatColor].Add(new PointF(fillX, fillY));
                        }
                        else
                        {
                            pointList[(int)this.emguProcessedGrayDepth[seedPoint].Intensity].Add(new PointF(fillX, fillY));
                        }
                    }
                }

                this.emguDepthWithBoxes = this.emguProcessedGrayDepth.Copy();
                double boxColor = 127;
                this.GoodColors.Clear();
                int boxCounter = 0;
                for (int i = 1; i < 255; i++)
                {
                    if (pointList[i].Count > 100)
                    {
                        Rectangle temp = Emgu.CV.PointCollection.BoundingRectangle(pointList[i].ToArray());
                        if ((temp.Height < 0.6 * this.emguProcessedGrayDepth.Height)
                            && (temp.Width < 0.6 * this.emguProcessedGrayDepth.Width))
                        {
                            this.emguDepthWithBoxes.Draw(temp, new Gray(boxColor), 2);
                        }
                        boxCounter++;
                        if (pointList[i].Count < 4000)
                        {
                            this.GoodColors.Add(i);
                        }
                        
                    }
                }
               /* if (this.GoodColors.Count() != boxCounter)
                {
                    throw new ExternalException();
                }*/

                emguDepthProcessedImageBox.Image = this.emguDepthWithBoxes;
           //     this.emguProcessedGrayDepth
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (true == colorReceived)
            {
                //Assign raw color
                this.emguRawColor = this.emguRawColor.Resize(.5, INTER.CV_INTER_NN).Copy();
                this.emguProcessedColor = this.emguProcessedColor.Resize(0.5, INTER.CV_INTER_NN);

                emguColorImageBox.Image = this.emguRawColor; 
  
                //Assign colored pixels
                for (int fillY = this.emguProcessedColor.Height - 1; fillY > 0; fillY--)
                {
                    for (int fillX = this.emguProcessedColor.Width - 1; fillX > 0; fillX--)
                    {
                        Point seedPoint = new Point(fillX, fillY);
                        if (!this.GoodColors.Contains((int)this.emguProcessedGrayDepth[seedPoint].Intensity))
                        {
                            double b = this.emguProcessedColor[seedPoint].Blue;
                            double g = this.emguProcessedColor[seedPoint].Green;
                            double r = this.emguProcessedColor[seedPoint].Red;
                            double a = this.emguProcessedColor[seedPoint].Alpha;

                            this.emguProcessedColor[seedPoint] = new Bgra(b,g,r,a/8);
                        }
                        /*else
                        {
                            this.emguProcessedColor[seedPoint] = new Bgra(0, 255, 0, 255);
                        }*/
                    }
                }
                this.GoodColors.Clear();

                //Assign processed color
                emguColorProcessedImageBox.Image = this.emguProcessedColor;
            }
        }

        private void emguColorImageBox_Click(object sender, EventArgs e)
        {

        }

        private static Bitmap ColorImageFrameToBitmap(ColorImageFrame colorFrame)
        {
            byte[] pixelBuffer = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(pixelBuffer);

            Bitmap bitmapFrame = new Bitmap(colorFrame.Width, colorFrame.Height,
                PixelFormat.Format32bppRgb);

            BitmapData bitmapData = bitmapFrame.LockBits(new Rectangle(0, 0,
                                             colorFrame.Width, colorFrame.Height),
            ImageLockMode.WriteOnly, bitmapFrame.PixelFormat);

            IntPtr intPointer = bitmapData.Scan0;
            Marshal.Copy(pixelBuffer, 0, intPointer, colorFrame.PixelDataLength);

            bitmapFrame.UnlockBits(bitmapData);
            return bitmapFrame;
        }
    }
}
