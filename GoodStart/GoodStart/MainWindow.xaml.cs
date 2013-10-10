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
        public MainWindow()
        {
            InitializeComponent();

            //Runtime initialization is handled when the window is opened. When the window
            //is closed, the runtime MUST be unitialized.
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(MainWindow_Unloaded);

            sensor.DepthStream.Enable();
            sensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(sensor_DepthFrameReady);
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

                    for (int depthIndex = 0, colorIndex = 0;
                        depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                        depthIndex++, colorIndex += 4)
                    {

                        // Calculate the distance represented by the two depth bytes
                        int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                        // Map the distance to an intesity that can be represented in RGB
                        var intensity = CalculateIntensityFromDistance(depth);

                        if (depth > 20 && depth < 1000)
                        {
                            // Apply the intensity to the color channels
                            pixels[colorIndex + BlueIndex] = 255; //blue
                            pixels[colorIndex + GreenIndex] = 255; //green
                            pixels[colorIndex + RedIndex] = 255; //red                    
                        }
                        else
                        {
                            pixels[colorIndex + BlueIndex] = 0; //blue
                            pixels[colorIndex + GreenIndex] = 0; //green
                            pixels[colorIndex + RedIndex] = 0; //red
                        }
                    }
                    receivedData = true;

                    BitmapSource source = BitmapSource.Create(640, 480, 96, 96,
                        PixelFormats.Gray16, null, pixels, 640 * 2);

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
            const int MinDepthDistance = 850;
            const int MaxDepthDistanceOffset = 3150;

            int newMax = distance - MinDepthDistance;
            if (newMax > 0)
                return (byte)(255 - (255 * newMax
                / (MaxDepthDistanceOffset)));
            else
                return (byte)255;
        }

    }
}
