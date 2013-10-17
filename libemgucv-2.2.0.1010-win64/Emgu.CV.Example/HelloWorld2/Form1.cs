﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
        private Image<Bgra, Byte> emguRawColor;

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
                if (null != depthFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                    depthReceived = true;
                }
            }

            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    this.emguRawColor = new Image<Bgra, Byte>(colorFrame.Width, colorFrame.Height, new Bgra(255,0,255,255));
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
                emguDepthImageBox.Image = this.emguOverlayedDepth;
            }

            // do our processing outside of the using block
            // so that we return resources to the kinect as soon as possible
            if (true == colorReceived)
            {
                //this.emguRawColor = new Image<Bgra, Byte>(this.colorWidth, this.colorHeight);
                //this.emguRawColor = new Image<Bgra, Byte>(this.colorWidth, this.colorHeight, new Bgra(0,255,0,255));
                
                /*long pixelIndex = 0;
                for (int j = 0; j < this.emguRawColor.Width; j++)
                {
                    for (int i = 0; i < this.emguRawColor.Height; i++, pixelIndex += 4)
                    {
                        emguRawColor[i, j] = new Bgra(this.colorPixels[pixelIndex], this.colorPixels[(pixelIndex + 1)], this.colorPixels[(pixelIndex + 2)], this.colorPixels[(pixelIndex + 3)]);
                    }
                }*/
                emguColorImageBox.Image = this.emguRawColor;
                
            }
        }
    }
}
