using System;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Kinect;

namespace HelloWorld
{
   class Program
   {
       /// <summary>
       /// The main entry point for the application.
       /// </summary>
       [STAThread]
       static void Main()
       {
           Application.EnableVisualStyles();
           Application.SetCompatibleTextRenderingDefault(false);
           Application.Run(new Form1());
       }
   }
}
