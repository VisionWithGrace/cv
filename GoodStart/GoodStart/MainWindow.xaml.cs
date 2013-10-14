using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectDepthApplication1
{
    public partial class MainWindow : Window
    {
        //Instantiate the Kinect runtime. Required to initialize the device.
        //IMPORTANT NOTE: You can pass the device ID here, in case more than one Kinect device is connected.
        KinectSensor sensor = KinectSensor.KinectSensors.First();
        short[] rawDepthData;
        byte[] colorPixels;
        WriteableBitmap colorBitmap;
        public MainWindow()
        {
            InitializeComponent();

            //Runtime initialization is handled when the window is opened. When the window
            //is closed, the runtime MUST be unitialized.
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

            sensor.DepthStream.Enable();
            sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);


            sensor.ColorStream.Enable();
            sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            bool receivedData = false;
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {

                    this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 
                        96.0, 96.0, PixelFormats.Bgr32, null);
                    if (colorFrame != null)
                    {
                        // Write the pixel data into our bitmap
                        this.colorBitmap.WritePixels(
                          new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                          this.colorPixels,
                          this.colorBitmap.PixelWidth * sizeof(int),
                          0);
                        colorImage.Source = this.colorBitmap;
                    }
                }
            }
        }

        void sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            bool receivedData = false;

            using (DepthImageFrame DFrame = e.OpenDepthImageFrame())
            {
                if (DFrame == null)
                {
                    // The image processing took too long. More than 2 frames behind.
                }
                else
                {
                    rawDepthData = new short[DFrame.PixelDataLength];
                    DFrame.CopyPixelDataTo(rawDepthData);

                    var pixels = new byte[640 * 480 * 4];

                    const int BlueIndex = 0;
                    const int GreenIndex = 1;
                    const int RedIndex = 2;
                    const int AlphaIndex = 3;

                    for (int depthIndex = 0, colorIndex = 0;
                        depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                        depthIndex++, colorIndex += 4)
                    {

                        // Calculate the distance represented by the two depth bytes
                        int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                        // Map the distance to an intesity that can be represented in RGB
                        var intensity = CalculateIntensityFromDistance(depth);

                        if (depth >= 0 && depth < 4000)
                        {
                            // Apply the intensity to the color channels
                            pixels[colorIndex + BlueIndex] = intensity; //blue
                            pixels[colorIndex + GreenIndex] = intensity; //green
                            pixels[colorIndex + RedIndex] = intensity; //red
                            pixels[colorIndex + AlphaIndex] = 255/2; // alpha
                        }
                        else if (depth == -1)
                        {
                            pixels[colorIndex + BlueIndex] = 255; //blue
                            pixels[colorIndex + GreenIndex] = 0; //green
                            pixels[colorIndex + RedIndex] = 0; //red
                            pixels[colorIndex + AlphaIndex] = 255/2; // alpha
                        }
                        else
                        {
                            pixels[colorIndex + BlueIndex] = 0; //blue
                            pixels[colorIndex + GreenIndex] = 0; //green
                            pixels[colorIndex + RedIndex] = 0; //red
                            pixels[colorIndex + AlphaIndex] = 255/2; // alpha
                        }
                    }
                    receivedData = true;

                    BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Bgra32, null, pixels, 640 * 4);

                    depthImage.Source = source;
                }
            }

           /* if (receivedData)
            {
                BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Gray16, null, pixels, 640 * 2);

                depthImage.Source = source;
            }*/
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            sensor.Stop();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //RuntimeOptions.UseDepth is used because I am obtaining depth data
            sensor.Start();
        }

        public static byte CalculateIntensityFromDistance(int distance)
        {
            // This will map a distance value to a 0 - 255 range
            // for the purposes of applying the resulting value
            // to RGB pixels.
            const int MaxDepthDistance = 4000;
            const int MinDepthDistance = 800;
            const int MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

            int newMax = distance - MinDepthDistance;
            if (newMax > 0)
                return (byte)(255 - (255 * newMax
                / (MaxDepthDistanceOffset)));
            else
                return (byte)255;
        }
    }
}
